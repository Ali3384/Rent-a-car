using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
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
using HandyControl.Tools.Extension;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using Rent_a_car.pages.cars;

namespace Rent_a_car.pages.other_pages
{
    public partial class CarImages 
    {
        public List<BitmapImage> ImageList { get; set; }
        string connectionString = Properties.Settings.Default.ConnectionString;
        string mainimage;
        string images;
        string carPlate;
        string selectedID;
        string imageName;
        BitmapImage bitmap = new BitmapImage();
        DataTable imagesdata = new DataTable();
        public List<ImageInfo> Imageslist { get; set; }
        public CarImages()
        {
            InitializeComponent();
            Imageslist = new List<ImageInfo>();
            ImageList = new List<BitmapImage>();
            DataContext = this; // Set the DataContext to this window
            GetImages();
            fillImagesTable();
        }

        public void GetImages()
        {
            try
            {
                using (MySqlConnection connection2 = new MySqlConnection(connectionString))
                {
                    connection2.Open();
                    MySqlCommand command = new MySqlCommand("SELECT Cars_Image FROM cars WHERE Cars_No = @no", connection2);
                    command.Parameters.AddWithValue("@no", Properties.Settings.Default.CarSelectedCarPlate);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        string imageName = result.ToString();
                        string ftpUrl = $"ftp://admin%2540adigcars.pl@host653587.hostido.net.pl/public_html/images/{Properties.Settings.Default.CarSelectedCarPlate}/{imageName}";

                        BitmapImage bitmap = new BitmapImage();
                        using (MemoryStream stream = new MemoryStream())
                        {
                            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                            request.Method = WebRequestMethods.Ftp.DownloadFile;
                            request.Credentials = new NetworkCredential("admin@adigcars.pl", "M08011998m@");

                            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                            using (Stream responseStream = response.GetResponseStream())
                            {
                                responseStream.CopyTo(stream);
                            }

                            stream.Seek(0, SeekOrigin.Begin);
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                        }

                        mainimagebox.Source = bitmap;
                    }
                    else
                    {
                        MessageBox.Show("No image found for the selected car.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while getting data: " + ex.Message);
            }
        }

        private void fillImagesTable()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Images_Id, Image_Name FROM images WHERE Image_Type = 'Main' AND Image_ForCar = @plate";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@plate", Properties.Settings.Default.CarSelectedCarPlate);

                    // Create a data adapter and fill the data table
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    adapter.Fill(imagesdata);

                    // Rename columns if needed
                    imagesdata.Columns["Images_Id"].ColumnName = "ID";
                    imagesdata.Columns["Image_Name"].ColumnName = "Nazwa pliku";

