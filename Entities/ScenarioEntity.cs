using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Globalization;

namespace EcoSys.Entities
{
    public class ScenarioEntity : BaseEntity
    {
        public Dictionary<(string, string, string), DataTable> scenarios { get; set; } = new Dictionary<(string, string, string), DataTable>();     //Dictionary для хранения сценариев по годам, регионам и типам сценариев (ГОД, РЕГИОН, СЦЕНАРИЙ), DataTable
        public Dictionary<string, (string, DataTable)> conditions { get; set; } = new Dictionary<string, (string, DataTable)>();        //Dictionary для хранения сценарных условий с делением на подкатегории
        //public List<string> lines { get; } = new List<string>();
        //public List<string> columns { get; } = new List<string>();
        public HashSet<string> regions { get; } = new HashSet<string>();
        public List<string> years { get; } = new List<string>();
        public List<string> scenario_name { get; } = new List<string>();
        private async Task asyncFragmentizeScenario(DataTable table)        //Выделение таблиц и заполнение словарей
        {
            CultureInfo cult_info = new CultureInfo("ru-RU", false);
            TextInfo text_info = cult_info.TextInfo;

            string year = table.TableName;
            years.Add(year);     //Добавление года разбираемого сценария

            for (int vert_index = 0; vert_index < table.Columns.Count; vert_index += 10)
            {
                string region = table.Rows[0].Field<string>(vert_index);        //получение названия региона

                if (region is null) break;
                region = text_info.ToTitleCase(region.ToLower());       //Решение для обеспечения однообразия названий
                regions.Add(region);

                for (int scen_index = 2; scen_index < 85; scen_index += 20)
                {
                    scenarios.Add((year, region, scenario_name[scen_index / 20]), pickOutData(table, vert_index, scen_index, 1, 4));
                }
            }
        }
        public async void createTables(DataSet dataset)       //Метод для создания таблиц со сценариями (асинхронный)
        {
            createLinesAndColumns(dataset.Tables[0], 4, 6, 19);

            createScenarioNames(dataset.Tables[0]);

            var scenarios_tasks = new List<Task>();
            for (int i = 0; i < 4; i++)
            {
                Task table_task = asyncFragmentizeScenario(dataset.Tables[i]);
                scenarios_tasks.Add(table_task);
            }

            await Task.WhenAll(scenarios_tasks);
        }

        //private void createLinesAndColumns(DataTable table)
        //{
        //    //Создание заголовков столбцов для датасета
        //    for (int i = 1; i < 4; i++)
        //        columns.Add("Финансовые корпорации. " + table.Rows[4].Field<string>(i));
        //    for (int i = 4; i < 8; i++)
        //        columns.Add(table.Rows[3].Field<string>(i));

        //    //Создание заголовков строк для датасета
        //    for (int i = 6; i < 19; i++)
        //        lines.Add(table.Rows[i].Field<string>(0));
        //}

        private void createScenarioNames(DataTable table)
        {
            int step = 0, starting_index = 2;
            while (step < 5)
            {
                string scenario = table.Rows[starting_index].Field<string>(0);
                scenario = scenario.Substring(0, 1).ToUpper() + scenario.Substring(1).ToLower();
                scenario_name.Add(scenario);
                starting_index += 20;
                step++;
            }
        }
    }
}
