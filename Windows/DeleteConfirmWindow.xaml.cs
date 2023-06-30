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
    }
}