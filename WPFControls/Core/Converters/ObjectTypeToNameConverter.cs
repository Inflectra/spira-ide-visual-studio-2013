















using System;
using System.Windows.Data;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Core.Converters
{
	public class ObjectTypeToNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value.GetType().Name;
		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
