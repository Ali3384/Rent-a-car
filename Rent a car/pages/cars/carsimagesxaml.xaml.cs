using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace Rent_a_car.pages.cars
{
    public class ImageInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для carsimagesxaml.xaml
    /// </summary>
    public partial class carsimagesxaml
    {
        public List<ImageInfo> Images { get; set; }
        public string platenumber = Properties.Settings.Default.SelectedCarPlate;
        
        public carsimagesxaml()
        {
            InitializeComponent();
            Properties.Settings.Default.CarSelectedImages = "";
            Properties.Settings.Default.CarSelectedMainImage = "";
            Images = new List<ImageInfo>();

            // Accept any SSL certificate for development purposes
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Always accept for development purposes
            return true;
        }

        private void imageadd_Click(object sender, RoutedEventArgs e)
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
                Images.Clear();
                foreach (var filePath in fileDialog.FileNames)
                {
                    Images.Add(new ImageInfo
                    {
                        FileName = System.IO.Path.GetFileName(filePath),
                        FilePath = filePath
                    });
                }

                RefreshDataGrid();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button deleteButton && deleteButton.Tag is ImageInfo imageInfo)
            {
                Images.Remove(imageInfo);
                RefreshDataGrid();
            }
        }

        private void RefreshDataGrid()
        {
            listofimages.ItemsSource = null;
            listofimages.ItemsSource = Images;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            string ftpUrl = "ftp://admin%2540adigcars.pl@host653587.hostido.net.pl/public_html/images"; // Change to your FTP server URL
            string username = "admin@adigcars.pl"; // Change to your FTP username
            string password = "M08011998m@"; // Change to your FTP password
            string fileNames = ConcatenateFileNames();
            Properties.Settings.Default.CarSelectedImages = fileNames;
            try
            {
                // Create the folder on the FTP server
                string folderUrl = $"{ftpUrl}/{platenumber}";
                CreateFtpFolder(folderUrl, username, password);

                // Upload each image
                foreach (var image in Images)
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
                this.Close();
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
                request.EnableSsl = true;

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
                    request.EnableSsl = true;

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
        private string ConcatenateFileNames()
        {
            string concatenatedNames = "";

            foreach (var image in Images)
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

        private void imageadd_Копировать_Click(object sender, RoutedEventArgs e)
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
                mainimage.Source = new BitmapImage(new Uri(Properties.Settings.Default.CarSelectedMainImage));
            }
            }
    }
}
