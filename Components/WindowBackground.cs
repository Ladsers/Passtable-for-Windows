using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Tools;
using Microsoft.Win32;

namespace Passtable.Components
{
    public class WindowBackground
    {
        private static bool CheckWin11()
        {
            const string keyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            const string valueName = "CurrentBuildNumber";
            var buildValue = Registry.GetValue(keyName, valueName, "").ToString();
            return int.Parse(buildValue) >= 22000;
        }

        public static void SetBackground(Window window)
        {
            if (CheckWin11()) window.SystemBackdropType = BackdropType.Mica;
            else window.Background = new SolidColorBrush(Colors.White);
        }
    }
}