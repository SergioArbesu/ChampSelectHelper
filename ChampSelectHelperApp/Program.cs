using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;   

namespace ChampSelectHelperApp
{
    class Program
    {
        public static readonly string APP_NAME = "Champion Select Helper";
        public static readonly string APP_VERSION = "0.0";
        public static readonly string APP_DEVELOPER = "Arbesu";

        public static string CHAMPIONS_JSON_URL = "https://cdn.merakianalytics.com/riot/lol/resources/latest/en-US/champions.json";

        public static bool LAUNCHED_AT_STARTUP;

        private static App _instance;

        [STAThread]
        static void Main(string[] args)
        {
            LAUNCHED_AT_STARTUP = args.Contains("-startup");

            _instance = new App();
            _instance.InitializeComponent();
            _instance.Run();
        }
    }
}
