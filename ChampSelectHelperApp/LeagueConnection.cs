using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampSelectHelperApp
{
    internal class LeagueConnection
    {
        private LCUClient lcuclient;
        private int lastId = -1;

        public LeagueConnection()
        {
            lcuclient = new LCUClient();

            lcuclient.SubscribeEvent("OnJsonApiEvent_lol-champ-select_v1_current-champion", OnChampionPicked);
            lcuclient.SubscribeEvent("OnJsonApiEvent_lol-gameflow_v1_gameflow-phase", OnGameflowPhase);

            lcuclient.Connect();
        }

        private async void OnChampionPicked(LCUEventArgs args)
        {
            if (args.Data is null) return;
            if (!int.TryParse(args.Data, out int champId)) return;
            if (lastId == champId) return;

            lastId = champId;
            Dictionary<int, ChampionSettings> settingsDict = FileHandler.GetChampionSettings();

            if (!settingsDict.TryGetValue(champId, out var settings)) return;

            List<Task> tasks = new();

            if (settings.skinsOrChromasId.Count > 0)
            {

            }

            if (settings.spellsId[0] != -1)
            {
                var spellsData = new
                {
                    spell1Id = settings.spellsId[0],
                    spell2Id = settings.spellsId[1]
                };
                string body = JsonConvert.SerializeObject(spellsData);
                var response = await lcuclient.SendRequest("PATCH", "/lol-champ-select/v1/session/my-selection", body);
            }

            if (settings.perksId[0] != -1)
            {

            }
        }

        private void OnGameflowPhase(LCUEventArgs args)
        {
            if (args.Data is null) return;

            if (args.Data == "ChampSelect") lastId = -1;
        }
    }
}
