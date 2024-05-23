using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using MySql.Data.MySqlClient;

namespace Rent_a_car.pages.rent
{
    /// <summary>
    /// Логика взаимодействия для rentaddwindow.xaml
    /// </summary>
    public partial class rentaddwindow
    {
        string connectionString = Properties.Settings.Default.ConnectionString;
        
        public rentaddwindow()
        {
            InitializeComponent();
        }

        
    }
}
