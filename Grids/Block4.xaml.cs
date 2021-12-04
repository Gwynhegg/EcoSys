using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для Block4.xaml
    /// </summary>
    public partial class Block4 : UserControl, IGrid
    {
        Entities.ScenarioEntity scenarios;

        Dictionary<string, List<string>> regions;
        private DataTable current_table = null;
        private DataTable final_table = null;
        private List<object> response_bundle = null;
        int current_step = 0;

        public Block4(Entities.ScenarioEntity scenarios, Dictionary<string, List<string>> regions)
        {
            InitializeComponent();

            this.scenarios = scenarios;
            this.regions = regions;

            getAllRegions();
            getYears();
            getScenarios();
        }

        ~Block4()
        {
            GC.Collect();
        }

        private void getAllRegions()        //метод для отображения списка всех регионов
        {
            foreach (KeyValuePair<string, List<string>> pair in regions)        //Для каждого округа и принадлежащих ему регионов...
                foreach (string region in pair.Value)       //Для каждого региона из данного округа...
                {
                    ComboBoxItem cb = new ComboBoxItem();       //Создаем новый ComboBoxItem объект
                    cb.Content = region;        //Отображаем на нем название региона
                    regions_box.Items.Add(cb);
                }
        }

        private void getScenarios()
        {
            foreach (string scenario in scenarios.scenario_name)
            {
                var cb = new ComboBoxItem();
                cb.Content = scenario;
                scenario_choose.Items.Add(cb);
            }
        }


        private void getYears()
        {
            for (int year = 2022; year < 2026; year++)
                years_box.Items.Add(new ComboBoxItem() { Content = String.Format("Прогноз на {0} год", year) });
        }

        private async void getData(string region_request, int year_request)
        {
            initializeComponents();
            displayCurrentStep();
                //Auxiliary.ColorsStructure.getColor(scenario_name);

            await Task.Run(() => Dispatcher.Invoke(() => this.current_table = scenarios.getScenarioData(region_request, year_request, current_step)));

            if (final_table == null) 
            {
                final_table = current_table.Clone();
                for (int i = 0; i < scenarios.lines.Count; i++)
                    final_table.Rows.Add();
            }
            scenarios_grid.AutoGeneratingColumn += r2_AutoGeneratingColumn;
            scenarios_grid.ItemsSource = current_table.AsDataView();
            Auxiliary.GridStandard.standardizeGrid(scenarios_grid);
            scenarios_grid.Visibility = Visibility.Visible;

            await Task.Run(() => Dispatcher.Invoke(() => createGraphs(current_step, this.ActualHeight, graphs_grid.ActualWidth)));
        }

        private async void regions_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (years_box.SelectedIndex != -1)
            {
                resetData();
                loading.Visibility = Visibility.Visible;
                await Task.Run(() => Dispatcher.Invoke(() => getData(regions_box.Text, years_box.SelectedIndex)));
                loading.Visibility = Visibility.Hidden;
            }
        }

        private async void years_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (regions_box.SelectedIndex != -1)
            {
                resetData();
                loading.Visibility = Visibility.Visible;
                await Task.Run(() => Dispatcher.Invoke(() => getData(regions_box.Text, years_box.SelectedIndex)));
                loading.Visibility = Visibility.Hidden;
            }
        }

        private async void createGraphs(int current_step, double height, double width)
        {
            response_bundle = new List<object>();
            await Task.Run(() => Dispatcher.Invoke(() => response_bundle = scenarios.getCategoriesData(current_step, height, width)));

            graphs_list.ItemsSource = response_bundle;
        }

        private void displayCurrentStep()
        {
            current_category.Text = scenarios.categories[current_step];
        }

        private void initializeComponents()
        {
            scenarios_grid.Visibility = Visibility.Visible;
            current_category.Visibility = Visibility.Visible;
            command_buttons.Visibility = Visibility.Visible;
            scenario_choose.Visibility = Visibility.Visible;
        }


        private async void export_to_exc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (current_table == null || response_bundle == null) throw new ArgumentNullException();

                loading.Visibility = Visibility.Visible;
               // await Task.Run(() => Dispatcher.Invoke(() => Entities.ExcelRecorder.writeToExcel(response_bundle, scenarios_tab.Items, region_query, categories_box.Text, years_box.Text)));

            }
            catch (ArgumentNullException exc)
            {
                var dialog_result = MessageBox.Show("Часть данных, необходимых для импортирования, отсутствует. Убедитесь, что все поля выбраны", "", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK) return;
            }
            catch (Exception exc)
            {
                var dialog_result = MessageBox.Show("Проблема при инициализации работы с Excel (допустимо несоответствие версий)", "", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK) return;
            }
            finally
            {
                loading.Visibility = Visibility.Hidden;
            }
        }

        private void scenario_choose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (current_step > 0) back_button.IsEnabled = true;
            if (current_step < 12) forward_button.IsEnabled = true;

            if (scenario_choose.SelectedIndex != -1) 
                for (int i = 0; i < scenarios.columns.Count; i++)
                    final_table.Rows[current_step][i] = current_table.Rows[scenario_choose.SelectedIndex][i];

            if (checkConditions()) show_final.IsEnabled = true;
        }

        private bool checkConditions()
        {
            for (int i = 0; i < scenarios.lines.Count; i++)
                if (final_table.Rows[i][0] == null) return false;
            return true;
        }

        private async void back_button_Click(object sender, RoutedEventArgs e)
        {
            scenario_choose.SelectedIndex = -1;
            current_step--;
            if (current_step > 0) back_button.IsEnabled = true; else back_button.IsEnabled = false;
            loading.Visibility = Visibility.Visible;
            await Task.Run(() => Dispatcher.Invoke(() => getData(regions_box.Text, years_box.SelectedIndex)));
            loading.Visibility = Visibility.Hidden;
        }

        private async void forward_button_Click(object sender, RoutedEventArgs e)
        {
            scenario_choose.SelectedIndex = -1;
            current_step++;
            if (current_step < 12) forward_button.IsEnabled = true; else forward_button.IsEnabled = false;
            loading.Visibility = Visibility.Visible;
            await Task.Run(() => Dispatcher.Invoke(() => getData(regions_box.Text, years_box.SelectedIndex)));
            loading.Visibility = Visibility.Hidden;
        }


        private void r2_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)        //Кастомный метод для правильного отображения заголовков
        {
            if (e.PropertyName.Contains('.') && e.Column is DataGridBoundColumn)
            {
                DataGridBoundColumn dataGridBoundColumn = e.Column as DataGridBoundColumn;
                dataGridBoundColumn.Binding = new Binding("[" + e.PropertyName + "]");
            }
        }
        public void hideGrid()
        {
            this.Visibility = Visibility.Hidden;
        }

        public void showGrid()
        {
            this.Visibility = Visibility.Visible;
        }

        private void switch_grids_Click(object sender, RoutedEventArgs e)
        {
            if (scenarios_grid.Visibility == Visibility.Visible)
            {
                scenarios_grid.Visibility = Visibility.Hidden;
                graphs_grid.Visibility = Visibility.Visible;
            }
            else
            {
                scenarios_grid.Visibility = Visibility.Visible;
                graphs_grid.Visibility = Visibility.Hidden;
            }
        }

        private void show_final_Click(object sender, RoutedEventArgs e)
        {
            scenarios_grid.ItemsSource = final_table.AsDataView();
            scenarios_grid.Visibility = Visibility.Visible;
            graphs_grid.Visibility = Visibility.Hidden;
        }

        private void resetData()
        {
            final_table = null;
            scenarios_grid.ItemsSource = null;
            graphs_grid.Visibility = Visibility.Hidden;
            current_step = 0;
            back_button.IsEnabled = false;
            forward_button.IsEnabled = true;
            show_final.IsEnabled = false;
            scenario_choose.SelectedIndex = -1;
        }

        private void clear_grids_Click(object sender, RoutedEventArgs e)
        {
            resetData();
        }
    }
}
