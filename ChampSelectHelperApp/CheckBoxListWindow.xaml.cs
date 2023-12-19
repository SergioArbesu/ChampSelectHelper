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

namespace ChampSelectHelperApp
{
    /// <summary>
    /// Lógica de interacción para CheckBoxListWindow.xaml
    /// </summary>
    public partial class CheckBoxListWindow : Window
    {
        public List<CheckBoxListItem> CheckBoxList { get; private set; }

        public CheckBoxListWindow(Window owner, List<CheckBoxListItem> checkBoxList, string title)
        {
            InitializeComponent();

            Owner = owner;

            CheckBoxList = checkBoxList;
            Title = title;

            CheckBoxList.Add(new CheckBoxListItem("Original", true));
            CheckBoxList.Add(new CheckBoxListItem("Covenant Azir", false));
            CheckBoxList.Add(new CheckBoxListItem("Elderwood SKT", false));
            checkBoxListBox.ItemsSource = CheckBoxList;
        }
    }

    public class CheckBoxListItem
    {
        public string Name { get; private set; }
        public bool IsChecked { get; set; }

        public CheckBoxListItem(string name, bool isChecked)
        {
            Name = name;
            IsChecked = isChecked;
        }
    }
}
