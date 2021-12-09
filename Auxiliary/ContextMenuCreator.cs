using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace EcoSys.Auxiliary
{
    public static class ContextMenuCreator
    {
        public static void createContextMenu(System.Windows.Controls.DataGrid grid)
        {
            ContextMenu menu = new ContextMenu();
            MenuItem item_larger_font_size = new MenuItem() { Header = "Увеличить шрифт"};
            item_larger_font_size.Click += upFontSize;
            MenuItem item_smaller_font_size = new MenuItem() { Header = "Уменьшить шрифт" };
            item_smaller_font_size.Click += downFontSize;

            menu.Items.Add(item_larger_font_size);
            menu.Items.Add(item_smaller_font_size);

             grid.ContextMenu = menu;
        }

        private static void upFontSize(object sender, RoutedEventArgs e)
        {
            var data_grid = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as DataGrid);
            if (data_grid.FontSize < 20) data_grid.FontSize += 2;
        }

        private static void downFontSize(object sender, RoutedEventArgs e)
        {
            var data_grid = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as DataGrid);
            if (data_grid.FontSize > 8) data_grid.FontSize -= 2;
        }
    }
}
