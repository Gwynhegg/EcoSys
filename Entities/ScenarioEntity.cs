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
        public Dictionary<(string, string), DataTable> conditions { get; set; } = new Dictionary<(string, string), DataTable>();        //Dictionary для хранения сценарных условий с делением на подкатегории
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

        private async Task asyncFragmentizeConditions(DataTable table)      //Выделение сценарных условий с делением по категориям
        {
            CultureInfo cult_info = new CultureInfo("ru-RU", false);
            TextInfo text_info = cult_info.TextInfo;

            for (int hor_index = 0; hor_index < table.Rows.Count; hor_index += 50)
            {
                string condition_name = String.Empty;       //Строка для хранения названия главного сценария
                for (int i = hor_index - 1; i < hor_index + 2; i++)
                    if (i >= 0)
                    {
                        condition_name = table.Rows[i].Field<string>(0);
                        if (condition_name != null)
                        {
                            hor_index = i;
                            break;
                        }
                    }

                int row_step = 2, column_step = 8, current_column = 0;

                string sub_condition = table.Rows[hor_index + row_step].Field<string>(current_column);

                while (sub_condition != null)
                {
                    conditions.Add((condition_name, sub_condition), pickConditions(table, hor_index + row_step, current_column));
                    current_column += column_step;

                    if (current_column < table.Columns.Count)
                        sub_condition = table.Rows[hor_index + row_step].Field<string>(current_column);
                    else
                        break;
                }
                Console.WriteLine();
            }
            
        }
        public async void createTables(DataSet dataset)       //Метод для создания таблиц со сценариями (асинхронный)
        {
            createLinesAndColumns(dataset.Tables[0], 4, 6, 19);

            createScenarioNames(dataset.Tables[0]);

            var scenarios_tasks = new List<Task>();
            for (int i = 0; i < 4; i++)     //Добавление задач для считывания сценариев за 4 указанных года
            {
                Task table_task = asyncFragmentizeScenario(dataset.Tables[i]);
                scenarios_tasks.Add(table_task);
            }

            Task conditions_task = asyncFragmentizeConditions(dataset.Tables[4]);       //Добавление задачи для считывания сценарных условий с листа
            scenarios_tasks.Add(conditions_task);

            await Task.WhenAll(scenarios_tasks);
        }

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

        private DataTable pickConditions(DataTable table, int row_start, int col_start)
        {
            col_start += 1;
            row_start += 2;

            var result_table = new DataTable();

            var lines_col = new DataColumn();
            lines_col.ColumnName = "Год";
            lines_col.DataType = System.Type.GetType("System.String");

            result_table.Columns.Add(lines_col);

            for (int i = 0; i < 5; i++)
            {
                var data_col = new DataColumn();
                data_col.ColumnName = table.Rows[row_start - 1].Field<string>(col_start + i);
                data_col.DataType = System.Type.GetType("System.Decimal");
                data_col.AllowDBNull = true;

                result_table.Columns.Add(data_col);
            }

            for (int index = 0; index < 28; index++)
            {
                var row = result_table.NewRow();

                row[0] = table.Rows[row_start + index].Field<double>(col_start - 1).ToString();

                for (int i = 0; i < 5; i++)
                {
                    var temp = table.Rows[row_start + index].Field<double?>(col_start + i);
                    if (temp != null) row[i + 1] = temp;
                }

                result_table.Rows.Add(row);
            }
            return result_table;
        }
    }
}
