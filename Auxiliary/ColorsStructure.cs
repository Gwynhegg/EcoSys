using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Text;

namespace EcoSys.Auxiliary
{
    public static class ColorsStructure
    {
        private static Dictionary<string, Brush> brushes = new Dictionary<string, Brush>
        {
            { "Инерционный сценарий", Brushes.YellowGreen },
            { "Умеренно негативный сценарий", Brushes.IndianRed },
            { "Пессимистичный сценарий", Brushes.DarkRed },
            { "Умеренно оптимистичный сценарий", Brushes.LightGreen },
            { "Оптимистичный сценарий", Brushes.ForestGreen }
        };

        public static Brush getColor(string key)
        {
            return brushes[key];
        }
    }
}
