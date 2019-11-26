using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Passtable
{
    public class GridItem
    {
        public string Note { get; }
        public string Login { get; }
        public string Password { get; }
        public GridItem(string note, string login, string password)
        {
            Note = note;
            Login = login;
            Password = password;
        }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<GridItem> gridItems;

        public MainWindow()
        {
            InitializeComponent();
            gridMain.ItemsSource = gridItems;
            gridMain.ClipboardCopyMode = DataGridClipboardCopyMode.None;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            gridItems = new List<GridItem>();

            //Items for test
            gridItems.Add(new GridItem("test1", "test2", "test3"));
            for (int i = 0; i < 100; i++) gridItems.Add(new GridItem(i.ToString(), (i * 2).ToString(), (i * 3).ToString()));
            //
        }

        private void gridMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (gridMain.SelectedItem == null) return;
            int colID = gridMain.CurrentCell.Column.DisplayIndex;
            string cellStr = (gridMain.SelectedCells[colID].Column.GetCellContent(gridMain.SelectedItem) as TextBlock)
                .Text;
            Clipboard.SetText(cellStr);
        }

        private void gridMain_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
            {
                if (gridMain.SelectedItem == null) return;
                int colID = gridMain.CurrentCell.Column.DisplayIndex;
                string cellStr = (gridMain.SelectedCells[colID].Column.GetCellContent(gridMain.SelectedItem) as TextBlock)
                    .Text;
                Clipboard.SetText(cellStr);
            }
        }
    }
}
