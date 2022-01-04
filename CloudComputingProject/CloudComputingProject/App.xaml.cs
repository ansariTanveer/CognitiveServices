using CloudComputingProject.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CloudComputingProject
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new DashboardPage());
            //MainPage = new NavigationPage(new OCRPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
