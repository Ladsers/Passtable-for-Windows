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
                case EditErrorKey.BadItem:
                    return Strings.err_edit_itemMustContain;
                case EditErrorKey.BadData:
                    return Strings.err_edit_dataInvalidChars;
                case EditErrorKey.BadPrimaryPassword:
                    return "BadPrimaryPassword";
                case EditErrorKey.BadFileName:
                    return "BadFileName";
                case EditErrorKey.PasswordsDoNotMatch:
                    return Strings.err_edit_passwordsDoNotMatch;
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
            _label.Content = GetErrorMsg(key);
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