using Newtonsoft.Json.Linq;
using System.Windows.Media.Imaging;

namespace ChampSelectHelper
{
    public class SpellInfo : Info
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BitmapImage Icon { get; set; }

        public SpellInfo(JObject spell)
        {
            Id = (int)spell["id"];
            Name = (string)spell["name"];

            string path = "https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default" 
                + ((string)spell["iconPath"]).ToLowerInvariant().Substring(21);
            Icon = HelperFunctions.CreateBitmapImage(path);
        }
    }
}
