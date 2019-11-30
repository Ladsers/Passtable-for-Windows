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
using System.Windows.Threading;

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
            gridItems.Add(new GridItem("http://example.com/", "typicaluser@example.com", "THECAKEISALIE"));
            for (int i = 0; i < 100; i++) gridItems.Add(new GridItem(i.ToString(), (i * 2).ToString(), (i * 3).ToString()));
            //
        }

        private void CellDataToClipboard()
        {
            if (gridMain.SelectedItem == null) return;
            int colID = gridMain.CurrentCell.Column.DisplayIndex;
            string cellStr = (gridMain.SelectedCells[colID].Column.GetCellContent(gridMain.SelectedItem) as TextBlock)
                .Text;
            Clipboard.SetText(cellStr);

            DataGridCell cell = gridMain.SelectedCells[colID].Column.GetCellContent(gridMain.SelectedItem).Parent as DataGridCell;
            Point coords = cell.PointToScreen(new Point(0, 0));
            ToolTip tt = new ToolTip();
            tt.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            tt.HorizontalOffset = coords.X + 10;
            tt.VerticalOffset = coords.Y + 10;
            tt.Content = $"{gridMain.CurrentCell.Column.Header} copied";
            tt.IsOpen = true;
            DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1), IsEnabled = true };
            timer.Tick += new EventHandler(delegate (object timerSender, EventArgs timerArgs)
            {
                tt.IsOpen = false;
                timer = null;
            });
        }

        private void gridMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CellDataToClipboard();
        }

        private void gridMain_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
            {
                CellDataToClipboard();
            }
        }
    }
}
