﻿using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace NetworkProcessMonitor
{
    public class CSVLoggerHelper
    {
        private String RelativePath;
        private String ErrorFilePath;
        private bool GlobalIgnoreErrors = false;
        private bool IgnoreCSVHeader = false;

        public CSVLoggerHelper(String logPath, bool createCSVFileNow = false, bool ignoreHeader = false)
        {
            this.RelativePath = logPath;
            this.ErrorFilePath = RelativePath + (RelativePath.EndsWith(@"\") ? "" : @"\")
                    + @"NPM_ErrorCSVLog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
            this.IgnoreCSVHeader = ignoreHeader;
            if (createCSVFileNow) CreateCSVFileIfDoesntExist();
        }

        private void CreateCSVFileIfDoesntExist()
        {
            if (!System.IO.File.Exists(ErrorFilePath))
            {
                System.IO.Directory.CreateDirectory(RelativePath);

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(ErrorFilePath, true))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteComment("File created at " + DateTime.Now.ToString());
                    csv.NextRecord();
                    if(!this.IgnoreCSVHeader)
                    {
                        csv.WriteField("Callstack class name");
                        csv.WriteField("Severity");
                        csv.WriteField("Additional info");
                        csv.WriteField("Error object in JSON");
                        csv.NextRecord();
                    }
                }
            }
        }

        public void LogObject(String className, Int32 severity, String additionalInfo, object errorObjectToLog, bool ignoreWritingErrors = false)
        {
            CreateCSVFileIfDoesntExist();
            try
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(ErrorFilePath, true))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteField(className);
                        csv.WriteField(severity);
                        csv.WriteField(additionalInfo);
                        csv.WriteField(Newtonsoft.Json.JsonConvert.SerializeObject(errorObjectToLog));
                        csv.NextRecord();
                    }
                }
            }
            catch (Exception e)
            {
                if(!GlobalIgnoreErrors && !ignoreWritingErrors)
                {
                    var dialogResult = System.Windows.MessageBox.Show(
                            $"Failed to log exception to CSV file in '{ErrorFilePath}' because of error during writing: {e.Message}\nIgnore future errors?",
                            "Failed to log exception to CSV file", 
                            MessageBoxButton.YesNo, 
                            MessageBoxImage.Error
                        );
                    if (dialogResult == MessageBoxResult.Yes)
                        GlobalIgnoreErrors = true;
                }
            }
        }
    }
}