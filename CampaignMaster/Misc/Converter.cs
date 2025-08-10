using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace CampaignMaster.Misc {

    public class AttributeToModifierValueConverter : MarkupExtension, IValueConverter {

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is not int score) {
                return null;
            }

            var result = (int)Math.Floor(((decimal)score - 10) / 2);
            return result == 0 ? "0" : result > 0 ? "+" + result : result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

    }

    public class ColorToSolidColorBrushValueConverter : MarkupExtension, IValueConverter {

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (null == value) {
                return null;
            }

            if (value is Color) {
                var color = (Color)value;
                return new SolidColorBrush(color);
            }

            var type = value.GetType();
            throw new InvalidOperationException("Unsupported type [" + type.Name + "]");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

    }

    public class DrawModeIsNotScratchValueConverter : MarkupExtension, IValueConverter {

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is not DrawMode dm)
                return null;

            return dm != DrawMode.SCRATCH;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

    }

    public class OpacityToPercentValueConverter : MarkupExtension, IValueConverter {

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (null == value)
                return null;

            if (value is double)
                return ((double)value).ToString("P0", CultureInfo.InvariantCulture);

            var type = value.GetType();
            throw new InvalidOperationException("Unsupported type [" + type.Name + "]");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

    }

    public class GridRectToDoubleValueConverter : MarkupExtension, IValueConverter {

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (null == value)
                return null;

            if (value is Rect)
                return (((Rect)value).Width / 10);

            var type = value.GetType();
            throw new InvalidOperationException("Unsupported type [" + type.Name + "]");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (null == value)
                return null;

            if (value is double)
                return new Rect(0, 0, (double)value * 10, (double)value * 10);

            var type = value.GetType();
            throw new InvalidOperationException("Unsupported type [" + type.Name + "]");
        }

    }

}