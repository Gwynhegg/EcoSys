using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
            {
                if (result_table == null)
                    result_table = this.balance[(region, year)];
                else
                    result_table = summarizeDataTables(result_table, this.balance[(region, year)]);
            }

            roundDataTable(result_table, 2);

            return result_table;
        }

        public DataTable getBalanceData(HashSet<string> regions, int year)       //Метод доступа к сальдированным таблицам (БЛОК 2)
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
            {
                for (int i = 0; i < year; i++)
                {
                    if (result_table == null)
                        result_table = this.balance[(region, this.years.ElementAt(i))];
                    else
                        result_table = summarizeDataTables(result_table, this.balance[(region, this.years.ElementAt(i))]);
                }
            }

            roundDataTable(result_table, 2);

            return result_table;
        }

        public DataTable getPassiveData(HashSet<string> regions, string year)       //Метод доступа к пассивной части таблиц (БЛОК 1)
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
            {
                if (result_table == null)
                    result_table = this.passive[(region, year)];
                else
                    result_table = summarizeDataTables(result_table, this.passive[(region, year)]);
            }

            roundDataTable(result_table, 2);

            return result_table;
        }

        public DataTable getPassiveData(HashSet<string> regions, int year)       //Метод доступа к пассивной части таблиц (БЛОК 2)
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
            {
                for (int i = 0; i < year; i++)
                {
                    if (result_table == null)
                        result_table = this.passive[(region, this.years.ElementAt(i))];
                    else
                        result_table = summarizeDataTables(result_table, this.passive[(region, this.years.ElementAt(i))]);
                }
            }

            roundDataTable(result_table, 2);

            return result_table;
        }

        public DataTable getActiveData(HashSet<string> regions, string year)       //Метод доступа к активной части таблиц (БЛОК 1)
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
            {
                if (result_table == null)
                    result_table = this.active[(region, year)];
                else
                    result_table = summarizeDataTables(result_table, this.active[(region, year)]);
            }

            roundDataTable(result_table, 2);

            return result_table;
        }

        public DataTable getActiveData(HashSet<string> regions, int year)       //Метод доступа к активной части таблиц (БЛОК 2)
        {
            DataTable result_table = null;
            foreach (string region in regions)      //По каждому из регионов получаем соответствующий словарь, после чего суммируем ячейки (при необходимости)
            {
                for (int i = 0; i < year; i++)
                {
                    if (result_table == null)
                        result_table = this.active[(region, this.years.ElementAt(i))];
                    else
                        result_table = summarizeDataTables(result_table, this.active[(region, this.years.ElementAt(i))]);
                }
            }

            roundDataTable(result_table, 2);

            return result_table;
        }

        private async Task asyncFragmentizeData(Dictionary<(string, string), DataTable> used_dict, DataTable table)
        {
            CultureInfo cult_info = new CultureInfo("ru-RU", false);
            TextInfo text_info = cult_info.TextInfo;

            for (int vert_index = 0; vert_index < table.Columns.Count; vert_index += 10)
            {
                string region = table.Rows[0].Field<string>(vert_index);        //получение названия региона

                if (region is null)
                    break;

                region = text_info.ToTitleCase(region.ToLower());       //Решение для обеспечения однообразия названий
                this.regions.Add(region);

                for (int hor_index = 2; hor_index < 728; hor_index += 33)       //Пока не решена проблема с дополнительными вычислениями на листах
                {
                    string year = table.Rows[hor_index].Field<string>(vert_index);      //Получение временного промежутка
                    this.years.Add(year);

                    used_dict.Add((region, year), pickOutData(table, vert_index, hor_index, 1, 2));   //добавление таблицы-выборки в словарь
                }
            }
        }

        public async Task createTables(DataSet dataset)     //В данном методе асинхронно заполяются все три словаря
        {
            createLinesAndColumns(dataset.Tables[0], 3, 4, 31);   //создаем заголовки строк и столбцов

            Task import_balance = asyncFragmentizeData(this.balance, dataset.Tables[0]);     //Создаем задачи по заполнению каждой из необходимых таблиц
            Task import_actives = asyncFragmentizeData(this.active, dataset.Tables[1]);
            Task import_passives = asyncFragmentizeData(this.passive, dataset.Tables[2]);

            var complete_tasks = new List<Task>() { import_balance, import_actives, import_passives };      //Группируем лист задач

            await Task.WhenAll(complete_tasks);    //Дожидаемся выполнения всего списка задач
        }

        public bool checkCorrectness()      //Проверка данных на корректность при импортировании. Самый простой способ - проверить, что все словари и наборы данных не пустые
        {
            List<int> data_fullness = new List<int> { this.balance.Count, this.passive.Count, this.active.Count, this.regions.Count, this.years.Count };
            if (data_fullness.Any(item => item == 0))
                return false;
            else
                return true;
        }
    }
}
