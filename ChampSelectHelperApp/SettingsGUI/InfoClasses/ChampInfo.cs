using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ChampSelectHelperApp
{
    class ChampInfo : Info
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IconUri { get; set; }
        public List<SkinInfo> Skins { get; set; }

        public ChampInfo(int id, string name, string iconUri, List<SkinInfo> skins)
        {
            Id = id;
            Name = name;
            IconUri = iconUri;
            Skins = skins;
        }

        public ChampInfo(JObject champion)
        {
            Id = (int)champion["id"];
            Name = (string)champion["name"];
            IconUri = (string)champion["icon"];

            JArray skins = (JArray)champion["skins"];
            Skins = new List<SkinInfo>(skins.Count);
            foreach (JObject skin in skins)
            {
                Skins.Add(new SkinInfo(skin));
            }
        }
    }

    class SkinInfo : Info
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IconUri { get; set; }
        public List<ChromaInfo>? Chromas { get; set; }

        public SkinInfo(int id, string name, string iconUri, List<ChromaInfo>? chromas)
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
                Chromas = new List<ChromaInfo>(chromas.Count);
                foreach (JObject chroma in chromas)
                {
                    Chromas.Add(new ChromaInfo(chroma));
                }
            }
            else Chromas = null;
        }
    }

    class ChromaInfo : Info
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
