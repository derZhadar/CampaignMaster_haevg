using System.Windows;

namespace CampaignMaster.Windows {

    /// <summary>
    /// Interaction logic for wndSettings.xaml
    /// </summary>
    public partial class wndSettings : Window {

        public wndSettings() {
            InitializeComponent();
        }

        private void ButtonCloseClick(object sender, RoutedEventArgs e) {
            this.Close();
        }

    }

}