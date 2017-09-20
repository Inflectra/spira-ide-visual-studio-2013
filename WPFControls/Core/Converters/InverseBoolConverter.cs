















using System;
using System.Windows.Data;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Core.Converters
{
	public class InverseBoolConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
