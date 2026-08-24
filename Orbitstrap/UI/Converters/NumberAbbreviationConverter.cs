using System;
using System.Globalization;
using System.Windows.Data;

namespace Orbitstrap.UI.Converters
{
    public class NumberAbbreviationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "0";
            double num;
            try { num = System.Convert.ToDouble(value); }
            catch { return value.ToString() ?? "0"; }

            if (num >= 1_000_000_000) return $"{num / 1_000_000_000:0.#}B";
            if (num >= 1_000_000)     return $"{num / 1_000_000:0.#}M";
            if (num >= 1_000)         return $"{num / 1_000:0.#}K";
            return num.ToString("0", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
