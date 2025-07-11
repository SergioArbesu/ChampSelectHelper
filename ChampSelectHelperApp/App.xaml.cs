﻿using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace ChampSelectHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private NotifyIcon icon;
        private LeagueConnection leagueConn;

        public App()
        {
            // #######
            // Start System Tray Icon configuration
            // #######

            icon = new NotifyIcon();
            icon.Text = Program.APP_NAME;
            icon.Visible = true;
            using (Stream stream = GetResourceStream(new Uri(@"pack://application:,,,/Resources/System Icons/icon.ico")).Stream)
            {
                icon.Icon = new Icon(stream);       
            }

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripLabel label = new ToolStripLabel();
            ToolStripMenuItem launchAtStartup = new ToolStripMenuItem();
            ToolStripMenuItem openSettings = new ToolStripMenuItem();
            ToolStripMenuItem quitApp = new ToolStripMenuItem();
            icon.ContextMenuStrip = contextMenu;

            icon.MouseClick += (sender, e) => { if (e.Button == MouseButtons.Left) OpenSettingsWindow(sender, e); };
            icon.BalloonTipClicked += OpenSettingsWindow;

            label.Text = Program.APP_NAME + " v" + Program.APP_VERSION;
            label.Font = new Font(label.Font, FontStyle.Bold);
            contextMenu.Items.Add(label);

            contextMenu.Items.Add(new ToolStripSeparator());

            launchAtStartup.Text = "Launch At System Startup";
            launchAtStartup.CheckOnClick = true;
            launchAtStartup.Checked = FileHandler.LaunchesAtStartup();
            launchAtStartup.CheckedChanged += (sender, ev) => FileHandler.ToggleLaunchAtStartup();
            contextMenu.Items.Add(launchAtStartup);

            openSettings.Text = "Settings";
            using (Stream stream = GetResourceStream(new Uri(@"pack://application:,,,/Resources/System Icons/settings.ico")).Stream)
            {
                openSettings.Image = Image.FromStream(stream);
            }
            openSettings.Click += OpenSettingsWindow;
            contextMenu.Items.Add(openSettings);

            quitApp.Text = "Quit";
            using (Stream stream = GetResourceStream(new Uri(@"pack://application:,,,/Resources/System Icons/quit.ico")).Stream)
            {
                quitApp.Image = Image.FromStream(stream);
            }
            quitApp.Click += (sender, ev) => Shutdown();
            contextMenu.Items.Add(quitApp);

            // #######
            // End System Tray Icon configuration
            // #######

            if (!Program.LAUNCHED_AT_STARTUP) ShowNotification(Program.APP_NAME + " is running in the background.");

            leagueConn = new LeagueConnection();
        }

        public void ShowNotification(string text)
        {
            icon.BalloonTipTitle = Program.APP_NAME;
            icon.BalloonTipText = text;
            icon.ShowBalloonTip(5000);
        }

        public void ShowMessageWindow(string text)
        {
            // what was this for lol
        }

        private async void OpenSettingsWindow(object? sender, EventArgs ev)
        {
            if (SettingsWindow.Current is not null)
            {
                if (!SettingsWindow.Current.IsActive)
                    SettingsWindow.Current.Activate();

                if (!SettingsWindow.Current.IsFocused)
                    SettingsWindow.Current.Focus();
            }
            else
            {
                var loadingWindow = new LoadingWindow();
                var settWindow = new SettingsWindow();
                loadingWindow.Show();
                await settWindow.InitializeWindow();
                if (SettingsWindow.Current is not null) settWindow.Show();
                loadingWindow.Close();
                //add try catch block for and add a messagebox when an error is thrown
            }
        }
    }
}
