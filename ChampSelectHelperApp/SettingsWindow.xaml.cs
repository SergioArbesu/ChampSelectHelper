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
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.ComponentModel;
using System.Net.Http;
using System.Windows.Controls.Primitives;

namespace ChampSelectHelperApp
{
    /// <summary>
    /// Lógica de interacción para SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private NetworkListManager networkListManager = new();

        private ChampInfo[] champions;
        private PerkTreeInfo[] perkTrees;
        private List<SpellInfo> spells;

        private Dictionary<int, ChampionSettings> settingsDict = new();

        private BitmapImage noChampImg = new BitmapImage(new Uri(@"pack://application:,,,/Resources/noChamp.png"));

        private int primaryStyleIndex = -1; // substitute this variables with checking the to be saved variables
        private int subStyleIndex = -1;

        // if what is being saved are chromas, saves the skin of origin
        private List<int> saveSkinsOrChromas = new List<int>();
        private int saveOriginSkin = -1; // -1 means that skins are being saved

        private int[] saveStyles = new int[2] {-1,-1};
        private int[] savePerks = new int[9] {-1,-1,-1,-1,-1,-1,-1,-1,-1};
        private int[] saveSpells = new int[2] {-1,-1};

        public SettingsWindow()
        {
            InitializeComponent();

            //Closing += CloseWindow; to be decided if this is useful

            Title = Program.APP_NAME + " v" + Program.APP_VERSION;
        }

        public async void InitializeWindow()
        {
            CheckConnectivity();

            string response = await App.httpclient.GetStringAsync(Program.PERKS_JSON_URL);
            JArray parsedPerks = JArray.Parse(response);
            CreatePerkTreeInfo(parsedPerks);
            response = await App.httpclient.GetStringAsync(Program.CHAMPIONS_JSON_URL);
            JObject parsedChampions = JObject.Parse(response);
            CreateChampInfo(parsedChampions);
            response = await App.httpclient.GetStringAsync(Program.SPELLS_JSON_URL);
            JArray parsedSpells = JArray.Parse(response);
            CreateSpellInfo(parsedSpells);

            InitializeElements();
        }

        private void CheckConnectivity()
        {
            if (networkListManager.IsConnectedToInternet) return;

            var result = MessageBox.Show("There was a problem while trying to connect to the internet. Check your internet " +
                "connection.\n\nDo you want to retry?", "Connecction Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes)
            {
                CheckConnectivity();
            }
            else
            {
                Close();
            }
        }

        private void CreateChampInfo(JObject champJObject)
        {
            champions = new ChampInfo[champJObject.Count];
            int i = 0;
            foreach (var champion in champJObject)
            {
                JObject value = (JObject)champion.Value;
                ChampInfo champInfo = new ChampInfo(value);
                champions[i] = champInfo;
                i++;
            }
        }

        private void CreatePerkTreeInfo(JArray perkTreeJArray)
        {
            perkTrees = new PerkTreeInfo[5];
            Task[] tasks = new Task[5];
            int i = 0;
            foreach (JObject perkTree in perkTreeJArray)
            {
                PerkTreeInfo perkTreeInfo = new PerkTreeInfo();
                perkTrees[i] = perkTreeInfo;
                tasks[i] = Task.Run(() => perkTreeInfo.CreatePerkTree(perkTree));
                i++;
            }
            Task.WaitAll(tasks);
        }

        private void CreateSpellInfo(JArray spellJArray)
        {
            spells = new List<SpellInfo>();
            foreach (JObject spell in spellJArray)
            {
                JArray gameModes = (JArray)spell["gameModes"];
                foreach (string gameMode in gameModes)
                {
                    if (gameMode == "CLASSIC")
                    {
                        spells.Add(new SpellInfo(spell));
                        break;
                    }
                }
            }
        }

        private void InitializeElements()
        {
            //always use after having created champInfo
            foreach (ChampInfo champion in champions)
            {
                championComboBox.Items.Add(champion.Name);
            }

            foreach (SpellInfo spell in spells)
            {
                spell1ComboBox.Items.Add(spell.Name);
                spell2ComboBox.Items.Add(spell.Name);
            }

            championImage.Source = noChampImg;


            //Display elements when the loading has ended
            championComboBox.Visibility = Visibility.Visible;
            championImage.Visibility = Visibility.Visible;
        }

        private void CloseWindow(object? sender, CancelEventArgs ev)
        {
            networkListManager = null;
            champions = null;
            perkTrees = null;
        }

        private void SaveChampion()
        {
            ChampionSettings settings = new ChampionSettings(
                champions[championComboBox.SelectedIndex].Id,
                saveStyles,
                savePerks,
                saveSpells,
                saveSkinsOrChromas,
                saveOriginSkin
                );
            settingsDict[settings.id] = settings;
            // save in file
        }

        private void SaveSkinsOrChromas()
        {

        }

        private void SavePerks()
        {
            int first = savePerks[0];
            if (first == -1)
            {
                for (int i = 1; i < savePerks.Length; i++)
                {
                    if (savePerks[i] != -1) return;
                }
            }
            else
            {
                for (int i = 1; i <= savePerks.Length; i++)
                {
                    if (savePerks[i] == -1) return;
                }
            }

            SaveChampion();
        }

        private void SaveSpells()
        {
            //reconsider if saving should be done this way
        }

        private void championComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            skinComboBox.Items.Clear();

            if (championComboBox.SelectedIndex == -1)
            {
                championImage.Source = noChampImg;
                skinCheckBox.IsChecked = false;
                perksCheckBox.IsChecked = false;
                spellsCheckBox.IsChecked = false;
                skinCheckBox.IsEnabled = false;
                perksCheckBox.IsEnabled = false;
                spellsCheckBox.IsEnabled = false;
            }
            else
            {
                championImage.Source = champions[championComboBox.SelectedIndex].Icon;
                foreach (SkinInfo skin in champions[championComboBox.SelectedIndex].Skins)
                {
                    skinComboBox.Items.Add(skin.Name);
                }
                skinCheckBox.IsEnabled = true;
                perksCheckBox.IsEnabled = true;
                spellsCheckBox.IsEnabled = true;
            }
        }

