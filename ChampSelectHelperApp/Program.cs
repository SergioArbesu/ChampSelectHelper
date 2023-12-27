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
        public static readonly string APP_DEVELOPER = "MrArbesu";

        public static readonly string CHAMPIONS_JSON_URL = "https://cdn.merakianalytics.com/riot/lol/resources/latest/en-US/champions.json";
        public static readonly string SPELLS_JSON_URL = "https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/summoner-spells.json";
        public static readonly string PERKS_JSON_URL = "http://ddragon.leagueoflegends.com/cdn/12.10.1/data/en_US/runesReforged.json";
        public static readonly string ICON_URL_START = "https://ddragon.leagueoflegends.com/cdn/img/";
        // icon paths are added to https://ddragon.leagueoflegends.com/cdn/img/

        public static readonly string WINDOWS_STARTUP_FLAG = "--systemstart";
        public static bool LAUNCHED_AT_STARTUP;

        private static App instance;

        [STAThread]
        static void Main(string[] args)
        {
            LAUNCHED_AT_STARTUP = args.Contains(WINDOWS_STARTUP_FLAG);

            instance = new App();
            instance.InitializeComponent();
            instance.Run();
        }
    }
}
