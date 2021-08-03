using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для Block1.xaml
    /// </summary>
    public partial class Block2 : UserControl, IGrid
    {

        private HashSet<string> region_query;

        private Entities.DataEntity data;

        private DataTable current_table = null;

        public Block2(Entities.DataEntity data)
        {
            InitializeComponent();
            this.data = data;
            region_query = new HashSet<string>();

            getAllRegions();
            getYears();
        }

        ~Block2()
        {
            GC.Collect();
        }

        public void hideGrid()
        {
            this.Visibility = Visibility.Hidden;
        }

        public void showGrid()
        {
            this.Visibility = Visibility.Visible;
        }
        
        private void getAllRegions()        //метод для отображения списка всех регионов
        {
            TreeViewItem item = new TreeViewItem() { Header = String.Format("Все регионы ({0})",data.regions.Count)};

            item.IsExpanded = true;

            foreach (string region in data.regions)     //заполняем лист необходимыми чекбоксами
            {
                CheckBox cb = new CheckBox();
                cb.Content = region;
                cb.Checked += checkRegion;      //настраиваем обработчики событий для изменения состояния флажка
                cb.Unchecked += checkRegion;
                item.Items.Add(cb);
            }
            categories.Items.Add(item);
           
        }

        private void getYears()     //добавление списка всех годов, встреченных в файле
        {
            for (int i=1; i < data.years.Count; i++)
                year_choose.Items.Add(new ComboBoxItem() { Content = ("На 1 января " + data.years.ElementAt(i) + "а") });

            string[] temp = data.years.Last<string>().Split(' ');
            year_choose.Items.Add(new ComboBoxItem() { Content = "На 1 января " + (Int32.Parse(temp[0]) + 1) + " " + temp[1] + "а"});
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (region_grid.Visibility == Visibility.Hidden)
            {
                region_grid.Visibility = Visibility.Visible;
                show_hide.Content = "Скрыть";
            } else
            {
                region_grid.Visibility = Visibility.Hidden;
                show_hide.Content = "Показать";
            }
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

        private void matrix_type_SelectionChanged(object sender, SelectionChangedEventArgs e)       //Было принято решение о динамической загрузке таблицы
        {
            if (year_choose.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "") 
                getData();
                
        }

        private void year_choose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (matrix_type.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "") 
                getData();
        }

        private void regions_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (matrix_type.SelectedIndex != -1 && year_choose.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "") 
                getData();
        }

        private void getData()      //привязка к выбранному типу матрицы и отправление соответствующих запросов
        {

            int selected_index = matrix_type.SelectedIndex;
            int selected_year_index = year_choose.SelectedIndex + 1;

            switch (selected_index)
            {
                case 0:
                    this.current_table = data.getPassiveData(region_query, selected_year_index);
                    break;
                case 1:
                    this.current_table = data.getActiveData(region_query, selected_year_index);
                    break;
                case 2:
                    this.current_table = data.getBalanceData(region_query, selected_year_index);
                    break;
            }

            this.data_field.ItemsSource = current_table.AsDataView();        //устанавливаем полученную через словарь таблицу в качестве представления
            this.data_field.Visibility = Visibility.Visible;
        }

        private void r2_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)        //Кастомный метод для правильного отображения заголовков
        {
            if (e.PropertyName.Contains('.') && e.Column is DataGridBoundColumn)
            {
                DataGridBoundColumn dataGridBoundColumn = e.Column as DataGridBoundColumn;
                dataGridBoundColumn.Binding = new Binding("[" + e.PropertyName + "]");
            }
        }

        private void export_to_exc_Click(object sender, RoutedEventArgs e)      //экспорт полученного DataTable в Excel
        {
            try
            {
                Entities.ExcelRecorder.writeToExcel(current_table, region_query, year_choose.Text, matrix_type.Text);
            }
            catch
            {
                var dialog_result = MessageBox.Show("Проблема при инициализации работы с Excel (допустимо несоответствие версий)", "", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK) return;
            }
        }
    }
}
