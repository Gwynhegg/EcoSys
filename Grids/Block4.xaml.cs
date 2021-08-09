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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для Block4.xaml
    /// </summary>
    public partial class Block4 : UserControl, IGrid
    {
        Entities.ScenarioEntity scenarios;
        public Block4(Entities.ScenarioEntity scenarios)
        {
            InitializeComponent();

            this.scenarios = scenarios;

            getCategories();
        }

        private void getCategories()
        {
            foreach (string category in scenarios.categories)
                categories_box.Items.Add(new ComboBoxItem() { Content = category });
        }

        private void categories_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getData();
        }

        private void getData()
        {
            int selected_category = categories_box.SelectedIndex;

            List<object> response_bundle = scenarios.getCategoriesData(selected_category, tables_list.ActualHeight);

            tables_list.ItemsSource = response_bundle;
        }

        public void hideGrid()
        {
            this.Visibility = Visibility.Hidden;
        }

        public void showGrid()
        {
            this.Visibility = Visibility.Visible;
        }
    }
}
