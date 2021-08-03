using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace EcoSys.Grids
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class SettingsGrid : UserControl, IGrid
    {
        Entities.DataEntity data;

        WorkWindow parent_window;

        public SettingsGrid(Entities.DataEntity data, WorkWindow parent)
        {
            InitializeComponent();
            this.data = data;
            parent_window = parent;
        }

        ~SettingsGrid()
        {
            GC.Collect();
        }

        private void JSONButton_Click(object sender, RoutedEventArgs e)
        {

            var save_filedialog = new SaveFileDialog();     //Происходит открытие диалогового окна для сохранения файла

            save_filedialog.Filter = "JSON (*.json)|*.json";
            save_filedialog.RestoreDirectory = true;

            if (save_filedialog.ShowDialog() == true)
            {
                if (save_filedialog.FileName == string.Empty) throw new Exception();
                try
                {
                    var writer = File.CreateText(save_filedialog.FileName);
                    System.ComponentModel.TypeDescriptor.AddAttributes(typeof((string, string)), new System.ComponentModel.TypeConverterAttribute(typeof(Entities.TupleConverter<string, string>)));        //использование кастомного конвертера
                    writer.Write(JsonConvert.SerializeObject(data));        //Сериализуем объект с помощью Newtonsoft
                    writer.Close();
                    MessageBox.Show("Файл успешно создан!", "", MessageBoxButton.OK);
                }
                catch (Exception exc)
                {
                    var dialog_result = MessageBox.Show("Возникла ошибка при создании файла. Попробуйте еще раз", "Ошибка сохранения", MessageBoxButton.OK);
                    if (dialog_result == MessageBoxResult.OK) return;
                }
                    
            }

            
        }

        private void BackToChoose_Click(object sender, RoutedEventArgs e)       //кнопка для возвращения на предыдущую форму
        {
            var result = MessageBox.Show("При переходе на предыдущий экран прогресс будет утерян. Продолжить?", "Предупреждение", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                WelcomeWindow welcome_window = new WelcomeWindow();
                welcome_window.Show();
                GC.Collect();
                parent_window.Close();
            }
        }

        public void hideGrid()
        {
            this.Visibility = Visibility.Hidden;
        }

        public void showGrid()
        {
            this.Visibility = Visibility.Visible;
        }

    }
}