                    // Assign the data table to the data grid
                    imagesdatagrid.ItemsSource = imagesdata.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while filling data: " + ex.Message);
            }
        }

        private void deleteimage_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Czy chcesz usunąć wybrany plik?",
                    "Czy jesteś pewien ?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (imagesdatagrid.SelectedItem != null)
                {
                    // Cast the selected item to a DataRowView to access its columns
                    var selectedItem = imagesdatagrid.SelectedItem as DataRowView;

                    if (selectedItem != null)
                    {
                        // Get the value of the "ID" column
                        selectedID = selectedItem["ID"].ToString();
                        imageName = selectedItem["Nazwa pliku"].ToString(); // Ensure you have the image name column
                        carPlate = Properties.Settings.Default.CarSelectedCarPlate;

                        try
                        {
                            using (MySqlConnection connection = new MySqlConnection(connectionString))
                            {
                                connection.Open();

                                string deleteQuery = "DELETE FROM images WHERE Images_Id = @id";
                                using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection))
                                {
                                    deleteCmd.Parameters.AddWithValue("@id", selectedID);
                                    deleteCmd.ExecuteNonQuery();
                                }
                            }

                            // Now delete the file from the FTP server
                            string ftpUrl = $"ftp://admin%2540adigcars.pl@host653587.hostido.net.pl/public_html/images/{carPlate}/{imageName}";
                            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                            request.Method = WebRequestMethods.Ftp.DeleteFile;
                            request.Credentials = new NetworkCredential("admin@adigcars.pl", "M08011998m@");
                            request.UsePassive = true;
                            request.EnableSsl = false; // Set to true if your server requires SSL

                            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                            {
                                // Check for successful status code
                                if (response.StatusCode != FtpStatusCode.FileActionOK)
                                {
                                    MessageBox.Show("Error deleting the file from FTP server");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error while deleting the car: " + ex.Message);
                        }

                        updateImages();
                    }
                }
            }
        }

        public void updateImages()
        {
            imagesdata.Clear();
            imagesdata.Columns["ID"].ColumnName = "Images_Id";
            imagesdata.Columns["Nazwa pliku"].ColumnName = "Image_Name";
            fillImagesTable();
            imagesdatagrid.Items.Refresh();
        }

        // New images
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.BMP;*.JPG;*.JPEG;*.PNG;*.TIFF;*.TIF;*.ICO;*.HEIC;*.WEBP",
                Multiselect = true,
                Title = "Please choose image files"
            };

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                Imageslist.Clear();
                foreach (var filePath in fileDialog.FileNames)
                {
                    Imageslist.Add(new ImageInfo
                    {
                        FileName = System.IO.Path.GetFileName(filePath),
                        FilePath = filePath
                    });
                }
                string fileNames = ConcatenateFileNames();
                Properties.Settings.Default.CarSelectedImages = fileNames;
                InsertImagesIntoDatabase(connectionString);
                save();
                updateImages();
            }
        }

        private void save()
        {
            string ftpUrl = "ftp://admin%2540adigcars.pl@host653587.hostido.net.pl/public_html/images"; // Change to your FTP server URL
            string username = "admin@adigcars.pl"; // Change to your FTP username
            string password = "M08011998m@"; // Change to your FTP password
            try
            {
                // Create the folder on the FTP server
                string folderUrl = $"{ftpUrl}/{carPlate}";
                CreateFtpFolder(folderUrl, username, password);

                // Upload each image
                foreach (var image in Imageslist)
                {
                    string fileUrl = $"{folderUrl}/{image.FileName}";
                    UploadFileToFtp(fileUrl, image.FilePath, username, password);
                }

                // Upload the main image
                if (!string.IsNullOrEmpty(Properties.Settings.Default.CarSelectedMainImage))
                {
                    string mainImageUrl = $"{folderUrl}/{Properties.Settings.Default.CarSelectedMainImageName}";
                    UploadFileToFtp(mainImageUrl, Properties.Settings.Default.CarSelectedMainImage, username, password);
                }

                MessageBox.Show("Images uploaded successfully.");
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void CreateFtpFolder(string folderUrl, string username, string password)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(folderUrl);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(username, password);
                request.UsePassive = true;
                request.EnableSsl = false; // Set to true if your server requires SSL

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    // Folder created successfully
                }
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response != null && response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    throw;
                }
                else
                {
                    // Folder already exists, ignore error
                }
            }
        }

        private void UploadFileToFtp(string fileUrl, string filePath, string username, string password)
        {
            const int MaxRetryCount = 3;
            int retryCount = 0;
            bool success = false;

            while (retryCount < MaxRetryCount && !success)
            {
                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fileUrl);
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(username, password);
                    request.UsePassive = true;
                    request.EnableSsl = false; // Set to true if your server requires SSL

                    byte[] fileContents = File.ReadAllBytes(filePath);
                    request.ContentLength = fileContents.Length;

                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(fileContents, 0, fileContents.Length);
                    }

                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        // File uploaded successfully
                        success = true;
                    }
                }
                catch (WebException ex)
                {
                    retryCount++;
                    if (retryCount >= MaxRetryCount)
                    {
                        MessageBox.Show($"Failed to upload file '{filePath}' after {MaxRetryCount} attempts. Error: {ex.Message}");
                        break;
                    }
                    else
                    {
                        // Optionally, log the retry attempt
                    }
                }
            }
        }

        public List<string> CarSelectedImagesList { get; private set; }

        private string ConcatenateFileNames()
        {
            string concatenatedNames = "";

            foreach (var image in Imageslist)
            {
                concatenatedNames += $"{image.FileName};";
            }

            // Remove the trailing semicolon if there are any images
            if (!string.IsNullOrEmpty(concatenatedNames))
            {
                concatenatedNames = concatenatedNames.TrimEnd(';');
            }

            return concatenatedNames;
        }

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
            carPlate = Properties.Settings.Default.CarSelectedCarPlate;
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
                        cmd.Parameters.AddWithValue("@carplate", carPlate);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void imagesdatagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (imagesdatagrid.SelectedItem != null)
                {
                    // Cast the selected item to a DataRowView to access its columns
                    var selectedItem = imagesdatagrid.SelectedItem as DataRowView;

                    if (selectedItem != null)
                    {
                        // Get the value of the "ID" column
                        selectedID = selectedItem["ID"].ToString();
                        imageName = selectedItem["Nazwa pliku"].ToString(); // Ensure you have the image name column
                        carPlate = Properties.Settings.Default.CarSelectedCarPlate;
                        string ftpUrl = $"ftp://admin%2540adigcars.pl@host653587.hostido.net.pl/public_html/images/{carPlate}/{imageName}";

                        BitmapImage bitmap = new BitmapImage();

                        using (MemoryStream stream = new MemoryStream())
                        {
                            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                            request.Method = WebRequestMethods.Ftp.DownloadFile;
                            request.Credentials = new NetworkCredential("admin@adigcars.pl", "M08011998m@");

                            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                            using (Stream responseStream = response.GetResponseStream())
                            {
                                responseStream.CopyTo(stream);
                            }

                            stream.Seek(0, SeekOrigin.Begin);
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                        }

                        // Show the image in a dialog box
                        ImageDialog dialog = new ImageDialog();
                        dialog.SetImageSource(bitmap);
                        dialog.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while getting image: " + ex.Message);
            }
        }

        private void changeimagebtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.BMP;*.JPG;*.JPEG;*.PNG;*.TIFF;*.TIF;*.ICO;*.HEIC;*.WEBP",
                Multiselect = false,
                Title = "Please choose main image file"
            };

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                Properties.Settings.Default.CarSelectedMainImage = fileDialog.FileName;
                Properties.Settings.Default.CarSelectedMainImageName = fileDialog.SafeFileName;
                
                UpdateFtpFile();
                DeleteMainImageFromFtp();
                UpdateSQL();

                mainimagebox.Source = new BitmapImage(new Uri(Properties.Settings.Default.CarSelectedMainImage));
            }
        }
        public void UpdateSQL()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                connection.Open();

                string insertQuery = "UPDATE cars SET Cars_Image = @carmainimage WHERE Cars_No = @plate";
                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@plate", Properties.Settings.Default.CarSelectedCarPlate);
                    insertCmd.Parameters.AddWithValue("@carmainimage", Properties.Settings.Default.CarSelectedMainImageName);
                    insertCmd.ExecuteNonQuery();
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating data: " + ex.Message);
            }
        }
        public void DeleteMainImageFromFtp()
        {

                        carPlate = Properties.Settings.Default.CarSelectedCarPlate;

                        try
                        {
                            using (MySqlConnection connection2 = new MySqlConnection(connectionString))
                            {
                                connection2.Open();
                                MySqlCommand command = new MySqlCommand("SELECT Cars_Image FROM cars WHERE Cars_No = @no", connection2);
                                command.Parameters.AddWithValue("@no", carPlate);
                                object result = command.ExecuteScalar();

                                if (result != null)
                                {
                                   string imageName = result.ToString();


                                    // Now delete the file from the FTP server
                                    string ftpUrl = $"ftp://admin%2540adigcars.pl@host653587.hostido.net.pl/public_html/images/{carPlate}/{imageName}";
                                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                                    request.Method = WebRequestMethods.Ftp.DeleteFile;
                                    request.Credentials = new NetworkCredential("admin@adigcars.pl", "M08011998m@");
                                    request.UsePassive = true;
                                    request.EnableSsl = false; // Set to true if your server requires SSL

                                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                                    {
                                       // Check for successful status code
                                        if (response.StatusCode != FtpStatusCode.FileActionOK)
                                        {
                                            MessageBox.Show("Error deleting the file from FTP server");
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error while deleting the car: " + ex.Message);
                        }

                   
                
            
        }
        public void UpdateFtpFile()
        {
            try
            {
                string ftpUrl = "ftp://admin%2540adigcars.pl@host653587.hostido.net.pl/public_html/images";
                string username = "admin@adigcars.pl";
                string password = "M08011998m@";
                string carPlate = Properties.Settings.Default.CarSelectedCarPlate;
                string filePath = Properties.Settings.Default.CarSelectedMainImage;
                string fileName = Properties.Settings.Default.CarSelectedMainImageName;

                // Create the folder on the FTP server if it doesn't exist
                string folderUrl = $"{ftpUrl}/{carPlate}";
                CreateFtpFolder2(folderUrl, username, password);

                // Upload the new main image
                string fileUrl = $"{folderUrl}/{fileName}";
                UploadFileToFtp2(fileUrl, filePath, username, password);

                MessageBox.Show("Main image updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating main image: " + ex.Message);
            }
        }

        private void UploadFileToFtp2(string fileUrl, string filePath, string username, string password)
        {
            const int MaxRetryCount = 3;
            int retryCount = 0;
            bool success = false;

            while (retryCount < MaxRetryCount && !success)
            {
                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fileUrl);
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(username, password);
                    request.UsePassive = true;
                    request.EnableSsl = false; // Set to true if your server requires SSL

                    byte[] fileContents = File.ReadAllBytes(filePath);
                    request.ContentLength = fileContents.Length;

                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(fileContents, 0, fileContents.Length);
                    }

                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        // File uploaded successfully
                        success = true;
                    }
                }
                catch (WebException ex)
                {
                    retryCount++;
                    if (retryCount >= MaxRetryCount)
                    {
                        MessageBox.Show($"Failed to upload file '{filePath}' after {MaxRetryCount} attempts. Error: {ex.Message}");
                        break;
                    }
                    else
                    {
                        // Optionally, log the retry attempt
                    }
                }
            }
        }

        private void CreateFtpFolder2(string folderUrl, string username, string password)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(folderUrl);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(username, password);
                request.UsePassive = true;
                request.EnableSsl = false; // Set to true if your server requires SSL

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    // Folder created successfully
                }
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response != null && response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    throw;
                }
                else
                {
                    // Folder already exists, ignore error
                }
            }
        }


    }
}
