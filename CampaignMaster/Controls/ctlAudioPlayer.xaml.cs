using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SamCorp.WPF.Alerts;
using SamCorp.WPF.Extended;

namespace CampaignMaster.Controls {

    /// <summary>
    /// Interaction logic for ctlAudioPlayer.xaml
    /// </summary>
    public partial class ctlAudioPlayer : UserControl {

        private readonly MediaPlayerExtended _MediaPlayer = new();
        private string _CurrentAudio = "";

        public ctlAudioPlayer() {
            InitializeComponent();
        }

        private void PlayButtonClick(object sender, RoutedEventArgs e) {
            if (sender is not Button btn) {
                return;
            }

            btnStop.Visibility = Visibility.Visible;


            if (_CurrentAudio.Equals(btn.Tag)) {
                if (btn.Content is not TextBlock tb) {
                    return;
                }

                if (_MediaPlayer.IsPlaying) {
                    _MediaPlayer.Pause();
                    tb.Text = "\ue102";
                } else {
                    _MediaPlayer.Play();
                    tb.Text = "\ue103";
                }
            } else {
                ResetButtonState();

                _MediaPlayer.Stop();
                _MediaPlayer.Open(new Uri(@$"Resources\Sounds\{btn.Tag}.mp3", UriKind.Relative));
                _MediaPlayer.Play();

                btn.BorderBrush = Brushes.DodgerBlue;

                if (btn.Content is not TextBlock tb) {
                    return;
                }

                tb.Text = "\ue103";

                _CurrentAudio = btn.Tag.ToString();

                Alert.FadeInfo("Playing ambiente", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_CurrentAudio));
            }
        }

        private void StopButtonClick(object sender, RoutedEventArgs e) {
            btnStop.Visibility = Visibility.Hidden;
            _MediaPlayer.Stop();
            ResetButtonState();
        }

        private void ResetButtonState() {
            foreach (var grdContentChild in grdContent.Children.Cast<UIElement>().Where(e => e is Button && e != btnStop)) {
                if (grdContentChild is not Button btn) {
                    continue;
                }

                btn.BorderBrush = Brushes.PapayaWhip;

                if (btn.Content is not TextBlock tb) {
                    return;
                }

                tb.Text = "\ue102";
            }
        }

    }

}