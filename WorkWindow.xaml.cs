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

        public WorkWindow(Entities.DataEntity data)
        {
            InitializeComponent();

            this.data = data;
        }

        ~WorkWindow()
        {
            GC.Collect();
        }


        private void Settings_Click_1(object sender, RoutedEventArgs e)
        {
            if (!alreadyExist<Grids.SettingsGrid>())        //Если элемента нет - создаем его
            {
                var settings = new Grids.SettingsGrid(data, this);
                Grid.SetColumn(settings, 0);
                Grid.SetColumnSpan(settings, 2);
                Grid.SetRow(settings, 1);
                main_grid.Children.Insert(0, settings);
            }

            main_menu.IsExpanded = false;
        }

        private void Block1Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!alreadyExist<Grids.Block1>())
            {
                var block1 = new Grids.Block1(data, this);
                Grid.SetColumn(block1, 0);
                Grid.SetColumnSpan(block1, 2);
                Grid.SetRow(block1, 1);
                main_grid.Children.Insert(0, block1);
            }
            main_menu.IsExpanded = false;
        }

        private bool alreadyExist<T>()      //Проверка существования с использованием обобщенного типа
        {
            if (main_grid.Children.OfType<T>().Any())      //Если элемент уже есть, то скрываем все другие гриды и показываем его
            {
                foreach (object grid in main_grid.Children)
                    if (grid is Grids.IGrid)
                        if (grid is T) ((Grids.IGrid)grid).showGrid(); else ((Grids.IGrid)grid).hideGrid();
                return true;
            }
            else return false;
        }
    }
}
