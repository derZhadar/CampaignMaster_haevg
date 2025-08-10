using System;
using System.Windows;
using SamCorp.WPF.Alerts;
using SamCorp.WPF.ViewModels;

namespace CampaignMaster.Windows {

    /// <summary>
    /// Interaction logic for wndCampaignEditor.xaml
    /// </summary>
    public partial class wndCampaignEditor : Window {

        public wndCampaignEditor() {
            InitializeComponent();
        }

        private void LiveMap_Click(object sender, RoutedEventArgs e) {
            try {
                this.Close();
            } catch (Exception ex) {
                Alert.Error(ex);
            }
        }

        private void PrepareMap_Click(object sender, RoutedEventArgs e) {
            try {
                this.Close();
            } catch (Exception ex) {
                Alert.Error(ex);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            try {
                new wndStart().Show();
                Close();
            } catch (Exception ex) {
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Storyboard_Completed(object sender, EventArgs e) {
            ((ViewModelBase)DataContext).FadeMessage = "";
        }

    }

}