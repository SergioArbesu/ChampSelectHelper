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
        private Dictionary<int, ChampionSettings> champSettDict = new();

        private BitmapImage noChampImg = new BitmapImage(new Uri(@"pack://application:,,,/Resources/noChamp.png"));

        private int primaryStyleIndex = -1;
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

        public void InitializeWindow()
        {
            CheckConnectivity();
            try
            {
                using (WebClient httpClient = new())
                {
                    string response = httpClient.DownloadString(Program.PERKS_JSON_URL);
                    JArray parsedArray = JArray.Parse(response);
                    CreatePerkTreeInfo(parsedArray);
                    response = httpClient.DownloadString(Program.CHAMPIONS_JSON_URL);
                    JObject parsedObject = JObject.Parse(response);
                    CreateChampInfo(parsedObject);
                }
            }
            catch (WebException ex) 
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
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
            //Task[] tasks = new Task[champions.Length];
            int i = 0;
            foreach (var champion in champJObject)
            {
                JObject value = (JObject)champion.Value;
                ChampInfo champInfo = new ChampInfo(value);
                champions[i] = champInfo;
                //tasks[i] = Task.Run(() => champInfo.CreateChampion(value));
                i++;
            }
            //Task.WaitAll(tasks);
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
            string tag = (string)((Image)sender).Tag;
            int tagInt = int.Parse(tag);

            if (primaryStyleIndex == tagInt) return;
            else primaryStyleIndex = tagInt;

            foreach (Image style in subStyleGrid.Children)
            {
                style.Visibility = Visibility.Visible;
                if ((string)style.Tag == tag)
                {
                    style.Visibility = Visibility.Collapsed;
                }
            }

            ChangePrimaryStyle(tagInt);
        }

        private void subStyle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string tag = (string)((Image)sender).Tag;
            int tagInt = int.Parse(tag);

            if (subStyleIndex == tagInt) return;
            
            subStyleIndex = tagInt;
            ChangeSubStyle(tagInt);
        }

        private void ChangePrimaryStyle(int index)
        {
            keyStonesItemsControl.ItemsSource = perkTrees[index].Slots[0];
            primarySlot1ItemsControl.ItemsSource = perkTrees[index].Slots[1];
            primarySlot2ItemsControl.ItemsSource = perkTrees[index].Slots[2];
            primarySlot3ItemsControl.ItemsSource = perkTrees[index].Slots[3];
        }

        private void ChangeSubStyle(int index)
        {

        }

        private void keyStonesImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < keyStonesItemsControl.Items.Count; i++)
            {
                Image image = (Image)VisualTreeHelper.GetChild(keyStonesItemsControl.ItemContainerGenerator.ContainerFromIndex(i), 0);
                if (image == sender)
                {
                    image.Source = ((PerkInfo)((ContentPresenter)image.TemplatedParent).DataContext).Icon;
                }
                else
                {
                    image.Source = ((PerkInfo)((ContentPresenter)image.TemplatedParent).DataContext).GrayIcon;
                }
            }
        }

        private void primarySlot1Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < primarySlot1ItemsControl.Items.Count; i++)
            {
                Image image = (Image)VisualTreeHelper.GetChild(primarySlot1ItemsControl.ItemContainerGenerator.ContainerFromIndex(i), 0);
                if (image == sender)
                {
                    image.Source = ((PerkInfo)((ContentPresenter)image.TemplatedParent).DataContext).Icon;
                }
                else
                {
                    image.Source = ((PerkInfo)((ContentPresenter)image.TemplatedParent).DataContext).GrayIcon;
                }
            }
        }

        private void primarySlot2Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < primarySlot2ItemsControl.Items.Count; i++)
            {
                Image image = (Image)VisualTreeHelper.GetChild(primarySlot2ItemsControl.ItemContainerGenerator.ContainerFromIndex(i), 0);
                if (image == sender)
                {
                    image.Source = ((PerkInfo)((ContentPresenter)image.TemplatedParent).DataContext).Icon;
                }
                else
                {
                    image.Source = ((PerkInfo)((ContentPresenter)image.TemplatedParent).DataContext).GrayIcon;
                }
            }
        }

        private void primarySlot3Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < primarySlot3ItemsControl.Items.Count; i++)
            {
                Image image = (Image)VisualTreeHelper.GetChild(primarySlot3ItemsControl.ItemContainerGenerator.ContainerFromIndex(i), 0);
                if (image == sender)
                {
                    image.Source = ((PerkInfo)((ContentPresenter)image.TemplatedParent).DataContext).Icon;
                }
                else
                {
                    image.Source = ((PerkInfo)((ContentPresenter)image.TemplatedParent).DataContext).GrayIcon;
                }
            }
        }
    }
}
