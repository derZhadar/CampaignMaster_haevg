using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CampaignMaster.Controls {

    /// <summary>
    /// Interaction logic for ctlRandomMapGeneratorToolBar.xaml
    /// </summary>
    public partial class ctlRandomMapGeneratorToolBar : UserControl {

        public ctlRandomMapGeneratorToolBar() {
            InitializeComponent();
        }

        private void RandomMapClick(object sender, RoutedEventArgs e) {
            if (sender is not Button btn) {
                return;
            }

            Process.Start(new ProcessStartInfo(btn.Tag.ToString()) { UseShellExecute = true });
        }

    }

}