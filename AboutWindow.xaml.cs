using System.Reflection;
using System.Windows;
using Passtable.Components;
using Passtable.Resources;

namespace Passtable
{
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
            WindowBackground.SetBackground(this);

            var versionTag = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            versionTag = versionTag.Remove(versionTag.Length - 2);
            
            tbAbout.Text = $"{Strings.info_winApp}\n{Strings.info_version} {versionTag}\n{Strings.info_createdBy}";
        }

        private void ProjectLink_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.ladsers.com/Passtable");
        }
        
        private void RepoLink_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Ladsers/Passtable-for-Windows");
        }
    }
}