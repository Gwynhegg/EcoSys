using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace EcoSys.Entities
{
    public class DataEntity        //Класс содержит в себе три Dictionary, отвечающих за хранение набора матриц АКТИВЫ, ПАССИВЫ, САЛЬДО
    {
        public Dictionary<(string, string), DataTable> balance { get; set; } = new Dictionary<(string, string), DataTable>();
        public Dictionary<(string, string), DataTable> active { get; set; } = new Dictionary<(string, string), DataTable>();
        public Dictionary<(string, string), DataTable> passive { get; set; } = new Dictionary<(string, string), DataTable>();

        public List<string> lines { get; } = new List<string>();
        public List<string> columns { get; } = new List<string>();
        public HashSet<string> regions { get; } = new HashSet<string>();
        public HashSet<string> years { get; } = new HashSet<string>();

        public DataTable getBalanceData(HashSet<string> regions, string year)       //Метод доступа к сальдированным таблицам
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
                if (result_table == null) result_table = balance[(region, year)]; else result_table = summarizeDataTables(result_table, balance[(region, year)]);
            return result_table;
        }

        public DataTable getPassiveData(HashSet<string> regions, string year)       //Метод доступа к пассивной части таблиц
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
                if (result_table == null) result_table = passive[(region, year)]; else result_table = summarizeDataTables(result_table, passive[(region, year)]);
            return result_table;
        }

        public DataTable getActiveData(HashSet<string> regions, string year)       //Метод доступа к активной части таблиц
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
                if (result_table == null) result_table = balance[(region, year)]; else result_table = summarizeDataTables(result_table, balance[(region, year)]);
            return result_table;
        }

        private DataTable summarizeDataTables(DataTable first_table, DataTable second_table)        //Метод для суммирования ячеек таблицы данных
        {
            for (int i = 0; i < first_table.Rows.Count; i++)
                for (int j = 0; j < first_table.Columns.Count; j++)
                    first_table.Rows[i].SetField<Double>(j, first_table.Rows[i].Field<Double>(j) + second_table.Rows[i].Field<Double>(j));
            return first_table;
        }
        private async Task asyncFragmentizeData(Dictionary<(string, string), DataTable> used_dict, DataTable table)
        {
            CultureInfo cult_info = new CultureInfo("ru-RU", false);
            TextInfo text_info = cult_info.TextInfo;

            for (int vert_index = 0; vert_index < table.Columns.Count; vert_index += 10)
            {
                string region = table.Rows[0].Field<string>(vert_index);        //получение названия региона

                if (region is null) break;
                region = text_info.ToTitleCase(region.ToLower());       //Решение для обеспечения однообразия названий
                regions.Add(region);

                for (int hor_index = 2; hor_index < 728; hor_index += 33)       //Пока не решена проблема с дополнительными вычислениями на листах
                {
                    string year = table.Rows[hor_index].Field<string>(vert_index);      //Получение временного промежутка
                    years.Add(year);

                    used_dict.Add((region, year), pickOutData(table, vert_index, hor_index));   //добавление таблицы-выборки в словарь
                }
            }
        } 

        public async void createTables(DataSet dataset)     //В данном методе асинхронно заполяются все три словаря
        {
            createLinesAndColumns(dataset.Tables[0]);   //создаем заголовки строк и столбцов

            Task import_balance = asyncFragmentizeData(balance, dataset.Tables[0]);
            Task import_actives = asyncFragmentizeData(active, dataset.Tables[1]);
            Task import_passives = asyncFragmentizeData(passive, dataset.Tables[2]);

            var complete_tasks = new List<Task>() {import_balance, import_actives, import_passives};

            await Task.WhenAll(complete_tasks);
            Console.WriteLine();
        }

        private void createLinesAndColumns(DataTable table)
        {
            //Создание заголовков столбцов для датасета
            for (int i = 1; i < 4; i++) 
                columns.Add("Финансовые корпорации. " + table.Rows[3].Field<string>(i));
            for (int i = 4; i < 8; i++)
                columns.Add(table.Rows[2].Field<string>(i));

            //Создание заголовков строк для датасета
            for (int i = 4; i < 31; i++)
                lines.Add(table.Rows[i].Field<string>(0));
        }

        private DataTable pickOutData(DataTable table, int col_start, int row_start)
        {
            col_start += 1;     //отступы до самих данных
            row_start += 3;

            var result_table = new DataTable();

            foreach (string column in columns)      //заполнение столбцов
            {
                var data_col = new DataColumn();
                data_col.ColumnName = column;
                data_col.DataType = System.Type.GetType("System.Double");

                result_table.Columns.Add(data_col);
            }

            for (int i=0; i < lines.Count; i++)
            {
                var row = result_table.NewRow();        //создание строк
                
                for (int j = 0; j < columns.Count; j++)
                    row[result_table.Columns[j]] = table.Rows[row_start + i].Field<double>(col_start + j);      //заполнение таблицы согласно отступам в документе

                result_table.Rows.Add(row);
            }
            return result_table;
        }

    }
}
