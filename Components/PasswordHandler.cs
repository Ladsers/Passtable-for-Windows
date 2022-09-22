using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Passtable.Components
{
    public static class PasswordHandler
    {
        public static void ShowHidePassword(PasswordBox passwordBox, TextBox textBox)
        {
            if (passwordBox.Visibility == Visibility.Visible)
            {
                textBox.Text = passwordBox.Password;
                var needFocus = passwordBox.IsKeyboardFocused;
                passwordBox.Visibility = Visibility.Hidden;
                textBox.Visibility = Visibility.Visible;
                if (!needFocus) return;
                textBox.CaretIndex = textBox.Text.Length;
                Keyboard.Focus(textBox);
            }
            else
            {
                passwordBox.Password = textBox.Text;
                var needFocus = textBox.IsKeyboardFocused;
                textBox.Visibility = Visibility.Hidden;
                passwordBox.Visibility = Visibility.Visible;
                if (!needFocus) return;
                passwordBox.GetType()
                    .GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(passwordBox, new object[] { passwordBox.Password.Length, 0 });
                Keyboard.Focus(passwordBox);
            }
        }

        public static string GetCorrectData(PasswordBox passwordBox, TextBox textBox)
        {
            return passwordBox.Visibility == Visibility.Visible ? passwordBox.Password : textBox.Text;
        }
    }
}