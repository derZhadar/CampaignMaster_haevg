using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SamCorp.WPF.Alerts;

using CampaignMaster.Data;
using CampaignMaster.Windows;

namespace CampaignMaster.Controls.Views
{
    /// <summary>
    /// Interaction logic for viewMaps.xaml
    /// </summary>
    public partial class viewMaps : ctlViewBase
    {
        public viewMaps()
        {
            InitializeComponent();
        }

        private void LoadMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((wndCampaignMaster)Application.Current.MainWindow).LoadMap((claMap)((Button)sender).Tag);
                SlideOutOfView();
            }
            catch(Exception ex) { Alert.Error(ex); }
        }
    }
}
