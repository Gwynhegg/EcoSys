using System;
using System.Collections.Generic;
using System.Data;
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

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для Block3.xaml
    /// </summary>
    public partial class Block3 : UserControl, IGrid
    {

        private Entities.ScenarioEntity scenarios_entity;

        private DataTable current_table = null;

        public Block3(Entities.ScenarioEntity scenarios)
        {
            InitializeComponent();

            this.scenarios_entity = scenarios;

            getAllRegions();
           
        }


        private void getAllRegions()
        {
            foreach (string region in scenarios_entity.regions)
                regions_box.Items.Add(new ComboBoxItem() { Content = region });
        }


        public void hideGrid()
        {
            this.Visibility = Visibility.Hidden;
        }

        public void showGrid()
        {
            this.Visibility = Visibility.Visible;
        }

        private async void getData()
        {
            loading.Visibility = Visibility.Visible;

            int selected_index = regions_box.SelectedIndex;

            await Task.Run(() => current_table = scenarios_entity.getScenarioModels(selected_index));
            data_grid.ItemsSource = current_table.AsDataView();

            loading.Visibility = Visibility.Hidden;

        }

        ~Block3()
        {
            GC.Collect();
        }

        private void regions_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getData();
            data_grid.Visibility = Visibility.Visible;
        }
    }
}
