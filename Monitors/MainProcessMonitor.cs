using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using NetworkProcessMonitor.Helpers;
using NetworkProcessMonitor.Monitors;

namespace NetworkProcessMonitor
{
    public class MainProcessMonitor
    {
        private readonly MainWindowForm MainWindowFormCallback;
        private readonly SortableBindingList<ProcessData> ProcessDataSource;
        private readonly CancellationTokenSource CancellationTokenTask;

        private readonly Int32 ListRefreshRate;
        private IAsyncResult UIInvokeStatus;
        private TrafficMonitor TrafficMonitorTask;


        public MainProcessMonitor(MainWindowForm form, SortableBindingList<ProcessData> processDataSource, CancellationTokenSource _cancelTasks)
        {
            MainWindowFormCallback = form;
            ProcessDataSource = processDataSource;
            CancellationTokenTask = _cancelTasks;
            try
            {
                ListRefreshRate = Int32.Parse(ConfigurationManager.AppSettings[Consts.CONFIG_LIST_REFRESH_RATE]);
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show(
                        $"Failed to load '{Consts.CONFIG_LIST_REFRESH_RATE}' from config file because of exception during reading: {e.Message}\n" 
                            + Consts.NETWORKPROCESSMONITOR_CONFIG_REMEDIUM + "\nSetting refresh rate to default value: 1000 ms",
                        "Failed to load default settings", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error
                    );
                ListRefreshRate = 1000;
            }

        }

        private static bool IsProcessEqualProcessData(Process process, ProcessData processData)
        {
            return process.Id == processData.PID && process.ProcessName == processData.ProcessName && process.SessionId == processData.SessionId;
        }


        public void Run()
        {
            TrafficMonitorTask = new TrafficMonitor(MainWindowFormCallback, ProcessDataSource, CancellationTokenTask);
            TrafficMonitorTask.StartMonitoringInternetTrafficAsync();

            while (!CancellationTokenTask.IsCancellationRequested)
            {
                Stopwatch startStopwatch = Stopwatch.StartNew();
                try
                {
                    ManageNewProcesses();
                    UpdateShowingProcessToolStrip();
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Received exception on monitor thread: {e.Message}");
                    if (!(MainWindowForm.ErrorLogger is null)) MainWindowForm.ErrorLogger.LogObject(Utils.GetCallerClassFuncName(), 10, "Failed to refresh process list", e);
                }
                SleepDelay(startStopwatch);
            }
        }

        private void ManageNewProcesses()
        {
            Process[] processList = Process.GetProcesses();
            List<ProcessData> processDataSourceCopy;
            lock (MainWindowForm.ProcessDataSourceLocker)
            {
                processDataSourceCopy = ProcessDataSource.ToList();
            }

            List<ProcessData> processesToBeAdded = FindNewProcesses(processList, processDataSourceCopy);
            List<ProcessData> deadProcesses = FindDeadProcessesFromSource(processDataSourceCopy, processList);
            InvokeManagingNewProcessesOnUIThread(processesToBeAdded, deadProcesses, processDataSourceCopy);
        }

        private List<ProcessData> FindNewProcesses(Process[] processList, List<ProcessData> processListCopy)
        {
            List<ProcessData> processesToBeAdded = new List<ProcessData>();
            foreach (Process process in processList)
            {
                ProcessData resProcess = processListCopy.FirstOrDefault(processData => IsProcessEqualProcessData(process, processData));
                if (resProcess == null)
                {
                    processesToBeAdded.Add(new ProcessData(process));
                }
            }
            return processesToBeAdded;
        }

        private List<ProcessData> FindDeadProcessesFromSource(List<ProcessData> processDataSourceCopy, Process[] processList)
        {
            List<ProcessData> deadProcesses = new List<ProcessData>();
            foreach (ProcessData processData in processDataSourceCopy.Where(processData => processData.isAlive))
            {
                Process process = processList.FirstOrDefault(_process => IsProcessEqualProcessData(_process, processData));
                if(process == null)
                {
                    deadProcesses.Add(processData);
                }
            }
            return deadProcesses;
        }

