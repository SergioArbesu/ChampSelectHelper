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
            bool exists = BOOT_KEY.GetValue(Program.APP_NAME) is not null;

            // Update path to current location in case it changed
            if (exists) BOOT_KEY.SetValue(Program.APP_NAME, Assembly.GetExecutingAssembly().Location + " " + Program.WINDOWS_STARTUP_FLAG);

            return exists;
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

        // Saves a string in a file in the AppData directory
        public static void SaveInFile(string fileName, string body, bool append)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(DATA_DIRECTORY, fileName), append))
            {
                sw.Write(body);
            }
        }

        // Returns the string contained in a file located in the AppData directory. If the file doesn't exist, returns an empty string.
        public static string GetFileContent(string fileName)
        {
            string path = Path.Combine(DATA_DIRECTORY, fileName);

            if (!File.Exists(path)) return "";

            using (StreamReader sr = new StreamReader(path))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
