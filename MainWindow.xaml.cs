﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        public string Note { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string PseudoPassword { get; }
        public GridItem(string note, string login, string password)
        {
            Note = note;
            Login = login;
            Password = password;
            PseudoPassword = "********";
        }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        List<GridItem> gridItems;
        int lpSysRowID;
        static bool lpSysWork;
        static string lpSysPassword;
        string pathSave;
        bool isOpen;

        static MainWindow mainWindow;

        public MainWindow()
        {
            InitializeComponent();
            gridMain.ItemsSource = gridItems;
            gridMain.ClipboardCopyMode = DataGridClipboardCopyMode.None;
            mainWindow = this;
            pathSave = "";
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            gridItems = new List<GridItem>();
            lpSysRowID = -2;
            lpSysWork = false;
            isOpen = false;
            //Items for test
            //gridItems.Add(new GridItem("http://example.com/", "typicaluser@example.com", "THECAKEISALIE"));
            //for (int i = 0; i < 100; i++) gridItems.Add(new GridItem(i.ToString(), (i * 2).ToString(), (i * 3).ToString()));
            //
        }

        private void CellDataToClipboard()
        {
            if (gridMain.SelectedItem == null) return;

            lpSysWork = false;
            UnhookWindowsHookEx(_hookID);
            mainWindow.btnCopySuper.Content = "Login -> Password";

            int colID = gridMain.CurrentCell.Column.DisplayIndex;
            int rowID = gridMain.Items.IndexOf(gridMain.CurrentItem);
            switch (colID)
            {
                case 0:
                    Clipboard.SetText(gridItems[rowID].Note);
                    break;
                case 1:
                    Clipboard.SetText(gridItems[rowID].Login);
                    break;
                case 2:
                    lpSysPassword = gridItems[rowID].Password; //additional password protection
                    Clipboard.SetText(lpSysPassword);
                    _hookID = SetHook(_proc);
                    break;
            }
            
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

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                var key = KeyInterop.KeyFromVirtualKey(vkCode);
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && Key.V == key)
                {
                    Thread.Sleep(400);
                    if (Clipboard.GetText() != lpSysPassword) Clipboard.SetText(lpSysPassword);
                    else
                    {
                        Clipboard.SetText("");
                        lpSysWork = false;
                        UnhookWindowsHookEx(_hookID);
                        mainWindow.btnCopySuper.Content = "Login -> Password";
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void LogPassSystem()
        {
            if (!lpSysWork)
            {
                if (lpSysRowID < 0)
                {
                    MessageBox.Show("Nothing selected!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Clipboard.SetText(gridItems[lpSysRowID].Login);
                lpSysPassword = gridItems[lpSysRowID].Password;
                _hookID = SetHook(_proc);

                btnCopySuper.Content = "Stop!";
                lpSysWork = true;
            }
            else
            {
                lpSysWork = false;
                UnhookWindowsHookEx(_hookID);
                btnCopySuper.Content = "Login -> Password";
            }
        }

        private void btnCopySuper_Click(object sender, RoutedEventArgs e)
        {
            LogPassSystem();
        }

        private void gridMain_CurrentCellChanged(object sender, EventArgs e)
        {
            if (gridMain.Items.IndexOf(gridMain.CurrentItem) != -1)
            lpSysRowID = gridMain.Items.IndexOf(gridMain.CurrentItem);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.D)
            {
                LogPassSystem();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lpSysRowID < 0)
            {
                MessageBox.Show("Nothing selected!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            lpSysWork = false;
            UnhookWindowsHookEx(_hookID);
            btnCopySuper.Content = "Login -> Password";
            gridItems.RemoveAt(lpSysRowID);
            lpSysRowID = -2;
            gridMain.Items.Refresh();
            btnSave.IsEnabled = true;
            btnSaveAs.IsEnabled = true;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var editForm = new EditGridWindow();
            editForm.Owner = this;
            editForm.Title = "Add new item";
            if (editForm.ShowDialog() == true) 
            {
                gridItems.Add(new GridItem(editForm.tbNote.Text, editForm.tbLogin.Text, editForm.pbPassword.Password));
                gridMain.Items.Refresh();
                btnSave.IsEnabled = true;
                btnSaveAs.IsEnabled = true;
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lpSysRowID < 0)
            {
                MessageBox.Show("Nothing selected!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            lpSysWork = false;
            UnhookWindowsHookEx(_hookID);
            btnCopySuper.Content = "Login -> Password";

            var editForm = new EditGridWindow();
            editForm.Owner = this;
            editForm.Title = "Edit item";
            editForm.tbNote.Text = gridItems[lpSysRowID].Note;
            editForm.tbLogin.Text = gridItems[lpSysRowID].Login;
            editForm.pbPassword.Password = gridItems[lpSysRowID].Password;
            if (editForm.ShowDialog() == true)
            {
                if (gridItems[lpSysRowID].Note != editForm.tbNote.Text ||
                    gridItems[lpSysRowID].Login != editForm.tbLogin.Text ||
                    gridItems[lpSysRowID].Password != editForm.pbPassword.Password)
                {
                    btnSave.IsEnabled = true;
                    btnSaveAs.IsEnabled = true;
                }
                gridItems[lpSysRowID].Note = editForm.tbNote.Text;
                gridItems[lpSysRowID].Login = editForm.tbLogin.Text;
                gridItems[lpSysRowID].Password = editForm.pbPassword.Password;
                gridMain.Items.Refresh();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (pathSave == "")
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Passtable files|*.pst|All files(*.*)|*.*";
                if (saveFileDialog.ShowDialog() == false)
                    return;
                pathSave = saveFileDialog.FileName;
            }
            try
            {
                string output = "ABBABBA" + "\n";
                for (int i=0; i < gridItems.Count-1; i++) 
                {
                    output += gridItems[i].Note + "\t" + gridItems[i].Login + "\t" + gridItems[i].Password + "\n";
                }
                output += gridItems[gridItems.Count - 1].Note + "\t" + gridItems[gridItems.Count - 1].Login +
                    "\t" + gridItems[gridItems.Count - 1].Password;
                File.WriteAllText(pathSave, output);
                btnSave.IsEnabled = false;
            }
            catch
            {
                MessageBox.Show("Failed to save file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                pathSave = "";
                return;
            }
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnOpen_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isOpen) OpenFileInNewWindow();
            else OpenFileInThisWindow();
        }

        private void OpenFileInNewWindow() 
        {
            var passtableApp = new MainWindow();
            passtableApp.Show();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Passtable files|*.pst|All files(*.*)|*.*";
            if (openFileDialog.ShowDialog() == false)
            {
                passtableApp.Close();
                return;
            }

            passtableApp.pathSave = openFileDialog.FileName;

            try
            {
                string inStr;
                using (StreamReader sr = new StreamReader(passtableApp.pathSave))
                {
                    inStr = sr.ReadToEnd();
                }
                string[] arrStr = inStr.Split(new char[] { '\n' });
                for (int i = 1; i < arrStr.Length; i++)
                {
                    string[] recStr = arrStr[i].Split(new char[] { '\t' });
                    passtableApp.gridItems.Add(new GridItem(recStr[0], recStr[1], recStr[2]));
                }
                passtableApp.gridMain.Items.Refresh();
                passtableApp.Title = "\"" + openFileDialog.SafeFileName + "\" – Passtable";
                isOpen = true;
            }
            catch
            {
                MessageBox.Show("Failed to open file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                passtableApp.pathSave = "";
                passtableApp.Close();
                return;
            }
        }
        private void OpenFileInThisWindow() 
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Passtable files|*.pst|All files(*.*)|*.*";
            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }

            pathSave = openFileDialog.FileName;

            try
            {
                string inStr;
                using (StreamReader sr = new StreamReader(pathSave))
                {
                    inStr = sr.ReadToEnd();
                }
                string[] arrStr = inStr.Split(new char[] { '\n' });
                for (int i = 1; i < arrStr.Length; i++)
                {
                    string[] recStr = arrStr[i].Split(new char[] { '\t' });
                    gridItems.Add(new GridItem(recStr[0], recStr[1], recStr[2]));
                }
                gridMain.Items.Refresh();
                Title = "\"" + openFileDialog.SafeFileName + "\" – Passtable";
                isOpen = true;
            }
            catch
            {
                MessageBox.Show("Failed to open file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                pathSave = "";
                Close();
                return;
            }
        }

        private void mnNew_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var passtableApp = new MainWindow();
            passtableApp.Show();
        }
    }
}
