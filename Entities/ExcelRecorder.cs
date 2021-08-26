using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Data;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using ClosedXML.Excel;
using System.Windows.Controls;

namespace EcoSys.Entities
{
    public static class ExcelRecorder
    {
        public static async void writeToExcel(DataTable table, HashSet<string> regions, string year, string type)
        {
            var save_filedialog = new SaveFileDialog();     //Происходит открытие диалогового окна для сохранения файла

            save_filedialog.Filter = "Excel-Файл (*.xlsx)|*.xlsx|Excel-файл (*.xlsm)|*.xlsm";
            save_filedialog.RestoreDirectory = true;

            if (save_filedialog.ShowDialog() == true)
            {
                if (save_filedialog.FileName == string.Empty) throw new Exception();
                try
                {
                    IXLWorkbook workbook = new XLWorkbook();

                    IXLWorksheet worksheet = workbook.Worksheets.Add("Таблица 1");

                    worksheet.Cell(1, 1).Value = String.Format("Задействованные регионы: {0}",  String.Join(',', regions));
                    worksheet.Cell(2, 1).Value = String.Format("Год выборки: {0}", year);
                    worksheet.Cell(3, 1).Value = String.Format("Тип матрицы: {0}", type);

                    worksheet.Cell(5, 1).InsertTable(table);

                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(save_filedialog.FileName);

                    MessageBox.Show("Файл успешно создан!", "", MessageBoxButton.OK);
                }
                catch (Exception exc)
                {
                    var dialog_result = MessageBox.Show("Возникла ошибка при создании файла. Попробуйте еще раз", "Ошибка сохранения", MessageBoxButton.OK);
                    if (dialog_result == MessageBoxResult.OK) return;
                }

            }
        }

        public static async void writeToExcel(List<DataTable> tables, HashSet<string> regions, string year)
        {
            var save_filedialog = new SaveFileDialog();     //Происходит открытие диалогового окна для сохранения файла

            save_filedialog.Filter = "Excel-Файл (*.xlsx)|*.xlsx|Excel-файл (*.xlsm)|*.xlsm";
            save_filedialog.RestoreDirectory = true;

            if (save_filedialog.ShowDialog() == true)
            {
                if (save_filedialog.FileName == string.Empty) throw new Exception();
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
                catch (Exception exc)
                {
                    var dialog_result = MessageBox.Show("Возникла ошибка при создании файла. Попробуйте еще раз", "Ошибка сохранения", MessageBoxButton.OK);
                    if (dialog_result == MessageBoxResult.OK) return;
                }

            }
        }

        public static async void writeToExcel(DataTable table, string region)
        {
            var save_filedialog = new SaveFileDialog();     //Происходит открытие диалогового окна для сохранения файла

            save_filedialog.Filter = "Excel-Файл (*.xlsx)|*.xlsx|Excel-файл (*.xlsm)|*.xlsm";
            save_filedialog.RestoreDirectory = true;

            if (save_filedialog.ShowDialog() == true)
            {
                if (save_filedialog.FileName == string.Empty) throw new Exception();
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
                catch (Exception exc)
                {
                    var dialog_result = MessageBox.Show("Возникла ошибка при создании файла. Попробуйте еще раз", "Ошибка сохранения", MessageBoxButton.OK);
                    if (dialog_result == MessageBoxResult.OK) return;
                }

            }
        }

        public static async void writeToExcel(List<object> bundle, ItemCollection tables, HashSet<string> region, string category, string year)
        {
            var save_filedialog = new SaveFileDialog();     //Происходит открытие диалогового окна для сохранения файла

            save_filedialog.Filter = "Excel-Файл (*.xlsx)|*.xlsx|Excel-файл (*.xlsm)|*.xlsm";
            save_filedialog.RestoreDirectory = true;

            if (save_filedialog.ShowDialog() == true)
            {
                if (save_filedialog.FileName == string.Empty) throw new Exception();
                try
                {
                    IXLWorkbook workbook = new XLWorkbook();

                    IXLWorksheet graph_worksheet = workbook.Worksheets.Add("Сценарные условия");

                    IXLWorksheet data_worksheet = workbook.Worksheets.Add("Прогнозные сценарии");

                    data_worksheet.Cell(1, 1).Value = "Прогнозные сценарии движения банковского капитала";
                    data_worksheet.Cell(3, 1).Value = String.Format("Выбранные регионы: {0}", String.Join(',', region));
                    data_worksheet.Cell(4, 1).Value = String.Format("Год сценария: {0}", year);

                    int starting_row = 6;
                    foreach (var item in tables)
                    {
                        var instance = (TabItem)item;

                        data_worksheet.Cell(starting_row++, 1).Value = instance.Header;

                        DataTable temp = ((DataView)((DataGrid)instance.Content).ItemsSource).Table;
                        data_worksheet.Cell(starting_row, 1).InsertTable(temp);

                        starting_row += temp.Rows.Count + 1;
                    }

                    data_worksheet.Columns().AdjustToContents();


                    workbook.SaveAs(save_filedialog.FileName);

                    MessageBox.Show("Файл успешно создан!", "", MessageBoxButton.OK);
                }
                catch (Exception exc)
                {
                    var dialog_result = MessageBox.Show("Возникла ошибка при создании файла. Попробуйте еще раз", "Ошибка сохранения", MessageBoxButton.OK);
                    if (dialog_result == MessageBoxResult.OK) return;
                }

            }
        }
    }
}
