using System;
using System.Windows;
using SamCorp.WPF.Alerts;
using SamCorp.WPF.Controls;

namespace CampaignMaster.Windows {

    /// <summary>
    /// Interaction logic for wndStart.xaml
    /// </summary>
    public partial class wndStart : Window, ICloseable {

        public wndStart() {
            InitializeComponent();
        }

        private void NewCampaign_Click(object sender, RoutedEventArgs e) {
            try {
                new wndCampaignEditor().Show();
                this.Close();
            } catch (Exception ex) {
                Alert.Error(ex);
            }
        }

        //private void LoadLocalCampaign_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        wndCampaignEditor ce = new wndCampaignEditor();
        //        ce.DataContext = new vmCampaignEditor() { Campaign = (mdlCampaign)((Button)sender).Tag };
        //        ce.Show();
        //        this.Close();
        //    }
        //    catch (Exception ex) { Alert.Error(ex); }
        //}

        private void CampaignToolbarButtonClick(object sender, RoutedEventArgs e) {
            Close();
        }

    }

}