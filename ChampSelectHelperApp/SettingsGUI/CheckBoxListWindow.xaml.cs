using System.Collections.Generic;
using System.Windows;

namespace ChampSelectHelper
{
    /// <summary>
    /// Lógica de interacción para CheckBoxListWindow.xaml
    /// </summary>
    public partial class CheckBoxListWindow : Window
    {
        public List<CheckBoxListItem> CheckBoxList { get; private set; } //remove this when everything is settled and working

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
