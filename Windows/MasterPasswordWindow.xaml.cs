using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Passtable.Components;
using Passtable.Containers;
using Passtable.Resources;
using PasswordBox = HandyControl.Controls.PasswordBox;

namespace Passtable
{
    public partial class MasterPasswordWindow
    {
        public bool WithoutChange { get; private set; }

        private bool _saveMode;
        private bool _firstCheck = true;

        private readonly EditErrorWindow _error;

        public MasterPasswordWindow()
        {
            InitializeComponent();
            WindowBackground.SetBackground(this);
            _error = new EditErrorWindow(lbError);
            pbPassword.Focus();
        }

        public void PrepareForSave(Askers.Mode mode)
        {
            btEnter.Content = Strings.bt_save;
            pbConfirm.Visibility = Visibility.Visible;
            _saveMode = true;
            if (mode != Askers.Mode.SaveAs) return;

            btEnter.SetValue(Grid.ColumnSpanProperty, 1);
            btWithoutChange.Visibility = Visibility.Visible;
        }

        public void ShowMsgIncorrectPassword() => lbError.Visibility = Visibility.Visible;

        private void btEnter_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnWithoutChange_Click(object sender, RoutedEventArgs e)
        {
            WithoutChange = true;
            DialogResult = true;
        }

        private void MasterPasswordWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) DialogResult = false;
        }

        private void NotifyError(bool isMsg)
        {
            var res = Verifier.VerifyPrimary(pbPassword.Password);
            switch (res)
            {
                case 1:
                    if (isMsg) _error.Show(EditErrorKey.PrimaryPasswordEmpty);
                    else btEnter.IsEnabled = false;
                    return;
                case 2:
                    if (isMsg) _error.Show(EditErrorKey.PrimaryPasswordInvalidChars);
                    else btEnter.IsEnabled = false;
                    return;
                case 3:
                    if (isMsg) _error.Show(EditErrorKey.PrimaryPasswordSlash);
                    else btEnter.IsEnabled = false;
                    return;
                case 4:
                    if (isMsg) _error.Show(EditErrorKey.PrimaryPasswordTooLong);
                    else btEnter.IsEnabled = false;
                    return;
            }

            if (_saveMode && pbPassword.Password != pbConfirm.Password)
            {
                if (isMsg) _error.Show(EditErrorKey.PasswordsDoNotMatch);
                else btEnter.IsEnabled = false;
                return;
            }

            _error.Show(EditErrorKey.Ok);
            btEnter.IsEnabled = true;
        }

        private async void CheckError(PasswordBox passwordBox)
        {
            if (_firstCheck && pbPassword.Password.Length == 0) return;
            _firstCheck = false;
            _error.Hide();
            NotifyError(false);

            async Task<bool> UserKeepsTyping()
            {
                var temp = passwordBox.Password;
                await Task.Delay(500);
                return temp != passwordBox.Password;
            }

            if (!await UserKeepsTyping()) NotifyError(true);
        }

        private void PbPassword_OnKeyUp(object sender, KeyEventArgs e)
        {
            CheckError(pbPassword);
        }

        private void PbConfirm_OnKeyUp(object sender, KeyEventArgs e)
        {
            CheckError(pbConfirm);
        }

        private void MasterPasswordWindow_OnContentRendered(object sender, EventArgs e)
        {
            // fixing the HandyControls PasswordBox issue: focus is not set.
            if (Keyboard.PrimaryDevice.ActiveSource == null) return;
            var tabPressEventArgs =
                new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Tab)
                    { RoutedEvent = Keyboard.KeyDownEvent };
            InputManager.Current.ProcessInput(tabPressEventArgs);
        }

        private void MasterPasswordWindow_OnLayoutUpdated(object sender, EventArgs e)
        {
            // instead of "no resize" (HandyControls Mica material issue)
            SizeToContent = SizeToContent.Height;
        }
    }
}