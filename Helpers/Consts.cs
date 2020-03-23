using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProcessMonitor.Helpers
{
    public static class Consts
    {
        public static readonly System.Drawing.Color DEAD_PROCESS_BACKGROUND_COLOR = System.Drawing.Color.FromArgb(224, 224, 224); // light gray
        public static readonly System.Drawing.Color DEAD_PROCESS_SELECTED_COLOR = System.Drawing.Color.FromArgb(189, 221, 255); // light blue
        public const String CONFIG_LIST_REFRESH_RATE = "ListRefreshRate";
        public const String CONFIG_ERROR_LOGGER_PATH = "ErrorLoggerPath";
        public const String NETWORKPROCESSMONITOR_CONFIG_REMEDIUM = "Settings in App.config seems to be broken. Fix it or download new file.";
        public const Int32 THREAD_CANCELLATION_DELAY_CHECKER_MS = 10;
    }
}
