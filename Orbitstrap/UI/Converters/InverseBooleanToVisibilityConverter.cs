using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Orbitstrap.UI.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => value is Visibility v && v == Visibility.Collapsed;
}

// Alias used in some XAML files
public class InvertBooleanToVisibilityConverter : InverseBooleanToVisibilityConverter { }
