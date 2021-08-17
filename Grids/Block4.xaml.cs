﻿using System;
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


        public Block4(Entities.ScenarioEntity scenarios)
        {
            InitializeComponent();

            this.scenarios = scenarios;
            region_query = new HashSet<string>();

            getAllRegions();
            getCategories();
            getYears();
        }

        ~Block4()
        {
            GC.Collect();
        }

        private void getAllRegions()
        {
            TreeViewItem item = new TreeViewItem() { Header = String.Format("Все регионы ({0})", scenarios.regions.Count) };

            item.IsExpanded = true;

            foreach (string region in scenarios.regions)     //заполняем лист необходимыми чекбоксами
            {
                CheckBox cb = new CheckBox();
                cb.Content = region;
                cb.Checked += checkRegion;      //настраиваем обработчики событий для изменения состояния флажка
                cb.Unchecked += checkRegion;
                item.Items.Add(cb);
            }
            categories.Items.Add(item);
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

        private void categories_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getData();
        }

        private async void getData()
        {
            loading.Visibility = Visibility.Visible;

            scenarios_tab.Items.Clear();

            int year_request = years_box.SelectedIndex;
            
            foreach (string scenario_name in scenarios.scenario_name)
            {
                var item = new TabItem();
                item.Header = scenario_name;

                var data_grid = new DataGrid();

                foreach (string region in region_query)
                {
                    await Task.Run(() => Dispatcher.Invoke(() => data_grid.ItemsSource = scenarios.getScenarioData(year_request, region, scenario_name).AsDataView()));
                }

                item.Content = data_grid;

                scenarios_tab.Items.Add(item);
            }

            loading.Visibility = Visibility.Hidden;
            graphs_grid.Visibility = Visibility.Hidden;
            change_picture.Content = "Перейти к графикам";

            scenarios_tab.Visibility = Visibility.Visible;

            change_picture.IsEnabled = true;
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

        private void categories_box_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            createGraphs();
            if (regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "" && years_box.SelectedIndex != -1)
                getData();
        }

        private void years_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "" && categories_box.SelectedIndex != -1)
                getData();
        }

        private void regions_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (categories_box.SelectedIndex != -1 && years_box.SelectedIndex != -1 && regions_text.Text != "Нажмите на кнопку справа для выбора регионов..." && regions_text.Text != "")
                getData();
        }

        private async void createGraphs()
        {
            loading_graphs.Visibility = Visibility.Visible;

            int selected_index = categories_box.SelectedIndex;
            double height = this.ActualHeight;
            double width = graphs_grid.ActualWidth;

            var response_bundle = new List<object>();
            await Task.Run(() => Dispatcher.Invoke(() => response_bundle = scenarios.getCategoriesData(selected_index, height, width)));

            loading_graphs.Visibility = Visibility.Hidden;

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
    }
}