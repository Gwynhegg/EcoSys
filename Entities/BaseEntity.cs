using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace EcoSys.Entities
{
    public class BaseEntity
    {
        protected List<string> lines { get; } = new List<string>();
        protected List<string> columns { get; } = new List<string>();

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
                data_col.DataType = System.Type.GetType("System.Decimal");
                data_col.AllowDBNull = true;

                result_table.Columns.Add(data_col);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                var row = result_table.NewRow();        //создание строк

                row[0] = lines[i];
                for (int j = 1; j <= columns.Count; j++)
                {
                    var temp = ((decimal?)(table.Rows[row_start + i].Field<double?>(col_start + (j - 1))));       //заполнение таблицы согласно отступам в документе
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
                        if (table.Rows[i].Field<decimal?>(j) != null) table.Rows[i].SetField<decimal>(j, Math.Round(table.Rows[i].Field<decimal>(j), num_of_symb));
        }

        private protected void createLinesAndColumns(DataTable table, int col_names_index, int row_names_index, int num_of_rows)
        {
            //Создание заголовков столбцов для датасета
            for (int i = 1; i < 4; i++)
                columns.Add("Финансовые корпорации. " + table.Rows[col_names_index].Field<string>(i));
            for (int i = 4; i < 8; i++)
                columns.Add(table.Rows[col_names_index - 1].Field<string>(i));

            //Создание заголовков строк для датасета
            for (int i = row_names_index; i < num_of_rows; i++)
                lines.Add(table.Rows[i].Field<string>(0));
        }
    }
}
