using System.Collections.Generic;

namespace ChampSelectHelperApp
{
    public class ChampionSettings
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
