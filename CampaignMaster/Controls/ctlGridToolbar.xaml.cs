using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CampaignMaster.ViewModels;

namespace CampaignMaster.Controls {

    /// <summary>
    /// Interaction logic for ctlGridToolbar.xaml
    /// </summary>
    public partial class ctlGridToolbar : UserControl {

        public ctlGridToolbar() {
            InitializeComponent();
        }

        public void SlideToolbar() {
            var dest = (((TransformGroup)RenderTransform).Children[0] as TranslateTransform).Y == 0 ? -ActualHeight : 0;
            var duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
            var anim = new DoubleAnimation(dest, duration);
            (((TransformGroup)RenderTransform).Children[0] as TranslateTransform).BeginAnimation(TranslateTransform.YProperty, anim);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            vmDrawingBoard.GridBrush = (sender as Button).Background;
            SlideToolbar();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            (((TransformGroup)RenderTransform).Children[0] as TranslateTransform).Y = -ActualHeight;
        }

        private void ShowToolbarButtonClick(object sender, RoutedEventArgs e) {
            SlideToolbar();
        }

    }

}