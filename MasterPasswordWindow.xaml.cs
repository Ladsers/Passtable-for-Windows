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
        public bool saveMode = false;

        private char keyTemp = '\x00';
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
            if (pbPassword.Password.Length == 0 || pbPassword.Password.Length > 32) 
            { 
                btnEnter.IsEnabled = false;
                lbLatinInfo.Visibility = Visibility.Hidden;
            }
            else btnEnter.IsEnabled = true;

            for (int i = 0; i < pbPassword.Password.Length; i++)
            {
                if (Encoding.UTF8.GetByteCount(new char[] { pbPassword.Password[i] }) > 1)
                {
                    if (saveMode)
                    {
                        pbPassword.Password = pbPassword.Password.Remove(i, 1);
                        pbPassword.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(pbPassword, new object[] { i, 0 });

                        tt.IsOpen = true;
                        timer.IsEnabled = true;
                        timer.Stop();
                        timer.Start();
                    }
                    else
                    {
                        pbPassword.Password = pbPassword.Password.Remove(i, 1);
                        pbPassword.Password += keyTemp;
                        pbPassword.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(pbPassword, new object[] { i+1, 0 });

                        lbLatinInfo.Visibility = Visibility.Visible;
                    }
                    
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

        private void pbPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            keyTemp = KeyToChar(e.Key);
        }

        char KeyToChar(Key key)
        {

            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt) ||
                Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                return '\x00'; //skip
            }

            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool caps = (Console.CapsLock && !shift) || (!Console.CapsLock && shift);

            switch (key)
            {
                case Key.A: return (caps ? 'A' : 'a');
                case Key.B: return (caps ? 'B' : 'b');
                case Key.C: return (caps ? 'C' : 'c');
                case Key.D: return (caps ? 'D' : 'd');
                case Key.E: return (caps ? 'E' : 'e');
                case Key.F: return (caps ? 'F' : 'f');
                case Key.G: return (caps ? 'G' : 'g');
                case Key.H: return (caps ? 'H' : 'h');
                case Key.I: return (caps ? 'I' : 'i');
                case Key.J: return (caps ? 'J' : 'j');
                case Key.K: return (caps ? 'K' : 'k');
                case Key.L: return (caps ? 'L' : 'l');
                case Key.M: return (caps ? 'M' : 'm');
                case Key.N: return (caps ? 'N' : 'n');
                case Key.O: return (caps ? 'O' : 'o');
                case Key.P: return (caps ? 'P' : 'p');
                case Key.Q: return (caps ? 'Q' : 'q');
                case Key.R: return (caps ? 'R' : 'r');
                case Key.S: return (caps ? 'S' : 's');
                case Key.T: return (caps ? 'T' : 't');
                case Key.U: return (caps ? 'U' : 'u');
                case Key.V: return (caps ? 'V' : 'v');
                case Key.W: return (caps ? 'W' : 'w');
                case Key.X: return (caps ? 'X' : 'x');
                case Key.Y: return (caps ? 'Y' : 'y');
                case Key.Z: return (caps ? 'Z' : 'z');
            
                default: return '\x00';
            }
        }
    }
}