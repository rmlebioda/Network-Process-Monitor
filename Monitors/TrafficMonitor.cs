using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
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
        private const int REFERSH_RATE_MS = 1000;

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
                long sleepTime = REFERSH_RATE_MS;
                List<Int32> ListOfPidTransfersToBeZeroed = new List<Int32>();
                while (!CancellationTokenTask.IsCancellationRequested)
                {
                    Thread.Sleep(Consts.THREAD_CANCELLATION_DELAY_CHECKER_MS);
                    sleepTime = REFERSH_RATE_MS - stopWatch.ElapsedMilliseconds;
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

                #region TCP/IP actions
                m_EtwSession.Source.Kernel.TcpIpRecv += data =>
                {
                    SafeAddNetworkData(PIDUsageDictionary, data.ProcessID, data.size, 0);
                };
                m_EtwSession.Source.Kernel.TcpIpSend += data =>
                {
                    SafeAddNetworkData(PIDUsageDictionary, data.ProcessID, 0, data.size);
                };
                m_EtwSession.Source.Kernel.TcpIpRecvIPV6 += data =>
                {
                    SafeAddNetworkData(PIDUsageDictionary, data.ProcessID, data.size, 0);
                };
                m_EtwSession.Source.Kernel.TcpIpSendIPV6 += data =>
                {
                    SafeAddNetworkData(PIDUsageDictionary, data.ProcessID, 0, data.size);
                };
                #endregion

                #region UDP actions
                m_EtwSession.Source.Kernel.UdpIpRecv += data =>
                {
                    SafeAddNetworkData(PIDUsageDictionary, data.ProcessID, data.size, 0);
                };
                m_EtwSession.Source.Kernel.UdpIpSend += data =>
                {
                    SafeAddNetworkData(PIDUsageDictionary, data.ProcessID, 0, data.size);
                };
                m_EtwSession.Source.Kernel.UdpIpRecvIPV6 += data =>
                {
                    SafeAddNetworkData(PIDUsageDictionary, data.ProcessID, data.size, 0);
                };
                m_EtwSession.Source.Kernel.UdpIpSendIPV6 += data =>
                {
                    SafeAddNetworkData(PIDUsageDictionary, data.ProcessID, 0, data.size);
                };
                #endregion
                m_EtwSession.Source.Process();
            }
        }

        private static void SafeAddNetworkData(Dictionary<int, CustomTransfer> PIDUsageDictionary, Int32 PID, Int32 Rcvd, Int64 Sent)
        {
            lock (PIDUsageDictionary)
            {
                if (PIDUsageDictionary.ContainsKey(PID))
                {
                    PIDUsageDictionary[PID].Received += Rcvd;
                    PIDUsageDictionary[PID].Sent += Sent;
                }
                else
                {
                    PIDUsageDictionary[PID] = new CustomTransfer
                    {
                        Received = Rcvd,
                        Sent = Sent
                    };
                }
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
                            processData => (processData.isAlive || (Utils.ElapsedTime(processData.EndTime) < 5*REFERSH_RATE_MS)) 
                                && processData.PID == PIDUsagePair.Key);
                        if (!(processDataToBeUpdated is null))
                        {
                            processDataToBeUpdated.AddReceivedSize(PIDUsagePair.Value.Received);
                            processDataToBeUpdated.AddUploadSize(PIDUsagePair.Value.Sent);
                            processDataToBeUpdated.SetDownloadingTransfer(Utils.CalculateSpeedPerSecond(PIDUsagePair.Value.Received, stopWatch.ElapsedMilliseconds));
                            processDataToBeUpdated.SetUploadingTransfer(Utils.CalculateSpeedPerSecond(PIDUsagePair.Value.Sent, stopWatch.ElapsedMilliseconds));
                            ListOfPidTransfersToBeZeroed.Add(PIDUsagePair.Key);
                        }
                        else
                        {
                            if (!(MainWindowForm.ErrorLogger is null))
                            {
                                MainWindowForm.ErrorLogger.LogObject(
                                    className: Utils.GetCallerClassFuncName(),
                                    severity: 1,
                                    additionalInfo: $"Failed to find active/recently killed (in past {5 * REFERSH_RATE_MS}ms) process with "
                                        + $"PID: {PIDUsagePair.Key} and usage {PIDUsagePair.Value} bytes",
                                    errorObjectToLog: PIDUsagePair
                                );
                            }
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
