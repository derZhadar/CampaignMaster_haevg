namespace CampaignMaster.Controls.Views {

    /// <summary>
    /// Interaction logic for viewCampaign.xaml
    /// </summary>
    public partial class viewCampaign : ctlViewBase {

        public viewCampaign() {
            InitializeComponent();
        }

        protected override void OnSlideOutOfView() {
            base.OnSlideOutOfView();

            App.SaveCampaign();
        }

    }

}