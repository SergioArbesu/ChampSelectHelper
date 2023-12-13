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

        private UIElement[] skinUIElements;
        private UIElement[] chromaUIElements;

        public SettingsWindow()
        {
            InitializeComponent();

            Title = Program.APP_NAME + " v" + Program.APP_VERSION;

            noChampImg = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/Resources/noChamp.png"));

            skinUIElements = new UIElement[]
            {
                skinComboBox, skinImage, skinRndmCheckBox
            };
            foreach (UIElement ui in skinUIElements)
            {
                ui.IsEnabled = false;
            }
            chromaUIElements = new UIElement[]
            {
                chromaComboBox, chromaRndmCheckBox
            };
            foreach (UIElement ui in chromaUIElements)
            {
                ui.IsEnabled = false;
            }
            foreach (UIElement ui in chromaUIElements)
            {
                ui.IsEnabled = false;
            }
            chromaCheckBox.IsEnabled = false;
            skinCheckBox.IsEnabled = false;
        }

        public void InitializeWindow()
        {
            CheckConnectivity();
            try
            {
                using (HttpClient httpClient = new())
                {
                    string response = httpClient.GetStringAsync(Program.CHAMPIONS_JSON_URL).Result;
                    JObject parsedResponse = JObject.Parse(response);
                    ChampInfoArrayParser(parsedResponse);
                    InitializeElements();
                    return;
                }
            }
            catch (HttpRequestException e) 
            {
                MessageBox.Show(e.Message, e.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void ChampInfoArrayParser(JObject champJObject)
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

        private void CheckConnectivity()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                return;
            }
            var result = MessageBox.Show("There was a problem while trying to connect to the internet. Check your internet connection.\n\nDo you want to retry?", "Connecction Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                InitializeWindow();
            }
            else
            {
                Close();
            }
        }

        private void championComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            skinComboBox.Items.Clear();

            if (championComboBox.SelectedIndex == -1)
            {
                championImage.Source = noChampImg;
                skinCheckBox.IsEnabled = false;
                skinCheckBox.IsChecked = false;
            }
            else
            {
                championImage.Source = champInfo[championComboBox.SelectedIndex].Icon;
                foreach (SkinInfo skin in champInfo[championComboBox.SelectedIndex].Skins)
                {
                    skinComboBox.Items.Add(skin.Name);
                }
                skinCheckBox.IsEnabled = true;
            }
        }

        private void skinComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            chromaComboBox.Items.Clear();

            if (skinComboBox.SelectedIndex == -1)
            {
                skinImage.Source = null;
            }
            else
            {
                CheckConnectivity();
                BitmapImage bitimg = new BitmapImage(new Uri(champInfo[championComboBox.SelectedIndex].Skins[skinComboBox.SelectedIndex].IconUri));
                bitimg.DownloadCompleted += (sender, ev) =>
                {
                    skinImage.Source = bitimg;

                    ChromaInfo[]? chromas = champInfo[championComboBox.SelectedIndex].Skins[skinComboBox.SelectedIndex].Chromas;
                    if (chromas is null)
                    {
                        foreach (UIElement ui in chromaUIElements)
                        {
                            ui.IsEnabled = false;
                        }
                    }
                    else
                    {
                        foreach (ChromaInfo chroma in chromas)
                        {
                            chromaComboBox.Items.Add(chroma.Name);
                        }
                        chromaCheckBox.IsEnabled = true;
                    }
                };
            }
        }

        private void skinCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (skinCheckBox.IsChecked == true)
            {
                foreach (UIElement ui in skinUIElements)
                {
                    ui.IsEnabled = true;
                }
            }
            else
            {
                skinComboBox.SelectedIndex = -1;
                skinRndmCheckBox.IsChecked = false;
                chromaCheckBox.IsChecked = false;
                foreach (UIElement ui in skinUIElements)
                {
                    ui.IsEnabled = false;
                }
                chromaCheckBox.IsEnabled = false;
            }
        }

        private void skinRndmCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (skinRndmCheckBox.IsChecked == true)
            {
                skinComboBox.SelectedIndex = -1;
                chromaCheckBox.IsChecked = false;
                skinComboBox.IsEnabled = false;
                chromaCheckBox.IsEnabled = true;
            }
            else
            {
                skinComboBox.IsEnabled = true;
                chromaCheckBox.IsChecked = false;
                chromaCheckBox.IsEnabled = false;
            }
        }

        private void chromaCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (chromaCheckBox.IsChecked == true)
            {
                if (skinRndmCheckBox.IsChecked == true)
                {
                    chromaComboBox.SelectedIndex = -1;
                    chromaRndmCheckBox.IsChecked = true;
                    chromaRndmCheckBox.IsEnabled = false;
                }
                else
                {
                    chromaComboBox.IsEnabled = true;
                    chromaRndmCheckBox.IsEnabled = true;
                }
            }
            else
            {
                chromaComboBox.SelectedIndex = -1;
                chromaRndmCheckBox.IsChecked = false;
                chromaComboBox.IsEnabled = false;
                chromaRndmCheckBox.IsEnabled = false;
            }
        }

        private void chromaRndmCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (chromaRndmCheckBox.IsChecked == true)
            {
                chromaComboBox.SelectedIndex = -1;
                chromaComboBox.IsEnabled = false;
            }
            else
            {
                chromaComboBox.IsEnabled = true;
            }
        }
    }
}
