using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public List<CheckBoxListItem> CheckBoxList { get; set; } //remove this when everything is settled and working

        public CheckBoxListWindow(Window owner, List<CheckBoxListItem> checkBoxList, string title)
        {
            InitializeComponent();

            Owner = owner;

            CheckBoxList = checkBoxList;
            Title = title;

            checkBoxItemsControl.ItemsSource = CheckBoxList;
        }
    }

    public class CheckBoxListItem
    {
        public string Name { get; private set; }
        public bool IsChecked { get; set; }
        public int Id { get; private set; }

        public CheckBoxListItem(bool isChecked, string name, int id)
        {
            Name = name;
            IsChecked = isChecked;
            Id = id;
        }
    }
}
