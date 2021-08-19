using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using LiveCharts.Wpf;
using LiveCharts;

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

        public DataTable getScenarioData(int year_index, string region, string scenario_name)
        {
            var year = years[year_index];

            var result = scenarios[(year, region, scenario_name)];
            roundDataTable(result, 2);

            return result;
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
                    result_list.Add(new System.Windows.Controls.DataGrid() { ItemsSource = segmented_data.AsDataView() });

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

            chart.Series.Add(new LineSeries() { Values = real_price, Title = "Инерционный сценарий"});

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
                Console.WriteLine();
            }
            
        }
        public async Task createTables(DataSet dataset)       //Метод для создания таблиц со сценариями (асинхронный)
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

            Task models_task = asyncFragmentizeModels(dataset.Tables[5]);

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

            for (int i = 0; i < 5; i++)
            {
                var data_col = new DataColumn();

                data_col.ColumnName = table.Rows[row_start - 2].Field<string>(col_start + i);
                var combined_name = table.Rows[row_start - 1].Field<string>(col_start + i);

                if (combined_name != null) data_col.ColumnName += "." + combined_name;

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
