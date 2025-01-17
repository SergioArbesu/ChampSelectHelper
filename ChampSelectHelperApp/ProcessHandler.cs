using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Management;

namespace ChampSelectHelperApp
{
    public static class ProcessHandler
    {
        public static (string authToken, string port)? GetLeagueProcessStatus()
        {
            Process[] processes = Process.GetProcessesByName("LeagueClientUx");
            if (processes.Length <= 0) return null;

            using (ManagementObjectCollection moc = 
                new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = "
                + processes[0].Id.ToString()).Get())
            {
                string commandOutput = (string)moc.OfType<ManagementObject>().First()["CommandLine"];

                string authToken = Regex.Match(commandOutput, "\"--remoting-auth-token=(.+?)\"").Groups[1].Value;
                string port = Regex.Match(commandOutput, "\"--app-port=(\\d+?)\"").Groups[1].Value;
                return (authToken, port);
            }
        }
    }
}
