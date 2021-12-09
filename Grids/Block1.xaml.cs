using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для Block1.xaml
    /// </summary>
    public partial class Block1 : UserControl, IGrid
    {
        private readonly HashSet<string> region_query;       //Строка запроса, содержащая перечисление регионов, по которым будет происходить выборка
        private readonly Entities.DataEntity data;       //Передаваемый data-объект
        private DataTable current_table = null;     //ВСпомогательный DataTable, посредник между Data и интерфейсом
        private readonly Dictionary<string, List<string>> regions;       //Словарь округов и принадлежащих к ним регионов
        private System.Windows.Controls.Primitives.Popup pop;

        public Block1(Entities.DataEntity data, Dictionary<string, List<string>> regions)
        {
            InitializeComponent();
            this.data = data;
            region_query = new HashSet<string>();
            this.regions = regions;
            createPopUp();
            Auxiliary.ContextMenuCreator.createContextMenu(data_field);

            getAllRegions();
            getYears();
        }
        ~Block1()
        {
            GC.Collect();
        }
        private void getAllRegions()        //Метод для отображения списка всех регионов в обозначенном Grid
        {
            TreeViewItem item = new TreeViewItem() { Header = String.Format("Все регионы и округа ({0})", data.regions.Count) };      //Отображение будет происходит с помощью TreeViewItem

            item.IsExpanded = true;

            foreach (KeyValuePair<string, List<string>> pair in regions)        //Для каждого округа и принадлежащих ему регионов...
            {
                TreeViewItem constitution = new TreeViewItem() { Header = String.Format("{0} ({1})", pair.Key, pair.Value.Count) };     //Создаем заголовок, содержащий название округа

                constitution.IsExpanded = true;

                CheckBox select_all = new CheckBox() { Content = "Выбрать все" };
                select_all.Checked += checkAll;
                select_all.Unchecked += checkAll;
                constitution.Items.Add(select_all);
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
        //Блок для возможной доработки функции "Выбрать все внутри округа"
        //private void checkAllChildren(object sender, RoutedEventArgs e)
        //{
        //    var checkbox = (CheckBox)sender;
        //    var tree = (TreeViewItem)categories.Items[0];
        //    if (checkbox.IsChecked == true)
        //        foreach (object item in tree.Items)
        //        {
        //            int a = 0;
        //            if (item is CheckBox) ((CheckBox)item).IsChecked = true;

        //        }
        //}
        private void getYears()     //добавление списка всех годов, встреченных в файле
        {
            foreach (string year in data.years)
            {
                year_choose.Items.Add(new ComboBoxItem() { Content = year });       //Для каждого года создаем свой комбобокс, помещаем его в необходимый контейнер
            }
        }
        private async void getData(int selected_index, string selected_year)      //привязка к выбранному типу матрицы и отправление соответствующих запросов
        {
            switch (selected_index)     //В соответствии с выбранным типом матрицы вызываем соответствующую функцию объекта data, передавая вышеуказанные аргументы
            {
                case 0:
                    await Task.Run(() => current_table = data.getPassiveData(region_query, selected_year));
                    break;
                case 1:
                    await Task.Run(() => current_table = data.getActiveData(region_query, selected_year));
                    break;
                case 2:
                    await Task.Run(() => current_table = data.getBalanceData(region_query, selected_year));
                    break;
            }
            data_field.ItemsSource = current_table.AsDataView();        //устанавливаем полученную через словарь таблицу в качестве представления данных
            data_field.Columns[0].CanUserSort = false;
            data_field.Visibility = Visibility.Visible;
            Auxiliary.GridStandard.standardizeGrid(data_field);
        }
        private void Button_Click(object sender, RoutedEventArgs e)     //Обработчик события нажатия мыши на кнопку "Скрыть/Показать". Делает с Grid регионов то, из-за чего так и названа
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
        private void checkRegion(object sender, RoutedEventArgs e)      //обработчик события изменения параметра IsChecked у CheckBox регионов
        {
            var checkbox = (CheckBox)sender;
            string content = checkbox.Content.ToString();
            if (checkbox.IsChecked == true)
            {
                try
                {
                    region_query.Add(content);      //Если данный объект выбран, то регион добавляет в массив запросов
                    regions_text.Text = String.Join(", ", region_query);        //Элементы обновленного массива отображаются на форме
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

            TreeViewItem tree_ancestor = (sender as CheckBox).Parent as TreeViewItem;
        }
        private void checkAll(object sender, RoutedEventArgs e)
        {
            TreeViewItem tree_ancestor = (sender as CheckBox).Parent as TreeViewItem;
            if ((sender as CheckBox).IsChecked == true)
                for (int i = 1; i < tree_ancestor.Items.Count; i++)
                    (tree_ancestor.Items[i] as CheckBox).IsChecked = true;
            else
                for (int i = 1; i < tree_ancestor.Items.Count; i++)
                    (tree_ancestor.Items[i] as CheckBox).IsChecked = false;
        }
        private async void matrix_type_SelectionChanged(object sender, SelectionChangedEventArgs e)       //Было принято решение о динамической загрузке таблицы
        {
            if (year_choose.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "")
            {
                loading.Visibility = Visibility.Visible;
                await Task.Run(() => this.Dispatcher.Invoke(() => getData(matrix_type.SelectedIndex, ((ComboBoxItem)year_choose.SelectedItem).Content.ToString())));
                loading.Visibility = Visibility.Hidden;
            }
        }
        private async void year_choose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (matrix_type.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "")
            {
                loading.Visibility = Visibility.Visible;
                await Task.Run(() => this.Dispatcher.Invoke(() => getData(matrix_type.SelectedIndex, ((ComboBoxItem)year_choose.SelectedItem).Content.ToString())));
                loading.Visibility = Visibility.Hidden;
            }
        }
        private async void regions_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (matrix_type.SelectedIndex != -1 && year_choose.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "")
            {
                loading.Visibility = Visibility.Visible;
                await Task.Run(() => this.Dispatcher.Invoke(() => getData(matrix_type.SelectedIndex, ((ComboBoxItem)year_choose.SelectedItem).Content.ToString())));
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
                    await Task.Run(() => this.Dispatcher.Invoke(() => Auxiliary.ExcelRecorder.writeToExcel(current_table, region_query, year_choose.Text, matrix_type.Text)));
                }
                else if (result == MessageBoxResult.Yes)
                {
                    var selected_year = ((ComboBoxItem)year_choose.SelectedItem).Content.ToString();

                    List<DataTable> output_tables = null;

                    loading.Visibility = Visibility.Visible;
                    await Task.Run(() => output_tables = new List<DataTable>() { data.getPassiveData(region_query, selected_year), data.getActiveData(region_query, selected_year), data.getBalanceData(region_query, selected_year) });

                    await Task.Run(() => this.Dispatcher.Invoke(() => Auxiliary.ExcelRecorder.writeToExcel(output_tables, region_query, year_choose.Text)));
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
        private void createPopUp()
        {
            pop = new System.Windows.Controls.Primitives.Popup();
            var text = "\n  - Для выбора нужного региона нажмите на кнопку \"Показать\", а затем выделить интересующие регионы  \n" +
                "  - Поля снизу предназначены для выбора интересующего вас года и типа матрицы \n" +
                "  - После заполнения всех полей будет построена таблица, которая будет отображена на экране \n" +
                "  - С помощью кнопки \"Экспорт\" вы можете создать Excel-файл, содержащий полученную таблицу \n";
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
