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
using MySqlX.XDevAPI;
using Rent_a_car.pages.clients;
using Rent_a_car.pages.other_pages;
namespace Rent_a_car.pages.rent
{
    /// <summary>
    /// Логика взаимодействия для rentmainpage.xaml
    /// </summary>
    public partial class rentmainpage : Page
    {
        string connectionString = Properties.Settings.Default.ConnectionString;
        string rentcomboboxselection;
        string rentfiltertext;
        DataTable rents = new DataTable();
        public rentmainpage()
        {
            InitializeComponent();
            fillRentTable();
        }
        private void fillRentTable()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Rent_ID,Rent_Client_Name,Rent_Client_Surname,Client_ID, Rent_Car_Plate_No,Rent_From,Rent_Cost FROM rentings WHERE Rent_Status = 'aktywny'";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);

                    adapter.Fill(rents);
                    rents.Columns["Rent_ID"].ColumnName = "ID";
                    rents.Columns["Rent_Client_Name"].ColumnName = "Imie Klienta";
                    rents.Columns["Rent_Client_Surname"].ColumnName = "Nazwisko Klienta";
                    rents.Columns["Client_ID"].ColumnName = "Klient ID";
                    rents.Columns["Rent_Car_Plate_No"].ColumnName = "Numer rejestracyjny";
                    rents.Columns["Rent_From"].ColumnName = "Od kiedy";
                    rents.Columns["Rent_Cost"].ColumnName = "Koszt wynajmu";
                    rentdatagrid.ItemsSource = rents.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while filling data: " + ex.Message);
            }
        }
        private void rentfilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            rentfiltertext = rentfilter.Text;
            DataView dataView = rents.DefaultView;
            if (string.IsNullOrEmpty(rentfiltertext))
            {
                dataView.RowFilter = string.Empty;
            }
            else if (!string.IsNullOrEmpty(rentcomboboxselection))
            {
                try
                {
                    string escapedFilterText = rentfiltertext.Replace("'", "''");
                    string columnName = rentcomboboxselection.Contains(" ") ? $"[{rentcomboboxselection}]" : rentcomboboxselection;
                    dataView.RowFilter = $"{columnName} LIKE '%{escapedFilterText}%'";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while filtering data: " + ex.Message);
                }
            }
            rentdatagrid.ItemsSource = dataView;
        }
        private void rentcombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rentcomboboxselection = ((ComboBoxItem)rentcombobox.SelectedItem).Content.ToString();
        }

        private void addrent_Click(object sender, RoutedEventArgs e)
        {
            rentaddwindow rentaddwindow = new rentaddwindow();
            rentaddwindow.ShowDialog();
            updateRent();
        }
        public void updateRent()
        {

           

            rents.Clear();
            rents.Columns["ID"].ColumnName = "Rent_ID";
            rents.Columns["Imie Klienta"].ColumnName = "Rent_Client_Name";
            rents.Columns["Nazwisko Klienta"].ColumnName = "Rent_Client_Surname";
            rents.Columns["Klient ID"].ColumnName = "Client_ID"; 
            rents.Columns["Numer rejestracyjny"].ColumnName = "Rent_Car_Plate_No";
            rents.Columns["Od kiedy"].ColumnName = "Rent_From";
            rents.Columns["Koszt wynajmu"].ColumnName = "Rent_Cost";
            fillRentTable();
            rentdatagrid.Items.Refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            updateRent();
        }

        private void deleterent_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Czy chcesz usunąć wybrany wynajem ?",
                    "Czy jesteś pewien ?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (rentdatagrid.SelectedItem != null)
                {

                    // Cast the selected item to a DataRowView to access its columns
                    var selectedItem = rentdatagrid.SelectedItem as DataRowView;

                    if (selectedItem != null)
                    {
                        // Get the value of the "Numer Vin" column
                        string numerVin = selectedItem["ID"].ToString();
                        string plateno = selectedItem["Numer rejestracyjny"].ToString();
                        string clientName = selectedItem["Imie Klienta"].ToString();
                        try
                        {
                            MySqlConnection connection = new MySqlConnection(connectionString);
                            connection.Open();

                            string updateQuery = "UPDATE rentings SET Rent_Status = 'Nie aktywny' WHERE Rent_ID = @id";
                            using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection))
                            {
                                updateCmd.Parameters.AddWithValue("@id", numerVin);
                                updateCmd.ExecuteNonQuery();
                            }

                            string updateQuery2 = "UPDATE rentperiods SET Rent_Status = 'Nie aktywny' WHERE Client_Name = @name AND Car_Plate_No = @plateno";
                            using (MySqlCommand updateCmd2 = new MySqlCommand(updateQuery2, connection))
                            {
                                updateCmd2.Parameters.AddWithValue("@name", clientName);
                                updateCmd2.Parameters.AddWithValue("@plateno", plateno);
                                updateCmd2.ExecuteNonQuery();
                            }
                            string updateQuery3 = "UPDATE cars SET Car_Status = 'Nie wynajęte' WHERE Cars_No = @plate";
                            using (MySqlCommand updateCmd3 = new MySqlCommand(updateQuery3, connection))
                            {
                                updateCmd3.Parameters.AddWithValue("@plateno", plateno);
                                updateCmd3.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error while updating status of rentperiod");
                        }

                    }
                    updateRent();
                }
            }
        }

        private void rentdatagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (rentdatagrid.SelectedItem != null)
            {

                // Cast the selected item to a DataRowView to access its columns
                var selectedItem = rentdatagrid.SelectedItem as DataRowView;

                if (selectedItem != null)
                {
                    // Get the value of the "Numer Vin" column
                    Properties.Settings.Default.RentCarPlate = selectedItem["Numer rejestracyjny"].ToString();
                    Properties.Settings.Default.RentClientName = selectedItem["Imie Klienta"].ToString();
                    Properties.Settings.Default.IsRentings = true;
                    Properties.Settings.Default.FillQuery = "SELECT Period_ID,Client_Name,Car_Plate_No,Period_From,Period_Until,Period_Cost,Payment_Status, Payment_Amount, Client_ID FROM rentperiods WHERE Rent_Status = 'aktywny' AND Client_Name = @clientname AND Car_Plate_No = @plateno";

                    Properties.Settings.Default.Save();
                    periodmainwindow periodmainwindow = new periodmainwindow();
                    periodmainwindow.ShowDialog();

                    clientsmainpage clientsmainpage = new clientsmainpage();
                    clientsmainpage.updateClient();
                }
                
            }
        }
    }
}
