using System;
using System.Collections.Generic;
using System.Data;
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
using System.Linq;

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для Block5.xaml
    /// </summary>
    public partial class Block5 : UserControl, IGrid
    {
        Entities.ModelEntity model;
        Entities.ScenarioEntity scenario;
        DataTable final_table = null;
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

        public void checkFields()
        {
            for (int i = 0; i < equations_list.Items.Count; i++)
            {
                if ((equations_list.Items[i] as Grids.EquationHolder).parser.getAnswerToString().Contains("Y"))
                {
                    finalize_btn.IsEnabled = false;
                    return;
                }
            }
            calculateFinalTable();
        }

        private void calculateFinalTable()
        {
            double?[] answers = new double?[12];
            for (int i = 0; i < equations_list.Items.Count; i++)
            {
                var equation = equations_list.Items[i] as Grids.EquationHolder;
                answers[equation.getUsedCategory()] = equation.parser.answer;
            }
            if (answers.Count(x => x != null) == equations_list.Items.Count) { 
                finalizeTable(answers);
                finalize_btn.IsEnabled = true;
            }
        }

        private void finalizeTable(double?[] answers)
        {
            final_table = model.getCalculatedModelData(answers, scenario.scenario_models[model.regions[used_region_box.SelectedIndex]]);
            final_grid.ItemsSource = final_table.AsDataView();
            Auxiliary.GridStandard.standardizeGrid(final_grid);
        }

        private void used_region_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            equations_list.Items.Clear();
            foreach (string category in model.categories)
                try
                {
                    var temp = model.model_equations[(model.regions[used_region_box.SelectedIndex], category)];
                    equations_list.Items.Add(new Grids.EquationHolder(this, category, temp, model.tips[category]) { Width = equations_list.ActualWidth, Height = equations_list.ActualHeight / 6});
                }
                catch
                {
                    Console.WriteLine("Данное значение отсутствует в спиcке");
                }
            equations_list.Visibility = Visibility.Visible;
            command_buttons.Visibility = Visibility.Visible;
            to_equation.Visibility = Visibility.Hidden;
            finalize_btn.Visibility = Visibility.Visible;
            finalize_btn.IsEnabled = false;
        }

        private void finalize_btn_Click(object sender, RoutedEventArgs e)
        {
            equations_list.Visibility = Visibility.Hidden;
            final_grid.Visibility = Visibility.Visible;
            finalize_btn.Visibility = Visibility.Visible;
        }

        private void to_equation_Click(object sender, RoutedEventArgs e)
        {
            equations_list.Visibility = Visibility.Visible;
            final_grid.Visibility = Visibility.Hidden;
            to_equation.Visibility = Visibility.Hidden;
        }
    }
}
