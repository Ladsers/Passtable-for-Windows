using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Passtable
{
    /// <summary>
    /// Логика взаимодействия для MasterPasswordWindow.xaml
    /// </summary>
    public partial class MasterPasswordWindow : Window
    {
        ToolTip tt;
        DispatcherTimer timer;
        public bool invalidPassword = false;
        public bool withoutChange = false;
        public MasterPasswordWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Point coords = pbPassword.PointToScreen(new Point(0, 0));
            tt = new ToolTip
            {
                Placement = System.Windows.Controls.Primitives.PlacementMode.Relative,
                HorizontalOffset = coords.X,
                VerticalOffset = coords.Y + 28,
                Background = new SolidColorBrush(Color.FromRgb(255, 243, 184)),
                Content = "Only latin letters, numbers or symbols",
                IsOpen = false
            };
            timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1, 750), IsEnabled = false };
            timer.Tick += new EventHandler(delegate (object timerSender, EventArgs timerArgs)
            {
                tt.IsOpen = false;
                timer.IsEnabled = false;
            });
            if (invalidPassword)
            {
                ToolTip tIn = new ToolTip
                {
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Relative,
                    HorizontalOffset = coords.X,
                    VerticalOffset = coords.Y + 28,
                    Background = new SolidColorBrush(Color.FromRgb(255, 179, 157)),
                    Content = "Invalid password",
                    IsOpen = true
                };
                DispatcherTimer timerIn = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1, 750), IsEnabled = true };
                timerIn.Tick += new EventHandler(delegate (object timerSender, EventArgs timerArgs)
                {
                    tIn.IsOpen = false;
                    timerIn = null;
                });
            }
        }

        private void cbShowPassword_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            tbPassword.Text = pbPassword.Password;
            pbPassword.Visibility = Visibility.Hidden;
            tbPassword.Visibility = Visibility.Visible;
        }

        private void cbShowPassword_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            pbPassword.Visibility = Visibility.Visible;
            tbPassword.Visibility = Visibility.Hidden;
        }

        private void cbShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            cbShowPassword.IsChecked = false;
        }

        private void pbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (pbPassword.Password.Length == 0 || pbPassword.Password.Length > 32) btnEnter.IsEnabled = false;
            else btnEnter.IsEnabled = true;

            for (int i = 0; i < pbPassword.Password.Length; i++)
            {
                if (Encoding.UTF8.GetByteCount(new char[] { pbPassword.Password[i] }) > 1)
                {
                    pbPassword.Password = pbPassword.Password.Remove(i, 1);
                    pbPassword.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(pbPassword, new object[] { i, 0 });

                    tt.IsOpen = true;
                    timer.IsEnabled = true;
                    timer.Stop();
                    timer.Start();
                }
            }
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnWithoutChange_Click(object sender, RoutedEventArgs e)
        {
            withoutChange = true;
            DialogResult = true;
        }
    }
}