        private void InvokeManagingNewProcessesOnUIThread(List<ProcessData> processesToBeAdded, List<ProcessData> deadProcesses, List<ProcessData> processDataSourceCopy)
        {
            UIInvokeStatus = MainWindowFormCallback.BeginInvoke((MethodInvoker)delegate
            {
                MarkDeadProcesses(deadProcesses, processDataSourceCopy);
                AddNewProcesses(processesToBeAdded);
                if(processesToBeAdded.Count > 0 || deadProcesses.Count > 0) UpdateSortedDataInDataGridView(MainWindowFormCallback);
            });
        }

        private void MarkDeadProcesses( List<ProcessData> deadProcesses, List<ProcessData> processDataSourceCopy)
        {
            foreach (ProcessData processData in deadProcesses)
            {
                int index = processDataSourceCopy.FindIndex(_process => _process == processData);
                foreach (DataGridViewCell cell in MainWindowFormCallback.GetProcessGridView().Rows[index].Cells)
                {
                    cell.Style.BackColor = Consts.DEAD_PROCESS_BACKGROUND_COLOR;
                    cell.Style.SelectionBackColor = Consts.DEAD_PROCESS_SELECTED_COLOR;
                }
                lock (MainWindowForm.ProcessDataSourceLocker)
                {
                    ProcessDataSource[index].MarkDead(MainWindowFormCallback.GetProcessGridView(), index, MainWindowFormCallback.ShouldHideRow(processDataSourceCopy[index]));
                }
            }
        }

        private void AddNewProcesses(List<ProcessData> processesToBeAdded)
        {
            lock (MainWindowForm.ProcessDataSourceLocker)
            {
                foreach (ProcessData pd in processesToBeAdded)
                {
                    ProcessDataSource.Add(pd);
                }
            }
        }

        private void SleepDelay(Stopwatch stopwatch)
        {
            Int64 sleepTime = ListRefreshRate - stopwatch.ElapsedMilliseconds;
            Debug.WriteLine($"Executed one loop in {stopwatch.ElapsedMilliseconds}, refresh timer: {ListRefreshRate}, refreshing in {sleepTime} ms");
            while ((sleepTime > 0 || !UIInvokeStatus.IsCompleted) && !CancellationTokenTask.IsCancellationRequested)
            {
                Thread.Sleep(Consts.THREAD_CANCELLATION_DELAY_CHECKER_MS);
                sleepTime = ListRefreshRate - stopwatch.ElapsedMilliseconds;
            }
        }

        public void GridChangedUpdateChildrenEvent()
        {
            UpdateShowingProcessToolStrip();
        }

        private void UpdateShowingProcessToolStrip()
        {
            MainWindowFormCallback.SetShowingProcesses(MainWindowFormCallback.GetProcessGridView().GetVisibleRowsCount());
        }


        public static void UpdateSortedDataInDataGridView(MainWindowForm form, bool shouldMarkDeadProcesses = true)
        {
            if (form.GetProcessGridView().SortedColumn != null)
            {
                form.GetProcessGridView().Sort(
                        form.GetProcessGridView().SortedColumn,
                        SortableBindingList<ProcessData>.GetCompatibleListSortOrderFrom(form.GetProcessGridView().SortOrder)
                    );
                if (shouldMarkDeadProcesses) MarkDeadProcessesInGrid(form);
            }
        }

        public static void MarkDeadProcessesInGrid(MainWindowForm form)
        {
            foreach (DataGridViewRow row in form.GetProcessGridView().Rows)
            {
                if (((bool)row.Cells["isAlive"].Value) == false)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style.BackColor = Consts.DEAD_PROCESS_BACKGROUND_COLOR;
                        cell.Style.SelectionBackColor = Consts.DEAD_PROCESS_SELECTED_COLOR;
                    }
                }
            }
        }
    }
}
