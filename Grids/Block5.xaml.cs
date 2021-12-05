using System;
using System.Collections.Generic;
using System.Text;
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
    /// Логика взаимодействия для Block5.xaml
    /// </summary>
    public partial class Block5 : UserControl, IGrid
    {
        Entities.ModelEntity model;
        Entities.ScenarioEntity scenario;
        public Block5(Entities.ModelEntity model, Entities.ScenarioEntity scenario)
        {
            InitializeComponent();
            this.model = model;
            this.scenario = scenario;
            getRegions();
        }

        private void getRegions()
        {
            foreach (string region in model.regions)
                used_region_box.Items.Add(new ComboBoxItem() { Content = region});
        }

        public void hideGrid()
        {
            this.Visibility = Visibility.Hidden;
        }

        public void showGrid()
        {
            this.Visibility = Visibility.Visible;
        }

        private void used_region_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            equations_list.Items.Clear();
            foreach (string category in model.categories)
                try
                {
                    var temp = model.model_equations[(model.regions[used_region_box.SelectedIndex], category)];
                    equations_list.Items.Add(new Grids.EquationHolder(category, temp, model.tips[category]) { Width = equations_list.ActualWidth, Height = equations_list.ActualHeight / 6});
                }
                catch
                {
                    Console.WriteLine("Данное значение отсутствует в спике");
                }
            equations_list.Visibility = Visibility.Visible;
            command_buttons.Visibility = Visibility.Visible;
        }
    }
}
