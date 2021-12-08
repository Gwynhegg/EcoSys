using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EcoSys.Entities
{
    public class ModelEntity
    {
        public Dictionary<(string, string), string> model_equations { get; set; } = new Dictionary<(string, string), string>();     //Dictionary для хранения моделей по типу (Регион, показатель) -> уравнение
        public List<string> regions { get; set;} = new List<string>();        //HashSet для хранения наименований регионов
        public List<string> categories { get; set; } = new List<string>();        //HashSet для хранения категорий - столбцов
        public Dictionary<string, List<string>> tips { get; set; } = new Dictionary<string, List<string>>();

        public DataTable getCalculatedModelData(double?[] values, DataTable model)      //на основе высчитанных в блоке 5 значений формируем новую таблицу, скомпилированную из уравнения модели
        {                                                                               //блока 4 и вычисленных значений
            var result_table = new DataTable();
            result_table = model.Copy();
            result_table.Rows.Clear();
            for (int i = 0; i < model.Rows.Count; i++)
            {
                var row = result_table.NewRow();
                row[0] = model.Rows[i].Field<string>(0);
                for (int j = 1; j < model.Columns.Count; j++)
                {
                    var equation = model.Rows[i].Field<string>(j);
                    if (equation != null && values[i] != null && !equation.Equals(String.Empty)) 
                    {
                        var temp = Math.Round((EasyEquationParser.getRatioFromString(equation) * (double)values[i]), 3).ToString();     //Использование парсера для выделения коэффицентов уравнения блока 4
                        if (temp.Equals("-0")) row[j] = "0"; else row[j] = temp; 
                    }
                }
                result_table.Rows.Add(row);
            }
            return result_table;
        }
        public async Task createTables(DataSet dataset)     //Асинхронное создание всех необходимых таблиц
        {
            var table = dataset.Tables[0];
            getRegions(table);
            getCategories(table);

            List<Task> model_tasks = new List<Task>();
            Task equation_task = getModelEquations(table);
            Task tips_task = getModelTips(table);
            model_tasks.Add(equation_task);
            model_tasks.Add(tips_task);
            await Task.WhenAll(model_tasks);
        }

        private void getRegions(DataTable table)
        {
            CultureInfo cult_info = new CultureInfo("ru-RU", false);
            TextInfo text_info = cult_info.TextInfo;

            int starting_index = 1;
            bool ending_condition = false;
            while (!ending_condition)
            {
                string region = table.Rows[starting_index++].Field<string>(0);        //получение названия региона
                region = text_info.ToTitleCase(region.ToLower());       //Решение для обеспечения однообразия названий
                regions.Add(region);
                if (table.Rows[starting_index].Field<string>(0) == null) ending_condition = true;
            }
        }

        private void getCategories(DataTable table)     //Получение имени категорий (столбцов) таблицы
        {
            int starting_index = 1;
            bool ending_condition = false;
            while (!ending_condition)
            {
                string category = table.Rows[0].Field<string>(starting_index++);        //получение названия региона
                categories.Add(category);
                if (table.Rows[0].Field<string>(starting_index) == null) ending_condition = true;
            }
        }

        private async Task getModelEquations(DataTable table)       //Метод для считывания математического выражения для расчета значений
        {
            for (int i = 1; i < regions.Count; i++)
                for(int j = 1; j < categories.Count; j++)
                {
                    string equation = table.Rows[i].Field<string?>(j);
                    if (equation != null) model_equations.Add((regions[i - 1], categories[j - 1]), equation);
                }
        }

        private async Task getModelTips(DataTable table)        //Метод для считывания описаний используемых переменных
        {
            for (int i = 1; i < categories.Count + 1; i++)
            {
                int starting_index = 2 + regions.Count;
                bool ending_condition = false;
                tips.Add(categories[i - 1], new List<string>());
                while (!ending_condition)
                {
                    var temp = table.Rows[starting_index++].Field<string?>(i);
                        if (temp != null && starting_index < table.Rows.Count) tips[categories[i - 1]].Add(temp); else ending_condition = true;
                   
                }
            }
        }

        public bool checkCorrectness()
        {
            if (model_equations.Count != 0 && regions.Count != 0 && categories.Count != 0 && tips.Count != 0) return true; else return false;
        }
    }
}
