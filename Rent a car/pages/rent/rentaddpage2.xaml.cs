using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
namespace Rent_a_car.pages.rent
{
    /// <summary>
    /// Логика взаимодействия для rentaddpage2.xaml
    /// </summary>
    public partial class rentaddpage2 : Page
    {
        string carscomboboxselection;
        string carsfiltertext;
        DataTable cars = new DataTable();
        string connectionString = Properties.Settings.Default.ConnectionString;
        public rentaddpage2()
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
                    string query = "SELECT Cars_ID,Cars_Brand,Cars_Model,Cars_Year,Cars_Vin,Cars_No, Cars_Fuel, Cars_Status, Cars_Insurance FROM cars WHERE Cars_Status != 'Wynajęte'";
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

        private void nextbtn_Click(object sender, RoutedEventArgs e)
        {
            if (carsdatagrid.SelectedItems.Count > 0)
            {
                foreach (DataRowView row in carsdatagrid.SelectedItems)
                {
                    System.Data.DataRow myRow = row.Row;
                    Properties.Settings.Default.SelectedCarPlate = myRow["Numer rejestracyjny"].ToString();
                    Properties.Settings.Default.Save();
                }
               rentaddpage4window rentaddpage4Window = new rentaddpage4window();
               rentaddpage4Window.ShowDialog();
                Window current = Window.GetWindow(this);
                current.Close();
            }
        }

        private void carsdatagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (carsdatagrid.SelectedItems.Count > 0)
            {
                foreach (DataRowView row in carsdatagrid.SelectedItems)
                {
                    System.Data.DataRow myRow = row.Row;
                    Properties.Settings.Default.SelectedCarPlate = myRow["Numer rejestracyjny"].ToString();
                    Properties.Settings.Default.Save();
                }
                rentaddpage4window rentaddpage4Window = new rentaddpage4window();
                rentaddpage4Window.ShowDialog();
                Window current = Window.GetWindow(this);
                current.Close();
            }
        }
    }
}
