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

        private Dictionary<int, ChampionSettings> champSettDict = new();

        private BitmapImage noChampImg = new BitmapImage(new Uri(@"pack://application:,,,/Resources/noChamp.png"));

        private int primaryStyleIndex = -1; // substitute this variables with checking the to be saved variables
        private int subStyleIndex = -1;

        private int[] savePerks = new int[8];

        public SettingsWindow()
        {
            InitializeComponent();

            //Closing += CloseWindow; to be decided if this is useful

            Title = Program.APP_NAME + " v" + Program.APP_VERSION;

            skinCheckBox.IsEnabled = false;
            skinComboBox.IsEnabled = false;
            skinRndmCheckBox.IsEnabled = false;
            skinRndmDialogButton.IsEnabled = false;
            skinImage.IsEnabled = false;

            chromaCheckBox.IsEnabled = false;
            chromaComboBox.IsEnabled = false;
            chromaRndmCheckBox.IsEnabled = false;
            chromaRndmDialogButton.IsEnabled = false;
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
            foreach (ChampInfo info in champions!)
            {
                championComboBox.Items.Add(info.Name);
            }

            championImage.Source = noChampImg;

            championComboBox.Visibility = Visibility.Visible;
            championImage.Visibility = Visibility.Visible;
        }

        private void CloseWindow(object? sender, CancelEventArgs ev)
        {
            networkListManager = null;
            champions = null;
            perkTrees = null;
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
            //save it
            //TODO: add option to choose the regular chroma here and in dropdown
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
            if (primaryStyleIndex == index) return;
            primaryStyleIndex = index;

            foreach (Border border in primaryStyleGrid.Children)
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

            if (index == -1)
            {
                keyStonesItemsControl.ItemsSource = null;
                primarySlot1ItemsControl.ItemsSource = null;
                primarySlot2ItemsControl.ItemsSource = null;
                primarySlot3ItemsControl.ItemsSource = null;
            }
            else 
            {
                keyStonesItemsControl.ItemsSource = perkTrees[index].Slots[0];
                primarySlot1ItemsControl.ItemsSource = perkTrees[index].Slots[1];
                primarySlot2ItemsControl.ItemsSource = perkTrees[index].Slots[2];
                primarySlot3ItemsControl.ItemsSource = perkTrees[index].Slots[3];
            }
            if (index == subStyleIndex) ChangeSubStyle(-1);
        }

        private void ChangeSubStyle(int index)
        {
            if (subStyleIndex == index) return;
            subStyleIndex = index;

            foreach (Border border in subStyleGrid.Children)
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

            if (index == -1)
            {
                subSlot1ItemsControl.ItemsSource = null;
                subSlot2ItemsControl.ItemsSource = null;
                subSlot3ItemsControl.ItemsSource = null;
            }
            else
            {
                subSlot1ItemsControl.ItemsSource = perkTrees[index].Slots[1];
                subSlot2ItemsControl.ItemsSource = perkTrees[index].Slots[2];
                subSlot3ItemsControl.ItemsSource = perkTrees[index].Slots[3];
            }
        }

        private void keyStonesImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < keyStonesItemsControl.Items.Count; i++)
            {
                Border border = (Border)VisualTreeHelper.GetChild(keyStonesItemsControl.ItemContainerGenerator.ContainerFromIndex(i), 0);
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

        private void primarySlot1Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < primarySlot1ItemsControl.Items.Count; i++)
            {
                Border border = (Border)VisualTreeHelper.GetChild(primarySlot1ItemsControl.ItemContainerGenerator.ContainerFromIndex(i), 0);
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

        private void primarySlot2Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < primarySlot2ItemsControl.Items.Count; i++)
            {
                Border border = (Border)VisualTreeHelper.GetChild(primarySlot2ItemsControl.ItemContainerGenerator.ContainerFromIndex(i), 0);
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

        private void primarySlot3Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < primarySlot3ItemsControl.Items.Count; i++)
            {
                Border border = (Border)VisualTreeHelper.GetChild(primarySlot3ItemsControl.ItemContainerGenerator.ContainerFromIndex(i), 0);
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

        private void subSlot1Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < subSlot1ItemsControl.Items.Count; i++)
            {
                Border border = (Border)VisualTreeHelper.GetChild(subSlot1ItemsControl.ItemContainerGenerator.ContainerFromIndex(i), 0);
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

        private void subSlot2Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < subSlot2ItemsControl.Items.Count; i++)
            {
                Border border = (Border)VisualTreeHelper.GetChild(subSlot2ItemsControl.ItemContainerGenerator.ContainerFromIndex(i), 0);
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

        private void subSlot3Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < subSlot3ItemsControl.Items.Count; i++)
            {
                Border border = (Border)VisualTreeHelper.GetChild(subSlot3ItemsControl.ItemContainerGenerator.ContainerFromIndex(i), 0);
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
            foreach (Border border in offensivePerkGrid.Children)
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

        private void flexPerkImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (Border border in flexPerkGrid.Children)
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

        private void defensivePerkImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (Border border in defensivePerkGrid.Children)
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
    }
}
