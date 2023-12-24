using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace ChampSelectHelperApp
{
    public class PerkTreeInfo
    {
        public int Id { get; private set; }
        public PerkInfo[][] Slots { get; private set; }

        public PerkTreeInfo(JObject perkTree)
        {
            Id = (int)perkTree["id"];

            JArray slots = (JArray)perkTree["slots"];
            Slots = new PerkInfo[slots.Count][];
            int i = 0;
            foreach (JObject slot in slots)
            {
                JArray perks = (JArray)slot["runes"];
                Slots[i] = new PerkInfo[perks.Count];
                int j = 0;
                foreach (JObject perk in perks)
                {
                    Slots[i][j] = new PerkInfo(perk);
                    j++;
                }
                i++;
            }
        }
    }

    public class PerkInfo
    {
        public int Id { get; private set; }
        public BitmapImage Icon { get; private set; }
        public BitmapImage GrayIcon { get; private set; }
        public bool IsSelected { get; set; }

        public PerkInfo(JObject perk)
        {
            IsSelected = false;

            Id = (int)perk["id"];
            Icon = new BitmapImage(new Uri(Program.PERKS_ICON_URL_START + (string)perk["icon"]));

            //var test = new FormatConvertedBitmap(Icon, Icon.Format, Icon.Palette, 100);
            //GrayIcon = new FormatConvertedBitmap(test, PixelFormats.Gray8, BitmapPalettes.Gray256Transparent, 100);

            using (HttpClient client = new())
            {
                using Stream stream = client.GetStreamAsync(Program.PERKS_ICON_URL_START + (string)perk["icon"]).Result;
                Bitmap bitmap = new Bitmap(stream);

                Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);

                for (int x = 0; x < bitmap.Height; x++)
                {
                    for (int i = 0; i < bitmap.Width; i++)
                    {
                        Color oc = bitmap.GetPixel(i, x);
                        int grayScale = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                        Color nc = Color.FromArgb(oc.A, grayScale, grayScale, grayScale);
                        newBitmap.SetPixel(i, x, nc);
                    }
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    newBitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Position = 0;

                    BitmapImage bitmapImg = new BitmapImage();
                    bitmapImg.BeginInit();
                    bitmapImg.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImg.StreamSource = memoryStream;
                    bitmapImg.EndInit();
                    bitmapImg.Freeze();
                    GrayIcon = bitmapImg;
                }
            }
        }
    }
}
