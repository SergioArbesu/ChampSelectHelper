using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampSelectHelperApp
{
    class ChampionSettings
    {
        public int id;
        public int[] stylesId;
        public int[] selectedPerkId;
        public int[] spellsId;
        public List<int> skinsOrChromasId;
        public int originSkinId;

        public ChampionSettings(int id, int[] stylesId, int[] selectedPerkId, int[] spellsId, List<int> skinsChromasId, int originSkinId)
        {
            this.id = id;
            this.stylesId = stylesId;
            this.selectedPerkId = selectedPerkId;
            this.spellsId = spellsId;
            this.skinsOrChromasId = skinsChromasId;
            this.originSkinId = originSkinId;
        }
    }
}
