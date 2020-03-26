using NetworkProcessMonitor.Helpers;
using NetworkProcessMonitor.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProcessMonitor.Models
{
    public class ProcessData
    {
        public static readonly string[] SIZE_FORMAT_COLUMNS = { "ReceivedBytes", "UploadedBytes" };
        public static readonly string[] SPEED_FORMAT_COLUMNS = { "Downloading", "Uploading" };
        public const String PROCESS_NAME_COLUMN_NAME = "Process name";
        public const String PROCESS_ID_COLUMN_NAME = "Process ID";
        public const String RECEIVED_BYTES_COLUMN_NAME = "Received bytes";
        public const String UPLOADED_BYTES_COLUMN_NAME = "Uploaded bytes";
        public const String DOWNLOADING_COLUMN_NAME = "Downloading";
        public const String UPLOADING_COLUMN_NAME = "Uploading";
        public const String STARTED_COLUMN_NAME = "Started";
        public const String FINISHED_COLUMN_NAME = "Finished";
        public const String IMAGE_FULL_PATH_COLUMN_NAME = "Image path";

        private static UInt64 UID = 0;
        private static readonly object UIDLocker = new object();

        public UInt64 UniqueID { get; private set; }
        public bool isAlive { get; private set; }
        [System.ComponentModel.DisplayName(PROCESS_NAME_COLUMN_NAME)]
        public String ProcessName { get; private set; }
        [System.ComponentModel.DisplayName(PROCESS_ID_COLUMN_NAME)]
        public Int64 PID { get; private set; }
        [System.ComponentModel.DisplayName(RECEIVED_BYTES_COLUMN_NAME)]
        public Int64 ReceivedBytes { get; private set; }
        [System.ComponentModel.DisplayName(UPLOADED_BYTES_COLUMN_NAME)]
        public Int64 UploadedBytes { get; private set; }
        [System.ComponentModel.DisplayName(DOWNLOADING_COLUMN_NAME)]
        public Int64 Downloading { get; private set; }
        [System.ComponentModel.DisplayName(UPLOADING_COLUMN_NAME)]
        public Int64 Uploading { get; private set; }
        [System.ComponentModel.DisplayName(STARTED_COLUMN_NAME)]
        public System.DateTime StartTime { get; private set; }
        [System.ComponentModel.DisplayName(FINISHED_COLUMN_NAME)]
        public System.DateTime EndTime { get; private set; }
        public Int32 SessionId { get; private set; }
        [System.ComponentModel.DisplayName(IMAGE_FULL_PATH_COLUMN_NAME)]
        public String ImagePath { get; private set; }


        private UInt64 GetUniqueID()
        {
            lock (UIDLocker)
            {
                return UID++;
            }
        }

        public ProcessData(Process process)
        {
            // process identificators
            UniqueID = GetUniqueID();
            PID = process.Id;
            ProcessName = process.ProcessName;
            SessionId = process.SessionId;

            ReceivedBytes = 0;
            UploadedBytes = 0;
            Downloading = 0;
            Uploading = 0;
            isAlive = true;
            try
            {
                StartTime = process.StartTime;
            }
            catch (Exception)
            {
                StartTime = DateTime.MinValue;
            }
            EndTime = DateTime.MinValue;
            try
            {
                // https://stackoverflow.com/questions/9501771/how-to-avoid-a-win32-exception-when-accessing-process-mainmodule-filename-in-c
                // ImagePath = process.MainModule.FileName;     // faster, but MainModule is sometimes unaccessible and is not showing path for most apps
                // ImagePath = Utils.GetMainModuleFilepath(process.Id);  // good, but slow
                ImagePath = Utils.GetProcessName(process.Id);   // fast and seems reliable
            } 
            catch(Exception)
            {
                ImagePath = String.Empty;
            }
        }

        public void MarkDead(DataGridViewWithProcessDataListSource dataGridView = null, int index = 0, bool hideRow = true)
        {
            EndTime = DateTime.Now;
            isAlive = false;
            if (!(dataGridView is null)) dataGridView.UpdateRowVisibility(index, !hideRow);
        }

        public void AddReceivedSize(Int64 receivedSize)
        {
            this.ReceivedBytes += receivedSize;
        }
        public void AddUploadSize(Int64 uploadedSize)
        {
            this.UploadedBytes += uploadedSize;
        }

        public void SetDownloadingTransfer(Int64 downloadSpeed)
        {
            this.Downloading = downloadSpeed;
        }
        public void SetUploadingTransfer(Int64 uploadSpeed)
        {
            this.Uploading = uploadSpeed;
        }
    }
}
