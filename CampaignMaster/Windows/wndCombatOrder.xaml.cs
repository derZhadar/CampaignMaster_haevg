using System.Linq;
using System.Windows;

namespace CampaignMaster.Windows {

    /// <summary>
    /// Interaction logic for wndCombatOrder.xaml
    /// </summary>
    public partial class wndCombatOrder : Window {

        public wndCombatOrder() {
            InitializeComponent();
        }

        private void HideCombatOrder() {
            brdBackground.Visibility = Visibility.Hidden;
            this.SizeToContent = SizeToContent.Manual;
            Height = 115;
        }

        private void ShowCombatOrder() {
            brdBackground.Visibility = Visibility.Visible;
            this.SizeToContent = SizeToContent.Height;
        }

        private void ToggleCombatOrderVisibility() {
            if (Height <= 115) {
                ShowCombatOrder();
            } else {
                HideCombatOrder();
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            if (App.CurrentCampaign.CombatOrder.Any() || this.SizeToContent == SizeToContent.Manual) {
                ToggleCombatOrderVisibility();
            } else {
                Visibility = Visibility.Collapsed;
            }
        }

        private void ToggleNpcsClick(object sender, RoutedEventArgs e) {
            listNpcs.Visibility = listNpcs.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

    }

}