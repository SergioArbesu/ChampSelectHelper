using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Websocket.Client;

namespace ChampSelectHelper
{
    class LCUClient
    {
        private readonly int RETRY_DELAY;

        private HttpClient? httpClient;

        //private WebSocket? websocket2;
        private WebsocketClient? websocket;
        private Dictionary<string, Action<LCUEventArgs>> subscribers; //make this a custom event implementation

        private string? authToken, port;

        public bool IsConnected { get; private set; }

        public LCUClient(int retryDelay = 5000)
        {
            RETRY_DELAY = retryDelay;
            subscribers = new();
        }

        public void Connect()
        {
            Task.Run(TryConnectOrRetry);
        }

        public async Task<string> SendRequest(string httpMethod, string path, string? body = null)
        {
            if (!IsConnected) return "";
            if (httpMethod != "GET" && httpMethod != "POST" && httpMethod != "PUT" && httpMethod != "PATCH" && httpMethod != "DELETE") 
                throw new ArgumentException("HTTP method is not supported");

            var response = await httpClient.SendAsync(new HttpRequestMessage(new HttpMethod(httpMethod), "https://127.0.0.1:" + port + path)
            { Content = (body is null) ? null : new StringContent(body, Encoding.UTF8, "application/json") });

            string responseStr = await response.Content.ReadAsStringAsync();
            return responseStr;
        }

        public void SubscribeEvent(string eventName, Action<LCUEventArgs> eventHandler)
        {
            subscribers[eventName] = eventHandler;
        }

        public void UnsubscribeEvent(string eventName)
        {
            subscribers.Remove(eventName);
        }

        public void UnsubscribeAllEvents()
        {
            subscribers.Clear();
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
            httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.UTF8.GetBytes("riot:" + authToken)));

            // websocket initialization
            websocket = new WebsocketClient(new Uri($"wss://127.0.0.1:{port}/"), () =>
            {
                ClientWebSocket socket = new ClientWebSocket
                {
                    Options =
                    {
                        Credentials = new NetworkCredential("riot", authToken),
                        RemoteCertificateValidationCallback = (a,b,c,d) => true
                    }
                };
                socket.Options.AddSubProtocol("wamp");
                return socket;
            });

            websocket.MessageReceived.Subscribe(LCUMessageReceived);
            websocket.DisconnectionHappened.Subscribe(Disconnection);

            IsConnected = true;

            await websocket.Start();

            foreach (string key in subscribers.Keys)
            {
                await websocket.SendInstant($"[5, \"{key}\"]");
            }
        }

        private void LCUMessageReceived(ResponseMessage msg)
        {
            if (msg.Text is null) return;

            var messageArray = JArray.Parse(msg.Text);

            if (messageArray.Count < 3) return;
            if ((int)messageArray[0] != 8) return;
            
            string lcuEvent = (string)messageArray[1];
            
            if (!subscribers.TryGetValue(lcuEvent, out var subscriber)) return;

            JObject lcuEventArgs = (JObject)messageArray[2];
            subscriber.Invoke(new LCUEventArgs()
            {
                Path = (string)lcuEventArgs["uri"],
                EventType = (string)lcuEventArgs["eventType"],
                Data = (string)lcuEventArgs["evenType"] == "Delete" ? null : (string)lcuEventArgs["data"]
            });
        }

        private void Disconnection(DisconnectionInfo info)
        {
            IsConnected = false;
            if (httpClient is not null)
            {
                httpClient.Dispose();
                httpClient = null;
            }
            if (websocket is not null)
            {
                websocket.Dispose();
                websocket = null;

            }
            authToken = null; port = null;

            Task.Run(TryConnectOrRetry);
        }

        private (string? authToken, string? port) GetLeagueProcessInfo()
        {
            Process[] processes = Process.GetProcessesByName("LeagueClientUx");
            if (processes.Length <= 0) return (null, null);

            using (var mos = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = "
                + processes[0].Id.ToString()))
            using (var moc = mos.Get())
            {
                string commandOutput = (string)moc.OfType<ManagementObject>().First()["CommandLine"];

                string authToken = Regex.Match(commandOutput, "\"--remoting-auth-token=(.+?)\"").Groups[1].Value;
                string port = Regex.Match(commandOutput, "\"--app-port=(\\d+?)\"").Groups[1].Value;
                return (authToken, port);
            }
        }
    }
}
