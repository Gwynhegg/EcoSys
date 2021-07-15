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

            menu_grid.MinWidth = 0.2 * this.Width;
            menu_grid.MaxWidth = 0.3 * this.Width;

            this.MinWidth = this.Width;
            this.MinHeight = this.Height;
        }


        private void Settings_Click_1(object sender, RoutedEventArgs e)
        {
            main_grid.Children.Add(new Grids.SettingsGrid(data));

        }
    }
}
