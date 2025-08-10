using System;
using System.Windows;
using System.Windows.Controls;
using CampaignMaster.Data;
using CampaignMaster.ViewModels;
using CampaignMaster.Windows;
using SamCorp.WPF.Alerts;

namespace CampaignMaster.Controls.Views {

    /// <summary>
    /// Interaction logic for ctlFolderImages.xaml
    /// </summary>
    public partial class ctlFolderImages : ctlViewBase {

        private vmFolderImages FolderImagesContext {
            get => DataContext as vmFolderImages;
        }

        public ctlFolderImages() {
            InitializeComponent();
        }

        private void LoadMap_Click(object sender, RoutedEventArgs e) {
            try {
                ((wndCampaignMaster)Application.Current.MainWindow).LoadMap((claMap)((Button)sender).Tag);
                SlideOutOfView();
            } catch (Exception ex) {
                Alert.Error(ex);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            SlideOutOfView();
        }

        protected override void OnSlideIntoView() {
            base.OnSlideIntoView();

            FolderImagesContext.LoadImages();
        }

    }

}