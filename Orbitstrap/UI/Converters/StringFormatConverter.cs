using System;
using System.Globalization;
using System.Windows.Data;

namespace Orbitstrap.UI.Converters;

public class StringFormatConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		string text = value as string;
		string text2 = parameter as string;
		if (text == null)
		{
			return "";
		}
		if (text2 == null)
		{
			return text;
		}
		string[] array = text2.Split(new char[1] { '|' });
		object[] args = array;
		return string.Format(text, args);
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException("ConvertBack");
	}
}
