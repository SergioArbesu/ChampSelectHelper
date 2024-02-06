using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChampSelectHelperApp
{
    class Program
    {
        public static readonly string APP_NAME = "Champ Select Helper";
        public static readonly string APP_VERSION = "0.0";
        public static readonly string APP_DEVELOPER = "MrArbesu";

        public static readonly string WINDOWS_STARTUP_FLAG = "--systemstartup";
        public static bool LAUNCHED_AT_STARTUP;

        private static Mutex mutex = new Mutex(false, APP_NAME);

        private static App instance;

        [STAThread]
        static void Main(string[] args)
        {
            if (mutex.WaitOne(TimeSpan.Zero, false))
            {
                LAUNCHED_AT_STARTUP = args.Contains(WINDOWS_STARTUP_FLAG);

                instance = new App();
                instance.InitializeComponent();
                instance.Run();

                mutex.ReleaseMutex();
            }
        }
    }
}
