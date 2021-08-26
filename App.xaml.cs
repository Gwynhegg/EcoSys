using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
