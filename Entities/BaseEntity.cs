using System;
using System.Collections.Generic;
using System.Data;

namespace EcoSys.Entities
{
    public class BaseEntity
    {
        public List<string> lines { get; } = new List<string>();
        public List<string> columns { get; } = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table">Таблица DataTable, из которой производится выборка</param>
        /// <param name="col_start">Индекс начала выборки (столбец)</param>
        /// <param name="row_start">Индекс начала выборки (строка)</param>
        /// <param name="col_space">Горизонтальный отступ до поля числовых данных</param>
        /// <param name="row_space">Вертикальный отступ до поля числовых данных</param>
        /// <returns></returns>
        private protected DataTable pickOutData(DataTable table, int col_start, int row_start, int col_space, int row_space)
        {
            col_start += col_space;     //отступы до самих данных
            row_start += row_space;

            var result_table = new DataTable();

            var lines_col = new DataColumn();       //Добавление столбца для сохранения названий показателей
            lines_col.ColumnName = "Показатели (млн. руб.)";
            lines_col.DataType = System.Type.GetType("System.String");

            result_table.Columns.Add(lines_col);

            foreach (string column in columns)      //заполнение столбцов
            {
                var data_col = new DataColumn();
                data_col.ColumnName = column;
                data_col.DataType = System.Type.GetType("System.Double");       //НА ДАННЫЙ МОМЕНТ ТИП ДАННЫХ - Double, НО МОЖЕТ ПРИГОДИТСЯ ПЕРЕРАБОТКА
                data_col.AllowDBNull = true;

                result_table.Columns.Add(data_col);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                var row = result_table.NewRow();        //создание строк

                row[0] = lines[i];
                for (int j = 1; j <= columns.Count; j++)
                {
                    var temp = table.Rows[row_start + i].Field<double?>(col_start + (j - 1));       //заполнение таблицы согласно отступам в документе
                    if (temp != null) row[result_table.Columns[j]] = temp;
                }

                result_table.Rows.Add(row);
            }
            return result_table;
        }

        private protected void roundDataTable(DataTable table, int num_of_symb)      //Метод для задания количества знаков после запятой
        {
            for (int i = 0; i < table.Rows.Count; i++)
                for (int j = 1; j < table.Columns.Count; j++)
                    if (table.Rows[i].Field<double?>(j) != null) table.Rows[i].SetField<double>(j, Math.Round(table.Rows[i].Field<double>(j), num_of_symb));
        }

        private protected void createLinesAndColumns(DataTable table, int col_names_index, int row_names_index, int num_of_rows)        //Метод для создания столбцов и заголовков таблиц
        {
            //Создание заголовков столбцов для датасета
            for (int i = 1; i < 4; i++)
            {
                var temp = "Финансовые корпорации: " + table.Rows[col_names_index].Field<string>(i);
                if (temp.Contains("фин."))
                    temp = temp.Replace("фин.", "финансовые");
                if (temp.EndsWith(' ')) temp = temp.Remove(temp.Length - 1);
                columns.Add(temp);
            }

            for (int i = 4; i < 8; i++)
            {
                var temp = table.Rows[col_names_index - 1].Field<string>(i);
                if (temp.Contains("Гос."))
                    temp = temp.Replace("Гос.", "Государственные");
                columns.Add(temp);
            }

            //Создание заголовков строк для датасета
            for (int i = row_names_index; i < num_of_rows; i++)
                lines.Add(table.Rows[i].Field<string>(0));
        }


        private protected DataTable summarizeDataTables(DataTable first_table, DataTable second_table)        //Метод для суммирования ячеек таблицы данных
        {
            var result_table = first_table.Clone();     //В целях предотвращения багов было принято решение клонировать структуру одной из таблиц, а не суммировать в нее сразу
            for (int i = 0; i < first_table.Rows.Count; i++)
            {
                var row = result_table.NewRow();
                row[0] = first_table.Rows[i].Field<string>(0);

                for (int j = 1; j < first_table.Columns.Count; j++)
                    row[j] = sumValues(first_table.Rows[i].Field<double?>(j), second_table.Rows[i].Field<double?>(j));

                result_table.Rows.Add(row);
            }
            return result_table;
        }

        private protected object sumValues(double? first, double? second)
        {
            if (first == null) first = 0;
            if (second == null) second = 0;
            return first + second;
        }
    }
}
