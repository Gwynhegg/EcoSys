using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EcoSys
{
    /// <summary>
    /// Логика взаимодействия для WorkWindow.xaml
    /// </summary>
    public partial class WorkWindow : Window
    {
        Entities.DataEntity data;
        Entities.ScenarioEntity scenarios;

        public WorkWindow(Entities.DataEntity data, Entities.ScenarioEntity scenario)
        {
            InitializeComponent();

            this.data = data;
            this.scenarios = scenario;
        }

        ~WorkWindow()
        {
            GC.Collect();
        }


        private void Settings_Click_1(object sender, RoutedEventArgs e)
        {
            if (!alreadyExist<Grids.SettingsGrid>())        //Если элемента нет - создаем его
            createNewGrid(new Grids.SettingsGrid(data, scenarios, this));

            main_menu.IsExpanded = false;
        }

        private void Block1Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!alreadyExist<Grids.Block1>())
                createNewGrid(new Grids.Block1(data));

            main_menu.IsExpanded = false;
        }

        private void Block2Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!alreadyExist<Grids.Block2>())
                createNewGrid(new Grids.Block2(data));

            main_menu.IsExpanded = false;
        }

        private void BlockBtnClick(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Name)
            {
                case "Block1Btn":
                    if (!alreadyExist<Grids.Block1>())
                        createNewGrid(new Grids.Block1(data));
                    break;
                case "Block2Btn":
                    if (!alreadyExist<Grids.Block2>())
                        createNewGrid(new Grids.Block2(data));
                    break;
                case "Block3Btn":
                    if (!alreadyExist<Grids.Block2>())
                        createNewGrid(new Grids.Block3(scenarios));
                    break;

            }

            main_menu.IsExpanded = false;

        }

        private bool alreadyExist<T>()      //Проверка существования с использованием обобщенного типа
        {
                foreach (object grid in main_grid.Children)     //Пробегаем по всем дочерним элементам главного окна
                    if (grid is Grids.IGrid)        //Поскольку мы работаем только с кастомными гридами, не затрагиваем все остальное
                        if (grid is T) ((Grids.IGrid)grid).showGrid(); else ((Grids.IGrid)grid).hideGrid();     //Если грид найден, то показываем его, скрывая остальные. Работает даже в случаях, когда грида еще нет
            if (main_grid.Children.OfType<T>().Any()) return true; else return false;       //говорим о необходимости создавать новый грид
        }


        private void createNewGrid(UIElement grid)      //Создание нового пользовательского элемента и отображение его на главной форме
        {
            Grid.SetColumn(grid, 0);
            Grid.SetColumnSpan(grid, 2);
            Grid.SetRow(grid, 1);
            main_grid.Children.Insert(0, grid);
        }


    }
}
