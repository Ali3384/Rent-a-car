using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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

namespace Rent_a_car.pages.cars
{
 
    public partial class CarsInfoChange
    {
        string connectionString = Properties.Settings.Default.ConnectionString;
        DateTime insurancedate;
        DateTime selecteddate;
        public CarsInfoChange()
        {
            InitializeComponent();
            insurancedate = Convert.ToDateTime(Properties.Settings.Default.CarsSelectedInsurance);
            insurance.SelectedDate = insurancedate;
        }

        private void changecarinfoaddbtn_Click(object sender, RoutedEventArgs e)
        {
           
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
                string insertQuery = "UPDATE cars SET Cars_Insurance = @insurance WHERE Cars_ID = @id";
                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@insurance", insurance.SelectedDate.Value.ToString("yyyy-MM-dd"));
                    insertCmd.Parameters.AddWithValue("@id", Properties.Settings.Default.CarsSelectedID);
                    insertCmd.ExecuteNonQuery();
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating data: " + ex.Message);
            }
        }
    }
}
