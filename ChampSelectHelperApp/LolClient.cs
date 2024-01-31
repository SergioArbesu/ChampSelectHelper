using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ChampSelectHelperApp
{
    public class LolClient
    {
        private static HttpClient httpClient;

        private string token;
        private string port;

        public bool IsConnected { get; private set; }

        static LolClient()
        {
            httpClient = new HttpClient(new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual, //check if necessary
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            });
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public LolClient()
        {
            
        }

        private void TryConnectOrRetry()
        {
            if (IsConnected) return;
            TryConnect();

            Task.Delay(5000).ContinueWith(x => TryConnectOrRetry());
        }

        private void TryConnect()
        {
            
        }
    }
}
