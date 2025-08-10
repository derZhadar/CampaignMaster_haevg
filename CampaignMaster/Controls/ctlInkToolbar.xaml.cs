using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CampaignMaster.Misc;
using CampaignMaster.ViewModels;
using SamCorp.WPF.Alerts;

namespace CampaignMaster.Controls {

    /// <summary>
    /// Interaction logic for ctlInkToolbar.xaml
    /// </summary>
    public partial class ctlInkToolbar : UserControl {

        private InkCanvas inkDrawingBoard;
        private DrawMode _DrawMode;
        private double pensize;

        public static readonly DependencyProperty DrawingBoardProperty = DependencyProperty.Register(nameof(DrawingBoard), typeof(InkCanvas), typeof(ctlInkToolbar), new PropertyMetadata(DrawingBoardPropertyChanged));

        public InkCanvas DrawingBoard {
            get => (InkCanvas)GetValue(DrawingBoardProperty);
            set => SetValue(DrawingBoardProperty, value);
        }

        public static readonly DependencyProperty DrawModeProperty = DependencyProperty.Register(nameof(DrawMode), typeof(DrawMode), typeof(ctlInkToolbar), new PropertyMetadata(DrawModePropertyChanged));

        public DrawMode DrawMode {
            get => (DrawMode)GetValue(DrawModeProperty);
            set => SetValue(DrawModeProperty, value);
        }

        public static readonly DependencyProperty PenSizeProperty = DependencyProperty.Register(nameof(PenSize), typeof(double), typeof(ctlInkToolbar), new PropertyMetadata(PenSizePropertyChanged));

        public double PenSize {
            get => (double)GetValue(PenSizeProperty);
            set => SetValue(PenSizeProperty, value);
        }

        public int WallWidth => _WallWidths[_WallWidthIndex] switch {
            "L" => 15,
            "M" => 13,
            "S" => 10,
            _ => 0
        };

        public double ObjectSizeMultiplier => _ObjectSizes[_ObjectSizeIndex];

        private ObjectImageSource _SelectedObject;

        public ObjectImageSource SelectedObject {
            get => _SelectedObject;
            set {
                _SelectedObject = value;
                (btnSelectedObject.Content as Image).Source = value?.ImageSource;
                Alert.FadeInfo("Brush", value?.Name);
            }
        }

        private ObjectImageSource _SelectedFloor;

        public ObjectImageSource SelectedFloor {
            get => _SelectedFloor;
            set {
                _SelectedFloor = value;
                imgSelectedFloor.Source = value?.ImageSource;
                Alert.FadeInfo("Brush", value?.Name);
            }
        }

        public ObjectImageSource SelectedObjectImage => DrawMode is DrawMode.ROOM or DrawMode.ROOM_WILDSHAPE ? SelectedFloor : SelectedObject;

        public int ObjectWidth { get; private set; }
        public int ObjectHeight { get; private set; }

        //private ObjectImageSource _SelectedProp;

        //public ObjectImageSource SelectedProp {
        //    get => _SelectedProp;
        //    set {
        //        _SelectedProp = value;
        //        (btnSelectedProp.Content as Image).Source = value?.ImageSource;
        //    }
        //}

        public ctlInkToolbar() {
            InitializeComponent();
        }

        private void PenSizePropertyChanged(double size) {
            pensize = size;
            txtSize.Text = size.ToString();
            //btnSelectedColor.Content = size;
        }

        private static void PenSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((ctlInkToolbar)d).PenSizePropertyChanged((double)e.NewValue);
        }

        private void DrawingBoardPropertyChanged(InkCanvas ink) {
            inkDrawingBoard = ink;
        }

