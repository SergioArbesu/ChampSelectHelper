using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChampSelectHelperApp
{
    class ChampInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BitmapImage Icon { get; set; }
        public SkinInfo[] Skins { get; set; }

        public ChampInfo(int id, string name, string iconUri, SkinInfo[] skins)
        {
            Id = id;
            Name = name;
            Icon = new BitmapImage(new Uri(iconUri));
            Skins = skins;
        }

        public ChampInfo(JObject champion)
        {
            Id = (int)champion["id"];
            Name = (string)champion["name"];
            Icon = new BitmapImage(new Uri((string)champion["icon"]));

            JArray skins = (JArray)champion["skins"];
            Skins = new SkinInfo[skins.Count];
            int i = 0;
            foreach (JObject skin in skins)
            {
                Skins[i] = new SkinInfo(skin);
                i++;
            }
        }
    }

    class SkinInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IconUri { get; set; }
        public ChromaInfo[]? Chromas { get; set; }

        public SkinInfo(int id, string name, string iconUri, ChromaInfo[]? chromas)
        {
            Id = id;
            Name = name;
            IconUri = iconUri;
            Chromas = chromas;
        }

        public SkinInfo(JObject skin)
        {
            Id = (int)skin["id"];
            Name = (string)skin["name"];
            IconUri = (string)skin["uncenteredSplashPath"];

            JArray chromas = (JArray)skin["chromas"];
            if (chromas.HasValues && chromas[0].Type == JTokenType.Object)
            {
                Chromas = new ChromaInfo[chromas.Count];
                int i = 0;
                foreach (JObject chroma in chromas)
                {
                    Chromas[i] = new ChromaInfo(chroma);
                    i++;
                }
            }
            else
            {
                Chromas = null;
            }
        }
    }

    class ChromaInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ChromaInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public ChromaInfo(JObject chroma)
        {
            Id = (int)chroma["id"];
            Name = (string)chroma["name"];
        }
    }
}
