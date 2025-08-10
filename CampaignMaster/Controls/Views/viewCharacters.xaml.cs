namespace CampaignMaster.Controls.Views {

    /// <summary>
    /// Interaction logic for viewCharacters.xaml
    /// </summary>
    public partial class viewCharacters : ctlViewBase {

        public viewCharacters() {
            InitializeComponent();
        }

        protected override void OnSlideOutOfView()
        {
            App.SaveCampaign();
        }

    }

}