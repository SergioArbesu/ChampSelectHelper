using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChampSelectHelperApp
{
    public static class HelperFunctions
    {
        public static BitmapImage CreateBitmapImage(string uri)
        {
            var img = new BitmapImage();

            img.BeginInit();
            img.UriSource = new Uri(uri);
            img.UriCachePolicy = null;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();

            return img;
        }
    }
}
