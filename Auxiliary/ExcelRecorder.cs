using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace EcoSys.Auxiliary
{
    public static class ExcelRecorder
    {
        public static async void writeToExcel(DataTable table, HashSet<string> regions, string year, string type)
        {
            var save_filedialog = new SaveFileDialog
            {
                Filter = "Excel-Файл (*.xlsx)|*.xlsx|Excel-файл (*.xlsm)|*.xlsm",
                RestoreDirectory = true
            };     //Происходит открытие диалогового окна для сохранения файла

            if (save_filedialog.ShowDialog() == true)
            {
                if (save_filedialog.FileName == string.Empty)
                    throw new Exception();

                try
                {
                    IXLWorkbook workbook = new XLWorkbook();

                    IXLWorksheet worksheet = workbook.Worksheets.Add("Таблица 1");

                    worksheet.Cell(1, 1).Value = String.Format("Задействованные регионы: {0}", String.Join(',', regions));
                    worksheet.Cell(2, 1).Value = String.Format("Год выборки: {0}", year);
                    worksheet.Cell(3, 1).Value = String.Format("Тип матрицы: {0}", type);

                    worksheet.Cell(5, 1).InsertTable(table);

                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(save_filedialog.FileName);

                    MessageBox.Show("Файл успешно создан!", "", MessageBoxButton.OK);
                }
                catch (Exception)
                {
                    var dialog_result = MessageBox.Show("Возникла ошибка при создании файла. Попробуйте еще раз", "Ошибка сохранения", MessageBoxButton.OK);
                    if (dialog_result == MessageBoxResult.OK)
                    {
                        return;
                    }
                }

            }
        }

        public static async void writeToExcel(List<DataTable> tables, HashSet<string> regions, string year)
        {
            var save_filedialog = new SaveFileDialog
            {
                Filter = "Excel-Файл (*.xlsx)|*.xlsx|Excel-файл (*.xlsm)|*.xlsm",
                RestoreDirectory = true
            };     //Происходит открытие диалогового окна для сохранения файла

            if (save_filedialog.ShowDialog() == true)
            {
                if (save_filedialog.FileName == string.Empty)
                    throw new Exception();

                try
                {
                    IXLWorkbook workbook = new XLWorkbook();

                    tables[0].TableName = "Пассивная часть";
                    tables[1].TableName = "Активная часть";
                    tables[2].TableName = "Сальдированная часть";

                    for (int i = 0; i < 3; i++)
                    {
                        IXLWorksheet worksheet = workbook.Worksheets.Add(tables[i].TableName);

                        worksheet.Cell(1, 1).Value = String.Format("Задействованные регионы: {0}", String.Join(',', regions));
                        worksheet.Cell(2, 1).Value = String.Format("Год выборки: {0}", year);
                        worksheet.Cell(3, 1).Value = String.Format("Тип матрицы: {0}", tables[i].TableName);

                        worksheet.Cell(5, 1).InsertTable(tables[i]);

                        worksheet.Columns().AdjustToContents();
                    }


                    workbook.SaveAs(save_filedialog.FileName);

                    MessageBox.Show("Файл успешно создан!", "", MessageBoxButton.OK);
                }
                catch (Exception)
                {
                    var dialog_result = MessageBox.Show("Возникла ошибка при создании файла. Попробуйте еще раз", "Ошибка сохранения", MessageBoxButton.OK);
                    if (dialog_result == MessageBoxResult.OK)
                    {
                        return;
                    }
                }

            }
        }

        public static async void writeToExcel(DataTable table, string region)
        {
            var save_filedialog = new SaveFileDialog
            {
                Filter = "Excel-Файл (*.xlsx)|*.xlsx|Excel-файл (*.xlsm)|*.xlsm",
                RestoreDirectory = true
            };     //Происходит открытие диалогового окна для сохранения файла

            if (save_filedialog.ShowDialog() == true)
            {
                if (save_filedialog.FileName == string.Empty)
                    throw new Exception();

                try
                {
                    IXLWorkbook workbook = new XLWorkbook();

                    IXLWorksheet worksheet = workbook.Worksheets.Add("Таблица 1");

                    worksheet.Cell(1, 1).Value = "Сценарная модель движения банковского капитала";
                    worksheet.Cell(2, 1).Value = String.Format("Регион: {0}", region);

                    worksheet.Cell(4, 1).InsertTable(table);

                    worksheet.Columns().AdjustToContents();


                    workbook.SaveAs(save_filedialog.FileName);

                    MessageBox.Show("Файл успешно создан!", "", MessageBoxButton.OK);
                }
                catch (Exception)
                {
                    var dialog_result = MessageBox.Show("Возникла ошибка при создании файла. Попробуйте еще раз", "Ошибка сохранения", MessageBoxButton.OK);
                    if (dialog_result == MessageBoxResult.OK)
                    {
                        return;
                    }
                }

            }
        }
    }
}
