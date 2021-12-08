using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для Block1.xaml
    /// </summary>
    public partial class Block2 : UserControl, IGrid
    {
        private readonly HashSet<string> region_query;
        private readonly Entities.DataEntity data;
        private DataTable current_table = null;
        private readonly Dictionary<string, List<string>> regions;

        public Block2(Entities.DataEntity data, Dictionary<string, List<string>> regions)
        {
            InitializeComponent();
            this.data = data;
            region_query = new HashSet<string>();
            this.regions = regions;

            getAllRegions();
            getYears();
        }
        ~Block2()
        {
            GC.Collect();
        }
        private void getAllRegions()        //метод для отображения списка всех регионов
        {
            TreeViewItem item = new TreeViewItem() { Header = String.Format("Все регионы и округа ({0})", data.regions.Count) };      //Отображение будет происходит с помощью TreeViewItem

            item.IsExpanded = true;

            foreach (KeyValuePair<string, List<string>> pair in regions)        //Для каждого округа и принадлежащих ему регионов...
            {
                TreeViewItem constitution = new TreeViewItem() { Header = String.Format("{0} ({1})", pair.Key, pair.Value.Count) };     //Создаем заголовок, содержащий название округа

                constitution.IsExpanded = true;

                foreach (string region in pair.Value)       //Для каждого региона из данного округа...
                {
                    CheckBox cb = new CheckBox
                    {
                        Content = region        //Отображаем на нем название региона
                    };       //Создаем новывй CheckBox объект
                    cb.Checked += checkRegion;      //настраиваем обработчики событий для изменения состояния флажка
                    cb.Unchecked += checkRegion;
                    constitution.Items.Add(cb);     //Добавляем в контейнер CheckBox с регионом
                }

                item.Items.Add(constitution);       //Добавляем в контейнер уровнем выше каждый округ
            }

            categories.Items.Add(item);     //Добавляем в изначальный контейнер получившуюся структуру

        }
        private void getYears()     //добавление списка всех годов, встреченных в файле
        {
            for (int i = 1; i < data.years.Count; i++)
            {
                year_choose.Items.Add(new ComboBoxItem() { Content = ("На 1 января " + data.years.ElementAt(i) + "а") });
            }

            string[] temp = data.years.Last<string>().Split(' ');
            year_choose.Items.Add(new ComboBoxItem() { Content = "На 1 января " + (Int32.Parse(temp[0]) + 1) + " " + temp[1] + "а" });
        }
        private async void getData(int selected_index, int selected_year_index)      //привязка к выбранному типу матрицы и отправление соответствующих запросов
        {
            switch (selected_index)
            {
                case 0:
                    await Task.Run(() => current_table = data.getPassiveData(region_query, selected_year_index));
                    break;
                case 1:
                    await Task.Run(() => current_table = data.getActiveData(region_query, selected_year_index));
                    break;
                case 2:
                    await Task.Run(() => current_table = data.getBalanceData(region_query, selected_year_index));
                    break;
            }

            data_field.ItemsSource = current_table.AsDataView();        //устанавливаем полученную через словарь таблицу в качестве представления
            data_field.Columns[0].CanUserSort = false;
            data_field.Visibility = Visibility.Visible;
            Auxiliary.GridStandard.standardizeGrid(data_field);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
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
        private void checkRegion(object sender, RoutedEventArgs e)      //обработчик события нажатия флажка напротив региона
        {
            var checkbox = (CheckBox)sender;
            string content = checkbox.Content.ToString();
            if (checkbox.IsChecked == true)
            {
                try
                {
                    region_query.Add(content);
                    regions_text.Text = String.Join(", ", region_query);
                }
                catch (Exception)
                {
                    Console.WriteLine("Произошла ошибка. Элемент уже находится в наборе.");
                }
            }
            else
            {
                try        //обработчик события снятия флажка с региона
                {
                    region_query.Remove(content);
                    regions_text.Text = String.Join(", ", region_query);
                }
                catch (Exception)
                {
                    Console.WriteLine("Произошла ошибка. Элемент уже находится в наборе.");
                }
            }
        }
        private async void matrix_type_SelectionChanged(object sender, SelectionChangedEventArgs e)       //Было принято решение о динамической загрузке таблицы
        {
            if (year_choose.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "")
            {
                loading.Visibility = Visibility.Visible;
                await Task.Run(() => this.Dispatcher.Invoke(() => getData(matrix_type.SelectedIndex, year_choose.SelectedIndex + 1)));
                loading.Visibility = Visibility.Hidden;
            }


        }
        private async void year_choose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (matrix_type.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "")
            {
                loading.Visibility = Visibility.Visible;
                await Task.Run(() => this.Dispatcher.Invoke(() => getData(matrix_type.SelectedIndex, year_choose.SelectedIndex + 1)));
                loading.Visibility = Visibility.Hidden;
            }
        }
        private async void regions_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (matrix_type.SelectedIndex != -1 && year_choose.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "")
            {
                loading.Visibility = Visibility.Visible;
                await Task.Run(() => this.Dispatcher.Invoke(() => getData(matrix_type.SelectedIndex, year_choose.SelectedIndex + 1)));
                loading.Visibility = Visibility.Hidden;
            }
        }
        private async void export_to_exc_Click(object sender, RoutedEventArgs e)      //экспорт полученного DataTable в Excel
        {
            try
            {
                if (current_table == null)
                    throw new ArgumentNullException();

                var result = MessageBox.Show("Вы собираетесь импортировать текущую таблицу в Excel. Импортировать также остальные типы матрицы?", "Импорт таблицы", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    loading.Visibility = Visibility.Visible;
                    await Task.Run(() => this.Dispatcher.Invoke(() => Entities.ExcelRecorder.writeToExcel(current_table, region_query, year_choose.Text, matrix_type.Text)));
                }
                else if (result == MessageBoxResult.Yes)
                {
                    int selected_year = year_choose.SelectedIndex + 1;

                    List<DataTable> output_tables = null;

                    loading.Visibility = Visibility.Visible;
                    await Task.Run(() => output_tables = new List<DataTable>() { data.getPassiveData(region_query, selected_year), data.getActiveData(region_query, selected_year), data.getBalanceData(region_query, selected_year) });

                    await Task.Run(() => this.Dispatcher.Invoke(() => Entities.ExcelRecorder.writeToExcel(output_tables, region_query, year_choose.Text)));
                }
                else
                    return;
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
    }
}
