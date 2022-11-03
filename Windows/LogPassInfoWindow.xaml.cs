using System;
using System.Windows;
using Passtable.Components;

namespace Passtable
{
    public partial class LogPassInfoWindow
    {
        public LogPassInfoWindow()
        {
            InitializeComponent();
            WindowBackground.SetBackground(this);
        }

        private void LogPassInfoWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!WindowBackground.CheckWin11()) return;
            MaxHeight = Height;
            MinHeight = Height;
        }

        private void LogPassInfoWindow_OnLayoutUpdated(object sender, EventArgs e)
        {
            if (!WindowBackground.CheckWin11()) SizeToContent = SizeToContent.Height;
        }
    }
}