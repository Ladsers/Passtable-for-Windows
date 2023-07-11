using System.Diagnostics;
using System.Windows;
using Passtable.Components;

namespace Passtable
{
    public partial class UpdateInfoWindow
    {
        public UpdateInfoWindow()
        {
            InitializeComponent();
            WindowBackground.SetBackground(this);
        }

        private void GoToWebSite_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.passtable.com/download");
        }
    }
}