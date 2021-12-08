using System.Windows.Media;

namespace EcoSys.Auxiliary
{
    public static class GridStandard
    {
        public static void standardizeGrid(System.Windows.Controls.DataGrid grid)
        {
            grid.Background = Brushes.AliceBlue;
            grid.CanUserAddRows = false;
            grid.CanUserDeleteRows = false;
            grid.CanUserResizeColumns = true;
            grid.CanUserReorderColumns = true;
            grid.CanUserSortColumns = true;
            grid.ClipToBounds = true;
            grid.FrozenColumnCount = 1;
            grid.ColumnHeaderHeight = 30;
            grid.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            grid.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            grid.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            grid.VerticalContentAlignment = System.Windows.VerticalAlignment.Stretch;
        }
    }
}
