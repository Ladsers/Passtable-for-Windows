using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Passtable.Components;

namespace Passtable
{
    /// <summary>
    /// Логика взаимодействия для EditGridWindow.xaml
    /// </summary>
    public partial class EditGridWindow : Window
    {
        public EditGridWindow()
        {
            InitializeComponent();
        }

        private void cbShowPassword_Click(object sender, RoutedEventArgs e)
        {
            PasswordHandler.ShowHidePassword(pbPassword, tbPassword);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            pbPassword.Password = PasswordHandler.GetCorrectData(pbPassword, tbPassword);
            DialogResult = true;
        }

        private void tbNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbNote.Text != "" || tbLogin.Text != "" && pbPassword.Password != "") btnSave.IsEnabled = true;
            else btnSave.IsEnabled = false;
            if (tbNote.Text != "" || tbLogin.Text != "" && tbPassword.Text != "") btnSave.IsEnabled = true;
            else btnSave.IsEnabled = false;
        }

        private void tbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbNote.Text != "" || tbLogin.Text != "" && pbPassword.Password != "") btnSave.IsEnabled = true;
            else btnSave.IsEnabled = false;
            if (tbNote.Text != "" || tbLogin.Text != "" && tbPassword.Text != "") btnSave.IsEnabled = true;
            else btnSave.IsEnabled = false;
        }

        private void pbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (tbNote.Text != "" || tbLogin.Text != "" && pbPassword.Password != "") btnSave.IsEnabled = true;
            else btnSave.IsEnabled = false;
        }
        
        private void tbPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbNote.Text != "" || tbLogin.Text != "" && tbPassword.Text != "") btnSave.IsEnabled = true;
            else btnSave.IsEnabled = false;
        }

        private void EditGridWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.W)
            {
                cbShowPassword.IsChecked = !cbShowPassword.IsChecked;
                PasswordHandler.ShowHidePassword(pbPassword, tbPassword);
            }
            if (e.Key == Key.Escape) DialogResult = false;
        }
    }
}