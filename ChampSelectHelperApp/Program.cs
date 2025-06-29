using System;
using System.Linq;
using System.Threading;

namespace ChampSelectHelper
{
    class Program
    {
        public static readonly string APP_NAME = "Champ Select Helper";
        public static readonly string APP_VERSION = "1.0";

        public static readonly string WINDOWS_STARTUP_FLAG = "--systemstartup";
        public static bool LAUNCHED_AT_STARTUP;

        private static Mutex mutex = new Mutex(false, APP_NAME);

        private static App instance;

        [STAThread]
        static void Main(string[] args)
        {
            if (!mutex.WaitOne(TimeSpan.Zero, false)) return;

            LAUNCHED_AT_STARTUP = args.Contains(WINDOWS_STARTUP_FLAG);

            instance = new App();
            instance.InitializeComponent();
            instance.Run();

            mutex.ReleaseMutex();
        }
    }
}
