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

namespace Rent_a_car.pages.rent
{
    /// <summary>
    /// Логика взаимодействия для rentaddinfo.xaml
    /// </summary>
    public partial class rentaddinfo
    {
        public rentaddinfo()
        {
            InitializeComponent();
            Properties.Settings.Default.AddInfo = "Bez dodatkowych uwag";
        }

        private void infoaddbtn_Click(object sender, RoutedEventArgs e)
        {
            if(addinfo.Text.Length > 0)
            {
                Properties.Settings.Default.AddInfo = addinfo.Text;
                this.DialogResult = true;
            }
            else
            {
                Properties.Settings.Default.AddInfo = "Bez dodatkowych uwag";
                this.DialogResult = false;
            }
            Close();
        }
    }
}
