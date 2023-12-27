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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace ChampSelectHelperApp
{
    public class PerkTreeInfo
    {
        public int Id { get; private set; }
        public PerkInfo[][] Slots { get; private set; }

        public PerkTreeInfo() { }

        public async Task CreatePerkTree(JObject perkTree)
        {
            Id = (int)perkTree["id"];

            JArray slots = (JArray)perkTree["slots"];
            Slots = new PerkInfo[4][];

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < Slots.Length; i++)
            {
                JArray perks = (JArray)slots[i]["runes"];
                Slots[i] = new PerkInfo[perks.Count];
                int j = 0;
                foreach(JObject perk in perks)
                {
                    PerkInfo perkInfo = new PerkInfo();
                    Slots[i][j] = perkInfo;
                    tasks.Add(Task.Run(() => perkInfo.CreatePerk(perk)));
                    j++;
                }
            }

            await Task.WhenAll(tasks);
        }
    }

    public class PerkInfo
    {
        public int Id { get; private set; }
        public BitmapImage Icon { get; private set; }
        public BitmapImage GrayIcon { get; private set; }
        public bool IsSelected { get; set; }

        //public PerkInfo(JObject perk)
        public PerkInfo()
        {
            IsSelected = false;
        }

        public async Task CreatePerk(JObject perk)
        {
            Id = (int)perk["id"];

            using (Stream tempStream = await App.httpclient.GetStreamAsync(Program.ICON_URL_START + (string)perk["icon"]))
            using (MemoryStream stream = new MemoryStream())
            {
                await tempStream.CopyToAsync(stream);

                Icon = new BitmapImage();
                Icon.BeginInit();
                Icon.CacheOption = BitmapCacheOption.OnLoad;
                Icon.StreamSource = stream;
                Icon.EndInit();
                Icon.Freeze();
                
                Bitmap bitmap = new Bitmap(stream);
                Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color oc = bitmap.GetPixel(x, y);
                        int grayScale = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                        Color nc = Color.FromArgb(oc.A, grayScale, grayScale, grayScale);
                        newBitmap.SetPixel(x, y, nc);
                    }
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    newBitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Position = 0;

                    GrayIcon = new BitmapImage();
                    GrayIcon.BeginInit();
                    GrayIcon.CacheOption = BitmapCacheOption.OnLoad;
                    GrayIcon.StreamSource = memoryStream;
                    GrayIcon.EndInit();
                    GrayIcon.Freeze();
                }
            }
        }
    }
}
