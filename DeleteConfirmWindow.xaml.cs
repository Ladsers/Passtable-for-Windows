using System;
using System.Windows;
using System.Windows.Input;

namespace Passtable
{
    public partial class DeleteConfirmWindow : Window
    {
        public DeleteConfirmWindow()
        {
            InitializeComponent();
        }

        private void btnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DeleteConfirmWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) DialogResult = false;
        }
    }
}