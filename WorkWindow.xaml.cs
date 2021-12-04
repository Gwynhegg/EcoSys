using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EcoSys
{
    /// <summary>
    /// Логика взаимодействия для WorkWindow.xaml
    /// </summary>
    public partial class WorkWindow : Window
    {
        Entities.DataEntity data;
        Entities.ScenarioEntity scenarios;
        Entities.ModelEntity model;
        private bool auto_launch;

        public WorkWindow(Entities.DataEntity data, Entities.ScenarioEntity scenario, Entities.ModelEntity model, bool auto_launch)
        {
            InitializeComponent();

            this.auto_launch = auto_launch;
            this.data = data;       //передаем созданные объекты в рабочую область
            this.scenarios = scenario;
            this.model = model;

            var grid = new Grids.GreetsGrid();      //добавляем Grid приветственного экрана с отображение подсказок по работе с системой. В дальнейшем данный экран будет скрыт
            Grid.SetColumn(grid, 0);
            Grid.SetColumnSpan(grid, 2);
            Grid.SetRow(grid, 1);
            main_grid.Children.Insert(0, grid);

        }


        ~WorkWindow()
        {
            GC.Collect();
        }


        private void BlockBtnClick(object sender, RoutedEventArgs e)        //Обработчик события нажатия на кнопки (нужный метод определяется по имени кнопки-отправителя)
        {
            switch (((Button)sender).Name)
            {
                case "Settings":
                    if (!alreadyExist<Grids.SettingsGrid>())        //Проверка на существование указанного элемента Grid   
                        createNewGrid(new Grids.SettingsGrid(data, scenarios, this, auto_launch));       //Если элемента нет - переходим к функции его создания
                    break;
                case "Block1Btn":
                    if (!alreadyExist<Grids.Block1>())
                        createNewGrid(new Grids.Block1(data, Auxiliary.Regions.createConstituencies(data.regions)));
                    break;
                case "Block2Btn":
                    if (!alreadyExist<Grids.Block2>())
                        createNewGrid(new Grids.Block2(data, Auxiliary.Regions.createConstituencies(data.regions)));
                    break;
                case "Block3Btn":
                    if (!alreadyExist<Grids.Block3>())
                        createNewGrid(new Grids.Block3(scenarios));
                    break;
                case "Block4Btn":
                    if (!alreadyExist<Grids.Block4>())
                        createNewGrid(new Grids.Block4(scenarios, Auxiliary.Regions.createConstituencies(scenarios.regions)));
                    break;
                case "Block5Btn":
                    if (!alreadyExist<Grids.Block5>())
                        createNewGrid(new Grids.Block5(model, scenarios));
                    break;
            }

            main_menu.IsExpanded = false;       //Скрываем меню

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

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (object grid in main_grid.Children)
                if (grid is Grids.SettingsGrid)
                {
                    var writer = new System.IO.StreamWriter("settings");
                    writer.Write("auto_launch=" + ((Grids.SettingsGrid)grid).isAutolaunchActive());
                    writer.Close();
                }
        }
    }
}
