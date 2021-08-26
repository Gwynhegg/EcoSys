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
    /// Логика взаимодействия для Block4.xaml
    /// </summary>
    public partial class Block4 : UserControl, IGrid
    {
        Entities.ScenarioEntity scenarios;

        private HashSet<string> region_query;
        Dictionary<string, List<string>> regions;
        private DataTable current_table = null;
        private List<object> response_bundle = null;

        public Block4(Entities.ScenarioEntity scenarios, Dictionary<string, List<string>> regions)
        {
            InitializeComponent();

            this.scenarios = scenarios;
            region_query = new HashSet<string>();
            this.regions = regions;

            getAllRegions();
            getCategories();
            getYears();
        }

        ~Block4()
        {
            GC.Collect();
        }

        private void getAllRegions()        //метод для отображения списка всех регионов
        {
            TreeViewItem item = new TreeViewItem() { Header = String.Format("Все регионы и округа ({0})", scenarios.regions.Count) };      //Отображение будет происходит с помощью TreeViewItem

            item.IsExpanded = true;

            foreach (KeyValuePair<string, List<string>> pair in regions)        //Для каждого округа и принадлежащих ему регионов...
            {
                TreeViewItem constitution = new TreeViewItem() { Header = String.Format("{0} ({1})", pair.Key, pair.Value.Count) };     //Создаем заголовок, содержащий название округа

                constitution.IsExpanded = true;

                foreach (string region in pair.Value)       //Для каждого региона из данного округа...
                {
                    CheckBox cb = new CheckBox();       //Создаем новывй CheckBox объект
                    cb.Content = region;        //Отображаем на нем название региона
                    cb.Checked += checkRegion;      //настраиваем обработчики событий для изменения состояния флажка
                    cb.Unchecked += checkRegion;
                    constitution.Items.Add(cb);     //Добавляем в контейнер CheckBox с регионом
                }

                item.Items.Add(constitution);       //Добавляем в контейнер уровнем выше каждый округ
            }

            categories.Items.Add(item);     //Добавляем в изначальный контейнер получившуюся структуру
        }

        private void checkRegion(object sender, RoutedEventArgs e)      //обработчик события нажатия флажка напротив региона
        {
            var checkbox = (CheckBox)sender;
            string content = checkbox.Content.ToString();
            if (checkbox.IsChecked == true) try
                {
                    region_query.Add(content);
                    regions_text.Text = String.Join(", ", region_query);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Произошла ошибка. Элемент уже находится в наборе.");
                }
            else try        //обработчик события снятия флажка с региона
                {
                    region_query.Remove(content);
                    regions_text.Text = String.Join(", ", region_query);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Произошла ошибка. Элемент уже находится в наборе.");
                }
        }

        private void getCategories()
        {
            foreach (string category in scenarios.categories)
                categories_box.Items.Add(new ComboBoxItem() { Content = category });
        }

        private void getYears()
        {
            for (int year = 2022; year < 2026; year++)
                years_box.Items.Add(new ComboBoxItem() { Content = String.Format("Прогноз на {0} год", year) });
        }

        private async void getData(int year_request)
        {            
            foreach (string scenario_name in scenarios.scenario_name)
            {
                var item = new TabItem();

                item.Header = scenario_name;
                item.Background = Auxiliary.ColorsStructure.getColor(scenario_name);

                var data_grid = new DataGrid();

                await Task.Run(() => Dispatcher.Invoke(() => this.current_table = scenarios.getScenarioData(year_request, region_query, scenario_name)));

                data_grid.AutoGeneratingColumn += r2_AutoGeneratingColumn;
                data_grid.ItemsSource = current_table.AsDataView();

                item.Content = data_grid;

                scenarios_tab.Items.Add(item);
            }

            graphs_grid.Visibility = Visibility.Hidden;
            change_picture.Content = "Перейти к графикам";

            scenarios_tab.Visibility = Visibility.Visible;

            change_picture.IsEnabled = true;
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

        private void show_hide_Click(object sender, RoutedEventArgs e)
        {
            if (region_grid.Visibility == Visibility.Hidden)
            {
                region_grid.Visibility = Visibility.Visible;
                show_hide.Content = "Скрыть";
            }
            else
            {
                region_grid.Visibility = Visibility.Hidden;
                show_hide.Content = "Показать";
            }
        }

        private async void categories_box_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            loading_graphs.Visibility = Visibility.Visible;
            await Task.Run(() => Dispatcher.Invoke(() => createGraphs(categories_box.SelectedIndex, this.ActualHeight, graphs_grid.ActualWidth)));
            loading_graphs.Visibility = Visibility.Hidden;

        }

        private async void years_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "" && categories_box.SelectedIndex != -1)
            {
                loading.Visibility = Visibility.Visible;

                scenarios_tab.Items.Clear();
                await Task.Run(() => Dispatcher.Invoke(() => getData(years_box.SelectedIndex)));
                loading.Visibility = Visibility.Hidden;
            }
        }

        private async void regions_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (categories_box.SelectedIndex != -1 && years_box.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "")
            {
                loading.Visibility = Visibility.Visible;

                scenarios_tab.Items.Clear();
                await Task.Run(() => Dispatcher.Invoke(() => getData(years_box.SelectedIndex)));
                loading.Visibility = Visibility.Hidden;
            }
        }

        private async void createGraphs(int selected_index, double height, double width)
        {
            response_bundle = new List<object>();
            await Task.Run(() => Dispatcher.Invoke(() => response_bundle = scenarios.getCategoriesData(selected_index, height, width)));

            graphs_list.ItemsSource = response_bundle;
            graphs_grid.Visibility = Visibility.Visible;

            change_picture.Visibility = Visibility.Visible;
            change_picture.Content = "Перейти к прогнозам";

            if (scenarios_tab.Visibility == Visibility.Hidden) change_picture.IsEnabled = false;
        }

        private void change_picture_Click(object sender, RoutedEventArgs e)
        {
            if (graphs_grid.Visibility == Visibility.Visible)
            {
                change_picture.Content = "Перейти к графикам";
                graphs_grid.Visibility = Visibility.Hidden;
                scenarios_tab.Visibility = Visibility.Visible;
            }
            else
            {
                change_picture.Content = "Перейти к прогнозам";
                graphs_grid.Visibility = Visibility.Visible;
                scenarios_tab.Visibility = Visibility.Hidden;
            }
        }

        private async void export_to_exc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (current_table == null || response_bundle == null) throw new ArgumentNullException();

                loading.Visibility = Visibility.Visible;
                await Task.Run(() => Dispatcher.Invoke(() => Entities.ExcelRecorder.writeToExcel(response_bundle, scenarios_tab.Items, region_query, categories_box.Text, years_box.Text)));

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
    }
}
