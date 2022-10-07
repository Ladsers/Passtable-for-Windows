using System;
using System.Windows;
using Passtable.Components;

namespace Passtable
{
    public partial class DeleteConfirmWindow
    {
        public DeleteConfirmWindow()
        {
            InitializeComponent();
            WindowBackground.SetBackground(this);
        }

        private void btnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DeleteConfirmWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!WindowBackground.CheckWin11()) return;
            MaxHeight = Height;
            MinHeight = Height;
        }

        private void DeleteConfirmWindow_OnLayoutUpdated(object sender, EventArgs e)
        {
            if (!WindowBackground.CheckWin11()) SizeToContent = SizeToContent.Height;
        }
    }
}