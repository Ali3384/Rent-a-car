﻿using ControlzEx.Theming;
using MahApps.Metro.Controls;
using System;
using System.Windows;
using MySql.Data.MySqlClient;
using Rent_a_car.pages.clients;
using Rent_a_car.pages.rent;
using Rent_a_car.pages.cars;
using System.Windows.Controls;
using Rent_a_car.pages.payments;

namespace Rent_a_car
{
    public partial class MainWindow
    {
        string connectionString = Properties.Settings.Default.ConnectionString;
        double debitstatus;


        public MainWindow()
        {
            InitializeComponent();
            rentframe.Content = new Rent_a_car.pages.rent.rentmainpage();
            carsframe.Content = new Rent_a_car.pages.cars.carsmainpage();
            clientsframe.Content = new Rent_a_car.pages.clients.clientsmainpage();
            paymentsframe.Content = new Rent_a_car.pages.payments.paymentsmainpage();
            UpdateDebitStatus();
        }
        public void ThemeChangeEvery()
        {

        }
        public void ThemeChange(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn)
                {
                    ThemeManager.Current.ChangeTheme(this, "Dark.Steel");
                }
                else
                {
                    ThemeManager.Current.ChangeTheme(this, "Light.Steel");
                }
            }
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();
        }

        public void UpdateDebitStatus()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand("SELECT SUM(Client_Debit) FROM clients WHERE Client_Status = 'aktiv'", connection);
                    object result = command.ExecuteScalar();
                    debitstatus = result != DBNull.Value ? Convert.ToDouble(result) : 0.0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating debit status: " + ex.Message);
            }
            qarzlar.Content = debitstatus.ToString() + " zl";
        }

        public void UpdateDatabase()
        {
            string selectQuery = "SELECT *, STR_TO_DATE(Period_Until, '%Y-%m-%d') AS ConvertedDate, CURDATE() FROM rentperiods WHERE STR_TO_DATE(Period_From, '%Y-%m-%d') < CURDATE() AND period_status != 'passed' AND Rent_Status = 'aktiv';"; 

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection);
                    using (MySqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int periodId = reader.GetInt32("Period_ID");
                            string clientName = reader.GetString("Client_Name");
                            string carPlateNo = reader.GetString("Car_Plate_No");
                            DateTime periodFrom = DateTime.ParseExact(reader.GetString("Period_Until"), "yyyy-MM-dd", null);
                            DateTime periodUntil = periodFrom.AddDays(7); // Adding 7 days to Period_From
                            int periodCost = reader.GetInt32("Period_Cost");
                            string paymentStatus = reader.GetString("Payment_Status");
                            int clientID = reader.GetInt32("Client_ID");

                            // Step 1: Insert the copied row
                            InsertCopiedRow(clientName, carPlateNo, periodFrom, periodUntil, periodCost, paymentStatus, clientID);

                            // Step 2: Update the original row's Period_Status to 'passed'
                            UpdateOriginalRow(periodId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        private void InsertCopiedRow(string clientName, string carPlateNo, DateTime periodFrom, DateTime periodUntil, int periodCost, string paymentStatus, int clientID)
        {
            string insertQuery = "INSERT INTO rentperiods (Client_Name, Car_Plate_No, Period_From, Period_Until, Period_Cost, Payment_Status, Period_Status, Client_ID,Rent_Status) " +
                                 "VALUES (@Client_Name, @Car_Plate_No, @Period_From, @Period_Until, @Period_Cost, @Payment_Status, 'aktiv', @Client_ID, 'aktiv');";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@Client_Name", clientName);
                insertCommand.Parameters.AddWithValue("@Car_Plate_No", carPlateNo);
                insertCommand.Parameters.AddWithValue("@Period_From", periodFrom.ToString("yyyy-MM-dd"));
                insertCommand.Parameters.AddWithValue("@Period_Until", periodUntil.ToString("yyyy-MM-dd"));
                insertCommand.Parameters.AddWithValue("@Period_Cost", periodCost);
                insertCommand.Parameters.AddWithValue("@Payment_Status", paymentStatus);
                insertCommand.Parameters.AddWithValue("@Client_ID", clientID);
                try
                {
                    connection.Open();
                    insertCommand.ExecuteNonQuery();
                    UpdateClientDebit(clientID, periodCost);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        private void UpdateOriginalRow(int periodId)
        {
            string updateQuery = "UPDATE rentperiods SET Period_Status = 'passed' WHERE Period_ID = @Period_ID;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@Period_ID", periodId);

                try
                {
                    connection.Open();
                    updateCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        private void UpdateClientDebit(int clientID, int periodCost)
        {
            string updateDebitQuery = "UPDATE clients SET Client_Debit = Client_Debit + @Amount WHERE Client_ID = @Client_ID;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand updateDebitCommand = new MySqlCommand(updateDebitQuery, connection);
                updateDebitCommand.Parameters.AddWithValue("@Client_ID", clientID);
                updateDebitCommand.Parameters.AddWithValue("@Amount", periodCost);

                try
                {
                    connection.Open();
                    updateDebitCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
        clientsmainpage clientsmainpage = new clientsmainpage();
        rentmainpage rentmainpage = new rentmainpage();
        carsmainpage carsmainpage = new carsmainpage();
        paymentsmainpage paymentsmainpage = new paymentsmainpage();


        private void klientlar_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDatabase();
            clientsmainpage.updateClient();
            carsmainpage.updateCar();
            rentmainpage.updateRent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateDebitStatus();
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MainTab.SelectedItem != null)
            {
                TabItem selectedTab = (TabItem)MainTab.SelectedItem;
                switch (selectedTab.Name)
                {
                    case "arenda":
                        rentmainpage.updateRent();
                        break;
                    case "avtomobillar":
                        carsmainpage.updateCar();
                        break;
                    case "klientlar":
                        clientsmainpage.updateClient();
                        break;
                    case "tolovlar":
                        paymentsmainpage.updatePayments();
                        break;
                }
                UpdateDebitStatus();
            }
        }
    }
}
