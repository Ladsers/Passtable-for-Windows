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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Passtable.Components;
using Passtable.Containers;

namespace Passtable
{
    public class GridItem
    {
        public string Tag { get; set; }
        public string Note { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        
        public GridItem(string tag, string note, string login, string password)
        {
            Tag = tag;
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
        string masterPass;
        bool isOpen;

        private StatusBar _statusBar;
        static MainWindow mainWindow;

        public MainWindow()
        {
            InitializeComponent();
            gridMain.ItemsSource = gridItems;
            gridMain.ClipboardCopyMode = DataGridClipboardCopyMode.None;
            mainWindow = this;
            pathSave = "";
            masterPass = "";
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            gridItems = new List<GridItem>();
            lpSysRowID = -2;
            lpSysWork = false;
            isOpen = false;
            _statusBar = new StatusBar(dpSaveInfo, dpNoEntryInfo);
            //Items for test
            //gridItems.Add(new GridItem("http://example.com/", "typicaluser@example.com", "THECAKEISALIE"));
            //for (int i = 0; i < 100; i++) gridItems.Add(new GridItem(i.ToString(), (i * 2).ToString(), (i * 3).ToString()));
            //
        }

        private void gridMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //FindAndCopyToClipboard();
        }

        private void gridMain_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
            {
                FindAndCopyToClipboard();
            }
            if (e.Key == Key.Delete) DeleteEntry();
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.E)
            {
                EditEntry();
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
                    _statusBar.Show(StatusKey.NoEntry);
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
          DeleteEntry();
        }

        private void DeleteEntry()
        {
            if (lpSysRowID < 0)
            {
                _statusBar.Show(StatusKey.NoEntry);
                return;
            }

            var deleteConfirmWindow = new DeleteConfirmWindow
            {
                Owner = this
            };

            if (deleteConfirmWindow.ShowDialog() != true) return;
                
            lpSysWork = false;
            UnhookWindowsHookEx(_hookID);
            btnCopySuper.Content = "Login -> Password";
            gridItems.RemoveAt(lpSysRowID);
            lpSysRowID = -2;
            gridMain.Items.Refresh();

            SaveFileProcess();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var editForm = new EditGridWindow();
            editForm.Owner = this;
            editForm.Title = "Add new item";
            if (editForm.ShowDialog() == true)
            {
                gridItems.Add(new GridItem(editForm.cbTag.SelectedIndex.ToString(),editForm.tbNote.Text, editForm.tbLogin.Text, editForm.pbPassword.Password)); //!!!
                gridMain.Items.Refresh();
                isOpen = true;
                if (Title == "Passtable for Windows") Title = "Untitled – Passtable for Windows";

                SaveFileProcess();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditEntry();
        }

        private void EditEntry()
        {
            if (lpSysRowID < 0)
            {
                _statusBar.Show(StatusKey.NoEntry);
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
            editForm.cbTag.SelectedIndex = int.Parse(gridItems[lpSysRowID].Tag);
            if (editForm.ShowDialog() == true)
            {
                gridItems[lpSysRowID].Note = editForm.tbNote.Text;
                gridItems[lpSysRowID].Login = editForm.tbLogin.Text;
                gridItems[lpSysRowID].Password = editForm.pbPassword.Password;
                gridItems[lpSysRowID].Tag = editForm.cbTag.SelectedIndex.ToString();
                gridMain.Items.Refresh();
                
                SaveFileProcess();
            }
        }

        private bool SaveFileProcess(bool saveAs = false)
        {
            string pathSavePre = "";
            if (pathSave == "" || saveAs)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Passtable file|*.passtable";
                if (saveFileDialog.ShowDialog() == false)
                    return false;
                pathSavePre = saveFileDialog.FileName;
            }
            if (masterPass == "" || saveAs)
            {
                var masterPasswordWindow = new MasterPasswordWindow();
                masterPasswordWindow.Owner = this;
                masterPasswordWindow.Title = "Enter master password";
                masterPasswordWindow.btnEnter.Content = "Save";
                masterPasswordWindow.saveMode = true;
                if (saveAs && masterPass != "")
                    masterPasswordWindow.btnWithoutChange.Visibility = Visibility.Visible;
                if (masterPasswordWindow.ShowDialog() == false)
                    return false;
                if (!masterPasswordWindow.withoutChange)
                    masterPass = masterPasswordWindow.pbPassword.Password;
            }
            //Process cancellation protection
            if (pathSave == "" || saveAs) pathSave = pathSavePre;

            try
            {
                string output = FileVersion.GetChar(2, 1).ToString();
                string tableData = "";
                for (int i = 0; i < gridItems.Count - 1; i++)
                {
                    tableData += gridItems[i].Tag + "\t" + gridItems[i].Note + "\t" + gridItems[i].Login + "\t" + gridItems[i].Password + "\n";
                }
                tableData += gridItems[gridItems.Count - 1].Tag + "\t" + gridItems[gridItems.Count - 1].Note + "\t" + gridItems[gridItems.Count - 1].Login +
                    "\t" + gridItems[gridItems.Count - 1].Password;

                output += AesEncryptor.Encryption(tableData, masterPass);
                File.WriteAllText(pathSave, output);
                Title = System.IO.Path.GetFileNameWithoutExtension(pathSave) + " – Passtable for Windows";
                _statusBar.Show(StatusKey.Saved);
                
                return true;
            }
            catch
            {
                MessageBox.Show("Failed to save file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                pathSave = "";
                masterPass = "";
                return false;
            }
        }

        private void mnOpen_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isOpen) OpenFileProcess(new MainWindow());
            else OpenFileProcess(this);
        }
        
