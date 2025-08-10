using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CampaignMaster.ViewModels;
using SamCorp.WPF.Controls;

namespace CampaignMaster.Controls {

    /// <summary>
    /// Interaction logic for ctlCalendar.xaml
    /// </summary>
    public partial class ctlCalendar : UserControl {

        public static readonly DependencyProperty ShowButtonsProperty = DependencyProperty.Register(nameof(ShowButtons), typeof(bool), typeof(ctlCalendar), new PropertyMetadata(true));

        public static readonly Dictionary<int, Tuple<int, int>> MonthOffsets = new() {
            { 1, new Tuple<int, int>(1, 1) },
            { 2, new Tuple<int, int>(10, 1) },
            { 3, new Tuple<int, int>(19, 1) },
            { 4, new Tuple<int, int>(1, 7) },
            { 5, new Tuple<int, int>(10, 7) },
            { 6, new Tuple<int, int>(19, 7) },
            { 7, new Tuple<int, int>(1, 13) },
            { 8, new Tuple<int, int>(10, 13) },
            { 9, new Tuple<int, int>(19, 13) },
            { 10, new Tuple<int, int>(1, 19) },
            { 11, new Tuple<int, int>(10, 19) },
            { 12, new Tuple<int, int>(19, 19) }
        };

        private vmCalendar _CalendarContext => (DataContext as vmCalendar);
        private bool _HasCalendarContext => _CalendarContext != null;

        public bool ShowButtons {
            get => (bool)GetValue(ShowButtonsProperty);
            set => SetValue(ShowButtonsProperty, value);
        }

        public ctlCalendar() {
            InitializeComponent();
        }

        private void SetCalendarDateFromCoordinates(int col, int row) {
            if (!_HasCalendarContext) {
                return;
            }

            var monthColumn = (col + 1) >= 10 ? (col + 1) >= 19 ? 3 : 2 : 1;
            var monthRow = (row + 1) > 7 ? (row + 1) > 13 ? (row + 1) > 19 ? 4 : 3 : 2 : 1;
            var month = monthColumn + ((monthRow - 1) * 3);

            var monthWeek = (row + 1) - MonthOffsets[month].Item2;
            var monthDay = (col + 1) - MonthOffsets[month].Item1 + ((monthWeek - 1) * vmCalendar.DaysWeek);
            var day = (month - 1) * vmCalendar.DaysMonth + monthDay;

            _CalendarContext.Day = day;
        }

        private void GrdCalendar_OnLoaded(object sender, RoutedEventArgs e) {
            if (!_HasCalendarContext) {
                return;
            }

            foreach (var dic in _CalendarContext.HolyDays.Values) {
                foreach (var dicValue in dic.Values) {
                    var border = new Border {
                        Background = Brushes.Gold,
                        BorderThickness = new Thickness(0),
                        Opacity = 0.4
                    };

                    grdCalendar.Children.Add(border);

                    Grid.SetColumn(border, (int)dicValue.Item2.X);
                    Grid.SetRow(border, (int)dicValue.Item2.Y);
                }
            }

            for (var col = 0; col < grdCalendar.ColumnDefinitions.Count; col++) {
                for (var row = 0; row < grdCalendar.RowDefinitions.Count; row++) {
                    var btn = new SamButton {
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        HoverBackground = Brushes.LightSkyBlue,
                        Opacity = 0.4
                    };

                    btn.Click += DayButtonOnClick;

                    grdCalendar.Children.Add(btn);

                    Grid.SetColumn(btn, col);
                    Grid.SetRow(btn, row);
                }
            }
        }

        private void DayButtonOnClick(object sender, RoutedEventArgs e) {
            if (sender is not SamButton btn) {
                return;
            }

            SetCalendarDateFromCoordinates(Grid.GetColumn(btn), Grid.GetRow(btn));
        }

    }

}