using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;

namespace EcoSys.Entities
{
    public class DataEntity : BaseEntity        //Класс содержит в себе три Dictionary, отвечающих за хранение набора матриц АКТИВЫ, ПАССИВЫ, САЛЬДО
    {
        public Dictionary<(string, string), DataTable> balance { get; set; } = new Dictionary<(string, string), DataTable>();
        public Dictionary<(string, string), DataTable> active { get; set; } = new Dictionary<(string, string), DataTable>();
        public Dictionary<(string, string), DataTable> passive { get; set; } = new Dictionary<(string, string), DataTable>();
        public HashSet<string> regions { get; } = new HashSet<string>();
        public HashSet<string> years { get; } = new HashSet<string>();

        public DataTable getBalanceData(HashSet<string> regions, string year)       //Метод доступа к сальдированным таблицам (БЛОК 1)
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
                if (result_table == null) result_table = balance[(region, year)]; else result_table = summarizeDataTables(result_table, balance[(region, year)]);

            roundDataTable(result_table, 3);

            return result_table;
        }

        public DataTable getBalanceData(HashSet<string> regions, int year)       //Метод доступа к сальдированным таблицам (БЛОК 2)
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
                for (int i = 0; i < year; i++)
                    if (result_table == null) result_table = balance[(region, years.ElementAt(i))]; else result_table = summarizeDataTables(result_table, balance[(region, years.ElementAt(i))]);

            roundDataTable(result_table, 3);

            return result_table;
        }

        public DataTable getPassiveData(HashSet<string> regions, string year)       //Метод доступа к пассивной части таблиц (БЛОК 1)
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
                if (result_table == null) result_table = passive[(region, year)]; else result_table = summarizeDataTables(result_table, passive[(region, year)]);

            roundDataTable(result_table, 3);

            return result_table;
        }

        public DataTable getPassiveData(HashSet<string> regions, int year)       //Метод доступа к пассивной части таблиц (БЛОК 2)
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
                for (int i = 0; i < year; i++)
                    if (result_table == null) result_table = passive[(region, years.ElementAt(i))]; else result_table = summarizeDataTables(result_table, passive[(region, years.ElementAt(i))]);

            roundDataTable(result_table, 3);

            return result_table;
        }

        public DataTable getActiveData(HashSet<string> regions, string year)       //Метод доступа к активной части таблиц (БЛОК 1)
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
                if (result_table == null) result_table = active[(region, year)]; else result_table = summarizeDataTables(result_table, active[(region, year)]);

            roundDataTable(result_table, 3);

            return result_table;
        }

        public DataTable getActiveData(HashSet<string> regions, int year)       //Метод доступа к активной части таблиц (БЛОК 2)
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
                for (int i = 0; i < year; i++)
                    if (result_table == null) result_table = active[(region, years.ElementAt(i))]; else result_table = summarizeDataTables(result_table, active[(region, years.ElementAt(i))]);

            roundDataTable(result_table, 3);

            return result_table;
        }

        private DataTable summarizeDataTables(DataTable first_table, DataTable second_table)        //Метод для суммирования ячеек таблицы данных
        {
            var result_table = first_table.Clone();
            for (int i = 0; i < first_table.Rows.Count; i++)
            {
                var row = result_table.NewRow();
                row[0] = first_table.Rows[0].Field<string>(0);

                for (int j = 1; j < first_table.Columns.Count; j++)
                    row[j] = first_table.Rows[i].Field<decimal>(j) + second_table.Rows[i].Field<decimal>(j);

                result_table.Rows.Add(row);
            }
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

                    used_dict.Add((region, year), pickOutData(table, vert_index, hor_index, 1, 2));   //добавление таблицы-выборки в словарь
                }
            }
        } 

        public async Task createTables(DataSet dataset)     //В данном методе асинхронно заполяются все три словаря
        {
                createLinesAndColumns(dataset.Tables[0], 3, 4, 31);   //создаем заголовки строк и столбцов

                Task import_balance = asyncFragmentizeData(balance, dataset.Tables[0]);
                Task import_actives = asyncFragmentizeData(active, dataset.Tables[1]);
                Task import_passives = asyncFragmentizeData(passive, dataset.Tables[2]);

                var complete_tasks = new List<Task>() { import_balance, import_actives, import_passives };

                await Task.WhenAll(complete_tasks);    
        }

        public bool checkCorrectness()
        {
            List<int> data_fullness = new List<int> { balance.Count, passive.Count, active.Count, regions.Count, years.Count };
            if (data_fullness.Any(item => item == 0)) return false; else return true;
        }
    }
}
