using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChampSelectHelperApp
{
    class PerkTreeInfo
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

    class PerkInfo
    {
        public int Id { get; private set; }
        public BitmapImage Icon { get; private set; }

        public PerkInfo(JObject perk)
        {
            Id = (int)perk["id"];
            Icon = new BitmapImage(new Uri(Program.PERKS_ICON_URL_START + (string)perk["icon"]));
        }
    }
}
