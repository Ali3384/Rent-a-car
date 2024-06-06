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
                     brand = brandtxtbox.Text;
                     model = modeltxt.Text;
                     vinno = vinnotxt.Text;
                     plateno = platenotxt.Text;
                     year = int.Parse(yeartxt.Text);
                     fueltype = ((ComboBoxItem)fueltypecmbx.SelectedItem).Content.ToString();
                    insurance = insurancedatepicker.SelectedDate.Value.ToString("yyyy-MM-dd");

                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string insertQuery = "INSERT INTO cars (Cars_Brand, Cars_Model, Cars_Year, Cars_Vin, Cars_No, Cars_Fuel, Cars_Status, Cars_Insurance, Cars_Image) " +
                                             "VALUES (@brand, @model, @year, @vin, @plateno, @fueltype, 'Nie wynajęte', @insurance, @image)";

                        using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@brand", brand);
                            insertCmd.Parameters.AddWithValue("@model", model);
                            insertCmd.Parameters.AddWithValue("@year", year);
                            insertCmd.Parameters.AddWithValue("@vin", vinno);
                            insertCmd.Parameters.AddWithValue("@plateno", plateno);
                            insertCmd.Parameters.AddWithValue("@fueltype", fueltype);
                            insertCmd.Parameters.AddWithValue("@insurance", insurance);;
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
                InsertImagesIntoDatabase(connectionString);
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

        public List<string> CarSelectedImagesList { get; private set; }

        // Method to split the string and store the result in the property
        public void SplitCarSelectedImages()
        {
            // Retrieve the string from settings
            string carSelectedImages = Properties.Settings.Default.CarSelectedImages;

            // Check if the string is null or empty
            if (string.IsNullOrEmpty(carSelectedImages))
            {
                CarSelectedImagesList = new List<string>();
                return;
            }

            // Split the string into an array using ';' as the separator
            string[] imagesArray = carSelectedImages.Split(';');

            // Convert the array to a list, removing any empty entries
            CarSelectedImagesList = imagesArray.Where(image => !string.IsNullOrWhiteSpace(image)).ToList();
        }

        // Method to insert image names into the database
        public void InsertImagesIntoDatabase(string connectionString)
        {
            // Split the images first
            SplitCarSelectedImages();

            // Ensure there are images to insert
            if (CarSelectedImagesList == null || !CarSelectedImagesList.Any())
            {
                Console.WriteLine("No images to insert.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                foreach (var imageName in CarSelectedImagesList)
                {
                    string query = "INSERT INTO images (Image_Name, Image_Type, Image_ForCar) VALUES (@ImageName, 'Main', @carplate)";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ImageName", imageName);
                        cmd.Parameters.AddWithValue("@carplate", plateno);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

    }
}
