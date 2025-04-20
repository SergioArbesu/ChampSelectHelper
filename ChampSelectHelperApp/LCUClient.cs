using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using Newtonsoft.Json.Linq;
using SuperSocket.WebSocket;
using WebSocket4Net;

namespace ChampSelectHelperApp
{
    class LCUClient
    {
        private readonly int RETRY_DELAY;

        private HttpClient? httpClient;

        private WebSocket? websocket;
        private Dictionary<string, Action<LCUEventArgs>> suscribers; //make this a custom event implementation

        private string? authToken;
        private string? port;

        public bool IsConnected { get; private set; }

        public LCUClient(int retryDelay = 5000)
        {
            RETRY_DELAY = retryDelay;
            suscribers = new();
            Task.Run(TryConnectOrRetry);
        }

        public async Task<string> SendRequest(string httpMethod, string path, string? body = null)
        {
            if (!IsConnected) throw new InvalidOperationException("Client is not connected");
            if (httpMethod != "GET" && httpMethod != "POST" && httpMethod != "PUT" && httpMethod != "PATCH" && httpMethod != "DELETE") 
                throw new ArgumentException("HTTP method is not supported");

            var response = await httpClient.SendAsync(new HttpRequestMessage(new HttpMethod(httpMethod), path)
            { Content = body is null ? null : new StringContent(body, null, "application/json") });

            string responseStr = await response.Content.ReadAsStringAsync();
            return responseStr;
        }

        public void SuscribeEvent(string eventName, Action<LCUEventArgs> args)
        {
            suscribers[eventName] = args;
        }

        public void UnsuscribeEvent(string eventName)
        {
            suscribers.Remove(eventName);
        }

        private async Task TryConnectOrRetry()
        {
            await TryConnect();
            while (!IsConnected)
            {
                await Task.Delay(RETRY_DELAY);
                await TryConnect();
            }
        }

        private async Task TryConnect()
        {
            if (IsConnected) return;

            (authToken, port) = GetLeagueProcessInfo();
            if (authToken is null || port is null) return;

            // http client initalization
            httpClient = new HttpClient(new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual, //check if I need this
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            });
            httpClient.BaseAddress = new Uri($"https://127.0.0.1:{port}/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // websocket initialization
            websocket = new WebSocket($"wss://127.0.0.1:{port}/");
            websocket.Security.Credential = new NetworkCredential("riot", authToken);
            websocket.Security.RemoteCertificateValidationCallback = (a,b,c,d) => true;
            websocket.AddSubProtocol("wamp");
            websocket.PackageHandler += LCUMessageReceived;
            websocket.Closed += Disconnect;

            await websocket.OpenAsync();
            websocket.StartReceive();

            IsConnected = true;

            foreach (string key in suscribers.Keys)
            {
                await websocket.SendAsync($"[5, \"{key}\"]");
            }
        }

        private async ValueTask LCUMessageReceived(object sender, WebSocketPackage args)
        {
            var messageArray = JArray.Parse(args.Message);

            if (messageArray.Count < 3) return;
            if ((int)messageArray[0] != 8) return;
            
            string lcuEvent = (string)messageArray[1];
            if (!suscribers.ContainsKey(lcuEvent)) return;

            JObject lcuEventArgs = (JObject)messageArray[2];
            suscribers[lcuEvent].Invoke(new LCUEventArgs()
            {
                Path = (string)lcuEventArgs["uri"],
                EventType = (string)lcuEventArgs["eventType"],
                Data = (string)lcuEventArgs["evenType"] == "Delete" ? null : (string)lcuEventArgs["data"]
            });
        }

        private void Disconnect(object? sender, EventArgs args)
        {
            IsConnected = false;
            if (httpClient is not null)
            {
                httpClient.Dispose();
                httpClient = null;
            }
            if (websocket is not null)
            {
                websocket.PackageHandler -= LCUMessageReceived;
                websocket.Closed -= Disconnect;
                websocket = null;

            }
            authToken = null; port = null;
            Task.Run(TryConnectOrRetry);
        }

        private (string? authToken, string? port) GetLeagueProcessInfo()
        {
            Process[] processes = Process.GetProcessesByName("LeagueClientUx");
            if (processes.Length <= 0) return (null, null);

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
