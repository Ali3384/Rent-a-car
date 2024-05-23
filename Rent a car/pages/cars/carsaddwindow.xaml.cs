using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Rent_a_car.pages.cars
{
    /// <summary>
    /// Логика взаимодействия для carsaddwindow.xaml
    /// </summary>
    public partial class carsaddwindow
    {
        string connectionString = Properties.Settings.Default.ConnectionString;
        string brand;
        string model;
        string vinno;
        string plateno;
        int year;
        string fueltype;
        string insurance;
        string servicedate;
        string lpg;
        public carsaddwindow()
        {
            InitializeComponent();
        }

        private void fueltypecmbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxItem)fueltypecmbx.SelectedItem).Content.ToString() == "LPG/Benzin" || ((ComboBoxItem)fueltypecmbx.SelectedItem).Content.ToString() == "LPG/Hybrid")
            {
                lpgdatepicker.IsEnabled = true;
            }
            else
            {
                lpgdatepicker.IsEnabled = false;
            }
        }

        private void carsaddbtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(brandtxtbox.Text) &&
                !string.IsNullOrWhiteSpace(modeltxt.Text) &&
                !string.IsNullOrWhiteSpace(vinnotxt.Text) &&
                !string.IsNullOrWhiteSpace(platenotxt.Text) &&
                !string.IsNullOrWhiteSpace(yeartxt.Text) &&
                fueltypecmbx.SelectedItem != null &&
                insurancedatepicker.SelectedDate.HasValue &&
                servicedatepicker.SelectedDate.HasValue)
            {
                try
                {
                    string brand = brandtxtbox.Text;
                    string model = modeltxt.Text;
                    string vinno = vinnotxt.Text;
                    string plateno = platenotxt.Text;
                    int year = int.Parse(yeartxt.Text);
                    string fueltype = ((ComboBoxItem)fueltypecmbx.SelectedItem).Content.ToString();
                    string insurance = insurancedatepicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                    string servicedate = servicedatepicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                    string lpg = lpgdatepicker.IsEnabled && lpgdatepicker.SelectedDate.HasValue ? lpgdatepicker.SelectedDate.Value.ToString("yyyy-MM-dd") : "LPGsiz";

                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string insertQuery = "INSERT INTO cars (Cars_Brand, Cars_Model, Cars_Year, Cars_Vin, Cars_No, Cars_Fuel, Cars_Status, Cars_Insurance, Cars_ServiceDate, Cars_LPG_Date) " +
                                             "VALUES (@brand, @model, @year, @vin, @plateno, @fueltype, 'Olinmagan', @insurance, @servicedate, @lpg)";

                        using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@brand", brand);
                            insertCmd.Parameters.AddWithValue("@model", model);
                            insertCmd.Parameters.AddWithValue("@year", year);
                            insertCmd.Parameters.AddWithValue("@vin", vinno);
                            insertCmd.Parameters.AddWithValue("@plateno", plateno);
                            insertCmd.Parameters.AddWithValue("@fueltype", fueltype);
                            insertCmd.Parameters.AddWithValue("@insurance", insurance);
                            insertCmd.Parameters.AddWithValue("@servicedate", servicedate);
                            insertCmd.Parameters.AddWithValue("@lpg", lpg);

                            int rowsAffected = insertCmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Data inserted successfully.");
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("No data inserted.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while inserting data: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Iltimos barcha ma'lumot kiriting.", "Hato", MessageBoxButton.OK);
            }
        }


        private void yeartxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
