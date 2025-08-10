using System;
using System.Threading.Tasks;
using System.Windows;
using CampaignMaster.Misc;
using CampaignMaster.Models;
using CampaignMaster.Windows;
using SamCorp.WPF.Alerts;
using SamCorp.WPF.Logging;

namespace CampaignMaster {

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        private static mdlCampaign currentCampaign;

        public static mdlCampaign CurrentCampaign {
            get => currentCampaign;
            set {
                currentCampaign = value;
                CampaignChanged?.Invoke(currentCampaign, EventArgs.Empty);
            }
        }

        public static Settings Settings = new();

        public static event EventHandler CampaignChanged;

        public static async void SaveCampaign() {
            await CurrentCampaign.Save();
            Alert.FadeInfo("Campaign Saved!");
        }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            Log.Start(true);

            //initialize the splash screen and set it as the application main window
            var splashScreen = new wndSplashScreen();
            this.MainWindow = splashScreen;
            splashScreen.Show();

            //in order to ensure the UI stays responsive, we need to
            //do the work on a different thread
            Task.Run(async () => {
                await Task.Delay(200);
                //since we're not on the UI thread
                //once we're done we need to use the Dispatcher
                //to create and show the main window
                this.Dispatcher.Invoke(() => {
                    //initialize the main window, set it as the application main window
                    //and close the splash screen
                    Settings = Settings.LoadSettings();
                    var mainWindow = new wndCampaignMaster();
                    MainWindow = mainWindow;
                    mainWindow.Show();
                    new wndStart().Show();
                    splashScreen.Close();
                });
            });
        }

    }

}