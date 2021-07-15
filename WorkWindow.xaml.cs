using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EcoSys
{
    /// <summary>
    /// Логика взаимодействия для WorkWindow.xaml
    /// </summary>
    public partial class WorkWindow : Window
    {
        Entities.DataEntity data;

        public WorkWindow(Entities.DataEntity data)
        {
            InitializeComponent();

            this.data = data;
        }


        private void Settings_Click_1(object sender, RoutedEventArgs e)
        {
            var settings = new Grids.SettingsGrid(data, this);
            Grid.SetColumn(settings, 0);
            Grid.SetColumnSpan(settings, 2);
            main_grid.Children.Add(settings);

        }
    }
}