        private void mnSaveAs_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SaveFileProcess(true);
        }

        private void OpenFileProcess(MainWindow main)
        {
            if (main != this) main.Show();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Passtable file|*.passtable";
            if (openFileDialog.ShowDialog() == false)
            {
                if (main != this) main.Close();
                return;
            }
            main.pathSave = openFileDialog.FileName;

            try
            {
                string ver;
                using (StreamReader sr = new StreamReader(main.pathSave))
                {
                    ver = sr.ReadToEnd();
                }
                var verCheck = FileVersion.GetChar(2, 1);
                if (ver[0] != verCheck) throw new Exception();
            }
            catch
            {
                MessageBox.Show("Failed to open file! Unsupported version of the file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (main != this) main.Close();
                else
                {
                    main.pathSave = "";
                    main.masterPass = "";
                }
                return;
            }

            var masterPasswordWindow = new MasterPasswordWindow();
            masterPasswordWindow.Owner = main;
            masterPasswordWindow.Title = "Enter master password";
            masterPasswordWindow.saveMode = false;
            if (masterPasswordWindow.ShowDialog() == false)
            {
                if (main != this) main.Close();
                else main.pathSave = "";
                return;
            }
            main.masterPass = masterPasswordWindow.pbPassword.Password;

            try
            {
                string encrypted;
                using (StreamReader sr = new StreamReader(main.pathSave))
                {
                    encrypted = sr.ReadToEnd();
                }
                encrypted = encrypted.Remove(0, 1);
                string inStr = "";
                while (true)
                {
                    inStr = AesEncryptor.Decryption(encrypted, main.masterPass);
                    if (inStr == "/error")
                    {
                        masterPasswordWindow = new MasterPasswordWindow();
                        masterPasswordWindow.Owner = main;
                        masterPasswordWindow.Title = "Enter master password";
                        masterPasswordWindow.invalidPassword = true;
                        if (masterPasswordWindow.ShowDialog() == false) return;
                        main.masterPass = masterPasswordWindow.pbPassword.Password;
                    }
                    else break;
                }
                string[] arrStr = inStr.Split(new char[] { '\n' });
                for (int i = 0; i < arrStr.Length; i++)
                {
                    string[] recStr = arrStr[i].Split(new char[] { '\t' });
                    main.gridItems.Add(new GridItem(recStr[0], recStr[1], recStr[2], recStr[3]));
                }
                main.gridMain.Items.Refresh();
                main.Title = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName) + " – Passtable for Windows";
                main.isOpen = true;
            }
            catch
            {
                MessageBox.Show("Failed to open file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (main != this) main.Close();
                else
                {
                    main.pathSave = "";
                    main.masterPass = "";
                }
                return;
            }
        }

        private void mnNew_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var passtableApp = new MainWindow();
            passtableApp.Show();
        }

