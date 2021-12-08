using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для Block3.xaml
    /// </summary>
    public partial class Block3 : UserControl, IGrid
    {
        private readonly Entities.ScenarioEntity scenarios_entity;
        private DataTable current_table = null;

        public Block3(Entities.ScenarioEntity scenarios)
        {
            InitializeComponent();

            scenarios_entity = scenarios;

            getAllRegions();

        }
        ~Block3()
        {
            GC.Collect();
        }
        private void getAllRegions()
        {
            foreach (string region in scenarios_entity.regions)
            {
                regions_box.Items.Add(new ComboBoxItem() { Content = region });
            }
        }
        private async void getData(int selected_index)
        {
            await Task.Run(() => current_table = scenarios_entity.getScenarioModels(selected_index));

            data_grid.ItemsSource = current_table.AsDataView();
            data_grid.Columns[0].CanUserSort = false;
            data_grid.Visibility = Visibility.Visible;
            Auxiliary.GridStandard.standardizeGrid(data_grid);
        }
        private async void regions_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loading.Visibility = Visibility.Visible;
            await Task.Run(() => this.Dispatcher.Invoke(() => getData(regions_box.SelectedIndex)));
            loading.Visibility = Visibility.Hidden;
        }
        private async void export_to_exc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (current_table == null)
                    throw new ArgumentNullException();

                loading.Visibility = Visibility.Visible;
                await Task.Run(() => this.Dispatcher.Invoke(() => Entities.ExcelRecorder.writeToExcel(current_table, regions_box.Text)));
            }
            catch (ArgumentNullException)
            {
                var dialog_result = MessageBox.Show("Не выбрана таблица для импортирования", "", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK)
                    return;
            }
            catch (Exception)
            {
                var dialog_result = MessageBox.Show("Проблема при инициализации работы с Excel (допустимо несоответствие версий)", "", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK)
                    return;
            }
            finally
            {
                loading.Visibility = Visibility.Hidden;
            }
        }
        public void hideGrid() => this.Visibility = Visibility.Hidden;
        public void showGrid() => this.Visibility = Visibility.Visible;
    }
}
