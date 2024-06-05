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
        public carsaddwindow()
        {
            InitializeComponent();
        }

        private void carsaddbtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(brandtxtbox.Text) &&
                !string.IsNullOrWhiteSpace(modeltxt.Text) &&
                !string.IsNullOrWhiteSpace(vinnotxt.Text) &&
                !string.IsNullOrWhiteSpace(platenotxt.Text) &&
                !string.IsNullOrWhiteSpace(yeartxt.Text) &&
                fueltypecmbx.SelectedItem != null &&
                insurancedatepicker.SelectedDate.HasValue)
            {
                Properties.Settings.Default.SelectedCarPlate = platenotxt.Text;
                carsimagesxaml carsimagesxaml = new carsimagesxaml();
                carsimagesxaml.ShowDialog();
                try
                {
                    string brand = brandtxtbox.Text;
                    string model = modeltxt.Text;
                    string vinno = vinnotxt.Text;
                    string plateno = platenotxt.Text;
                    int year = int.Parse(yeartxt.Text);
                    string fueltype = ((ComboBoxItem)fueltypecmbx.SelectedItem).Content.ToString();
                    string insurance = insurancedatepicker.SelectedDate.Value.ToString("yyyy-MM-dd");

                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string insertQuery = "INSERT INTO cars (Cars_Brand, Cars_Model, Cars_Year, Cars_Vin, Cars_No, Cars_Fuel, Cars_Status, Cars_Insurance, Cars_Images_Sources, Cars_Image) " +
                                             "VALUES (@brand, @model, @year, @vin, @plateno, @fueltype, 'Nie wynajęte', @insurance, @sources, @image)";

                        using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@brand", brand);
                            insertCmd.Parameters.AddWithValue("@model", model);
                            insertCmd.Parameters.AddWithValue("@year", year);
                            insertCmd.Parameters.AddWithValue("@vin", vinno);
                            insertCmd.Parameters.AddWithValue("@plateno", plateno);
                            insertCmd.Parameters.AddWithValue("@fueltype", fueltype);
                            insertCmd.Parameters.AddWithValue("@insurance", insurance);
                            insertCmd.Parameters.AddWithValue("@sources", Properties.Settings.Default.CarSelectedImages);
                            insertCmd.Parameters.AddWithValue("@image", Properties.Settings.Default.CarSelectedMainImageName);
                            int rowsAffected = insertCmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
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
                MessageBox.Show("Proszę wypełnić wszystkie pola.", "Błąd", MessageBoxButton.OK);
            }
        }


        private void yeartxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
