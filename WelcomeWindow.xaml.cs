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
        private StreamWriter writer;        //объект для записи кэша и сохранения пути файлов
        private string data_last_path = String.Empty, scenario_last_path = String.Empty;     //переменная для запоминания последнего пути файла с данными МДФП
        private Entities.DataEntity data_entity;     //объект для хранения данных МДФП
        private Entities.ScenarioEntity scenario_entity;        //Объект для хранения сценариев ДФП

        private bool first_condition = false, second_condition = false;     //Переменные, отображающие готовность перехода в рабочую область
        public WelcomeWindow()
        {
            InitializeComponent();

            StreamReader reader = new StreamReader("cache");        //проверка на наличие кэша и отображение последнего пути

            tryToFindLastFile(reader, ref data_last_path, OpenLastButton, LastPath);
            tryToFindLastFile(reader, ref scenario_last_path, ScenarioLastButton, LastScenarioPath);

            reader.Close();
        }

        private void tryToFindLastFile(StreamReader reader, ref string param, Button button, TextBox text_box)
        {
            try
            {
                string input = reader.ReadLine();
                param = input.Split(' ')[0];        //запоминаем путь в переменной
                text_box.Text = input;
            }
            catch (System.IO.FileNotFoundException exc)
            {
                button.IsEnabled = false;
            }
            catch (System.NullReferenceException exc1)
            {
                button.IsEnabled = false;
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
                if (((Button)sender).Name == "ImportButton") importingExcelData(file_path, "Data"); else importingExcelData(file_path, "Scenario");
            }
            catch (IOException exc)
            {
                var dialog_result = MessageBox.Show("Возникла ошибка при считывании. Пожалуйста, убедитесь, что файл не используется другими процессами", "Ошибка импортирования", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK) return;
            }

        }

        private void createLogFile()        //Метод для создания/переписывания лог-файла
        {
            var writer = new StreamWriter("cache");

            writer.WriteLine(String.Join(' ', data_last_path, DateTime.Now.ToString(new CultureInfo("ru-RU"))));
            writer.WriteLine(String.Join(' ', scenario_last_path, DateTime.Now.ToString(new CultureInfo("ru-RU"))));

            writer.Close();
        }

        private async void importingExcelData(string file_path, string type)        //Метод для импорта данных. Использование асинхронных методов
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

            if (type == "Data")
            {
                data_entity = new Entities.DataEntity();

                await Task.Run(() => data_entity.createTables(dataset));     //Начинаем асинхронное заполнение таблиц на основе датасета

                data_last_path = file_path;

                firstOK();
            }
            else if (type == "Scenario")
            {
                scenario_entity = new Entities.ScenarioEntity();

                await Task.Run(() => scenario_entity.createTables(dataset));

                scenario_last_path = file_path;

                secondOK();
            }

        }

        private void importingJsonData(string file_path, string type)      //Десериализация json-файла
        {
            System.ComponentModel.TypeDescriptor.AddAttributes(typeof((string, string)), new System.ComponentModel.TypeConverterAttribute(typeof(Entities.TupleConverter<string, string>)));        //ИСпользование кастомного конвертера
            try
            {
                if (type == "Data")
                {
                    data_entity = JsonConvert.DeserializeObject<Entities.DataEntity>(File.ReadAllText(file_path));

                    data_last_path = file_path;

                    firstOK();
                }
                else if (type == "Scenario")
                {
                    System.ComponentModel.TypeDescriptor.AddAttributes(typeof((string, string, string)), new System.ComponentModel.TypeConverterAttribute(typeof(Entities.TripletConverter<string, string, string>)));        //ИСпользование кастомного конвертера

                    scenario_entity = JsonConvert.DeserializeObject<Entities.ScenarioEntity>(File.ReadAllText(file_path));

                    scenario_last_path = file_path;

                    secondOK();
                }
            }
            catch
            {
                MessageBox.Show("Не удалось открыть файл для чтения. Возможно, он уже открыт в другом приложении", "Ошибка импортирования", MessageBoxButton.OK);
                return;
            }
        }

        private void OpenLastButton_Click(object sender, RoutedEventArgs e)     //открытие последнего использованного файла
        {
            try
            {
                if (((Button)sender).Name == "OpenLastButton")
                    if (data_last_path.Contains(".json")) importingJsonData(data_last_path, "Data"); else importingExcelData(data_last_path, "Data");      //Проверка на формат последнего файла
                else
                    if (scenario_last_path.Contains(".json")) importingJsonData(scenario_last_path, "Scenario"); else importingExcelData(scenario_last_path, "Scenario");
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
                if (((Button)sender).Name == "ImportJSON") importingJsonData(file_path, "Data"); else importingJsonData(file_path, "Scenario");
            }
            catch (IOException exc)
            {
                var dialog_result = MessageBox.Show("Возникла ошибка при считывании. Пожалуйста, убедитесь, что файл не используется другими процессами", "Ошибка импортирования", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK) return;
            }
        }

        private void checkConditions()
        {
            if (first_condition && second_condition)
            {

                createLogFile();

                WorkWindow work_window = new WorkWindow(data_entity, scenario_entity);
                work_window.Show();
                this.Close();
            }
        }

        private void firstOK()
        {
            FirstIsOK.Visibility = Visibility.Visible;
            first_condition = true;
            checkConditions();
        }

        private void secondOK()
        {
            SecondIsOK.Visibility = Visibility.Visible;
            second_condition = true;
            checkConditions();
        }
    }
}