using System.Windows;
using Passtable.Components;
using Window = HandyControl.Controls.Window;

namespace Passtable
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            WindowBackground.SetBackground(this);
        }
    }
}