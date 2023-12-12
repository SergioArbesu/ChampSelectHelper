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
        public bool enabled;
        public int primaryStyleId;
        public int subStyleId;
        public int[] selectedPerkIds;
        public int spell1Id;
        public int spell2Id;
        public int skinId;
        public int? chromaId;

        public ChampionSettings(int id, bool enabled, int primaryStyleId, int subStyleId, int[] selectedPerkIds, int spell1Id, int spell2Id, int skinId)
        {
            if (selectedPerkIds.Length != 9)
            {
                throw new ArgumentException("selectedPerkIds array doesn't have length 9", nameof(selectedPerkIds));
            }

            this.id = id;
            this.enabled = enabled;
            this.primaryStyleId = primaryStyleId;
            this.subStyleId = subStyleId;
            this.selectedPerkIds = selectedPerkIds;
            this.spell1Id = spell1Id;
            this.spell2Id = spell2Id;
            this.skinId = skinId;
            this.chromaId = null;
        }

        public ChampionSettings(int id, bool enabled, int primaryStyleId, int subStyleId, int[] selectedPerkIds, int spell1Id, int spell2Id, int skinId, int? chromaId) :
            this(id, enabled, primaryStyleId, subStyleId, selectedPerkIds, spell1Id, spell2Id, skinId)
        {
            this.chromaId = chromaId;
        }
    }
}
