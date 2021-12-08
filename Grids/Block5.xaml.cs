using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для Block5.xaml
    /// </summary>
    public partial class Block5 : UserControl, IGrid
    {
        private readonly Entities.ModelEntity model;
        private readonly Entities.ScenarioEntity scenario;
        private DataTable final_table = null;
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
            {
                used_region_box.Items.Add(new ComboBoxItem() { Content = region });
            }
        }
        public void checkFields()
        {
            for (int i = 0; i < equations_list.Items.Count; i++)
            {
                if ((equations_list.Items[i] as Grids.EquationHolder).parser.getAnswerToString() is null)
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
                answers[equation.getUsedCategory() - 1] = equation.parser.answer;
            }
            if (answers.Count(x => x != null) == equations_list.Items.Count)
            {
                finalizeTable(answers);
                finalize_btn.IsEnabled = true;
            }
        }
        private void finalizeTable(double?[] answers)
        {
            final_table = model.getCalculatedModelData(answers, scenario.scenario_models[model.regions[used_region_box.SelectedIndex]]);
            final_grid.ItemsSource = final_table.AsDataView();
            final_grid.Columns[0].CanUserSort = false;
            Auxiliary.GridStandard.standardizeGrid(final_grid);
        }
        private void used_region_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            equations_list.Items.Clear();
            foreach (string category in model.categories)
            {
                try
                {
                    var temp = model.model_equations[(model.regions[used_region_box.SelectedIndex], category)];
                    equations_list.Items.Add(new Grids.EquationHolder(this, category, temp, model.tips[category]) { Width = equations_list.ActualWidth, Height = equations_list.ActualHeight / 6 });
                }
                catch
                {
                    Console.WriteLine("Данное значение отсутствует в спиcке");
                }
            }

            equations_list.Visibility = Visibility.Visible;
            command_buttons.Visibility = Visibility.Visible;
            to_equation.Visibility = Visibility.Hidden;
            finalize_btn.Visibility = Visibility.Visible;
            final_grid.Visibility = Visibility.Hidden;
            finalize_btn.IsEnabled = false;
            export_to_exc.IsEnabled = false;
            to_equation.Visibility = Visibility.Hidden;
        }
        private void finalize_btn_Click(object sender, RoutedEventArgs e)
        {
            equations_list.Visibility = Visibility.Hidden;
            final_grid.Visibility = Visibility.Visible;
            export_to_exc.IsEnabled = true;
            finalize_btn.Visibility = Visibility.Visible;
            to_equation.Visibility = Visibility.Visible;
        }
        private void to_equation_Click(object sender, RoutedEventArgs e)
        {
            equations_list.Visibility = Visibility.Visible;
            final_grid.Visibility = Visibility.Hidden;
            to_equation.Visibility = Visibility.Hidden;
        }
        private void clear_fields_Click(object sender, RoutedEventArgs e)
        {
            to_equation.Visibility = Visibility.Hidden;
            final_grid.Visibility = Visibility.Hidden;
            equations_list.Visibility = Visibility.Visible;
            for (int i = 0; i < equations_list.Items.Count; i++)
            {
                (equations_list.Items[i] as Grids.EquationHolder).clearValues();
            }
        }
        private async void export_to_exc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (final_table == null)
                {
                    throw new ArgumentNullException();
                }

                loading.Visibility = Visibility.Visible;
                await Task.Run(() => this.Dispatcher.Invoke(() => Entities.ExcelRecorder.writeToExcel(final_table, used_region_box.Text)));
            }
            catch (ArgumentNullException)
            {
                var dialog_result = MessageBox.Show("Не выбрана таблица для импортирования", "", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK)
                {
                    return;
                }
            }
            catch (Exception)
            {
                var dialog_result = MessageBox.Show("Проблема при инициализации работы с Excel (допустимо несоответствие версий)", "", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK)
                {
                    return;
                }
            }
            finally
            {
                loading.Visibility = Visibility.Hidden;
            }
        }
        public void hideGrid() => this.Visibility = Visibility.Hidden;
        public void showGrid() => this.Visibility = Visibility.Visible;
    }
}
