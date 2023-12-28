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
        public int[] perksId;
        public int[] spellsId;
        public List<int> skinsOrChromasId;
        public int originSkinId;

        public ChampionSettings(int id, int[] stylesId, int[] perksId, int[] spellsId, List<int> skinsOrChromasId, int originSkinId)
        {
            this.id = id;
            this.stylesId = stylesId;
            this.perksId = perksId;
            this.spellsId = spellsId;
            this.skinsOrChromasId = skinsOrChromasId;
            this.originSkinId = originSkinId;
        }
    }
}
