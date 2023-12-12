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
    }
}
