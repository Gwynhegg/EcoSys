using System;
using System.Collections.Generic;
using System.Text;

namespace EcoSys.Auxiliary
{
    public class GlobalSettings
    {
        private static GlobalSettings settings;
        //public int font_size { get; set; }
        public int decimal_places { get; set; }
       // public int tables_font_size { get; set; }

        private GlobalSettings()
        {
           // font_size = 12;
            decimal_places = 2;
           // tables_font_size = 12;
        }
        public static GlobalSettings getSettings()
        {
            if (settings == null)
                settings = new GlobalSettings();
            return settings;
        }
    }
}
