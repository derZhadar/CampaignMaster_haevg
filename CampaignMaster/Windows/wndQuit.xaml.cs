using System;
using System.Windows;

namespace CampaignMaster.Windows {

    /// <summary>
    /// Interaction logic for wndQuit.xaml
    /// </summary>
    public partial class wndQuit : Window {

        private int _Days = 0;

        public wndQuit() {
            InitializeComponent();
        }

        private void ButtonIncreaseDayClick(object sender, RoutedEventArgs e) {
            _Days++;
            txtDays.Text = _Days.ToString();
        }

        private void ButtonDecreaseDayClick(object sender, RoutedEventArgs e) {
            _Days = Math.Max(0, _Days - 1);
            txtDays.Text = _Days.ToString();
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e) {
            App.CurrentCampaign.Day += _Days;
            App.SaveCampaign();
            App.Settings.Save();
            Application.Current.Shutdown();
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e) {
            this.Close();
        }

    }

}