        private void skinCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (skinCheckBox.IsChecked == true)
            {
                skinComboBox.IsEnabled = true;
                skinRndmCheckBox.IsEnabled = true;
                skinImage.IsEnabled = true; //TODO: know if this is necessary (if not, clean the code here and everywhere else)
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

        private void skinComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            chromaComboBox.Items.Clear();
            chromaCheckBox.IsChecked = false;

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

                saveOriginSkin = -1;
                saveSkinsOrChromas.Clear();
                saveSkinsOrChromas.Add(champions[championComboBox.SelectedIndex].Skins[skinComboBox.SelectedIndex].Id);
                //call saving method
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

        private void chromaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chromaComboBox.SelectedIndex != -1)
            {
                saveOriginSkin = champions[championComboBox.SelectedIndex].Skins[skinComboBox.SelectedIndex].Id;
                saveSkinsOrChromas.Clear();
                saveSkinsOrChromas.Add(champions[championComboBox.SelectedIndex].Skins[skinComboBox.SelectedIndex].Chromas[chromaComboBox.SelectedIndex].Id);
                //call saving method
            }
        }

        private void chromaRndmCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (chromaRndmCheckBox.IsChecked == true)
            {
                chromaComboBox.SelectedIndex = -1;
                chromaComboBox.IsEnabled = false;
                chromaRndmDialogButton.IsEnabled = true;
            }
            else
            {
                chromaComboBox.IsEnabled = true;
                chromaRndmDialogButton.IsEnabled = false;
            }
        }

        private void skinRndmDialogButton_Click(object sender, RoutedEventArgs e)
        {
            List<CheckBoxListItem> checkBoxList = new();
            foreach (SkinInfo skin in champions[championComboBox.SelectedIndex].Skins)
            {
                checkBoxList.Add(new CheckBoxListItem(saveSkinsOrChromas.Contains(skin.Id), skin.Name, skin.Id));
            }

            new CheckBoxListWindow(this, checkBoxList, "Random Skins Pool").ShowDialog();

            //save it
            saveOriginSkin = -1;
            UpdateSaveSkinsOrChromas(checkBoxList);
        }

        private void chromaRndmDialogButton_Click(object sender, RoutedEventArgs e)
        {
            List<CheckBoxListItem> checkBoxList = new();
            SkinInfo skin = champions[championComboBox.SelectedIndex].Skins[skinComboBox.SelectedIndex];
            checkBoxList.Add(new CheckBoxListItem(saveSkinsOrChromas.Contains(skin.Id), "No Chroma", skin.Id));
            foreach (ChromaInfo chroma in skin.Chromas)
            {
                checkBoxList.Add(new CheckBoxListItem(saveSkinsOrChromas.Contains(chroma.Id), chroma.Name, chroma.Id));
            }

            new CheckBoxListWindow(this, checkBoxList, "Random Chromas Pool").ShowDialog();

            //save it
            saveOriginSkin = skin.Id;
            UpdateSaveSkinsOrChromas(checkBoxList);
        }

        private void UpdateSaveSkinsOrChromas(List<CheckBoxListItem> checkBoxList)
        {
            List<int> ids = new List<int>();
            foreach (CheckBoxListItem checkBoxItem in checkBoxList)
            {
                if (checkBoxItem.IsChecked)
                {
                    ids.Add(checkBoxItem.Id);
                }
            }
            if (ids.Count == 0)
            {
                MessageBox.Show("No elements were selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                saveSkinsOrChromas = ids;
            }
            //call save method
        }

        private void perksCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (perksCheckBox.IsChecked == true)
            {
                perksGrid.Visibility = Visibility.Visible;
            }
            else
            {
                ChangePrimaryStyle(-1);
                perksGrid.Visibility = Visibility.Hidden;

                ChangeMinorPerkGrid(offensivePerkGrid, null);
                ChangeMinorPerkGrid(flexPerkGrid, null);
                ChangeMinorPerkGrid(defensivePerkGrid, null);

                savePerks[6] = -1;
                savePerks[7] = -1;
                savePerks[8] = -1;
            }
        }

        private void primaryStyle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangePrimaryStyle(int.Parse((string)((Image)sender).Tag));
        }

        private void subStyle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeSubStyle(int.Parse((string)((Image)sender).Tag));
        }

        private void ChangePrimaryStyle(int index)
        {
            if (index == -1)
            {
                if (saveStyles[0] == -1) return;
                saveStyles[0] = -1;

                ChangeStyleGrid(primaryStyleGrid, index);

                keyStonesItemsControl.ItemsSource = null;
                primarySlot1ItemsControl.ItemsSource = null;
                primarySlot2ItemsControl.ItemsSource = null;
                primarySlot3ItemsControl.ItemsSource = null;

                subStyleGrid.Visibility = Visibility.Hidden;
                ChangeSubStyle(-1);

                savePerks[0] = -1;
                savePerks[1] = -1;
                savePerks[2] = -1;
                savePerks[3] = -1;
                return;
            }

            if (saveStyles[0] == perkTrees[index].Id) return;
            saveStyles[0] = perkTrees[index].Id;

            ChangeStyleGrid(primaryStyleGrid, index);

            foreach (Border border in subStyleGrid.Children)
            {
                int tag = int.Parse((string)((Image)border.Child).Tag);
                if (tag == index)
                {
                    border.Visibility = Visibility.Collapsed;
                }
                else 
                {
                    border.Visibility = Visibility.Visible;
                    if (tag == subStyleIndex) 
                    {
                        border.BorderBrush = Brushes.White;
                    }
                    else
                    {
                        border.BorderBrush = Brushes.Transparent;
                    }
                }
            }

            if (index == subStyleIndex) ChangeSubStyle(-1);

            subStyleGrid.Visibility = Visibility.Visible;
            
            keyStonesItemsControl.ItemsSource = perkTrees[index].Slots[0];
            primarySlot1ItemsControl.ItemsSource = perkTrees[index].Slots[1];
            primarySlot2ItemsControl.ItemsSource = perkTrees[index].Slots[2];
            primarySlot3ItemsControl.ItemsSource = perkTrees[index].Slots[3];

            savePerks[0] = -1;
            savePerks[1] = -1;
            savePerks[2] = -1;
            savePerks[3] = -1;
        }

        private void ChangeSubStyle(int index)
        {
            if (index == -1)
            {
                if (saveStyles[1] == -1) return;
                saveStyles[1] = -1;

                ChangeStyleGrid(subStyleGrid, index);

                subSlot1ItemsControl.ItemsSource = null;
                subSlot2ItemsControl.ItemsSource = null;
                subSlot3ItemsControl.ItemsSource = null;

                savePerks[4] = -1;
                savePerks[5] = -1;
                return;
            }

            if (saveStyles[1] == perkTrees[index].Id) return;
            saveStyles[1] = perkTrees[index].Id;

            ChangeStyleGrid(subStyleGrid, index);

            subSlot1ItemsControl.ItemsSource = perkTrees[index].Slots[1];
            subSlot2ItemsControl.ItemsSource = perkTrees[index].Slots[2];
            subSlot3ItemsControl.ItemsSource = perkTrees[index].Slots[3];

            savePerks[4] = -1;
            savePerks[5] = -1;
        }

        private void ChangeStyleGrid(UniformGrid grid, int index)
        {
            foreach (Border border in grid.Children)
            {
                int tag = int.Parse((string)((Image)border.Child).Tag);
                if (tag == index)
                {
                    border.BorderBrush = Brushes.White;
                }
                else
                {
                    border.BorderBrush = Brushes.Transparent;
                }
            }
        }

        private void keyStonesImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeSlotPerkItemsControl(keyStonesItemsControl, sender);

            savePerks[0] = (int)((Image)sender).Tag;
        }

        private void primarySlot1Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeSlotPerkItemsControl(primarySlot1ItemsControl, sender);

            savePerks[1] = (int)((Image)sender).Tag;
        }

        private void primarySlot2Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeSlotPerkItemsControl(primarySlot2ItemsControl, sender);

            savePerks[2] = (int)((Image)sender).Tag;
        }

        private void primarySlot3Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeSlotPerkItemsControl(primarySlot3ItemsControl, sender);

            savePerks[3] = (int)((Image)sender).Tag;
        }

        //TODO: add logic so there can only be two subtree perks at the same time

        private void subSlot1Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeSlotPerkItemsControl(subSlot1ItemsControl, sender);
        }

        private void subSlot2Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeSlotPerkItemsControl(subSlot2ItemsControl, sender);
        }

        private void subSlot3Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeSlotPerkItemsControl(subSlot3ItemsControl, sender);
        }

        private void ChangeSlotPerkItemsControl(ItemsControl itemsControl, object sender)
        {
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                Border border = (Border)VisualTreeHelper.GetChild(itemsControl.ItemContainerGenerator.ContainerFromIndex(i), 0);
                Image image = (Image)border.Child;
                if (image == sender)
                {
                    image.Source = ((PerkInfo)image.DataContext).Icon;
                    border.BorderBrush = Brushes.White;
                }
                else
                {
                    image.Source = ((PerkInfo)image.DataContext).GrayIcon;
                    border.BorderBrush = Brushes.Transparent;
                }
            }
        }

        private void offensivePerkImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeMinorPerkGrid(offensivePerkGrid, sender);

            savePerks[6] = int.Parse((string)((Image)sender).Tag);
        }

        private void flexPerkImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeMinorPerkGrid(flexPerkGrid, sender);

            savePerks[7] = int.Parse((string)((Image)sender).Tag);
        }

        private void defensivePerkImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeMinorPerkGrid(defensivePerkGrid, sender);

            savePerks[8] = int.Parse((string)((Image)sender).Tag);
        }

        private void ChangeMinorPerkGrid(UniformGrid grid, object? sender)
        {
            foreach (Border border in grid.Children)
            {
                Image image = (Image)border.Child;
                if (image == sender)
                {
                    border.BorderBrush = Brushes.White;
                }
                else
                {
                    border.BorderBrush = Brushes.Transparent;
                }
            }
        }

        private void spellsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (spellsCheckBox.IsChecked == true)
            {
                spell1ComboBox.IsEnabled = true;
                spell2ComboBox.IsEnabled = true;
            }
            else
            {
                spell1ComboBox.SelectedIndex = -1;
                spell2ComboBox.SelectedIndex = -1;
                spell1ComboBox.IsEnabled = false;
                spell2ComboBox.IsEnabled = false;
            }
        }

        private void spell1ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (spell1ComboBox.SelectedIndex == -1)
            {
                spell1Image.Source = null;
            }
            else
            {
                spell1Image.Source = spells[spell1ComboBox.SelectedIndex].Icon;
                if (spell1ComboBox.SelectedIndex == spell2ComboBox.SelectedIndex) spell2ComboBox.SelectedIndex = -1;
            }
        }

        private void spell2ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (spell2ComboBox.SelectedIndex == -1)
            {
                spell2Image.Source = null;
            }
            else
            {
                spell2Image.Source = spells[spell2ComboBox.SelectedIndex].Icon;
                if (spell1ComboBox.SelectedIndex == spell2ComboBox.SelectedIndex) spell1ComboBox.SelectedIndex = -1;
            }
        }
    }
}
