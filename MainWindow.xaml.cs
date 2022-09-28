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
using HandyControl.Tools;
using Passtable.Components;
using Passtable.Containers;
using Passtable.Exceptions;
using Window = HandyControl.Controls.Window;

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
        private DataSearcher _dataSearcher;
        int lpSysRowID;
        static bool lpSysWork;
        static string lpSysPassword;
        string filePath;
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
            filePath = "";
            masterPass = "";
            _dataSearcher = new DataSearcher(gridItems, gridMain);

            WindowBackground.SetBackground(this);
            //Background = new SolidColorBrush(Colors.White);
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
            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) ==
                (ModifierKeys.Control | ModifierKeys.Shift))
            {
                if (e.Key == Key.N)
                {
                    // new file
                    CloseFile(); 
                    return;
                }
                if (e.Key == Key.S) SaveFile(true); // save as
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.D) LogPassSystem();
                if (e.Key == Key.N) AddEntry();
                if (e.Key == Key.O) OpenFile();
            }
            if (e.Key == Key.F1) ShowAbout();
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
            if (_dataSearcher.SearchIsRunning)
            {
                _dataSearcher.DeleteAndGetAll(gridItems[lpSysRowID]);
                UnselectRow();
            }
            else
            {
                gridItems.RemoveAt(lpSysRowID);
                lpSysRowID = -2;
                gridMain.Items.Refresh();
                _dataSearcher.RememberCurrentState();
            }

            SaveFile();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddEntry();
        }

        private void AddEntry()
        {
            if (!isOpen && !SaveFile()) return;
            
            var editForm = new EditGridWindow
            {
                Owner = this,
                Title = "Add new item"
            };
            if (editForm.ShowDialog() != true) return;

            if (_dataSearcher.SearchIsRunning)
            {
                _dataSearcher.GetAll();
                UnselectRow();
            }

            gridItems.Add(new GridItem(editForm.cbTag.SelectedIndex.ToString(), editForm.tbNote.Text,
                editForm.tbLogin.Text, editForm.pbPassword.Password));
            gridMain.Items.Refresh();
            _dataSearcher.RememberCurrentState();
            isOpen = true;

            SaveFile();
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
            if (editForm.ShowDialog() != true) return;
            
            gridItems[lpSysRowID].Note = editForm.tbNote.Text;
            gridItems[lpSysRowID].Login = editForm.tbLogin.Text;
            gridItems[lpSysRowID].Password = editForm.pbPassword.Password;
            gridItems[lpSysRowID].Tag = editForm.cbTag.SelectedIndex.ToString();

            if (_dataSearcher.SearchIsRunning)
            {
                _dataSearcher.EditAndGetAll(gridItems[lpSysRowID]);
                UnselectRow();
            }
            else
            {
                gridMain.Items.Refresh();
                _dataSearcher.RememberCurrentState();
            }
                
            SaveFile();
        }

        private bool SaveFile(bool saveAs = false)
        {
            var filePathPreselected = "";
            if (filePath == "" || saveAs)
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Passtable file|*.passtable"
                };
                if (saveFileDialog.ShowDialog() != true) return false;
                filePathPreselected = saveFileDialog.FileName;
            }
            if (masterPass == "" || saveAs)
            {
                var masterPasswordWindow = new MasterPasswordWindow
                {
                    Owner = this,
                    Title = "Enter master password",
                    btnEnter = { Content = "Save" },
                    SaveMode = true
                };
                if (saveAs && masterPass != "")
                    masterPasswordWindow.btnWithoutChange.Visibility = Visibility.Visible;
                if (masterPasswordWindow.ShowDialog() == false)
                    return false;
                if (!masterPasswordWindow.withoutChange)
                    masterPass = masterPasswordWindow.pbPassword.Password;
            }
            //Process cancellation protection
            if (filePath == "" || saveAs) filePath = filePathPreselected;

            /* Preparing data for saving. */
            string res;
            if (gridItems.Count > 0)
            {
                var stringBuilder = new StringBuilder();
                foreach (var t in gridItems)
                {
                    stringBuilder.Append($"{t.Tag}\t{t.Note}\t{t.Login}\t{t.Password}\n");
                }

                res = stringBuilder.ToString().Remove(stringBuilder.Length - 1);
            }
            else res = "/emptyCollection";
            
            /* Encrypting data. */
            string strToSave;
            try
            {
                var encrypt = AesEncryptor.Encryption(res, masterPass);
                var decrypt = AesEncryptor.Decryption(encrypt, masterPass);
                if (decrypt == res) strToSave = FileVersion.GetChar(2, 1) + encrypt;
                else throw new EncryptionException();
            }
            catch
            {
                const string msg = "This is probably caused by using an unsupported character.";
                const string title = "Encryption error";
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            /* Saving encrypted data to the file. */
            try
            {
                File.WriteAllText(filePath, strToSave);
            }
            catch
            {
                const string msg = "The application does not have sufficient permissions to write data to this file. You need to save the data to a new file to avoid losing data.";
                const string title = "Write error";
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            
            Title = System.IO.Path.GetFileNameWithoutExtension(filePath) + " – Passtable for Windows";
            _statusBar.Show(StatusKey.Saved);
            return true;
        }

        private void CloseFile()
        {
            filePath = "";
            masterPass = "";
            gridItems.Clear();
            gridMain.Items.Refresh();
            _dataSearcher.RememberCurrentState();
            Title = "Passtable for Windows";
            isOpen = false;
        }

        private void mnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }
        
        private void mnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(true);
        }

        private void OpenFile(string pathToFile = "")
        {
            if (pathToFile == "")
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Passtable file|*.passtable"
                };
                if (openFileDialog.ShowDialog() != true) return;
                CloseFile(); // close previously opened file, if it is open
                filePath = openFileDialog.FileName;
            }
            else
            {
                CloseFile();
                filePath = pathToFile;
            }

            string encryptedData;
            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    encryptedData = sr.ReadToEnd();
                }
            }
            catch
            {
                MessageBox.Show("Failed to open file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (encryptedData.Length == 0)
            {
                const string msg = "The file is damaged.";
                const string title = "Critical error";
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (encryptedData[0] == FileVersion.GetChar(2, 1))
            {
                encryptedData = encryptedData.Remove(0, 1); // delete version indicator
            }
            else
            {
                const string msg = "This file was created in a later version of the app and cannot be opened. Update the app.";
                const string title = "Critical error";
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                AesEncryptor.Decryption(encryptedData, "/test");
            }
            catch
            {
                const string msg = "The file is damaged.";
                const string title = "Critical error";
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (!Askers.AskPrimaryPassword(this, false, false, out masterPass))
            {
                CloseFile();
                return;
            }
            
            try
            {
                string data;
                while (true)
                {
                    data = AesEncryptor.Decryption(encryptedData, masterPass);
                    if (data == "/error")
                    {
                        if (Askers.AskPrimaryPassword(this, false, true, out masterPass)) continue;
                        CloseFile();
                        return;
                    }

                    break;
                }

                if (data == "/emptyCollection")
                {
                    Title = System.IO.Path.GetFileNameWithoutExtension(filePath) + " – Passtable for Windows";
                    isOpen = true;
                    return;
                }
                
                foreach (var list in data.Split('\n'))
                {
                    var strs = list.Split('\t');
                    gridItems.Add(new GridItem(strs[0], strs[1], strs[2], strs[3]));
                }
                gridMain.Items.Refresh();
                _dataSearcher.RememberCurrentState();
                Title = System.IO.Path.GetFileNameWithoutExtension(filePath) + " – Passtable for Windows";
                isOpen = true;
            }
            catch
            {
                const string msg = "The file is damaged.";
                const string title = "Critical error";
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                CloseFile();
            }
        }

        private void mnNew_Click(object sender, RoutedEventArgs e)
        {
            CloseFile();
        }

        private void mnAbout_Click(object sender, RoutedEventArgs e)
        {
            ShowAbout();
        }

        private void ShowAbout()
        {
            var aboutWindows = new AboutWindow
            {
                Owner = this
            };
            aboutWindows.ShowDialog();
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

        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || !files[0].EndsWith(".passtable"))
            {
                const string msg = "This file type is not supported.";
                const string title = "Critical error";
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            OpenFile(files[0]);
        }

        private async void TbSearchData_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            async Task<bool> UserKeepsTyping() {
                var txt = tbSearchData.Text;   // remember text
                await Task.Delay(500);        // wait some
                return txt != tbSearchData.Text;  // return that text chaged or not
            }
            if (await UserKeepsTyping()) return;
            
            UnselectRow();
            _dataSearcher.SearchByDataAsync(tbSearchData.Text);
        }

        private void BtTag_OnClick(object sender, RoutedEventArgs e)
        {
            UnselectRow();
            _dataSearcher.SearchByTagAsync(btRed, btGreen, btBlue, btYellow, btPurple);
        }

        private void BtTagNone_OnClick(object sender, RoutedEventArgs e)
        {
            UnselectRow();
            btRed.IsChecked = false;
            btGreen.IsChecked = false;
            btBlue.IsChecked = false;
            btYellow.IsChecked = false;
            btPurple.IsChecked = false;
            _dataSearcher.SearchByTagAsync(btRed, btGreen, btBlue, btYellow, btPurple);
        }

        private void UnselectRow()
        {
            gridMain.UnselectAll();
            gridMain.CurrentCell = new DataGridCellInfo();
            lpSysRowID = -2;
        }
    }
}