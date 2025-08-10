using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CampaignMaster.Misc;
using SamCorp.WPF.Alerts;

namespace CampaignMaster.Controls.Views {

    public class ctlViewBase : UserControl {

        public bool IsActive { get; set; }

        public string Caption {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(nameof(Caption), typeof(string), typeof(ctlViewBase), new PropertyMetadata());

        public SlideDirection Direction {
            get => (SlideDirection)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(nameof(Direction), typeof(SlideDirection), typeof(ctlViewBase), new PropertyMetadata(SlideDirection.Down));

        public ctlViewBase() {
            IsManipulationEnabled = true;
            RenderTransform = new TranslateTransform();
            Loaded += View_Loaded;
        }

        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();

        //    object obj = Template.FindName("PART_InkCanvas", this);
        //    if (obj != null)
        //    {
        //        InkCanvas ink = (InkCanvas)obj;
        //        ink.Gesture += Ink_Gesture;
        //    }
        //}

        private void Ink_Gesture(object sender, InkCanvasGestureEventArgs e) {
            try {
                var gestureResults = e.GetGestureRecognitionResults();
                // erstes Resultat überprüfen, nur fortfahren wenn eine sehr sichere übereinstimmung gefunden wurde
                if (gestureResults[0].RecognitionConfidence <= RecognitionConfidence.Intermediate) {
                    if ((Direction == SlideDirection.Up && gestureResults[0].ApplicationGesture == ApplicationGesture.Up) ||
                        (Direction == SlideDirection.Down && gestureResults[0].ApplicationGesture == ApplicationGesture.Down) ||
                        (Direction == SlideDirection.Left && gestureResults[0].ApplicationGesture == ApplicationGesture.Left) ||
                        (Direction == SlideDirection.Right && gestureResults[0].ApplicationGesture == ApplicationGesture.Right))
                        SlideOutOfView();
                }
            } catch (Exception ex) {
                Alert.FadeError(ex);
            }
        }

        public void SlideIntoView() {
            try {
                Visibility = Visibility.Visible;
                var dp = Direction == SlideDirection.Up || Direction == SlideDirection.Down ? TranslateTransform.YProperty : TranslateTransform.XProperty;

                var duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
                var anim = new DoubleAnimation(0, duration);
                ((TranslateTransform)RenderTransform).BeginAnimation(dp, anim);

                OnSlideIntoView();

                IsActive = true;
            } catch (Exception ex) {
                Alert.Error(ex);
            }
        }

        protected virtual void OnSlideIntoView() {
        }

        public async void SlideOutOfView() {
            try {
                double toValue = 0;
                var dp = TranslateTransform.YProperty;

                toValue = ActualWidth;
                dp = TranslateTransform.XProperty;

                switch (Direction) {
                    case SlideDirection.Up:
                        toValue = -ActualHeight;
                        dp = TranslateTransform.YProperty;
                        break;

                    case SlideDirection.Down:
                        toValue = ActualHeight;
                        dp = TranslateTransform.YProperty;
                        break;

                    case SlideDirection.Left:
                        toValue = -ActualWidth;
                        dp = TranslateTransform.XProperty;
                        break;

                    case SlideDirection.Right:
                        toValue = ActualWidth;
                        dp = TranslateTransform.XProperty;
                        break;
                }

                var duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
                var anim = new DoubleAnimation(toValue, duration);
                ((TranslateTransform)RenderTransform).BeginAnimation(dp, anim);

                OnSlideOutOfView();

                await Task.Delay(300);

                Visibility = Visibility.Hidden;

                IsActive = false;
            } catch (Exception ex) {
                Alert.Error(ex);
            }
        }

        protected virtual void OnSlideOutOfView() {
        }

        private void SetInitialPosition() {
            switch (Direction) {
                case SlideDirection.Up:
                    (RenderTransform as TranslateTransform).Y = -ActualHeight;
                    break;

                case SlideDirection.Down:
                    (RenderTransform as TranslateTransform).Y = ActualHeight;
                    break;

                case SlideDirection.Left:
                    (RenderTransform as TranslateTransform).X = -ActualWidth;
                    break;

                case SlideDirection.Right:
                    (RenderTransform as TranslateTransform).X = ActualWidth;
                    break;
            }
        }

        private void View_Loaded(object sender, RoutedEventArgs e) {
            try {
                SetInitialPosition();
            } catch (Exception ex) {
                Alert.Error(ex);
            }
        }

        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e) {
            base.OnManipulationCompleted(e);

            try {
                if (Direction == SlideDirection.Up && Math.Abs(e.TotalManipulation.Translation.X) < 100 && e.TotalManipulation.Translation.Y < -250 ||
                    Direction == SlideDirection.Down && Math.Abs(e.TotalManipulation.Translation.X) < 100 && e.TotalManipulation.Translation.Y > 250 ||
                    Direction == SlideDirection.Left && e.TotalManipulation.Translation.X < -250 && Math.Abs(e.TotalManipulation.Translation.Y) < 100 ||
                    Direction == SlideDirection.Right && e.TotalManipulation.Translation.X > 250 && Math.Abs(e.TotalManipulation.Translation.Y) < 100)
                    SlideOutOfView();
            } catch (Exception ex) {
                Alert.Error(ex);
            }
        }

    }

}