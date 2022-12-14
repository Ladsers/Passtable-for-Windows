using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Tools;
using Microsoft.Win32;

namespace Passtable.Components
{
    public class WindowBackground
    {
        public static bool CheckWin11()
        {
            const string keyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            const string valueName = "CurrentBuildNumber";
            var buildValue = Registry.GetValue(keyName, valueName, "").ToString();
            return int.Parse(buildValue) >= 22000;
        }

        public static void SetBackground(Window window)
        {
            if (CheckWin11())
            {
                window.SystemBackdropType = BackdropType.Mica;
                FixBackground(window);
            }
            else window.Background = new SolidColorBrush(Colors.White);
        }
        
        private static void FixBackground(Window window)
        {
            // fixing HandyControls Mica material issue
            var hwnd = new WindowInteropHelper(window).Handle;
            SetWindowLong(hwnd, GwlStyle, GetWindowLong(hwnd, GwlStyle) & ~WsSysMenu);
        }
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        private const int GwlStyle = -16;
        private const int WsSysMenu = 0x80000;
    }
}