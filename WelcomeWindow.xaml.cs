using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ExcelDataReader;
using System.IO;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using Newtonsoft.Json;

namespace EcoSys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        string last_path = String.Empty;
        public WelcomeWindow()
        {
            InitializeComponent();

            try
            {
                StreamReader reader = new StreamReader("cache");        //проверка на наличие кэша и отображение последнего пути
                string input = reader.ReadLine();
                last_path = input.Split(' ')[0];        //запоминаем путь в переменной
                LastPath.Text = input;
                reader.Close();
            } 
            catch (System.IO.FileNotFoundException exc)
            {
                OpenLastButton.IsEnabled = false;
            }
            catch (System.NullReferenceException exc1)
            {
                OpenLastButton.IsEnabled = false;
            }
            
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)       //Событие при нажатии на кнопку импорта данных
        {
            string file_path = string.Empty;

            var file_dialog = new Microsoft.Win32.OpenFileDialog();
            file_dialog.Filter = "Excel files (.xls)|*.xls|Excel files (.xlsx)|*.xlsx";     //фильтруем только эксель файлы
            file_dialog.FilterIndex = 2;

            if (file_dialog.ShowDialog() == true)
                file_path = file_dialog.FileName;
            if (file_path == string.Empty) return;      //Получение пути файла

            try
            {
                importingExcelData(file_path);
                createLogFile(file_path);
            } 
            catch (IOException exc)
            {
                var dialog_result = MessageBox.Show("Возникла ошибка при считывании. Пожалуйста, убедитесь, что файл не используется другими процессами", "Ошибка импортирования", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK) return;
            }

        }

        private void createLogFile(string file_path)        //Метод для создания/переписывания лог-файла
        {
            var writer = new StreamWriter("cache");
            writer.Write(String.Join(' ', file_path, DateTime.Now.ToString(new CultureInfo("ru-RU"))));
            writer.Close();
        }

        private async void importingExcelData(string file_path)        //Метод для импорта данных. Использование асинхронных методов
        {
            FileStream stream;
            try
            {
                stream = File.OpenRead(file_path);      //Попытка открытия файла для чтения данных
            }
            catch
            {
                Console.WriteLine("Не удалось открыть файл. Возможно, он открыт в другом приложении");
                return;
            }
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration() { FallbackEncoding = Encoding.GetEncoding(1252) });
            var dataset = reader.AsDataSet();

            stream.Close();     //Закрываем считываемые потоки
            reader.Close();

            Entities.DataEntity new_entity = new Entities.DataEntity();

            await Task.Run(() => new_entity.createTables(dataset));     //Начинаем асинхронное заполнение таблиц на основе датасета

            WorkWindow work_window = new WorkWindow(new_entity);
            work_window.Show();
            this.Close();
        }

        private void importingJsonData(string file_path)      //Десериализация json-файла
        {
            System.ComponentModel.TypeDescriptor.AddAttributes(typeof((string, string)), new System.ComponentModel.TypeConverterAttribute(typeof(Entities.TupleConverter<string, string>)));        //ИСпользование кастомного конвертера
            //ДОБАВИТЬ ПРОВЕРКУ НА ОТКРЫТОСТЬ ФАЙЛА
            Entities.DataEntity new_entity;
            try
            {
                new_entity = JsonConvert.DeserializeObject<Entities.DataEntity>(File.ReadAllText(file_path));
            }
            catch
            {
                Console.WriteLine("Не удалось открыть файл для чтения. Возможно, он уже открыт в другом приложении");
                return;
            }
            WorkWindow work_window = new WorkWindow(new_entity);
            work_window.Show();
            this.Close();
        }

        private void OpenLastButton_Click(object sender, RoutedEventArgs e)     //открытие последнего использованного файла
        {
            try
            {
                if (last_path.Contains(".json")) importingJsonData(last_path); else importingExcelData(last_path);      //Проверка на формат последнего файла
                createLogFile(last_path);
            }
            catch (IOException exc)
            {
                var dialog_result = MessageBox.Show("Возникла ошибка при считывании. Пожалуйста, убедитесь, что файл не используется другими процессами", "Ошибка импортирования", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK) return;
            }
        }

        private void ImportJSON_Click(object sender, RoutedEventArgs e)     //открытие JSON-файла
        {
            string file_path = string.Empty;

            var file_dialog = new Microsoft.Win32.OpenFileDialog();
            file_dialog.Filter = "JSON file (.json)|*.json";     //фильтруем только JSON файлы

            if (file_dialog.ShowDialog() == true)
                file_path = file_dialog.FileName;
            if (file_path == string.Empty) return;      //Получение пути файла

            try
            {
                importingJsonData(file_path);
                createLogFile(file_path);
            }
            catch (IOException exc)
            {
                var dialog_result = MessageBox.Show("Возникла ошибка при считывании. Пожалуйста, убедитесь, что файл не используется другими процессами", "Ошибка импортирования", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK) return;
            }
        }
    }
}
