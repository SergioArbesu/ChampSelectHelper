using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Net.NetworkInformation;
using System.Reflection;

namespace ChampSelectHelperApp
{
    /// <summary>
    /// Lógica de interacción para SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private ChampInfo[] champInfo;
        private Dictionary<int, ChampionSettings> champSettDict = new();

        private BitmapImage noChampImg;

        public SettingsWindow()
        {
            Title = Program.APP_NAME + " v" + Program.APP_VERSION;

            noChampImg = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/Resources/noChamp.png"));
            
            InitializeComponent();
        }

        public void InitializeWindow()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    using (HttpClient httpClient = new())
                    {
                        string response = httpClient.GetStringAsync(Program.CHAMPIONS_JSON_URL).Result;
                        JObject parsedResponse = JObject.Parse(response);
                        CreateChampInfoArray(parsedResponse);
                        InitializeElements();
                        return;
                    }
                }
                catch (HttpRequestException e) 
                {
                    MessageBox.Show(e.Message, e.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            var result = MessageBox.Show("There was a problem while trying to connect. Check your internet connection.\n\nDo you want to retry?", "Connecction Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                InitializeWindow();
            }
            else
            {
                Close();
            }
        }

        private void InitializeElements()
        {
            //always use after having created champInfo
            foreach (ChampInfo info in champInfo!)
            {
                championComboBox.Items.Add(info.Name);
            }

            championImage.Source = noChampImg;

            championComboBox.Visibility = Visibility.Visible;
            championImage.Visibility = Visibility.Visible;
        }

        private void CreateChampInfoArray(JObject champJObject)
        {
            champInfo = new ChampInfo[champJObject.Count];
            int i = 0;
            foreach (var champion in champJObject)
            {
                var value = champion.Value;
                string name = (string)value["name"];

                JArray skins = (JArray)value["skins"];
                SkinInfo[] skinInfo = new SkinInfo[skins.Count];
                int j = 0;
                foreach (JObject skin in skins)
                {
                    JArray chromas = (JArray)skin["chromas"];
                    ChromaInfo[]? chromaInfo = null;
                    if (chromas.HasValues)
                    {
                        if (chromas[0].Type == JTokenType.Object)
                        {
                            chromaInfo = new ChromaInfo[chromas.Count];
                            int k = 0;
                            foreach (JObject chroma in chromas)
                            {
                                chromaInfo[k] = new ChromaInfo((int)chroma["id"], (string)chroma["name"]);
                                k++;
                            }
                        }
                    }
                    skinInfo[j] = new SkinInfo((int)skin["id"], (string)skin["name"], (string)skin["uncenteredSplashPath"], chromaInfo);
                    j++;
                }
                champInfo[i] = new ChampInfo((int)value["id"], name, (string)value["icon"], skinInfo);
                i++;
            }
        }

        private void championComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (championComboBox.SelectedIndex == -1)
            {
                championImage.Source = noChampImg;
            }
            else
            {
                championImage.Source = champInfo[championComboBox.SelectedIndex].Icon;
            }
        }
    }
}
