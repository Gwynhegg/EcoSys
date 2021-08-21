using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace EcoSys.Auxiliary
{
    public static class Regions
    {
        private static Dictionary<string, List<string>> constituencies;

        public static Dictionary<string, List<string>> createConstituencies(HashSet<string> regions_list)
        {
            var regions = JsonConvert.DeserializeObject<List<Region>>(File.ReadAllText("Resources/Regions.json"));
            correctMistakes(regions);

            constituencies = new Dictionary<string, List<string>>();

            foreach (string region in regions_list)
            {
                Region needed_region = regions.Find(x => x.Name == region);
                if (needed_region != null)
                    if (constituencies.ContainsKey(needed_region.FederalDistrictName))
                        if (constituencies[needed_region.FederalDistrictName] != null) constituencies[needed_region.FederalDistrictName].Add(region); 
                        else 
                        { 
                            constituencies[needed_region.FederalDistrictName] = new List<string>();
                            constituencies[needed_region.FederalDistrictName].Add(region);
                        }
                    else constituencies.Add(needed_region.FederalDistrictName, new List<string>() { region});
            }
            return constituencies;
        }

        private static void correctMistakes(List<Region> regions)
        {
            CultureInfo cult_info = new CultureInfo("ru-RU", false);
            TextInfo text_info = cult_info.TextInfo;

            for (int i = 0; i < regions.Count; i++)
            {
                regions[i].Name = text_info.ToTitleCase(regions[i].Name.ToLower());       //Решение для обеспечения однообразия названий
                if (regions[i].Name == "Республика Крым") regions[i].Name = "Крым";
                else if (regions[i].Name == "Город Севастополь") regions[i].Name = "Севастополь";
                else if (regions[i].Name == "Город Санкт-Петербург") regions[i].Name = "Санкт-Петербург";
                else if (regions[i].Name == "Республика Мордовии") regions[i].Name = "Республика Мордовия";
                else if (regions[i].Name == "Удмуртская Республики") regions[i].Name = "Республика Удмуртия";
                else if (regions[i].Name == "Чувашская Республики") regions[i].Name = "Республика Чувашия";
                else if (regions[i].Name == "Республика Ингушетии") regions[i].Name = "Республика Ингушетия";
                else if (regions[i].Name == "Ставропольский Край") regions[i].Name = "Ставропольский Край";
            }
        }
    }

    public class Region
    {
        public int ID { get; set; }
        public int OKTMO_ID { get; set; }
        public string Name { get; set; }
        public int FederalDistrictID { get; set; }
        public string FederalDistrictName { get; set; }
    }
}
