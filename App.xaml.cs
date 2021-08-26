using System.Windows;

namespace EcoSys
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void startApplication(object sender, StartupEventArgs e)
        {
            WelcomeWindow start_page = new WelcomeWindow(false);
            start_page.Show();
        }
    }
}
