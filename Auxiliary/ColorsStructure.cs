using System.Collections.Generic;
using System.Windows.Media;

namespace EcoSys.Auxiliary
{
    public static class ColorsStructure
    {
        private static readonly Dictionary<string, Brush> brushes = new Dictionary<string, Brush>
        {
            { "Инерционный сценарий", Brushes.YellowGreen },
            { "Умеренно негативный сценарий", Brushes.IndianRed },
            { "Пессимистичный сценарий", Brushes.DarkRed },
            { "Умеренно оптимистичный сценарий", Brushes.LightGreen },
            { "Оптимистичный сценарий", Brushes.ForestGreen }
        };

        public static Brush getColor(string key) => brushes[key];
    }
}
