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
    public static class FileSystem
    {
        private static readonly RegistryKey BOOT_KEY = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        private static readonly string DATA_DIRECTORY = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Program.APP_DEVELOPER, Program.APP_NAME);

        static FileSystem()
        {
            if(!Directory.Exists(DATA_DIRECTORY)) Directory.CreateDirectory(DATA_DIRECTORY);
        }

        // Checks wether or not the app will launch at system startup
        public static bool LaunchesAtStartup()
        {
            return BOOT_KEY.GetValue(Program.APP_NAME) is not null;
            // check if it should add a key reseting when the key exists in case the app changed directory
        }

        // Toggles wether or not the app will launch at system startup
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

        public static void StoreInFile(string fileName, string body, bool append)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(DATA_DIRECTORY, fileName), append))
            {
                sw.Write(body);
            }
        }

        public static string GetFileContent(string fileName)
        {
            return "";
        }
    }
}
