using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CampaignMaster.Controls;
using CampaignMaster.Models;
using SamCorp.WPF.Commands;
using SamCorp.WPF.Extensions;
using SamCorp.WPF.ViewModels;

namespace CampaignMaster.ViewModels {

    public class vmCalendar : ViewModelBase {

        private const int _HourMax = 23;
        private const int _HourMin = 0;

        public const int DaysWeek = 7;
        public const int DaysMonth = 28;
        public const int DaysYear = 336;

        private int _Hour = 8;
        private int _Day = 9;
        private int _Year = 998;

        private readonly List<string> _Months = new() {
            "Zarantyr",
            "Olarune",
            "Therendor",
            "Eyre",
            "Dravago",
            "Nymm",
            "Lharvion",
            "Barrakas",
            "Rhaan",
            "Sypheros",
            "Aryth",
            "Vult"
        };

        private readonly List<string> _Days = new() {
            "Sar",
            "Sul",
            "Mol",
            "Zol",
            "Wir",
            "Zor",
            "Far"
        };

        public Dictionary<int, Dictionary<int, Tuple<string, Point>>> HolyDays { get; }

        public int Hour {
            get => _Hour;
            set => SetField(ref _Hour, value);
        }

        public int Day {
            get => _Day;
            set {
                SetField(ref _Day, value);
                RaisePropertyChanged(null);

                if (!_Updating) {
                    SaveToCampaign();
                }
            }
        }

        public int WeekDay => GetWeekDay(MonthDay);

        public int MonthDay => GetMonthDay(Day, Month);

        public int MonthWeek => GetMonthWeek(MonthDay);

        public int Month => GetMonth(Day);

        public int Year {
            get => _Year;
            set => SetField(ref _Year, value);
        }

        public string DateString => $"{_Days[WeekDay]}, {MonthDay:00}. {_Months[Month - 1]} {Year}YK";

        public string HolyDay {
            get {
                if (!HolyDays.ContainsKey(Month)) {
                    return "-";
                }

                if (!HolyDays[Month].ContainsKey(MonthDay)) {
                    return "-";
                }

                return HolyDays[Month][MonthDay].Item1;
            }
        }

        public int IndicatorX => (MonthDay - 1) - ((MonthWeek - 1) * 7) + ctlCalendar.MonthOffsets[Month].Item1;
        public int IndicatorY => (MonthWeek - 1) + ctlCalendar.MonthOffsets[Month].Item2;

        public ICommand CommandAddDay => new Command(AddDay);

        public ICommand CommandSubstractDay => new Command(SubstractDay);

        public ICommand CommandAddHour => new Command(AddHour);

        public ICommand CommandSubstractHour => new Command(SubstractHour);

        public vmCalendar() {
            HolyDays = new Dictionary<int, Dictionary<int, Tuple<string, Point>>>();

            AddHolyDay(2, 9, "Crystalfall");
            AddHolyDay(2, 20, "The Day of Mourning");

            AddHolyDay(3, 15, "Sun's Blessing");

            AddHolyDay(5, 26, "Aureon's Crown");

            AddHolyDay(6, 12, "Brightblade");

            AddHolyDay(7, 23, "The Race of Eight Winds");

            AddHolyDay(8, 4, "The Hunt");
            AddHolyDay(8, 25, "Fathen's Fall");

            AddHolyDay(9, 9, "Boldrei's Feast");

            AddHolyDay(10, 18, "Wildnight");

            AddHolyDay(11, 11, "Thronehold");

            AddHolyDay(12, 26, "Long Shadows");
            AddHolyDay(12, 27, "Long Shadows");
            AddHolyDay(12, 28, "Long Shadows");

            App.CampaignChanged += App_CampaignChanged;
            UpdateFromCampaign();

            if (App.CurrentCampaign != null) {
                App.CurrentCampaign.PropertyChanged += CurrentCampaignOnPropertyChanged;
            }
        }

        private void CurrentCampaignOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName.IsNullOrEmpty() ||
                (!e.PropertyName.Equals(nameof(mdlCampaign.Year)) &&
                 !e.PropertyName.Equals(nameof(mdlCampaign.Day)) &&
                 !e.PropertyName.Equals(nameof(mdlCampaign.Hour)))) {
                return;
            }

            UpdateFromCampaign();
        }

        private void App_CampaignChanged(object sender, EventArgs e) {
            App.CurrentCampaign.PropertyChanged += CurrentCampaignOnPropertyChanged;
            UpdateFromCampaign();
        }

        private void AddHolyDay(int month, int monthDay, string description) {
            if (!HolyDays.ContainsKey(month)) {
                HolyDays.Add(month, new Dictionary<int, Tuple<string, Point>>());
            }

            HolyDays[month].Add(monthDay, new Tuple<string, Point>(description, GetCalendarCoordinates(month, monthDay)));
            RaisePropertyChanged(nameof(HolyDays));
        }

        public static int GetMonth(int day) {
            return (int)Math.Ceiling((decimal)day / DaysMonth);
        }

        public static int GetMonthDay(int day, int month) {
            return day - ((month - 1) * DaysMonth);
        }

        public static int GetWeekDay(int monthDay) {
            return monthDay - (int)(Math.Floor((decimal)monthDay / DaysWeek) * DaysWeek);
        }

        public static int GetMonthWeek(int monthDay) {
            return (int)(Math.Ceiling((decimal)monthDay / DaysWeek));
        }

        public static Point GetCalendarCoordinates(int month, int monthDay) {
            var monthWeek = GetMonthWeek(monthDay);
            var x = (monthDay - 1) - ((monthWeek - 1) * 7) + ctlCalendar.MonthOffsets[month].Item1;
            var y = (monthWeek - 1) + ctlCalendar.MonthOffsets[month].Item2;

            return new Point(x, y);
        }

        private bool _Updating;

        public void UpdateFromCampaign() {
            if (App.CurrentCampaign == null) {
                return;
            }

            _Updating = true;

            if (App.CurrentCampaign.Year != Year) {
                Year = App.CurrentCampaign.Year;
            }

            if (App.CurrentCampaign.Day != Day) {
                Day = App.CurrentCampaign.Day;
            }

            if (App.CurrentCampaign.Hour != Hour) {
                Hour = App.CurrentCampaign.Hour;
            }

            _Updating = false;
        }

        private void SaveToCampaign() {
            if (App.CurrentCampaign == null) {
                return;
            }

            if (App.CurrentCampaign.Year != Year) {
                App.CurrentCampaign.Year = Year;
            }

            if (App.CurrentCampaign.Day != Day) {
                App.CurrentCampaign.Day = Day;
            }

            if (App.CurrentCampaign.Hour != Hour) {
                App.CurrentCampaign.Hour = Hour;
            }
        }

        private void AddDay() {
            if (Day + 1 > DaysYear) {
                Year++;
                Day = 1;
            } else {
                Day++;
            }
        }

        private void SubstractDay() {
            if (Day - 1 <= 0) {
                Year--;
                Day = DaysYear;
            } else {
                Day--;
            }
        }

        private void AddHour() {
            if (Hour == _HourMax) {
                Hour = _HourMin;
                AddDay();
            } else
                Hour++;

            SaveToCampaign();
        }

        private void SubstractHour() {
            if (Hour == _HourMin) {
                Hour = _HourMax;
                SubstractDay();
            } else
                Hour--;

            SaveToCampaign();
        }

    }

}