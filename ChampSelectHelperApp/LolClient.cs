using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Websocket.Client;

namespace ChampSelectHelperApp
{
    public class LolClient
    {
        private readonly int RETRY_DELAY = 5000;

        private static HttpClient httpClient;

        private WebsocketClient socket;

        private string token;
        private string port;

        public bool IsConnected { get; private set; }

        static LolClient()
        {
            httpClient = new HttpClient(new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual, //check if I need this
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            });
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public LolClient()
        {
            TryConnectOrRetry();
        }

        private async Task TryConnectOrRetry()
        {
            if (IsConnected) return;
            TryConnect();

            Task.Delay(RETRY_DELAY).ContinueWith(x => TryConnectOrRetry());
        }

        private void TryConnect()
        {
            if (IsConnected) return;

            var processInfo = GetLeagueProcessInfo();
            if (processInfo is null) return;
            (string authToken, string port) = processInfo.Value;

            socket = new WebsocketClient(new Uri($"wss://127.0.0.1:{port}/"), () =>
            {
                ClientWebSocket client = new ClientWebSocket()
                {
                    Options =
                    {
                        Credentials = new NetworkCredential("riot", authToken),
                        RemoteCertificateValidationCallback = (a,b,c,d) => true,
                    }
                };
                client.Options.AddSubProtocol("wamp");
                return client;
            });
        }

        private (string authToken, string port)? GetLeagueProcessInfo()
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
