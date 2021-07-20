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

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для Block1.xaml
    /// </summary>
    public partial class Block1 : UserControl, IGrid
    {
        byte work_num = 1;

        HashSet<string> region_query;

        Entities.DataEntity data;

        WorkWindow parent_window;

        public Block1(Entities.DataEntity data, WorkWindow parent_window)
        {
            InitializeComponent();
            this.data = data;
            this.parent_window = parent_window;
            region_query = new HashSet<string>();

            getAllRegions();
            getYears();
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
            foreach (string year in data.years)
                year_choose.Items.Add(new ComboBoxItem() { Content = year });
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
            if (year_choose.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "") getData();
                
        }

        private void year_choose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (matrix_type.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "") getData();
        }

        private void regions_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (matrix_type.SelectedIndex != -1 && year_choose.SelectedIndex != -1) getData();
        }

        private void getData()      //привязка к выбранному типу матрицы и отправление соответствующих запросов
        {
            switch (matrix_type.SelectedIndex)
            {
                case 0:
                    data_field.ItemsSource = data.getPassiveData(region_query, year_choose.Text).DefaultView;
                    break;
                case 1:
                    data_field.ItemsSource = data.getActiveData(region_query, year_choose.Text).DefaultView;
                    break;
                case 2:
                    data_field.ItemsSource = data.getBalanceData(region_query, year_choose.Text).DefaultView;
                    break;
            }
            data_field.Visibility = Visibility.Visible;
        }
    }
}
