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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rent_a_car.pages.other_pages
{
    /// <summary>
    /// Логика взаимодействия для Password.xaml
    /// </summary>
    public partial class Password
    {
        private string _password = string.Empty;
        public Password()
        {
            InitializeComponent();
            Properties.Settings.Default.passed = false;
            Properties.Settings.Default.Save();
        }
        private void PasswordTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordTextBox.Password != null)
            {
                _password = PasswordTextBox.Password;
            }
            else
            {
                MessageBox.Show("Wprowadz haslo !!!");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_password == Properties.Settings.Default.password)
            {
                Properties.Settings.Default.passed = true;
                Properties.Settings.Default.Save();
                this.Close();
            }
            else
            {
                AnimateLabelHiding(PasswordLabel);
            }
        }
        private void AnimateLabelHiding(UIElement element)
        {
            PasswordLabel.Visibility = Visibility.Visible;
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(2))
            };
            animation.Completed += (s, e) => element.Visibility = Visibility.Hidden;
            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void Button_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_password == Properties.Settings.Default.password)
                {
                    Properties.Settings.Default.passed = true;
                    Properties.Settings.Default.Save();
                    this.Close();
                }
                else
                {
                    AnimateLabelHiding(PasswordLabel);
                }
            }

        }
    }
}
