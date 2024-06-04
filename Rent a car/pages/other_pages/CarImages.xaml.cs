using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Rent_a_car.pages.other_pages
{
    /// <summary>
    /// Логика взаимодействия для CarImages.xaml
    /// </summary>
    public partial class CarImages
    {
        public CarImages()
        {
            InitializeComponent();
            maincarousel.Items.Add("/sedan.png");
            maincarousel.Items.Add("/sedan.png");
            maincarousel.Items.Add("/sedan.png");
            maincarousel.Items.Add("/sedan.png");
            maincarousel.Items.Add("/sedan.png");
        }
    }
}
