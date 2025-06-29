using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ChampSelectHelper
{
    public static class HelperFunctions
    {
        private static readonly object locker = new object();

        public static BitmapImage CreateBitmapImage(string uri)
        {
            var img = new BitmapImage();

            img.BeginInit();
            img.UriSource = new Uri(uri);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.DownloadCompleted += OnDownloadCompleted;

            return img;
        }

        private static void OnDownloadCompleted(object? sender, EventArgs args)
        {
            BitmapImage img = (BitmapImage)sender;
            img.DownloadCompleted -= OnDownloadCompleted;
            img.Freeze();
        }

        public static async Task<T?> RequestAndRetry<T>(Func<Task<T>> func) where T : class
        {
            if (SettingsWindow.Current is null) return null;
            try
            {
                return await func.Invoke();
            }
            catch (HttpRequestException)
            {
                var result = MessageBox.Show("An error occurred while trying to connect to the internet.\n\nDo you want to retry?",
                    "Connecction Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (result == MessageBoxResult.No)
                {
                    SettingsWindow.Current?.Close();
                    SettingsWindow.Current = null;
                    return null;
                }
                return await RequestAndRetry(func);
            }
        }
    }
}
