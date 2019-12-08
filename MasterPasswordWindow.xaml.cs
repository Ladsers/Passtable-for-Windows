using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Passtable
{
    /// <summary>
    /// Логика взаимодействия для MasterPasswordWindow.xaml
    /// </summary>
    public partial class MasterPasswordWindow : Window
    {
        public MasterPasswordWindow()
        {
            InitializeComponent();
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
            if (pbPassword.Password.Length == 0 || pbPassword.Password.Length > 24) btnEnter.IsEnabled = false;
            else btnEnter.IsEnabled = true;
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnWithoutChange_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
