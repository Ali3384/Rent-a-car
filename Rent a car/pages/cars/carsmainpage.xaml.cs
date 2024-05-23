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
                    string query = "SELECT Cars_ID,Cars_Brand,Cars_Model,Cars_Year,Cars_Vin,Cars_No, Cars_Fuel, Cars_Status, Cars_Insurance, Cars_ServiceDate, Cars_LPG_Date FROM cars WHERE Cars_Status != 'deleted'";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);

                    adapter.Fill(cars);
                    cars.Columns["Cars_ID"].ColumnName = "ID";
                    cars.Columns["Cars_Brand"].ColumnName = "Markasi";
                    cars.Columns["Cars_Model"].ColumnName = "Modeli";
                    cars.Columns["Cars_Year"].ColumnName = "Yili";
                    cars.Columns["Cars_Vin"].ColumnName = "Vin No";
                    cars.Columns["Cars_No"].ColumnName = "Raqami";
                    cars.Columns["Cars_Fuel"].ColumnName = "Yoqilg'i turi";
                    cars.Columns["Cars_Status"].ColumnName = "Statusi";
                    cars.Columns["Cars_Insurance"].ColumnName = "Sug'urta";
                    cars.Columns["Cars_ServiceDate"].ColumnName = "Texnik ko'rik";
                    cars.Columns["Cars_LPG_Date"].ColumnName = "LPG";
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
                    string insurancedateStr = rowView["Sug'urta"].ToString();
                    string lpgdateStr = rowView["LPG"].ToString();
                    string servicedateStr = rowView["Texnik ko'rik"].ToString();

                    if (DateTime.TryParse(insurancedateStr, out DateTime date))
                    {
                        if ((date - DateTime.Now).TotalDays < 10)
                        {
                            
                            e.Row.Background = new SolidColorBrush(Colors.Red);
                            
                        }
                    }
                    
                    if (DateTime.TryParse(lpgdateStr, out DateTime date1))
                    {
                        if ((date1 - DateTime.Now).TotalDays < 30)
                        {
                            e.Row.Background = new SolidColorBrush(Colors.Red);
                        }
                    }
                    if (DateTime.TryParse(servicedateStr, out DateTime date2))
                    {
                        if ((date2 - DateTime.Now).TotalDays < 30)
                        {
                            e.Row.Background = new SolidColorBrush(Colors.Red);
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
            cars.Columns["Markasi"].ColumnName = "Cars_Brand";
            cars.Columns["Modeli"].ColumnName = "Cars_Model";
            cars.Columns["Yili"].ColumnName = "Cars_Year";
            cars.Columns["Vin No"].ColumnName = "Cars_Vin";
            cars.Columns["Raqami"].ColumnName = "Cars_No";
            cars.Columns["Yoqilg'i turi"].ColumnName = "Cars_Fuel";
            cars.Columns["Statusi"].ColumnName = "Cars_Status";
            cars.Columns["Sug'urta"].ColumnName = "Cars_Insurance";
            cars.Columns["Texnik ko'rik"].ColumnName = "Cars_ServiceDate";
            cars.Columns["LPG"].ColumnName = "Cars_LPG_Date";
            fillCarsTable();
            carsdatagrid.Items.Refresh();
        }
    }
}
