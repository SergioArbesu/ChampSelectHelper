using System;
using System.Linq;
using System.Threading;

namespace ChampSelectHelperApp
{
    class Program
    {
        public static readonly string APP_NAME = "Champ Select Helper";
        public static readonly string APP_VERSION;

        public static readonly string WINDOWS_STARTUP_FLAG = "--systemstartup";
        public static bool LAUNCHED_AT_STARTUP;

        private static Mutex mutex = new Mutex(false, APP_NAME);

        private static App instance;

        static Program()
        {
            Version version = Version.Parse(Environment.GetEnvironmentVariable("ClickOnce_CurrentVersion") ?? "0.0.0.0");
            APP_VERSION = $"{version.Major}.{version.Minor}";
        }

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
