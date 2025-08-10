using System;
using System.Windows;
using SamCorp.WPF.Alerts;

namespace CampaignMaster.Windows {

    /// <summary>
    /// Interaction logic for wndPlayerMap.xaml
    /// </summary>
    public partial class wndPlayerMap : Window {

        public wndPlayerMap() {
            InitializeComponent();
        }

        public void PositionWindow() {
            try {
                if (System.Windows.Forms.Screen.AllScreens.Length > 1) {
                    var index = 0;
                    if (System.Windows.Forms.Screen.AllScreens[0] == System.Windows.Forms.Screen.PrimaryScreen)
                        index = 1;

                    Top = 30;
                    Left = System.Windows.Forms.Screen.AllScreens[index].WorkingArea.Left;

                    WindowState = WindowState.Maximized;
                    ResizeMode = ResizeMode.NoResize;
                    WindowStyle = WindowStyle.None;
                }
            } catch (Exception ex) {
                Alert.Error(ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            PositionWindow();
        }

    }

}