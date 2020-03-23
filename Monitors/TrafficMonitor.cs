using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using NetworkProcessMonitor.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkProcessMonitor.Monitors
{
    public class TrafficMonitor
    {
        MainWindowForm MainWindowCallback;
        SortableBindingList<ProcessData> ProcessDataSource;
        CancellationTokenSource CancellationTokenTask;

        public TrafficMonitor(MainWindowForm form, SortableBindingList<ProcessData> processDataSource, CancellationTokenSource _cancelTasks)
        {
            MainWindowCallback = form;
            ProcessDataSource = processDataSource;
            CancellationTokenTask = _cancelTasks;
        }


        private class CustomTransfer
        {
            public Int64 Sent;
            public Int64 Received;
        }

        public void StartMonitoringInternetTrafficAsync()
        {
            new Task(() =>
            {
                Dictionary<Int32, CustomTransfer> PIDUsageDictionary = new Dictionary<int, CustomTransfer>();

                new Task(() => { MonitorInternetTraffic(PIDUsageDictionary); }, CancellationTokenTask.Token).Start();

                Stopwatch stopWatch = Stopwatch.StartNew();
                long sleepTime = 1000;
                List<Int32> ListOfPidTransfersToBeZeroed = new List<Int32>();
                while (!CancellationTokenTask.IsCancellationRequested)
                {
                    Thread.Sleep(Consts.THREAD_CANCELLATION_DELAY_CHECKER_MS);
                    sleepTime = 1000 - stopWatch.ElapsedMilliseconds;
                    if (sleepTime < 0)
                    {
                        UpdateProcessListTransfers(PIDUsageDictionary, stopWatch, ListOfPidTransfersToBeZeroed);
                    }
                }
            }, CancellationTokenTask.Token).Start();
        }

        private static void MonitorInternetTraffic(Dictionary<int, CustomTransfer> PIDUsageDictionary)
        {
            using (TraceEventSession m_EtwSession = new TraceEventSession("MyKernelAndClrEventsSession"))
            {
                m_EtwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);

                m_EtwSession.Source.Kernel.TcpIpRecv += data =>
                {
                    lock (PIDUsageDictionary)
                    {
                        if (PIDUsageDictionary.ContainsKey(data.ProcessID))
                        {
                            PIDUsageDictionary[data.ProcessID].Received += data.size;
                        }
                        else
                        {
                            PIDUsageDictionary[data.ProcessID] = new CustomTransfer
                            {
                                Received = data.size,
                                Sent = 0
                            };
                        }
                    }
                };
                m_EtwSession.Source.Kernel.TcpIpSend += data =>
                {
                    lock (PIDUsageDictionary)
                    {
                        if (PIDUsageDictionary.ContainsKey(data.ProcessID))
                        {
                            PIDUsageDictionary[data.ProcessID].Sent += data.size;
                        }
                        else
                        {
                            PIDUsageDictionary[data.ProcessID] = new CustomTransfer
                            {
                                Received = 0,
                                Sent = data.size
                            };
                        }
                    }
                };
                m_EtwSession.Source.Process();
            }
        }

        private void UpdateProcessListTransfers(Dictionary<int, CustomTransfer> PIDUsageDictionary, Stopwatch stopWatch, List<int> ListOfPidTransfersToBeZeroed)
        {
            long totalDownloaded = 0;
            long totalUploaded = 0;
            lock (PIDUsageDictionary)
            {
                lock (MainWindowForm.ProcessDataSourceLocker)
                {
                    foreach (Int32 PIDWithCurrentZeroTransferSpeed in ListOfPidTransfersToBeZeroed)
                    {
                        var processesDataToBeUpdated = ProcessDataSource.Where(processData =>
                            processData.PID == PIDWithCurrentZeroTransferSpeed);
                        foreach (ProcessData processData in processesDataToBeUpdated)
                        {
                            processData.SetDownloadingTransfer(0);
                            processData.SetUploadingTransfer(0);
                        }
                    }
                    ListOfPidTransfersToBeZeroed.Clear();
                    foreach (KeyValuePair<Int32, CustomTransfer> PIDUsagePair in PIDUsageDictionary)
                    {
                        ProcessData processDataToBeUpdated = ProcessDataSource.FirstOrDefault(
                            processData => processData.isAlive && processData.PID == PIDUsagePair.Key);
                        if (!(processDataToBeUpdated is null))
                        {
                            processDataToBeUpdated.AddReceivedSize(PIDUsagePair.Value.Received);
                            processDataToBeUpdated.AddUploadSize(PIDUsagePair.Value.Sent);
                            processDataToBeUpdated.SetDownloadingTransfer(Utils.CalculateSpeedPerSecond(PIDUsagePair.Value.Received, stopWatch.ElapsedMilliseconds));
                            processDataToBeUpdated.SetUploadingTransfer(Utils.CalculateSpeedPerSecond(PIDUsagePair.Value.Sent, stopWatch.ElapsedMilliseconds));
                            ListOfPidTransfersToBeZeroed.Add(PIDUsagePair.Key);
                        }
                    }
                }
                foreach (KeyValuePair<Int32, CustomTransfer> PIDUsagePair in PIDUsageDictionary)
                {
                    totalDownloaded += PIDUsagePair.Value.Received;
                    totalUploaded += PIDUsagePair.Value.Sent;
                }
                PIDUsageDictionary.Clear();
            }
            MainWindowCallback.Invoke((MethodInvoker)delegate
            {
                MainWindowCallback.GetProcessGridView().SuspendLayout();
                MainWindowCallback.AddTotalDownloadUpload(totalDownloaded, totalUploaded);
                MainProcessMonitor.UpdateSortedDataInDataGridView(MainWindowCallback);
                MainWindowCallback.UpdateVisibilityOfDeadProcessesInDataGridView(false);
                MainWindowCallback.GetProcessGridView().ResumeLayout();
                MainWindowCallback.GetProcessGridView().Refresh();
            });
            stopWatch.Restart();
        }
    }
}
