using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EcoSys.Auxiliary
{
    public static class PopUpCreator
    {
        public static System.Windows.Controls.Primitives.Popup getPopUp(string text)
        {
            var pop = new System.Windows.Controls.Primitives.Popup();
            var grid = new System.Windows.Controls.Grid();
            grid.Children.Add(new System.Windows.Controls.TextBlock()
            {
                Text = text,
                Background = Brushes.White
            });
            grid.Children.Add(new Border() { BorderBrush = Brushes.Black, BorderThickness = new Thickness(2, 2, 2, 2) });
            pop.Child = grid;
            pop.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
            return pop;
        }
    }
}
