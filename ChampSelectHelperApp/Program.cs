using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;   

namespace ChampSelectHelperApp
{
    class Program
    {
        public static readonly string APP_NAME = "Champ Select Helper";
        public static readonly string APP_VERSION = "0.0";
        public static readonly string APP_DEVELOPER = "Arbesu";

        public static readonly string CHAMPIONS_JSON_URL = "https://cdn.merakianalytics.com/riot/lol/resources/latest/en-US/champions.json";
        public static readonly string PERKS_JSON_URL = "http://ddragon.leagueoflegends.com/cdn/12.10.1/data/en_US/runesReforged.json";
        // Icon paths are added to https://ddragon.leagueoflegends.com/cdn/img/

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
