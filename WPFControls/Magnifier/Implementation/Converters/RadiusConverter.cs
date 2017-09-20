















using System;
using System.Windows.Data;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Mag.Converters
{
	internal class RadiusConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return (double)value * 2;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
