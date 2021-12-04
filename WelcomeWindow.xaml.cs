using ExcelDataReader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EcoSys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        private string data_last_path = String.Empty, scenario_last_path = String.Empty, model_last_path = String.Empty;     //переменная для запоминания последнего пути файла с данными МДФП, сценариев и моделей
        private Entities.DataEntity data_entity;     //объект для хранения данных МДФП
        private Entities.ScenarioEntity scenario_entity;        //Объект для хранения сценариев ДФП
        private Entities.ModelEntity model_entity;      //Объект для хранения моделей
        private bool autolaunch_available = false;    //Переменная, задающая параметр автозапуска
        private bool first_condition = false, second_condition = false, third_condition = false;     //Переменные, отображающие готовность перехода в рабочую область

        public WelcomeWindow(bool already_initialized)
        {
            InitializeComponent();

            System.Threading.Thread.Sleep(1000);

            try
            {
                StreamReader autolaunch = new StreamReader("settings");

                string is_auto = autolaunch.ReadLine().Split('=')[1];

                if (is_auto.Equals("True") && !already_initialized) autolaunch_available = true;
            }
            catch
            {
                Console.WriteLine("Файл автозагрузки не найден или был поврежден");
            }

            try
            {
                StreamReader reader = new StreamReader("cache");        //проверка на наличие кэша и отображение последнего пути

                tryToFindLastFile(reader, ref data_last_path, OpenLastButton, LastPath);        //производим парсинг строки, содержащей адрес последнего файла данных
                tryToFindLastFile(reader, ref scenario_last_path, ScenarioLastButton, LastScenarioPath);        //то же самое для файла сценариев
                tryToFindLastFile(reader, ref model_last_path, ModelLastButton, LastModelPath);

                reader.Close();
            }
            catch
            {
                Console.WriteLine("Файл кэша не найден. Убедитесь в его наличии");

                OpenLastButton.IsEnabled = false;       //Если файл кэша не найден, блокируем кнопки запуска последних используемых файлов
                ScenarioLastButton.IsEnabled = false;
                ModelLastButton.IsEnabled = false;

                return;
            }

            if (autolaunch_available)
            {
                OpenLastButton_Click(OpenLastButton, new RoutedEventArgs());
                OpenLastButton_Click(ScenarioLastButton, new RoutedEventArgs());
                OpenLastButton_Click(ModelLastButton, new RoutedEventArgs());
            }
        }

        /// <summary>
        /// Метод для автоматической загрузки кэша и поиска двух последних путей успешно открытых файлов данных и сценариев
        /// </summary>
        /// <param name="reader">Reader, с помощью которого осуществляется обращение к файлу кэша</param>
        /// <param name="param">Ссылка на строку, определяющую тип считываемого значения - данные или сценарии</param>
        /// <param name="button">Одна из двух кнопок, на которую будет "повешен" обработчик нажатия кнопки мыши</param>
        /// <param name="text_box">Указание UI компонента TextBox, с помощью которого будет выведен путь до файла</param>
        private void tryToFindLastFile(StreamReader reader, ref string param, Button button, TextBox text_box)
        {
            try
            {
                string input = reader.ReadLine();       //парсим строку, содержащую путь к файлу и дату последнего использования
                text_box.Text = String.Join(' ',input.Split("[delimeter]", StringSplitOptions.None));
                param = input.Split("[delimeter]", StringSplitOptions.None)[0];        //запоминаем путь в переменной для дальнейшего открытия
            }
            catch (Exception exc)
            {
                button.IsEnabled = false;
            }
        }


        private async void ImportButton_Click(object sender, RoutedEventArgs e)       //Событие при нажатии на кнопку импорта данных (обработчик клика лежит на двух кнопках сразу. Для этого добавлена проверка имени кнопки)
        {
            string file_path = string.Empty;

            var file_dialog = new Microsoft.Win32.OpenFileDialog();
            file_dialog.Filter = "Excel files (.xls)|*.xls|Excel files (.xlsx)|*.xlsx";     //фильтруем только эксель файлы
            file_dialog.FilterIndex = 2;

            if (file_dialog.ShowDialog() == true)
                file_path = file_dialog.FileName;
            if (file_path == string.Empty) return;      //Если файл не выбран, прерываем процедуру импортирования

            try
            {
                if (((Button)sender).Name == "ImportButton")
                {
                    LoadScreen1.Visibility = Visibility.Visible;

                    await Task.Run(() => importingExcelData(file_path, "Data"));

                    LoadScreen1.Visibility = Visibility.Hidden;
                    if (first_condition) FirstIsOK.Visibility = Visibility.Visible;
                    checkConditions();
                }
                else if (((Button)sender).Name == "ScenarioImportButton")
                {
                    LoadScreen2.Visibility = Visibility.Visible;

                    await Task.Run(() => importingExcelData(file_path, "Scenario"));     //В зависимости от типа данных, выбираем соответствующий параметр для функции

                    LoadScreen2.Visibility = Visibility.Hidden;
                    if (second_condition) SecondIsOK.Visibility = Visibility.Visible;
                    checkConditions();
                }
                else
                {
                    LoadScreen3.Visibility = Visibility.Visible;

                    await Task.Run(() => importingExcelData(file_path, "Model"));

                    LoadScreen3.Visibility = Visibility.Hidden;
                    if (third_condition) ThirdIsOK.Visibility = Visibility.Visible;
                    checkConditions();
                }
            }
            catch (IOException exc)
            {
                var dialog_result = MessageBox.Show("Возникла ошибка при считывании. Пожалуйста, убедитесь, что файл не используется другими процессами", "Ошибка импортирования", MessageBoxButton.OK);
                if (dialog_result == MessageBoxResult.OK) return;
            }
        }


        /// <summary>
        /// Метод для асинхронного считывания Excel-файла
        /// </summary>
        /// <param name="file_path">Путь к Excel-файлу</param>
        /// <param name="type">Тип файла, который мы собираемся открыть - данные или сценарии</param>
        private void importingExcelData(string file_path, string type)        //Метод для импорта данных. Использование асинхронных методов
        {
            FileStream stream;

            try
            {
                stream = File.OpenRead(file_path);      //Попытка открытия файла для чтения данных
            }
            catch
            {
                MessageBox.Show("Не удалось открыть файл. Убедитесь, что он не открыт в другом приложении");
                return;
            }

            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration() { FallbackEncoding = Encoding.GetEncoding(1252) });     //С помощью библиотеки ExcelReader считываем датасет файла Excel
            var dataset = reader.AsDataSet();

            stream.Close();     //Закрываем считываемые потоки
            reader.Close();

            if (type == "Data")
            {
                data_entity = new Entities.DataEntity();        //Создаем новый экземпляр класса "объекта данных"
                try
                {
                    data_entity.createTables(dataset);     //Начинаем асинхронное заполнение таблиц на основе датасета
                    if (!data_entity.checkCorrectness()) throw new Exception();
                    data_last_path = file_path;     //Запоминаем путь к последнему успешно открытому файлу
                    firstOK();
                }
                catch (Exception exc)
                {
                    MessageBox.Show(String.Format("Файл {0} не соответствует необходимой структуре. Убедитесь в правильности выбора файла", type));
                }

            }
            else if (type == "Scenario")
            {
                scenario_entity = new Entities.ScenarioEntity();        //Создаем новый экземпляр класса "файла сценариев"
                try
                {
                    scenario_entity.createTables(dataset);
                    if (!scenario_entity.checkCorrectness()) throw new Exception();
                    scenario_last_path = file_path;     //Запоминаем путь к последнему успешно импортированному файлу сценариев
                    secondOK();
                }
                catch
                {
                    MessageBox.Show(String.Format("Файл {0} не соответствует необходимой структуре. Убедитесь в правильности выбора файла", type));
                }
            }
            else if (type == "Model")
            {
                model_entity = new Entities.ModelEntity();        //Создаем новый экземпляр класса "файла сценариев"
                try
                {
                    model_entity.createTables(dataset);
                    if (!model_entity.checkCorrectness()) throw new Exception();
                    model_last_path = file_path;     //Запоминаем путь к последнему успешно импортированному файлу сценариев
                    thirdOK();
                }
                catch
                {
                    MessageBox.Show(String.Format("Файл {0} не соответствует необходимой структуре. Убедитесь в правильности выбора файла", type));
                }
            }
        }


        private async void ImportJSON_Click(object sender, RoutedEventArgs e)     //обработка события нажатия на кнопку "Импортировать JSON"
        {
            string file_path = string.Empty;

            var file_dialog = new Microsoft.Win32.OpenFileDialog();
            file_dialog.Filter = "JSON file (.json)|*.json";     //фильтруем только JSON файлы

            if (file_dialog.ShowDialog() == true)
                file_path = file_dialog.FileName;
            if (file_path == string.Empty) return;      //Проверка корректности имени выбранного файла

            try
            {
                LoadScreen1.Visibility = Visibility.Visible;

                await Task.Run(() => importingJsonData(file_path));       //вызываем соответствующую функцию
                data_last_path = file_path;     //Запоминаем путь к последнему успешно импортированному файлу сценариев

                LoadScreen1.Visibility = Visibility.Hidden;
                if (first_condition) FirstIsOK.Visibility = Visibility.Visible;
                checkConditions();
            }
            catch (IOException exc)
            {
                MessageBox.Show("Возникла ошибка при считывании. Пожалуйста, убедитесь, что файл не используется другими процессами", "Ошибка импортирования", MessageBoxButton.OK);
                return;
            }
        }


        /// <summary>
        /// Метод для считывания Json-файла (десериализации)
        /// </summary>
        /// <param name="file_path">Путь к Json-файлу</param>
        private void importingJsonData(string file_path)      //Десериализация json-файла
        {
            System.ComponentModel.TypeDescriptor.AddAttributes(typeof((string, string)), new System.ComponentModel.TypeConverterAttribute(typeof(Entities.TupleConverter<string, string>)));        //ИСпользование кастомного конвертера
            try
            {
                 data_entity = JsonConvert.DeserializeObject<Entities.DataEntity>(File.ReadAllText(file_path));        //Десериализуем Json-объект

                if (!data_entity.checkCorrectness()) throw new Exception();     //проверяем корректность (полноту заполнения данных)
                firstOK();
            }
            catch
            {
                MessageBox.Show("Не удалось открыть файл для чтения. Проверьте корректность Json файла", "Ошибка импортирования", MessageBoxButton.OK);
                return;
            }
        }


        private async void OpenLastButton_Click(object sender, RoutedEventArgs e)     //Обработчик события открытия последнего использованного файла
        {
            if (((Button)sender).Name == "OpenLastButton")            //Проверка на формат последнего файла и вызов соответствующей функции импортирования
            {
                LoadScreen1.Visibility = Visibility.Visible;

                if (data_last_path.Contains(".json"))
                    await Task.Run(() => importingJsonData(data_last_path));
                else
                    await Task.Run(() => importingExcelData(data_last_path, "Data"));

                LoadScreen1.Visibility = Visibility.Hidden;
               if (first_condition) FirstIsOK.Visibility = Visibility.Visible;
            }

            //Обработчик клика лежит одновременно на двух кнопках, поскольку функционал дублируется. Для этого добавлена проверка имени кнопки
            else if (((Button)sender).Name == "ScenarioLastButton")
            {
                LoadScreen2.Visibility = Visibility.Visible;

                await Task.Run(() => importingExcelData(scenario_last_path, "Scenario"));

                LoadScreen2.Visibility = Visibility.Hidden;
                if (second_condition) SecondIsOK.Visibility = Visibility.Visible;
            }
            else
            {
                LoadScreen3.Visibility = Visibility.Visible;

                await Task.Run(() => importingExcelData(model_last_path, "Model"));

                LoadScreen3.Visibility = Visibility.Hidden;
                if (third_condition) ThirdIsOK.Visibility = Visibility.Visible;
            }
            checkConditions();
        }

        private void checkConditions()      //проверка условия перехода в рабочую область программы
        {
            if (first_condition && second_condition && third_condition)        //Если условия соблюдены, то...
            {
                createLogFile();        //Создаем лог-файл

                WorkWindow work_window = new WorkWindow(data_entity, scenario_entity, model_entity, autolaunch_available);      //Передаем данные в рабочую область
                work_window.Show();
                this.Close();       //закрываем это окно
            }
        }


        private void createLogFile()        //Метод для создания/переписывания лог-файла
        {
            var writer = new StreamWriter("cache");     //Создание/открытие файла кэша

            writer.WriteLine(String.Join("[delimeter]", data_last_path, DateTime.Now.ToString(new CultureInfo("ru-RU"))));        //Записываем в кэш путь последних успешно загруженных файлов
            writer.WriteLine(String.Join("[delimeter]", scenario_last_path, DateTime.Now.ToString(new CultureInfo("ru-RU"))));
            writer.WriteLine(String.Join("[delimeter]", model_last_path, DateTime.Now.ToString(new CultureInfo("ru-RU"))));
            writer.Close();
        }


        private void FirstIsOK_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FirstIsOK.Visibility = Visibility.Hidden;
        }


        private void SecondIsOK_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SecondIsOK.Visibility = Visibility.Hidden;
        }

        private void ThirdIsOK_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ThirdIsOK.Visibility = Visibility.Hidden;

        }

        private void firstOK()      //Метод для смены состояния готовности первого файла и проверки состояний
        {
            first_condition = true;
        }


        private void secondOK()     
        {
            second_condition = true;
        }

        private void thirdOK()
        {
            third_condition = true;
        }
    }
}