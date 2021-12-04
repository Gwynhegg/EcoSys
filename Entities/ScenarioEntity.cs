using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EcoSys.Entities
{
    public class ScenarioEntity : BaseEntity
    {
        public Dictionary<(string, string, string), DataTable> scenarios { get; set; } = new Dictionary<(string, string, string), DataTable>();     //Dictionary для хранения сценариев по годам, регионам и типам сценариев (ГОД, РЕГИОН, СЦЕНАРИЙ), DataTable
        public Dictionary<(string, string), DataTable> conditions { get; set; } = new Dictionary<(string, string), DataTable>();        //Dictionary для хранения сценарных условий с делением на подкатегории
        public Dictionary<string, DataTable> scenario_models { get; set; } = new Dictionary<string, DataTable>();       //Dictionary для хранения сценарных моделей
        public HashSet<string> regions { get; } = new HashSet<string>();
        public List<string> years { get; } = new List<string>();
        public List<string> scenario_name { get; } = new List<string>();

        public List<string> categories { get; } = new List<string>();

        public DataTable getScenarioData(string region, int year_index, int current_step)
        {
            DataTable result_table = new DataTable();
            var year = years[year_index];
            result_table = scenarios[(year, region, scenario_name[0])].Clone();
            result_table.Columns[0].ColumnName = "Сценарии";
            foreach (string scenario in scenario_name)
            {
                var row = result_table.NewRow();
                row[0] = scenario;
                for (int i = 1; i < columns.Count; i++)
                {
                    var temp = scenarios[(year, region, scenario)].Rows[current_step].Field<double?>(i);
                    if (temp != null) row[i] = temp; else row[i] = 0;
                }
                result_table.Rows.Add(row);
            }
            roundDataTable(result_table, 2);

            return result_table;
        }

        public DataTable getScenarioModels(int region_index)
        {
            var region = regions.ElementAt(region_index);

            return scenario_models[region];
        }

        public List<object> getCategoriesData(int category_index, double current_height, double current_width)
        {
            var result_list = new List<object>();

            var choosed_category = categories[category_index];

            foreach (KeyValuePair<(string, string), DataTable> item in conditions)
                if (item.Key.Item1.Equals(choosed_category))
                {
                    result_list.Add(new System.Windows.Controls.Label() { Content = item.Key.Item2 });

                    var used_data = item.Value;

                    var segmented_data = used_data.Clone();

                    for (int j = years.Count; j > 0; j--)
                        segmented_data.ImportRow(used_data.Rows[used_data.Rows.Count - j]);

                    roundDataTable(segmented_data, 2);

                    System.Windows.Controls.DataGrid grid = new System.Windows.Controls.DataGrid() { ItemsSource = segmented_data.AsDataView() };
                    Auxiliary.GridStandard.standardizeGrid(grid);
                    result_list.Add(grid);

                    result_list.Add(getGraph(used_data, current_height, current_width));
                }

            return result_list;
        }

        private CartesianChart getGraph(DataTable data, double current_height, double current_width)
        {
            CartesianChart chart = new CartesianChart();

            string[] year_labels = new string[data.Rows.Count];
            for (int i = 0; i < year_labels.Length; i++)
                year_labels[i] = data.Rows[i].Field<string>(0);

            chart.AxisX.Add(new Axis() { Title = "Годы", Labels = year_labels });
            chart.AxisY.Add(new Axis() { Title = "руб.", MinValue = 0 });

            ChartValues<double> real_price = new ChartValues<double>();

            for (int i = 0; i < data.Rows.Count; i++)
                real_price.Add(data.Rows[i].Field<double>(1));

            chart.Series.Add(new LineSeries() { Values = real_price, Title = "Инерционный сценарий" });

            for (int i = 1; i < scenario_name.Count; i++)
            {
                ChartValues<double> scenario_price = new ChartValues<double>();

                for (int index = 0; index < data.Rows.Count; index++)
                    scenario_price.Add(data.Rows[index].Field<double>(i + 1));

                chart.Series.Add(new LineSeries() { Values = scenario_price, Title = data.Columns[1 + i].ColumnName });

            }

            chart.Height = current_height / 2.2;
            chart.Width = current_width / 1.1;

            return chart;
        }

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

        private async Task asyncFragmentizeModels(DataTable table)
        {
            CultureInfo cult_info = new CultureInfo("ru-RU", false);
            TextInfo text_info = cult_info.TextInfo;

            int index = 0;

            string region = table.Rows[index].Field<string>(0);
            region = text_info.ToTitleCase(region.ToLower());       //Решение для обеспечения однообразия названий


            while (region != null)
            {
                scenario_models.Add(region, pickModels(table, index, 0));

                index += 18;
                if (index < table.Rows.Count) region = table.Rows[index].Field<string>(0); else break;
                region = text_info.ToTitleCase(region.ToLower());       //Решение для обеспечения однообразия названий
            }

        }

        private async Task asyncFragmentizeConditions(DataTable table)      //Выделение сценарных условий с делением по категориям
        {
            CultureInfo cult_info = new CultureInfo("ru-RU", false);
            TextInfo text_info = cult_info.TextInfo;

            string condition_name;

            for (int hor_index = 0; hor_index < 205; hor_index += 33)
            {
                condition_name = table.Rows[hor_index].Field<string>(0); ;       //Строка для хранения названия главного сценария

                categories.Add(condition_name);

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
            }


            //Расчеты граф 8-12 придеться проводить отдельно, так как структура файла сильно нарушена
            condition_name = table.Rows[231].Field<string>(0); ;       //Строка для хранения названия главного сценария
            categories.Add(condition_name);
            pickConditionKeys(condition_name, conditions, 233, 300, table);

            condition_name = table.Rows[300].Field<string>(0); ;       //Строка для хранения названия главного сценария
            categories.Add(condition_name);
            pickConditionKeys(condition_name, conditions, 302, 467, table);

            condition_name = table.Rows[467].Field<string>(0); ;       //Строка для хранения названия главного сценария
            categories.Add(condition_name);
            pickConditionKeys(condition_name, conditions, 469, 568, table);

            condition_name = table.Rows[568].Field<string>(0); ;       //Строка для хранения названия главного сценария
            categories.Add(condition_name);
            pickConditionKeys(condition_name, conditions, 570, 669, table);

            condition_name = table.Rows[669].Field<string>(0); ;       //Строка для хранения названия главного сценария
            categories.Add(condition_name);
            pickConditionKeys(condition_name, conditions, 671, table.Rows.Count, table);
        }

        private void pickConditionKeys(string condition_name, Dictionary<(string, string), DataTable> conditions, int start_row, int end_row, DataTable table)
        {
            CultureInfo cult_info = new CultureInfo("ru-RU", false);
            TextInfo text_info = cult_info.TextInfo;
            for (int index = start_row; index < end_row; index += 33)
            {
                for (int current_column = 0; current_column < table.Columns.Count; current_column += 8)
                {
                    if (table.Rows[index].Field<string>(current_column) != null)
                    {
                        string region = text_info.ToTitleCase(table.Rows[index].Field<string>(current_column).ToLower());
                        string sub_condition = region + "." + table.Rows[index + 1].Field<string>(current_column);
                        conditions.Add((condition_name, sub_condition), pickConditions(table, index + 1, current_column));
                        Console.WriteLine();
                    }
                }
            }
        }

        public async Task createTables(DataSet dataset)       //Метод для создания таблиц со сценариями (асинхронный)
        {
            createLinesAndColumns(dataset.Tables[1], 4, 6, 19);

            createScenarioNames(dataset.Tables[1]);

            var scenarios_tasks = new List<Task>();
            for (int i = 1; i < 5; i++)     //Добавление задач для считывания сценариев за 4 указанных года
            {
                Task table_task = asyncFragmentizeScenario(dataset.Tables[i]);
                scenarios_tasks.Add(table_task);
            }

            Task conditions_task = asyncFragmentizeConditions(dataset.Tables[5]);       //Добавление задачи для считывания сценарных условий с листа

            Task models_task = asyncFragmentizeModels(dataset.Tables[0]);

            scenarios_tasks.Add(conditions_task);
            scenarios_tasks.Add(models_task);

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

        private DataTable pickModels(DataTable table, int row_start, int col_start)
        {
            col_start += 1;
            row_start += 3;

            var result_table = new DataTable();

            var lines_col = new DataColumn();
            lines_col.ColumnName = "Сценарные условия";
            lines_col.DataType = System.Type.GetType("System.String");

            result_table.Columns.Add(lines_col);

            var data_col = new DataColumn();
            data_col.ColumnName = columns[0];
            data_col.DataType = System.Type.GetType("System.String");
            data_col.AllowDBNull = true;

            result_table.Columns.Add(data_col);


            for (int i = 2; i < 6; i++)
            {
                data_col = new DataColumn();
                data_col.ColumnName = columns[i];
                data_col.DataType = System.Type.GetType("System.String");
                data_col.AllowDBNull = true;

                result_table.Columns.Add(data_col);
            }

            for (int index = 0; index < lines.Count - 1; index++)
            {
                var row = result_table.NewRow();

                row[0] = lines[index];

                for (int i = 0; i < result_table.Columns.Count - 1; i++)
                {
                    var temp = table.Rows[row_start + index].Field<string?>(col_start + i);
                    if (temp != null) row[i + 1] = temp; else row[i + 1] = "";
                }

                result_table.Rows.Add(row);
            }
            return result_table;
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
                data_col.DataType = System.Type.GetType("System.Double");
                data_col.AllowDBNull = true;

                result_table.Columns.Add(data_col);
            }

            for (int index = 0; index < 28; index++)
            {
                var row = result_table.NewRow();

                row[0] = table.Rows[row_start + index].Field<double?>(col_start - 1).ToString();

                for (int i = 0; i < 5; i++)
                {
                    var temp = table.Rows[row_start + index].Field<double?>(col_start + i);
                    if (temp != null) row[i + 1] = temp; else row[i + 1] = row[1];
                }

                result_table.Rows.Add(row);
            }
            return result_table;
        }

        public bool checkCorrectness()
        {
            List<int> data_fullness = new List<int> { scenarios.Count, scenario_models.Count, scenario_name.Count, regions.Count, years.Count, scenario_name.Count, categories.Count };
            if (data_fullness.Any(item => item == 0)) return false; else return true;
        }
    }
}
