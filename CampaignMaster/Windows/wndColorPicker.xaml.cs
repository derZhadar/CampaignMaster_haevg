using System.Windows;

namespace CampaignMaster.Windows {

    /// <summary>
    /// Interaction logic for wndColorPicker.xaml
    /// </summary>
    public partial class wndColorPicker : Window {

        public string SelectedColor {
            get => pickColorPicker.HexadecimalString;
            set => pickColorPicker.HexadecimalString = value;
        }

        public wndColorPicker() {
            InitializeComponent();
        }

        private void OkClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        public static string PickColor(string defaultColor = "#FFF") {
            var wnd = new wndColorPicker {
                SelectedColor = defaultColor
            };
            wnd.ShowDialog();

            return wnd.SelectedColor;
        }

    }

}