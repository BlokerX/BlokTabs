using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BlokTabs
{
    public partial class App : Application
    {
        public static string ExternalStorageAbsolutePath; 
        public App()
        {
            InitializeComponent();
            DeviceDisplay.KeepScreenOn = true;
            MainPage = new MainPage();
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
