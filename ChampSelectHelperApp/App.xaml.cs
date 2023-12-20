using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Input;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Windows;
using FontStyle = System.Drawing.FontStyle;
using MessageBox = System.Windows.MessageBox;
using Cursors = System.Windows.Input.Cursors;
//using System.Windows.Controls;

using System.Threading;

namespace ChampSelectHelperApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private NotifyIcon icon;

        public App()
        {
            icon = new NotifyIcon();
            icon.Text = Program.APP_NAME;
            icon.Visible = true;
            using (Stream stream = GetResourceStream(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/Resources/icon.ico")).Stream)
            {
                icon.Icon = new Icon(stream);       
            }

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripLabel label = new ToolStripLabel();
            ToolStripMenuItem openSettings = new ToolStripMenuItem();
            ToolStripMenuItem quitApp = new ToolStripMenuItem();
            icon.ContextMenuStrip = contextMenu;

            icon.MouseClick += (sender, ev) => { if (ev.Button == MouseButtons.Left) OpenSettingsWindow(); };

            contextMenu.RenderMode = ToolStripRenderMode.System;

            label.Text = Program.APP_NAME + " v" + Program.APP_VERSION;
            label.Font = new Font(label.Font, FontStyle.Bold);
            contextMenu.Items.Add(label);

            contextMenu.Items.Add(new ToolStripSeparator());

            openSettings.Text = "Settings";
            using (Stream stream = GetResourceStream(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/Resources/settings.ico")).Stream)
            {
                openSettings.Image = Image.FromStream(stream);
            }
            openSettings.Click += (sender, ev) => OpenSettingsWindow();
            contextMenu.Items.Add(openSettings);

            quitApp.Text = "Quit";
            using (Stream stream = GetResourceStream(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/Resources/quit.ico")).Stream)
            {
                quitApp.Image = Image.FromStream(stream);
            }
            quitApp.Click += (sender, ev) => Shutdown();
            contextMenu.Items.Add(quitApp);

            if (!Program.LAUNCHED_AT_STARTUP) ShowNotification(Program.APP_NAME + " is running in the background.");
        }

        public void ShowNotification(string text)
        {
            icon.BalloonTipTitle = Program.APP_NAME;
            icon.BalloonTipText = text;
            icon.ShowBalloonTip(5000);
        }

        public void ShowMessageWindow(string text)
        {

        }

        private void OpenSettingsWindow()
        {
            //create window with empty elements
            //load all the info
            //make visible all the elements
            var settWindow = new SettingsWindow();
            settWindow.Show();
            settWindow.InitializeWindow();
        }
    }
}
