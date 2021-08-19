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
    /// Логика взаимодействия для GreetsGrid.xaml
    /// </summary>
    public partial class GreetsGrid : UserControl, IGrid
    {

        System.Windows.Controls.Primitives.Popup pop;
        public GreetsGrid()
        {
            InitializeComponent();

            pop = new System.Windows.Controls.Primitives.Popup() { Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse, Child = new TextBlock() { Text = "Скопировано в буфер обмена", Background = Brushes.White, FontSize = 14, Padding =  new Thickness(2,2,2,2)} };
        }

        public void hideGrid()
        {
            this.Visibility = Visibility.Hidden;
        }

        public void showGrid()
        {
            this.Visibility = Visibility.Visible;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, "itchepurov@yandex.ru");
            pop.IsOpen = true;
        }

        private void Hyperlink_MouseLeave(object sender, MouseEventArgs e)
        {
            pop.IsOpen = false;
        }
    }
}
