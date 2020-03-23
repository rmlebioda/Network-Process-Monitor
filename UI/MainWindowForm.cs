using NetworkProcessMonitor.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace NetworkProcessMonitor
{
    public partial class MainWindowForm : Form
    {
        private static SortableBindingList<ProcessData> ProcessDataSource = new SortableBindingList<ProcessData>();
        private static CancellationTokenSource _cancelTasks;
        private static Task task;
        private Int64 TotalDownloadedLong = 0;
        private Int64 TotalUploadedLong = 0;
        private MainProcessMonitor MainProcessMonitorInstance;
        public static readonly object ProcessDataSourceLocker = new object();
        public static readonly CSVLoggerHelper ErrorLogger;

        static MainWindowForm()
        {
            try
            {
                string loggerPath = ConfigurationManager.AppSettings[Consts.CONFIG_ERROR_LOGGER_PATH];
                if (!loggerPath.Equals(String.Empty))
                {
                    MainWindowForm.ErrorLogger = new CSVLoggerHelper(
                                            logPath: loggerPath,
                                            createCSVFileNow: true,
                                            ignoreHeader: false
                                        );
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(
                        $"Failed to load '{Consts.CONFIG_LIST_REFRESH_RATE}' from config file because of exception during reading: {e.Message}"
                            + Consts.NETWORKPROCESSMONITOR_CONFIG_REMEDIUM + "\nErrors will not be logged in CSV file",
                        "Failed to load default settings",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                ErrorLogger = null;
            }
        }

        public MainWindowForm()
        {
            InitializeComponent();
            this.ProcessDataGridView.SetParentWindow(this);

            ProcessDataGridView.DataSource = ProcessDataSource;

            HideUnwantedColumns(ProcessDataGridView);
            SetColumnsToDefaultWidth(ProcessDataGridView);

            _cancelTasks = new CancellationTokenSource();
            MainProcessMonitorInstance = new MainProcessMonitor(this, ProcessDataSource, _cancelTasks);
            task = new Task(() => { MainProcessMonitorInstance.Run(); }, _cancelTasks.Token);
            task.Start();
        }

        private void SetColumnsToDefaultWidth(DataGridView dataGridView1)
        {
            dataGridView1.Columns["ProcessName"].Width = 250;
            dataGridView1.Columns["PID"].Width = 50;
            dataGridView1.Columns["ReceivedBytes"].Width = 110;
            dataGridView1.Columns["UploadedBytes"].Width = 110;
            dataGridView1.Columns["Downloading"].Width = 110;
            dataGridView1.Columns["Uploading"].Width = 110;
            dataGridView1.Columns["StartTime"].Width = 150;
            dataGridView1.Columns["EndTime"].Width = 150;
            dataGridView1.Columns["ImagePath"].Width = 600;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancelTasks.Cancel();
            task.Wait();
            _cancelTasks.Dispose();
        }

        public DataGridViewWithProcessDataListSource GetProcessGridView()
        {
            return this.ProcessDataGridView;
        }

        public static SortableBindingList<ProcessData> GetProcessDataSource()
        {
            return ProcessDataSource;
        }

        public void HideUnwantedColumns(DataGridView fromDataGridView)
        {
            fromDataGridView.Columns["isAlive"].Visible = false;
            fromDataGridView.Columns["SessionId"].Visible = false;
        }

        private void ProcessDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                MainProcessMonitor.MarkDeadProcessesInGrid(this);
                MainProcessMonitorInstance.GridChangedUpdateChildrenEvent();
            }
            catch(Exception exception)
            {
                if (!(ErrorLogger is null)) ErrorLogger.LogObject(Utils.GetCallerClassFuncName(), 0, "unknown", exception);
                else throw exception;
            }
        }

        public bool ShouldHideRow(ProcessData processData)
        {
            if (this.HideDeadProcesses.Checked && !processData.isAlive) return true;
            if (this.HideActiveProcessesCheck.Checked && processData.isAlive) return true;
            return false;
        }

        private void HideDeadProcesses_CheckedChanged(object sender, EventArgs e)
        {
            UpdateVisibilityOfDeadProcessesInDataGridView();
            MainProcessMonitorInstance.GridChangedUpdateChildrenEvent();
        }

        public void UpdateVisibilityOfDeadProcessesInDataGridView(bool shouldScrollToTop = true)
        {
            lock (ProcessDataSourceLocker)
            {
                for (int i = 0; i < ProcessDataSource.Count; i++)
                {
                    ProcessDataGridView.UpdateRowVisibility(i, !ShouldHideRow(ProcessDataSource[i]));
                }
            }
            if (shouldScrollToTop) ProcessDataGridView.ScrollToTop();
        }

        private void ProcessDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (ProcessData.SIZE_FORMAT_COLUMNS.Contains(this.ProcessDataGridView.Columns[e.ColumnIndex].Name))
            {
                e.Value = Utils.SizeSuffix((long)e.Value);
            }
            else if (ProcessData.SPEED_FORMAT_COLUMNS.Contains(this.ProcessDataGridView.Columns[e.ColumnIndex].Name))
            {
                e.Value = Utils.SpeedSuffix((long)e.Value);
            }
        }

        public void AddTotalDownloadUpload(long download, long upload)
        {
            TotalDownloadedLong += download;
            TotalUploadedLong += upload;
            this.TotalDownloaded.Text = Utils.SizeSuffix(TotalDownloadedLong, 2);
            this.TotalUploaded.Text = Utils.SizeSuffix(TotalUploadedLong, 2);
            this.TransferStatusStrip.Refresh();
        }

        public void SetShowingProcesses(int visibleRows)
        {
            this.ShowingProcesses.Text = visibleRows.ToString();
        }
    }
}