        private static void DrawingBoardPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((ctlInkToolbar)d).DrawingBoardPropertyChanged((InkCanvas)e.NewValue);
        }

        private Dictionary<DrawMode, DrawMode> _DoubleMappedDrawModes = new() {
            { DrawMode.SCRATCH, DrawMode.SCRATCH_AREA },
            { DrawMode.SHAPE, DrawMode.POLYGON },
            { DrawMode.ROOM, DrawMode.ROOM_WILDSHAPE },
            { DrawMode.OBJECT_ADD, DrawMode.OBJECT_AREA }
        };

        private void DrawModePropertyChanged(DrawMode value) {
            _DrawMode = value;

            foreach (UIElement element in pnlToolBar.Children) {
                if (element is not Button btn || btn.Tag is not DrawMode dm || element == btnSelectedColor) {
                    continue;
                }

                btn.Background = dm == _DrawMode ? Brushes.LightSkyBlue : Brushes.Gainsboro;
                btn.BorderBrush = dm == _DrawMode ? Brushes.DeepSkyBlue : Brushes.Gray;

                if (_DoubleMappedDrawModes.ContainsKey(dm)) {
                    if (_DoubleMappedDrawModes[dm] == _DrawMode) {
                        btn.Background = Brushes.Gold;
                        btn.BorderBrush = Brushes.Goldenrod;
                    }
                }
            }

            SetPaletteVisibility();

            Alert.FadeInfo("Drawmode changed", _DrawMode.ToString());
        }

        private void SetPaletteVisibility() {
            pnlFloorPalette.Visibility = Visibility.Hidden;
            pnlObjectPalette.Visibility = Visibility.Hidden;
            pnlAdditionalObjectPalette.Visibility = Visibility.Hidden;
            pnlCustomObjects.Visibility = Visibility.Hidden;
            btnSelectedObject.Visibility = Visibility.Hidden;
            btnSelectedFloor.Visibility = Visibility.Hidden;
            pnlColorPalette.Visibility = Visibility.Hidden;
            pnlAdditionalColorPalette.Visibility = Visibility.Hidden;
            pnlCustomColors.Visibility = Visibility.Hidden;
            btnSelectedColor.Visibility = Visibility.Hidden;

            switch (_DrawMode) {
                case DrawMode.OBJECT_ADD:
                case DrawMode.OBJECT_AREA:
                case DrawMode.OBJECT_SNAP:
                    pnlObjectPalette.Visibility = Visibility.Visible;
                    pnlAdditionalObjectPalette.Visibility = Visibility.Visible;
                    btnSelectedObject.Visibility = Visibility.Visible;
                    pnlCustomObjects.Visibility = Visibility.Visible;
                    txtSize.Text = "x" + _ObjectSizes[_ObjectSizeIndex];
                    break;


                case DrawMode.ROOM:
                case DrawMode.ROOM_WILDSHAPE:
                    pnlFloorPalette.Visibility = Visibility.Visible;
                    //pnlAdditionalFloorPalette.Visibility = Visibility.Visible;
                    btnSelectedFloor.Visibility = Visibility.Visible;
                    pnlColorPalette.Visibility = Visibility.Visible;
                    txtSize.Text = _WallWidths[_WallWidthIndex];
                    break;

                default:
                    pnlColorPalette.Visibility = Visibility.Visible;
                    pnlCustomColors.Visibility = Visibility.Visible;
                    pnlAdditionalColorPalette.Visibility = Visibility.Visible;
                    btnSelectedColor.Visibility = Visibility.Visible;
                    txtSize.Text = pensize.ToString();
                    break;
            }
        }

        private static void DrawModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((ctlInkToolbar)d).DrawModePropertyChanged((DrawMode)e.NewValue);
        }

        private void SlideColors() {
            var dest = (RenderTransform as TranslateTransform).X == 85 ? ActualWidth - 165 : 85;
            var duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            var anim = new DoubleAnimation(dest, duration);
            ((TranslateTransform)RenderTransform).BeginAnimation(TranslateTransform.XProperty, anim);
        }

        private void SlideColorsOut() {
            if ((RenderTransform as TranslateTransform).X != 85) {
                return;
            }

            var dest = ActualWidth - 165;
            var duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            var anim = new DoubleAnimation(dest, duration);
            ((TranslateTransform)RenderTransform).BeginAnimation(TranslateTransform.XProperty, anim);
        }

        private void ColorButton_Clicked(object sender, RoutedEventArgs e) {
            if (sender is not Button btn) {
                return;
            }

            inkDrawingBoard.DefaultDrawingAttributes.Color = ((SolidColorBrush)btn.Background).Color;
            btnSelectedColor.Background = btn.Background;
            btnSelectedColor.BorderBrush = btn.BorderBrush;

            btnSelectedFloor.Background = btn.Background;
            btnSelectedFloor.BorderBrush = btn.BorderBrush;

            SlideColorsOut();
        }

        private int _WallWidthIndex = 2;
        private List<string> _WallWidths = new() { "-", "S", "M", "L" };

        private int _ObjectSizeIndex = 2;
        private List<double> _ObjectSizes = new() { 0.25, 0.5, 1, 2, 3, 4 };

        private void SizeButton_Clicked(object sender, RoutedEventArgs e) {
            if (sender is not Button btn) {
                return;
            }

            var direction = Convert.ToDouble(btn.Tag.ToString());

            switch (DrawMode) {
                case DrawMode.ROOM:
                case DrawMode.ROOM_WILDSHAPE:
                    if ((direction == -1 && _WallWidthIndex > 0) || direction == 1 && _WallWidthIndex < _WallWidths.Count - 1) {
                        _WallWidthIndex += (int)direction;
                        txtSize.Text = _WallWidths[_WallWidthIndex];
                    }

                    break;
                case DrawMode.OBJECT_ADD:
                case DrawMode.OBJECT_SNAP:
                    if ((direction == -1 && _ObjectSizeIndex > 0) || direction == 1 && _ObjectSizeIndex < _ObjectSizes.Count - 1) {
                        _ObjectSizeIndex += (int)direction;
                        txtSize.Text = "x" + _ObjectSizes[_ObjectSizeIndex];
                    }

                    break;

                default:
                    PenSize = direction;
                    break;
            }
        }

        private void ObjectButton_Clicked(object sender, RoutedEventArgs e) {
            PaletteObjectButtonClick(sender, e);
        }

        private void btnSelectedColor_Click(object sender, RoutedEventArgs e) {
            SlideColors();
        }

        private void btnSelectedObject_Click(object sender, RoutedEventArgs e) {
            SlideColors();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            (RenderTransform as TranslateTransform).X = ActualWidth - 165;
        }

        private void DrawModeButton_Click(object sender, RoutedEventArgs e) {
            if (sender is not Button btn || btn.Tag is not DrawMode dm) {
                return;
            }

            if (_DoubleMappedDrawModes.ContainsKey(dm)) {
                if (DrawMode == dm) {
                    dm = _DoubleMappedDrawModes[dm];
                }
            }

            DrawMode = dm;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            (RenderTransform as TranslateTransform).X = ActualWidth - 165;
        }

        private void FloorButtonClick(object sender, RoutedEventArgs e) {
            (btnSelectedFloor.Content as Image).Source = ((sender as Button).Content as Image).Source;
            SlideColorsOut();
        }

        private void PaletteFloorButtonClick(object sender, RoutedEventArgs e) {
            if (sender is not Button btn) {
                return;
            }

            if (btn.DataContext is not ObjectImageSource ois) {
                SelectedFloor = null;

                btnSelectedFloor.Background = Brushes.Transparent;
                btnSelectedFloor.BorderBrush = Brushes.Gray;
                return;
            }

            SelectedFloor = ois;
            SlideColorsOut();
        }

        private void SelectFloorButtonClick(object sender, RoutedEventArgs e) {
            SlideColors();
        }

        private void PaletteObjectButtonClick(object sender, RoutedEventArgs e) {
            if (sender is not Button btn) {
                return;
            }

            if (btn.DataContext is not ObjectImageSource ois) {
                return;
            }

            SelectedObject = ois;
            ObjectWidth = ois.ObjectWidth;
            ObjectHeight = ois.ObjectHeight;

            SlideColorsOut();
        }

    }

}