        private void mnAbout_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
        
        private void CopyToClipboard(ClipboardKey key)
        {
            if (gridMain.SelectedItem == null) return;

            lpSysWork = false;
            UnhookWindowsHookEx(_hookID);
            mainWindow.btnCopySuper.Content = "Login -> Password";
            
            var rowId = gridMain.Items.IndexOf(gridMain.CurrentItem);
            switch (key)
            {
                case ClipboardKey.Note:
                    Clipboard.SetText(gridItems[rowId].Note);
                    break;
                case ClipboardKey.Username:
                    Clipboard.SetText(gridItems[rowId].Login);
                    break;
                case ClipboardKey.Password:
                    lpSysPassword = gridItems[rowId].Password; //additional password protection
                    Clipboard.SetText(lpSysPassword);
                    _hookID = SetHook(_proc);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
        }

        private void FindAndCopyToClipboard()
        {
            if (gridMain.SelectedItem == null) return;
            var colId = gridMain.CurrentCell.Column.DisplayIndex;
            var key = ClipboardKey.Note;
            if (colId == 2) key = ClipboardKey.Username;
            if (colId == 3) key = ClipboardKey.Password;
            CopyToClipboard(key);
                
            var rowId = gridMain.Items.IndexOf(gridMain.CurrentItem);
            var row = (DataGridRow)gridMain.ItemContainerGenerator.ContainerFromIndex(rowId);

            var sizeAnimation = new DoubleAnimation(24, 17, new Duration(TimeSpan.FromSeconds(0.4)));
            var opacityAnimation = new DoubleAnimation(1.0, 0.3, new Duration(TimeSpan.FromSeconds(0.4)));
                
            var btKey = "btCopyNote";
            if (colId == 2) btKey = "btCopyUsername";
            if (colId == 3) btKey = "btCopyPassword";
                
            DataGridUtils.GetObject<Image>(row, btKey).BeginAnimation(WidthProperty, sizeAnimation);
            DataGridUtils.GetObject<Image>(row, btKey).BeginAnimation(HeightProperty, sizeAnimation);
            DataGridUtils.GetObject<Image>(row, btKey).BeginAnimation(OpacityProperty, opacityAnimation);
        }
        
        private void btCopyNote_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(ClipboardKey.Note);
        }
        
        private void btCopyUsername_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(ClipboardKey.Username);
        }
        
        private void btCopyPassword_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(ClipboardKey.Password);
        }
        
        private void btShowPassword_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var rowId = gridMain.Items.IndexOf(gridMain.CurrentItem);
            var row = (DataGridRow)gridMain.ItemContainerGenerator.ContainerFromIndex(rowId);

            var textBlock = DataGridUtils.GetObject<TextBlock>(row, "tbPassword");
            var stackPanel = DataGridUtils.GetObject<StackPanel>(row, "spPassword");
            var imgShow = DataGridUtils.GetObject<Image>(row, "btShowPassword");
            var imgCopy = DataGridUtils.GetObject<Image>(row, "btCopyPassword");

            if (textBlock.Visibility == Visibility.Collapsed)
            {
                textBlock.Visibility = Visibility.Visible;
                imgShow.Source = (DrawingImage)FindResource("IconLock");
                imgShow.Margin = new Thickness(10, 0, 3, 0);
                imgCopy.Margin = new Thickness(2, 0, 10, 0);
                stackPanel.SetValue(Grid.ColumnProperty, 1);
            }
            else
            {
                textBlock.Visibility = Visibility.Collapsed;
                imgShow.Source = (DrawingImage)FindResource("IconShowPassword");
                imgShow.Margin = new Thickness(10, 0, 7, 0);
                imgCopy.Margin = new Thickness(7, 0, 10, 0);
                stackPanel.SetValue(Grid.ColumnProperty, 0);
            }
        }
        
    }
}