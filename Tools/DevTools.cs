using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using HandyControl.Controls;

namespace Passtable.Tools
{
    public static class DevTools
    {
        public static string AddDevelopInfo()
        {
            var branch = GitUtils.GetBranchName();

            var versionTag = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            versionTag = versionTag.Remove(versionTag.Length - 2);

            switch (branch)
            {
                case "developing":
                    return $" [{versionTag} Dev] // Do not use on real data! //";
                case "beta":
                    return $" [{versionTag} Beta]";
                default:
                    return "";
            }
        }

        public static void MarkIcon(Window window)
        {
            var branch = GitUtils.GetBranchName();
            if (branch != "developing" && branch != "beta") return;

            var projectDir = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName ?? "../..";
            var iconUri = new Uri($"{projectDir}/icon_dev.ico", UriKind.RelativeOrAbsolute);
            window.Icon = BitmapFrame.Create(iconUri);
        }
    }
}