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
        public Block3(Entities.ScenarioEntity scenarios)
        {
            InitializeComponent();

            this.scenarios_entity = scenarios;

            getAllRegions();
            getYears();
            getScenarioNames();
            
        }

        private void getScenarioNames()
        {
            foreach (string scenario in scenarios_entity.scenario_name)
                tab_grid.Items.Add(new TabItem() { Header = scenario });
            tab_grid.Visibility = Visibility.Hidden;
        }
        private void getAllRegions()
        {
            foreach (string region in scenarios_entity.regions)
                regions_box.Items.Add(new ComboBoxItem() { Content = region });
        }

        private void getYears()
        {
            foreach (string year in scenarios_entity.years)
                years_box.Items.Add(new ComboBoxItem() { Content = year });
        }

        public void hideGrid()
        {
            this.Visibility = Visibility.Hidden;
        }

        public void showGrid()
        {
            this.Visibility = Visibility.Visible;
        }

        private void getData()
        {
            int selected_region = regions_box.SelectedIndex;
            int selected_year = years_box.SelectedIndex;

            foreach (TabItem scenario in tab_grid.Items)
            {
                var data_grid = new DataGrid();
                data_grid.ItemsSource = scenarios_entity.getScenarioData(selected_year, selected_region, scenario.Header.ToString()).AsDataView();
                scenario.Content = data_grid;
            }
            tab_grid.Visibility = Visibility.Visible;
        }

        ~Block3()
        {
            GC.Collect();
        }

        private async void regions_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (years_box.SelectedIndex != -1)
            {
                loading.Visibility = Visibility.Visible;
                await Task.Run(() => getData());
                loading.Visibility = Visibility.Hidden;

            }
        }

        private void years_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (regions_box.SelectedIndex != -1)
                getData();
        }
    }
}
