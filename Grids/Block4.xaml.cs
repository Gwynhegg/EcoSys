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
        private readonly Entities.ScenarioEntity scenarios;
        private readonly Dictionary<string, List<string>> regions;
        private DataTable current_table = null;
        private DataTable final_table = null;
        private List<object> response_bundle = null;
        private int current_step = 0;
        private System.Windows.Controls.Primitives.Popup pop;

        public Block4(Entities.ScenarioEntity scenarios, Dictionary<string, List<string>> regions)
        {
            InitializeComponent();

            this.scenarios = scenarios;
            this.regions = regions;
            createPopUp();
            Auxiliary.ContextMenuCreator.createContextMenu(scenarios_grid);
            Auxiliary.ContextMenuCreator.createContextMenu(final_grid);

            getAllRegions();
            getYears();
        }
        ~Block4()
        {
            GC.Collect();
        }
        private void initializeComponents()
        {
            scenarios_grid.Visibility = Visibility.Visible;
            DropDownDataGrid.Visibility = Visibility.Visible;
            command_buttons.Visibility = Visibility.Visible;
            graphs_grid.Visibility = Visibility.Visible;
        }
        private void getAllRegions()        //метод для отображения списка всех регионов
        {
            foreach (KeyValuePair<string, List<string>> pair in regions)        //Для каждого округа и принадлежащих ему регионов...
            {
                foreach (string region in pair.Value)       //Для каждого региона из данного округа...
                {
                    ComboBoxItem cb = new ComboBoxItem
                    {
                        Content = region        //Отображаем на нем название региона
                    };       //Создаем новый ComboBoxItem объект
                    regions_box.Items.Add(cb);
                }
            }
        }
        private void getYears()
        {
            for (int year = 2022; year < 2026; year++)
            {
                years_box.Items.Add(new ComboBoxItem() { Content = String.Format("Прогноз на {0} год", year) });
            }
        }
        private void getData(string region_request, int year_request)
        {
            initializeComponents();
            displayCurrentStep();
            //Auxiliary.ColorsStructure.getColor(scenario_name);

            current_table = scenarios.getScenarioData(region_request, year_request, current_step);


            if (final_table == null)
            {
                final_table = current_table.Clone();
                final_table.Columns.Add(new DataColumn() { ColumnName = "Сценарий", DataType = System.Type.GetType("System.String") });
                for (int i = 0; i < scenarios.lines.Count; i++)
                {
                    final_table.Rows.Add();
                }
            }
            scenarios_grid.AutoGeneratingColumn += r2_AutoGeneratingColumn;
            scenarios_grid.ItemsSource = current_table.AsDataView();
            Auxiliary.GridStandard.standardizeGrid(scenarios_grid);
            scenarios_grid.Visibility = Visibility.Visible;
            createGraphs(current_step, this.ActualHeight, graphs_grid.ActualWidth);
        }
        private async void createGraphs(int current_step, double height, double width)
        {
            response_bundle = new List<object>();
            if (current_step < 7)
            {
                await Task.Run(() => this.Dispatcher.Invoke(() => response_bundle = scenarios.getCategoriesData(current_step, height, width)));
            }
            else
            {
                await Task.Run(() => this.Dispatcher.Invoke(() => response_bundle = scenarios.getCategoriesAndRegionData(current_step, regions_box.Text, height, width)));
            }

            graphs_list.ItemsSource = response_bundle;
        }
        private void displayCurrentStep() => DropDownDataGrid.Header = scenarios.categories[current_step] + ": Выберите сценарий";
        private async void regions_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (years_box.SelectedIndex != -1)
            {
                resetData();
                loading.Visibility = Visibility.Visible;
                await Task.Run(() => this.Dispatcher.Invoke(() => getData(regions_box.Text, years_box.SelectedIndex)));
                loading.Visibility = Visibility.Hidden;
            }
        }
        private async void years_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (regions_box.SelectedIndex != -1)
            {
                resetData();
                loading.Visibility = Visibility.Visible;
                await Task.Run(() => this.Dispatcher.Invoke(() => getData(regions_box.Text, years_box.SelectedIndex)));
                loading.Visibility = Visibility.Hidden;
            }
        }
        private async void forward_button_Click(object sender, RoutedEventArgs e)
        {
            current_step++;
            forward_button.IsEnabled = false;
            loading.Visibility = Visibility.Visible;
            await Task.Run(() => this.Dispatcher.Invoke(() => getData(regions_box.Text, years_box.SelectedIndex)));
            loading.Visibility = Visibility.Hidden;
        }
        private void show_final_Click(object sender, RoutedEventArgs e)
        {
            DropDownDataGrid.Visibility = Visibility.Hidden;
            graphs_grid.Visibility = Visibility.Hidden;

            final_grid.AutoGeneratingColumn += r2_AutoGeneratingColumn;
            final_grid.ItemsSource = final_table.AsDataView();
            final_grid.Columns[0].CanUserSort = false;
            Auxiliary.GridStandard.standardizeGrid(final_grid);
            final_grid.Visibility = Visibility.Visible;
            export_to_exc.IsEnabled = true;
        }
        private void resetData()
        {
            final_grid.Visibility = Visibility.Hidden;
            final_table = null;
            scenarios_grid.ItemsSource = null;
            current_step = 0;
            forward_button.IsEnabled = false;
            show_final.IsEnabled = false;
            export_to_exc.IsEnabled = false;
        }
        private void clear_grids_Click(object sender, RoutedEventArgs e)
        {
            resetData();
            getData(regions_box.Text, years_box.SelectedIndex);
        }
        private void scenarios_grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
            if (row != null)
            {
                if (current_step != 11)
                    forward_button.IsEnabled = true;

                row.IsSelected = true;
                int index = row.GetIndex();
                DropDownDataGrid.Header = String.Format("{0} : {1}", scenarios.categories[current_step], scenarios.scenario_name[index]);
                var new_row = final_table.NewRow();
                new_row[0] = scenarios.categories[current_step];
                for (int i = 1; i < final_table.Columns.Count - 1; i++)
                {
                    var temp = current_table.Rows[index].Field<double?>(i);
                    if (temp != null)
                    {
                        new_row[i] = temp;
                    }
                }
                new_row[final_table.Columns.Count - 1] = scenarios.scenario_name[index];
                final_table.Rows.RemoveAt(current_step);
                final_table.Rows.InsertAt(new_row, current_step);

                if (current_step == 11)
                    show_final.IsEnabled = true;
            }
        }
        private void DropDownDataGrid_Expanded(object sender, RoutedEventArgs e)
        {
            Grid.SetRow(graphs_grid, 6);
            Grid.SetRowSpan(graphs_grid, 3);
            createGraphs(current_step, this.ActualHeight, graphs_grid.ActualWidth);
        }
        private void DropDownDataGrid_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid.SetRow(graphs_grid, 5);
            Grid.SetRowSpan(graphs_grid, 4);
            createGraphs(current_step, this.ActualHeight, graphs_grid.ActualWidth);
        }
        private async void export_to_exc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (final_table == null)
                    throw new ArgumentNullException();

                loading.Visibility = Visibility.Visible;
                await Task.Run(() => this.Dispatcher.Invoke(() => Auxiliary.ExcelRecorder.writeToExcel(final_table, regions_box.Text)));

            }
            catch (ArgumentNullException)
            {
                var dialog_result = MessageBox.Show("Часть данных, необходимых для импортирования, отсутствует. Убедитесь, что все поля выбраны", "", MessageBoxButton.OK);
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
        private void r2_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)        //Кастомный метод для правильного отображения заголовков
        {
            if (e.PropertyName.Contains('.') && e.Column is DataGridBoundColumn)
            {
                DataGridBoundColumn dataGridBoundColumn = e.Column as DataGridBoundColumn;
                dataGridBoundColumn.Binding = new Binding("[" + e.PropertyName + "]");
            }
        }
        public void hideGrid() => this.Visibility = Visibility.Hidden;
        public void showGrid() => this.Visibility = Visibility.Visible;

        private void createPopUp()
        {
            var text = "\n  - Для выбора региона и года, на который рассчитан прогноз, выберите их с помощью элементов слева  \n" +
                "  - После выбора параметров появится таблица, содержащая значения показателей в соответствии с различными сценариями \n" +
                "  - Ниже будет представлены графики сценарных условий, разделенные по категориям \n" +
                "  - Если Вы захотите рассмотреть графики внимательнее, то всегда можете нажать стрелку в левом углу таблицы для ее скрытия \n" +
                "  - Для перехода к следующим сценарным условиям нужно выбрать один из сценариев в текущей таблице, после чего нажать кнопку \"Далее\" \n" +
                "  - Во время заполнения у вас есть возможность оборвать процесс и начать заполнение таблиц заново с помощью кнопки \"Очистить\" \n" +
                "  - По завершению заполнения последней категории и нажатию на кнопку \"Результат\" отобразится таблица, содержащая значения выбранных вами сценариев с их указанием \n" +
                "  - Вы можете выгрузить таблицу в Excel, нажав кнопку \"Экспорт\" \n";
            pop = Auxiliary.PopUpCreator.getPopUp(text);
        }
        private void InfoButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            pop.IsOpen = true;
        }
        private void InfoButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            pop.IsOpen = false;
        }
    }
}
