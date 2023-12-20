using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChampSelectHelperApp
{
    static class FyleSystem
    {
        private static readonly RegistryKey BOOT_KEY = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        private static readonly string APP_DATA_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Program.APP_DEVELOPER, Program.APP_NAME);

        public static bool LaunchesAtStartup()
        {
            // Update path to current executable if we moved.
            return BOOT_KEY.GetValue(Program.APP_NAME) is not null;
        }

        public static void ToggleLaunchAtStartup()
        {
            if (LaunchesAtStartup())
            {
                BOOT_KEY.DeleteValue(Program.APP_NAME);
            }
            else
            {
                BOOT_KEY.SetValue(Program.APP_NAME, Assembly.GetExecutingAssembly().Location + " " + Program.WINDOWS_STARTUP_FLAG);
            }
        }

        public static void StoreInFyle(string fyleName, string body, bool overwrite)
        {

        }

        public static string GetFyleContent(string fyleName)
        {
            return "";
        }
    }
}
