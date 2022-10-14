using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Passtable.Containers;
using Passtable.Resources;

namespace Passtable.Components
{
    public class EditErrorWindow
    {
        private readonly Label _label;

        public EditErrorWindow(Label label)
        {
            _label = label;
        }

        private static string GetErrorMsg(EditErrorKey key)
        {
            switch (key)
            {
                case EditErrorKey.InvalidItem:
                    return Strings.err_edit_itemMustContain;
                case EditErrorKey.DataInvalidChars:
                    return Strings.err_edit_dataInvalidChars;
                case EditErrorKey.PrimaryPasswordEmpty:
                    return Strings.err_primaryPassword_empty;
                case EditErrorKey.PasswordsDoNotMatch:
                    return Strings.err_edit_passwordsDoNotMatch;
                case EditErrorKey.PrimaryPasswordInvalidChars:
                    return Strings.err_primaryPassword_invalidChars +
                           Verifier.GetPrimaryAllowedChars(Strings.key_space);
                case EditErrorKey.PrimaryPasswordSlash:
                    return Strings.err_primaryPassword_slashChar;
                case EditErrorKey.PrimaryPasswordTooLong:
                    return Strings.err_primaryPassword_tooLong;
                case EditErrorKey.Ok:
                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
        }

        public void Show(EditErrorKey key)
        {
            if (key == EditErrorKey.Ok)
            {
                Hide();
                return;
            }

            _label.Visibility = Visibility.Visible;
            _label.Content = new TextBlock { Text = GetErrorMsg(key), TextWrapping = TextWrapping.Wrap };
            var opacityAnimation =
                new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(0.5)), FillBehavior.HoldEnd);
            _label.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        }

        public void Hide()
        {
            _label.Visibility = Visibility.Collapsed;
            var opacityAnimation =
                new DoubleAnimation(0.0, 0.0, new Duration(TimeSpan.FromSeconds(0.0)), FillBehavior.HoldEnd);
            _label.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        }
    }
}