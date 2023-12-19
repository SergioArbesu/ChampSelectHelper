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
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Reflection;
using NETWORKLIST;

namespace ChampSelectHelperApp
{
    /// <summary>
    /// Lógica de interacción para SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly NetworkListManager networkListManager;

        private ChampInfo[] champions;
        private Dictionary<int, ChampionSettings> champSettDict = new();

        private BitmapImage noChampImg;

        public SettingsWindow()
        {
            InitializeComponent();

            networkListManager = new NetworkListManager();

            Title = Program.APP_NAME + " v" + Program.APP_VERSION;

            noChampImg = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/Resources/noChamp.png"));

            skinCheckBox.IsEnabled = false;
            skinComboBox.IsEnabled = false;
            skinRndmCheckBox.IsEnabled = false;
            skinRndmDialogButton.IsEnabled = false;
            skinImage.IsEnabled = false;

            chromaCheckBox.IsEnabled = false;
            chromaComboBox.IsEnabled = false;
            chromaRndmCheckBox.IsEnabled = false;
            chromaRndmDialogButton.Visibility = Visibility.Hidden;
        }

        public void InitializeWindow()
        {
            CheckConnectivity();
            try
            {
                using (WebClient httpClient = new())
                {
                    string response = httpClient.DownloadString(Program.CHAMPIONS_JSON_URL);
                    JObject parsedResponse = JObject.Parse(response);
                    CreateChampInfo(parsedResponse);
                    InitializeElements();
                    return;
                }
            }
            catch (WebException ex) 
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void InitializeElements()
        {
            //always use after having created champInfo
            foreach (ChampInfo info in champions!)
            {
                championComboBox.Items.Add(info.Name);
            }

            championImage.Source = noChampImg;

            championComboBox.Visibility = Visibility.Visible;
            championImage.Visibility = Visibility.Visible;
        }

        private void CreateChampInfo(JObject champJObject)
        {
            champions = new ChampInfo[champJObject.Count];
            int i = 0;
            foreach (var champion in champJObject)
            {
                JObject value = (JObject)champion.Value;
                champions[i] = new ChampInfo(value);
                i++;
            }
        }

        private void CheckConnectivity()
        {
            if (networkListManager.IsConnectedToInternet)
            {
                return;
            }
            var result = MessageBox.Show("There was a problem while trying to connect to the internet. Check your internet connection.\n\nDo you want to retry?", "Connecction Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                CheckConnectivity();
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
                skinCheckBox.IsChecked = false;
                skinCheckBox.IsEnabled = false;
            }
            else
            {
                championImage.Source = champions[championComboBox.SelectedIndex].Icon;
                foreach (SkinInfo skin in champions[championComboBox.SelectedIndex].Skins)
                {
                    skinComboBox.Items.Add(skin.Name);
                }
                skinCheckBox.IsChecked = false;
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
                skinImage.Source = new BitmapImage(new Uri(champions[championComboBox.SelectedIndex].Skins[skinComboBox.SelectedIndex].IconUri));

                ChromaInfo[]? chromas = champions[championComboBox.SelectedIndex].Skins[skinComboBox.SelectedIndex].Chromas;
                if (chromas is null)
                {
                    chromaComboBox.IsEnabled = false;
                    chromaRndmCheckBox.IsEnabled = false;
                }
                else
                {
                    foreach (ChromaInfo chroma in chromas)
                    {
                        chromaComboBox.Items.Add(chroma.Name);
                    }
                    chromaCheckBox.IsEnabled = true;
                }
            }
        }

        private void skinCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (skinCheckBox.IsChecked == true)
            {
                skinComboBox.IsEnabled = true;
                skinRndmCheckBox.IsEnabled = true;
                skinImage.IsEnabled = true;
            }
            else
            {
                skinComboBox.SelectedIndex = -1;
                skinRndmCheckBox.IsChecked = false;
                chromaCheckBox.IsChecked = false;

                skinComboBox.IsEnabled = false;
                skinRndmCheckBox.IsEnabled = false;
                skinImage.IsEnabled = false;
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
                chromaCheckBox.IsEnabled = false;
                skinRndmDialogButton.IsEnabled = true;
            }
            else
            {
                skinComboBox.IsEnabled = true;
                skinRndmDialogButton.IsEnabled = false;
            }
        }

        private void chromaCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (chromaCheckBox.IsChecked != true)
            {
                chromaComboBox.SelectedIndex = -1;
                chromaRndmCheckBox.IsChecked = false;
                chromaComboBox.IsEnabled = false;
                chromaRndmCheckBox.IsEnabled = false;
            }
            else
            {
                chromaComboBox.IsEnabled = true;
                chromaRndmCheckBox.IsEnabled = true;
            }
        }

        private void chromaRndmCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (chromaRndmCheckBox.IsChecked == true)
            {
                chromaComboBox.SelectedIndex = -1;
                chromaComboBox.IsEnabled = false;
                chromaRndmDialogButton.Visibility = Visibility.Visible;
            }
            else
            {
                chromaComboBox.IsEnabled = true;
                chromaRndmDialogButton.Visibility = Visibility.Hidden;
            }
        }

        private void skinRndmDialogButton_Click(object sender, RoutedEventArgs e)
        {
            List<CheckBoxListItem> checkBoxList = new();
            foreach (SkinInfo skin in champions[championComboBox.SelectedIndex].Skins)
            {
                checkBoxList.Add(new CheckBoxListItem(false, skin.Name, skin.Id));
            }
            new CheckBoxListWindow(this, checkBoxList, "Random Skins Pool").ShowDialog();
            //save it
        }

        private void chromaRndmDialogButton_Click(object sender, RoutedEventArgs e)
        {
            List<CheckBoxListItem> checkBoxList = new();
            foreach (ChromaInfo chroma in champions[championComboBox.SelectedIndex].Skins[skinComboBox.SelectedIndex].Chromas!)
            {
                checkBoxList.Add(new CheckBoxListItem(false, chroma.Name, chroma.Id));
            }
            new CheckBoxListWindow(this, checkBoxList, "Random Chromas Pool").ShowDialog();
            //TODO: add option to choose the regular chroma here and in dropdown
        }
    }
}
