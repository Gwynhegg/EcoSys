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
        }

        public void hideGrid()
        {
            this.Visibility = Visibility.Hidden;
        }

        public void showGrid()
        {
            this.Visibility = Visibility.Visible;
        }
    }
}
