using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using Passtable.Components;
using Passtable.Containers;
using Passtable.Exceptions;
using Passtable.Resources;
using Passtable.Tools;
using StatusBar = Passtable.Components.StatusBar;

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

    public partial class MainWindow
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod,
            uint dwThreadId);

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
        private readonly DataSearcher _dataSearcher;
        int lpSysRowID;
        static bool lpSysWork;
        static string lpSysPassword;
        private string _appTitle;
        bool isOpen;
        bool copyIsBlocked;

        private string FilePath { get; set; }
        
        public string PrimaryPassword
        {
            private get => _primaryPassword;
            set => _primaryPassword = value;
        }
        private string _primaryPassword;
        
        private List<DataGridRow> _showedPasswordsRows;

        private StatusBar _statusBar;
        static MainWindow mainWindow;

        public MainWindow()
        {
            InitializeComponent();
            DevTools.MarkIcon(this);
            _appTitle = "Passtable for Windows" + DevTools.AddDevelopInfo();
            Title = _appTitle;
            gridMain.ItemsSource = gridItems;
            gridMain.ClipboardCopyMode = DataGridClipboardCopyMode.None;
            mainWindow = this;
            FilePath = "";
            PrimaryPassword = "";
            _dataSearcher = new DataSearcher(gridItems, gridMain);

            WindowBackground.SetBackground(this);
            HandleUiWidgets();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            gridItems = new List<GridItem>();
            lpSysRowID = -2;
            lpSysWork = false;
            isOpen = false;
            _statusBar = new StatusBar(dpSaveInfo, dpNoEntryInfo, dpNotEnoughData);
            _showedPasswordsRows = new List<DataGridRow>();
        }
        
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            StartupOpener.OpenFile(this, Environment.GetCommandLineArgs());
        }

        private void gridMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FindAndCopyToClipboard();
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

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.W)
            {
                ShowPassword();
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
                    else LogPassAbort(mainWindow.btLogPass);
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

                var username = gridItems[lpSysRowID].Login;
                var password = gridItems[lpSysRowID].Password;

                if (username == "" || password == "")
                {
                    _statusBar.Show(StatusKey.NotEnoughData);
                    return;
                }

                Clipboard.SetText(username);
                lpSysPassword = password;
                try
                {
                    _hookID = SetHook(_proc);
                    BtLogPassSetState(true, btLogPass);
                    lpSysWork = true;
                }
                catch
                {
                    ShowErrBox(Strings.err_title, Strings.err_logPass_msg);
                }
            }
            else LogPassAbort();
        }

        private static void LogPassAbort(Button button)
        {
            Clipboard.SetText("");
            lpSysWork = false;
            UnhookWindowsHookEx(_hookID);
            BtLogPassSetState(false, button);
        }

        private void LogPassAbort() => LogPassAbort(btLogPass);

        private void btnCopySuper_Click(object sender, RoutedEventArgs e)
        {
            LogPassSystem();
        }

        private void gridMain_CurrentCellChanged(object sender, EventArgs e)
        {
            if (gridMain.Items.IndexOf(gridMain.CurrentItem) != -1)
                lpSysRowID = gridMain.Items.IndexOf(gridMain.CurrentItem);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
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
                if (e.Key == Key.N) AddEntry();
                if (e.Key == Key.O) OpenFile();
                if (isOpen)
                {
                    if (e.Key == Key.D) LogPassSystem();
                    if (e.Key == Key.D1) SearchByTagShortcut(btRed);
                    if (e.Key == Key.D2) SearchByTagShortcut(btGreen);
                    if (e.Key == Key.D3) SearchByTagShortcut(btBlue);
                    if (e.Key == Key.D4) SearchByTagShortcut(btYellow);
                    if (e.Key == Key.D5) SearchByTagShortcut(btPurple);
                    if (e.Key == Key.D0) SearchByTagShortcut();
                    if (e.Key == Key.F) SearchByDataShortcut();
                }
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

            LogPassAbort();
            if (_dataSearcher.SearchIsRunning)
            {
                _dataSearcher.DeleteAndGetAll(gridItems[lpSysRowID]);
                ResetSearch();
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
                Title = Strings.title_addEntry
            };
            if (editForm.ShowDialog() != true) return;

            if (_dataSearcher.SearchIsRunning)
            {
                _dataSearcher.GetAll();
                ResetSearch();
            }

            gridItems.Add(new GridItem(editForm.SelectedTag.ToString(), editForm.tbNote.Text,
                editForm.tbLogin.Text, editForm.pbPassword.Password));
            gridMain.Items.Refresh();
            _dataSearcher.RememberCurrentState();

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

            LogPassAbort();

            var editForm = new EditGridWindow();
            editForm.Owner = this;
            editForm.Title = Strings.title_editEntry;
            editForm.tbNote.Text = gridItems[lpSysRowID].Note;
            editForm.tbLogin.Text = gridItems[lpSysRowID].Login;
            editForm.pbPassword.Password = gridItems[lpSysRowID].Password;
            editForm.SelectTag(int.Parse(gridItems[lpSysRowID].Tag));
            if (editForm.ShowDialog() != true) return;

            gridItems[lpSysRowID].Note = editForm.tbNote.Text;
            gridItems[lpSysRowID].Login = editForm.tbLogin.Text;
            gridItems[lpSysRowID].Password = editForm.pbPassword.Password;
            gridItems[lpSysRowID].Tag = editForm.SelectedTag.ToString();

            if (_dataSearcher.SearchIsRunning)
            {
                _dataSearcher.EditAndGetAll(gridItems[lpSysRowID]);
                ResetSearch();
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
            if (FilePath == "" || saveAs)
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Passtable file|*.passtable"
                };
                if (saveFileDialog.ShowDialog() != true) return false;
                filePathPreselected = saveFileDialog.FileName;
                if (!VerifyFileName(filePathPreselected)) return false;
            }

            if (PrimaryPassword == "" || saveAs)
            {
                var mode = saveAs && PrimaryPassword != "" ? Askers.Mode.SaveAs : Askers.Mode.Save;
                if (!Askers.AskPrimaryPassword(this, mode, false, ref _primaryPassword))
                    return false;
            }

            //Process cancellation protection
            if (FilePath == "" || saveAs) FilePath = filePathPreselected;

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
                var encrypt = AesEncryptor.Encryption(res, PrimaryPassword);
                var decrypt = AesEncryptor.Decryption(encrypt, PrimaryPassword);
                if (decrypt == res) strToSave = FileVersion.GetChar(2, 1) + encrypt;
                else throw new EncryptionException();
            }
            catch
            {
                ShowErrBox(Strings.err_encryption_title, Strings.err_encryption_msg);
                return false;
            }

            /* Saving encrypted data to the file. */
            try
            {
                File.WriteAllText(FilePath, strToSave);
            }
            catch
            {
                ShowErrBox(Strings.err_write_title, Strings.err_write_msg);
                return false;
            }

            if (!isOpen)
            {
                isOpen = true;
                HandleUiWidgets();
            }

            Title = $"{Path.GetFileNameWithoutExtension(FilePath)} – {_appTitle}";
            _statusBar.Show(StatusKey.Saved);
            return true;
        }

        private void CloseFile()
        {
            if (!isOpen) return;
            FilePath = "";
            PrimaryPassword = "";
            gridItems.Clear();
            gridMain.Items.Refresh();
            _dataSearcher.RememberCurrentState();
            Title = _appTitle;
            isOpen = false;
            HandleUiWidgets();
        }

        private void mnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void mnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(true);
        }

        public void OpenFile(string pathToFile = "")
        {
            if (pathToFile == "")
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Passtable file|*.passtable"
                };
                if (openFileDialog.ShowDialog() != true) return;
                CloseFile(); // close previously opened file, if it is open
                FilePath = openFileDialog.FileName;
            }
            else
            {
                CloseFile();
                FilePath = pathToFile;
            }

            string encryptedData;
            try
            {
                using (var sr = new StreamReader(FilePath))
                {
                    encryptedData = sr.ReadToEnd();
                }
            }
            catch
            {
                ShowErrBox(Strings.err_title, Strings.err_openFileFail);
                return;
            }

            if (encryptedData.Length == 0)
            {
                ShowErrBox(Strings.err_critical_title, Strings.err_critical_fileDamaged);
                return;
            }

            if (encryptedData[0] == FileVersion.GetChar(2, 1))
            {
                encryptedData = encryptedData.Remove(0, 1); // delete version indicator
            }
            else
            {
                ShowErrBox(Strings.err_critical_title, Strings.err_critical_updateApp);
                return;
            }

            try
            {
                AesEncryptor.Decryption(encryptedData, "/test");
            }
            catch
            {
                ShowErrBox(Strings.err_critical_title, Strings.err_critical_fileDamaged);
                return;
            }

            if (Verifier.VerifyPrimary(PrimaryPassword) != 0 && // to open file on app startup
                !Askers.AskPrimaryPassword(this, Askers.Mode.Open, false, ref _primaryPassword))
            {
                CloseFile();
                return;
            }

            try
            {
                string data;
                while (true)
                {
                    data = AesEncryptor.Decryption(encryptedData, PrimaryPassword);
                    if (data == "/error")
                    {
                        if (Askers.AskPrimaryPassword(this, Askers.Mode.Open, true, ref _primaryPassword)) continue;
                        CloseFile();
                        return;
                    }

                    break;
                }

                if (data == "/emptyCollection")
                {
                    Title = $"{Path.GetFileNameWithoutExtension(FilePath)} – {_appTitle}";
                    isOpen = true;
                    HandleUiWidgets();
                    return;
                }

                foreach (var list in data.Split('\n'))
                {
                    var strs = list.Split('\t');
                    gridItems.Add(new GridItem(strs[0], strs[1], strs[2], strs[3]));
                }

                gridMain.Items.Refresh();
                _dataSearcher.RememberCurrentState();
                Title = $"{Path.GetFileNameWithoutExtension(FilePath)} – {_appTitle}";
                isOpen = true;
                HandleUiWidgets();
            }
            catch
            {
                ShowErrBox(Strings.err_critical_title, Strings.err_critical_fileDamaged);
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

        private async void CopyToClipboard(ClipboardKey key)
        {
            if (gridMain.SelectedItem == null) return;

            if (copyIsBlocked) return;
            copyIsBlocked = true;
            await Task.Delay(200); // if the user copy too often (without delay), the app crashes.
            copyIsBlocked = false;

            LogPassAbort();

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
                    try
                    {
                        _hookID = SetHook(_proc);
                    }
                    catch
                    {
                        // ignored
                    }

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
            ShowPassword();
        }

        private void ShowPassword()
        {
            var rowId = gridMain.Items.IndexOf(gridMain.CurrentItem);
            var row = (DataGridRow)gridMain.ItemContainerGenerator.ContainerFromIndex(rowId);
            _showedPasswordsRows.Add(row);

            var textBlock = DataGridUtils.GetObject<TextBlock>(row, "tbPassword");
            var stackPanel = DataGridUtils.GetObject<StackPanel>(row, "spPassword");
            var imgShow = DataGridUtils.GetObject<Image>(row, "btShowPassword");
            var imgCopy = DataGridUtils.GetObject<Image>(row, "btCopyPassword");

            void Change(Visibility visibility, string imgSource, int showLeft, int showRight, int copyLeft,
                int copyRight, int column)
            {
                textBlock.Visibility = visibility;
                imgShow.Source = (DrawingImage)FindResource(imgSource);
                imgShow.Margin = new Thickness(showLeft, 0, showRight, 0);
                imgCopy.Margin = new Thickness(copyLeft, 0, copyRight, 0);
                stackPanel.SetValue(Grid.ColumnProperty, column);
            }

            if (textBlock.Visibility == Visibility.Collapsed) Change(Visibility.Visible, "IconLock", 7, 3, 2, 5, 1);
            else Change(Visibility.Collapsed, "IconShowPassword", 10, 7, 7, 10, 0);
        }

        private void AutoHidePassword()
        {
            foreach (var row in _showedPasswordsRows)
            {
                var textBlock = DataGridUtils.GetObject<TextBlock>(row, "tbPassword");
                var stackPanel = DataGridUtils.GetObject<StackPanel>(row, "spPassword");
                var imgShow = DataGridUtils.GetObject<Image>(row, "btShowPassword");
                var imgCopy = DataGridUtils.GetObject<Image>(row, "btCopyPassword");

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
                ShowErrBox(Strings.err_critical_title, Strings.err_critical_fileNotSupported);
                return;
            }

            OpenFile(files[0]);
        }

        private async void TbSearchData_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            async Task<bool> UserKeepsTyping()
            {
                var txt = tbSearchData.Text; // remember text
                await Task.Delay(500); // wait some
                return txt != tbSearchData.Text; // check changes
            }

            if (await UserKeepsTyping()) return;

            UnselectRow();
            _dataSearcher.SearchByDataAsync(tbSearchData.Text);
        }

        private void BtTag_OnClick(object sender, RoutedEventArgs e)
        {
            SearchByTag();
        }

        private void SearchByTagShortcut(ToggleButton button)
        {
            if (tbSearchData.Visibility != Visibility.Collapsed) return;
            button.IsChecked = button.IsChecked != true;
            SearchByTag();
        }

        private void SearchByTagShortcut()
        {
            if (tbSearchData.Visibility != Visibility.Collapsed) return;
            BtTagSetChecked(false, btRed, btGreen, btBlue, btYellow, btPurple);
            SearchByTag();
        }

        private void SearchByTag()
        {
            UnselectRow();
            BtSearchSetState(CheckTags(btRed, btGreen, btBlue, btYellow, btPurple));
            _dataSearcher.SearchByTagAsync(btRed, btGreen, btBlue, btYellow, btPurple);
        }

        private static bool CheckTags(params ToggleButton[] buttons)
        {
            return buttons.Any(button => button.IsChecked == true);
        }

        private void BtSearch_OnClick(object sender, RoutedEventArgs e)
        {
            if (_dataSearcher.SearchIsRunning || tbSearchData.Visibility == Visibility.Visible) ResetSearch();
            else SearchByData();
        }

        private void ResetSearch()
        {
            UnselectRow();
            BtTagSetVisibility(Visibility.Visible, btRed, btGreen, btBlue, btYellow, btPurple);
            BtTagSetChecked(false, btRed, btGreen, btBlue, btYellow, btPurple);
            BtSearchSetState(false);
            tbSearchData.Visibility = Visibility.Collapsed;
            tbSearchData.Text = "";
            _dataSearcher.SearchByTagAsync(btRed, btGreen, btBlue, btYellow, btPurple);
        }

        private void SearchByData()
        {
            BtTagSetVisibility(Visibility.Collapsed, btRed, btGreen, btBlue, btYellow, btPurple);
            tbSearchData.Visibility = Visibility.Visible;
            tbSearchData.Focus();
            BtSearchSetState(true);
        }

        private void SearchByDataShortcut()
        {
            if (tbSearchData.Visibility == Visibility.Collapsed)
            {
                ResetSearch();
                SearchByData();
            }
            else ResetSearch();
        }

        private static void BtTagSetVisibility(Visibility visibility, params ToggleButton[] buttons)
        {
            foreach (var button in buttons) button.Visibility = visibility;
        }

        private static void BtTagSetChecked(bool isChecked, params ToggleButton[] buttons)
        {
            foreach (var button in buttons) button.IsChecked = isChecked;
        }

        private void BtSearchSetState(bool isActive)
        {
            btSearch.Content = isActive ? Strings.bt_close : Strings.bt_search;
            btSearch.ToolTip = isActive ? null : Strings.bt_search_tip;
        }

        private static void BtLogPassSetState(bool isActive, Button button)
        {
            button.Content = isActive ? Strings.bt_abort : Strings.bt_logPass;
            button.ToolTip = isActive ? "Ctrl+D" : Strings.bt_logPass_tip;
        }

        private void UnselectRow()
        {
            gridMain.UnselectAll();
            gridMain.CurrentCell = new DataGridCellInfo();
            lpSysRowID = -2;
        }

        private void HandleUiWidgets()
        {
            ElementSetEnabled(isOpen, egTags, egLogPass, btnEdit, btnDelete);
            ResetSearch();
            btnAdd.Content = isOpen ? Strings.bt_add : Strings.bt_createTable;
            btnAdd.ToolTip = isOpen ? Strings.bt_add_tip : Strings.bt_createTable_tip;
        }

        private static void ElementSetEnabled(bool isEnabled, params UIElement[] elements)
        {
            foreach (var element in elements) element.IsEnabled = isEnabled;
        }

        private void ShowErrBox(string title, string msg) =>
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);

        private void LogPassInfo_OnClick(object sender, RoutedEventArgs e)
        {
            var logPassInfoWindow = new LogPassInfoWindow
            {
                Owner = this
            };
            logPassInfoWindow.ShowDialog();
        }

        private bool VerifyFileName(string filePath)
        {
            var names = filePath.Split('\\');
            var fileName = names[names.Length - 1];
            fileName = fileName.Remove(fileName.Length - 10); // remove file extension
            var nameVerifierRes = Verifier.VerifyFileName(fileName);

            if (nameVerifierRes == 3)
            {
                ShowErrBox(Strings.err_critical_title, Strings.err_fileName_spaceChar);
                return false;
            }

            if (0 < nameVerifierRes && nameVerifierRes < 5)
            {
                ShowErrBox(Strings.err_critical_title, Strings.err_fileName_invalid);
                return false;
            }

            if (nameVerifierRes == 5)
            {
                ShowErrBox(Strings.err_critical_title, Strings.err_fileName_tooLong);
                return false;
            }

            return true;
        }

        private void GridMain_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0) AutoHidePassword();
        }

        private void TbSearchData_OnLostFocus(object sender, RoutedEventArgs e)
        {
            UnselectRow();
        }
    }
}