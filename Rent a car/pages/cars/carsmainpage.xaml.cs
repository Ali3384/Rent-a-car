using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using Rent_a_car.pages.other_pages;
using Rent_a_car.pages.rent;

namespace Rent_a_car.pages.cars
{
    public partial class carsmainpage : Page
    {
        string carscomboboxselection;
        string carsfiltertext;
        DataTable cars = new DataTable();
        string connectionString = Properties.Settings.Default.ConnectionString;
        public carsmainpage()
        {
            InitializeComponent();
            fillCarsTable();
        }
        private void fillCarsTable()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Cars_ID,Cars_Brand,Cars_Model,Cars_Year,Cars_Vin,Cars_No, Cars_Fuel, Cars_Status, Cars_Insurance FROM cars WHERE Cars_Status != 'deleted'";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);

                    adapter.Fill(cars);
                    cars.Columns["Cars_ID"].ColumnName = "ID";
                    cars.Columns["Cars_Brand"].ColumnName = "Marka pojazdu";
                    cars.Columns["Cars_Model"].ColumnName = "Model pojazdu";
                    cars.Columns["Cars_Year"].ColumnName = "Rok produkcji";
                    cars.Columns["Cars_Vin"].ColumnName = "Vin No";
                    cars.Columns["Cars_No"].ColumnName = "Numer rejestracyjny";
                    cars.Columns["Cars_Fuel"].ColumnName = "Rodzaj paliwa";
                    cars.Columns["Cars_Status"].ColumnName = "Status";
                    cars.Columns["Cars_Insurance"].ColumnName = "Ubezpieczenia";
                    carsdatagrid.ItemsSource = cars.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while filling data: " + ex.Message);
            }
        }
        private void carsfilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            carsfiltertext = carsfilter.Text;
            DataView dataView = cars.DefaultView;
            if (string.IsNullOrEmpty(carsfiltertext))
            {
                dataView.RowFilter = string.Empty;
            }
            else if (!string.IsNullOrEmpty(carscomboboxselection))
            {
                try
                {
                    string escapedFilterText = carsfiltertext.Replace("'", "''");
                    string columnName = carscomboboxselection.Contains(" ") ? $"[{carscomboboxselection}]" : carscomboboxselection;
                    dataView.RowFilter = $"{columnName} LIKE '%{escapedFilterText}%'";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while filtering data: " + ex.Message);
                }
            }
            carsdatagrid.ItemsSource = dataView;
        }
        private void carscombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            carscomboboxselection = ((ComboBoxItem)carscombobox.SelectedItem).Content.ToString();
        }

        private void carsdatagrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
                DataRowView rowView = e.Row.Item as DataRowView;
                if (rowView != null)
                {
                    string insurancedateStr = rowView["Ubezpieczenia"].ToString();

                    if (DateTime.TryParse(insurancedateStr, out DateTime date))
                    {
                        if ((date - DateTime.Now).TotalDays < 10)
                        {

                        e.Row.Background = new SolidColorBrush(Color.FromRgb(255, 127, 127));

                        }
                    }
                }
        }


        


        private void addcar_Click(object sender, RoutedEventArgs e)
        {
            
            carsaddwindow carsaddwindow = new carsaddwindow();
            carsaddwindow.ShowDialog();
            updateCar();
        }
        public void updateCar()
        {
            cars.Clear();
            cars.Columns["ID"].ColumnName = "Cars_ID";
            cars.Columns["Marka pojazdu"].ColumnName = "Cars_Brand";
            cars.Columns["Model pojazdu"].ColumnName = "Cars_Model";
            cars.Columns["Rok produkcji"].ColumnName = "Cars_Year";
            cars.Columns["Vin No"].ColumnName = "Cars_Vin";
            cars.Columns["Numer rejestracyjny"].ColumnName = "Cars_No";
            cars.Columns["Rodzaj paliwa"].ColumnName = "Cars_Fuel";
            cars.Columns["Status"].ColumnName = "Cars_Status";
            cars.Columns["Ubezpieczenia"].ColumnName = "Cars_Insurance";
            fillCarsTable();
            carsdatagrid.Items.Refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            updateCar();
        }

        private void deletecar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Czy chcesz usunąć wybrany pojazd ?",
                    "Czy jesteś pewien ?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (carsdatagrid.SelectedItem != null)
                {
                    // Cast the selected item to a DataRowView to access its columns
                    var selectedItem = carsdatagrid.SelectedItem as DataRowView;

                    if (selectedItem != null)
                    {
                        // Get the value of the "Numer Vin" column
                        string numerVin = selectedItem["ID"].ToString();
                        try
                        {
                            MySqlConnection connection = new MySqlConnection(connectionString);
                            connection.Open();

                            string updateQuery = "DELETE FROM cars WHERE Cars_ID = @id";
                            using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection))
                            {
                                updateCmd.Parameters.AddWithValue("@id", numerVin);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error while delete car");
                        }
                    }
                    updateCar();
                }
            }
        }

        private void carsdatagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (carsdatagrid.SelectedItems.Count > 0)
            {
                foreach (DataRowView row in carsdatagrid.SelectedItems)
                {
                    System.Data.DataRow myRow = row.Row;
                    Properties.Settings.Default.CarsSelectedInsurance = myRow["Ubezpieczenia"].ToString();
                    Properties.Settings.Default.CarsSelectedID =  myRow["ID"].ToString();
                    Properties.Settings.Default.Save();
                }
                CarsInfoChange carsInfoChange = new CarsInfoChange();
                carsInfoChange.ShowDialog();
                updateCar();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (carsdatagrid.SelectedItems.Count > 0)
            {
                foreach (DataRowView row in carsdatagrid.SelectedItems)
                {
                    System.Data.DataRow myRow = row.Row;
                    Properties.Settings.Default.CarSelectedCarPlate = myRow["Numer rejestracyjny"].ToString();
                    Properties.Settings.Default.Save();
                }
                CarImages carImages = new CarImages();
                carImages.ShowDialog();
            }
        }
    }
}
