using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Passtable.Components;
using Passtable.Containers;
using PasswordBox = HandyControl.Controls.PasswordBox;

namespace Passtable
{
    /// <summary>
    /// Логика взаимодействия для EditGridWindow.xaml
    /// </summary>
    public partial class EditGridWindow
    {
        public int SelectedTag { private set; get; }

        private readonly EditErrorWindow _error;
        private readonly ToggleButton[] tagButtons;

        public EditGridWindow()
        {
            InitializeComponent();
            WindowBackground.SetBackground(this);
            _error = new EditErrorWindow(lbError);

            tagButtons = new[] { btNone, btRed, btGreen, btBlue, btYellow, btPurple };
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void NotifyError(bool isMsg)
        {
            if (!Verifier.VerifyData(tbNote.Text, tbLogin.Text, pbPassword.Password))
            {
                if (isMsg) _error.Show(EditErrorKey.DataInvalidChars);
                else btnSave.IsEnabled = false;
                return;
            }

            if (!Verifier.VerifyItem(tbNote.Text, tbLogin.Text, pbPassword.Password))
            {
                if (isMsg) _error.Show(EditErrorKey.InvalidItem);
                else btnSave.IsEnabled = false;
                return;
            }

            if (pbPassword.Password != null && pbConfirm.Password != null &&
                pbConfirm.Password.Length != 0 && pbPassword.Password != pbConfirm.Password)
            {
                if (isMsg) _error.Show(EditErrorKey.PasswordsDoNotMatch);
                else btnSave.IsEnabled = false;
                return;
            }

            _error.Show(EditErrorKey.Ok);
            btnSave.IsEnabled = true;
        }

        private async void CheckError(TextBox textBox)
        {
            _error.Hide();
            NotifyError(false);

            async Task<bool> UserKeepsTyping()
            {
                var temp = textBox.Text;
                await Task.Delay(500);
                return temp != textBox.Text;
            }

            if (!await UserKeepsTyping()) NotifyError(true);
        }

        private async void CheckError(PasswordBox passwordBox)
        {
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


        private void tbNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckError(tbNote);
        }

        private void tbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckError(tbLogin);
        }

        private void EditGridWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) DialogResult = false;
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.D0 || e.Key == Key.OemTilde) SelectTag(0);
                if (e.Key == Key.D1) SelectTag(1);
                if (e.Key == Key.D2) SelectTag(2);
                if (e.Key == Key.D3) SelectTag(3);
                if (e.Key == Key.D4) SelectTag(4);
                if (e.Key == Key.D5) SelectTag(5);
            }
        }

        private void PbPassword_OnKeyUp(object sender, KeyEventArgs e)
        {
            CheckError(pbPassword);
        }

        private void PbConfirm_OnKeyUp(object sender, KeyEventArgs e)
        {
            CheckError(pbConfirm);
        }

        public void SelectTag(int tagId)
        {
            foreach (var tagButton in tagButtons) tagButton.IsChecked = false;
            tagButtons[tagId].IsChecked = true;
            SelectedTag = tagId;
        }

        private void BtNone_OnClick(object sender, RoutedEventArgs e) => SelectTag(0);

        private void BtRed_OnClick(object sender, RoutedEventArgs e) => SelectTag(1);

        private void BtGreen_OnClick(object sender, RoutedEventArgs e) => SelectTag(2);

        private void BtBlue_OnClick(object sender, RoutedEventArgs e) => SelectTag(3);

        private void BtYellow_OnClick(object sender, RoutedEventArgs e) => SelectTag(4);

        private void BtPurple_OnClick(object sender, RoutedEventArgs e) => SelectTag(5);

        private void EditGridWindow_OnLayoutUpdated(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Height;
        }
    }
}