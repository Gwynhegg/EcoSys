using System;
using System.Collections.Generic;
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

        private async void getData()
        {
            loading.Visibility = Visibility.Visible;

            int selected_category = categories_box.SelectedIndex;
            List<object> response_bundle = null;

            await Task.Run(() =>  Dispatcher.Invoke(() => response_bundle = scenarios.getCategoriesData(selected_category, tables_list.ActualHeight)));

            try
            {
                tables_list.ItemsSource = response_bundle;

            }
            catch
            {
                MessageBox.Show("Не удалось составить набор ответных данных", "Ошибка", MessageBoxButton.OK);
            }
            loading.Visibility = Visibility.Hidden;
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
