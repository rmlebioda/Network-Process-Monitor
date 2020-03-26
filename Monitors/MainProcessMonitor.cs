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
using NetworkProcessMonitor.Models;
using NetworkProcessMonitor.Monitors;
using NetworkProcessMonitor.UI;

namespace NetworkProcessMonitor.Monitors
{
    public class MainProcessMonitor
    {
        private readonly MainWindowForm MainWindowFormCallback;
        private readonly StableSortableBindingList<ProcessData> ProcessDataSource;
        private readonly CancellationTokenSource CancellationTokenTask;

        private readonly Int32 ListRefreshRate;
        private IAsyncResult UIUpdateInvokeStatus;
        private TrafficMonitor TrafficMonitorTask;


        public MainProcessMonitor(MainWindowForm form, StableSortableBindingList<ProcessData> processDataSource, CancellationTokenSource _cancelTasks)
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
            TrafficMonitorTask = new TrafficMonitor(MainWindowFormCallback, ProcessDataSource, CancellationTokenTask, ListRefreshRate);
            TrafficMonitorTask.StartMonitoringInternetTrafficAsync();

            while (!CancellationTokenTask.IsCancellationRequested)
            {
                Stopwatch startStopwatch = Stopwatch.StartNew();
                try
                {
                    List<ProcessData> newProcesses = GetNewProcesses();
                    CustomTransfer TotalTransferSize = TrafficMonitorTask.UpdateProcessListTransfers();

                    InvokeUpdateUI(newProcesses, TotalTransferSize);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Received exception on monitor thread: {e.Message}");
                    if (!(MainWindowForm.ErrorLogger is null)) MainWindowForm.ErrorLogger.LogObject(Utils.GetCallerClassFuncName(), 10, "Failed to refresh process list", e);
                }
                SleepDelay(startStopwatch);
            }
        }

        private void InvokeUpdateUI(List<ProcessData> newProcesses, CustomTransfer totalTransferSize)
        {
            UIUpdateInvokeStatus = MainWindowFormCallback.BeginInvoke((MethodInvoker)delegate
            {
                DataGridViewWithProcessDataListSource dataGridView = MainWindowFormCallback.GetProcessGridView();

                dataGridView.SaveAndSuspendCurrentView();

                AddNewProcesses(newProcesses);
                MainWindowFormCallback.UpdateVisibilityOfDeadProcessesInDataGridView(false);
                UpdateSortedDataInDataGridView(shouldMarkDeadProcesses: true);

                dataGridView.RestoreAndResumeCurrentView();
                dataGridView.Refresh();

                UpdateBottomToolStrip(totalTransferSize);
            });
        }

        private List<ProcessData> GetNewProcesses()
        {
            Process[] processList = Process.GetProcesses();
            lock (MainWindowForm.ProcessDataSourceLocker)
            {
                List<ProcessData> processesToBeAdded = FindNewProcesses(processList);
                MarkDeadProcessesInSource(processList);
                return processesToBeAdded;
            }
            //InvokeManagingNewProcessesOnUIThread(processesToBeAdded, deadProcesses, processDataSourceCopy);
        }

        private List<ProcessData> FindNewProcesses(Process[] processList)
        {
            List<ProcessData> processesToBeAdded = new List<ProcessData>();
            foreach (Process process in processList)
            {
                ProcessData resProcess = ProcessDataSource.FirstOrDefault(processData => IsProcessEqualProcessData(process, processData));
                if (resProcess == null)
                {
                    processesToBeAdded.Add(new ProcessData(process));
                }
            }
            return processesToBeAdded;
        }

        private void MarkDeadProcessesInSource(Process[] processList)
        {
            List<ProcessData> deadProcesses = new List<ProcessData>();
            foreach (ProcessData processData in ProcessDataSource.Where(processData => processData.isAlive))
            {
                Process process = processList.FirstOrDefault(_process => IsProcessEqualProcessData(_process, processData));
                if(process == null)
                {
                    processData.MarkDead();
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
            while ((sleepTime > 0 || !UIUpdateInvokeStatus.IsCompleted) && !CancellationTokenTask.IsCancellationRequested)
            {
                Thread.Sleep(Consts.THREAD_CANCELLATION_DELAY_CHECKER_MS);
                sleepTime = ListRefreshRate - stopwatch.ElapsedMilliseconds;
            }
        }

        public void GridChangedUpdateChildrenEvent()
        {
            MainWindowFormCallback.UpdateNoOfShowedProcessesInTable();
        }

        private void UpdateBottomToolStrip(CustomTransfer totalTransferSize)
        {
            MainWindowFormCallback.UpdateNoOfShowedProcessesInTable();
            MainWindowFormCallback.AddTotalDownloadUpload(totalTransferSize.Received, totalTransferSize.Sent);
        }


        public void UpdateSortedDataInDataGridView(bool shouldMarkDeadProcesses = true)
        {
            if (MainWindowFormCallback.GetProcessGridView().SortedColumn != null)
            {
                MainWindowFormCallback.GetProcessGridView().Sort(
                        MainWindowFormCallback.GetProcessGridView().SortedColumn,
                        StableSortableBindingList<ProcessData>.GetCompatibleListSortOrderFrom(MainWindowFormCallback.GetProcessGridView().SortOrder)
                    );
            }
            if (shouldMarkDeadProcesses) MarkDeadProcessesInGrid();
        }

        public void MarkDeadProcessesInGrid()
        {
            foreach (DataGridViewRow row in MainWindowFormCallback.GetProcessGridView().Rows)
            {
                if ( (!(row.Cells["isAlive"].Value is null)) &&
                    ((bool)row.Cells["isAlive"].Value) == false)
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
