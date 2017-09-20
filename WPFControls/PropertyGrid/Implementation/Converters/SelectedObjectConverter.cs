















using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.PropertyGrid.Converters
{
	public class SelectedObjectConverter : IValueConverter
	{
		private const string ValidParameterMessage = @"parameter must be one of the following strings: 'Type', 'TypeName'";
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (parameter == null)
				throw new ArgumentNullException("parameter");

			if (!(parameter is string))
				throw new ArgumentException(SelectedObjectConverter.ValidParameterMessage);

			if (this.CompareParam(parameter, "Type"))
			{
				return this.ConvertToType(value, culture);
			}
			else if (this.CompareParam(parameter, "TypeName"))
			{
				return this.ConvertToTypeName(value, culture);
			}
			else
			{
				throw new ArgumentException(SelectedObjectConverter.ValidParameterMessage);
			}
		}

		private bool CompareParam(object parameter, string parameterValue)
		{
			return string.Compare((string)parameter, parameterValue, true) == 0;
		}

		private object ConvertToType(object value, CultureInfo culture)
		{
			return (value != null)
			  ? value.GetType()
			  : null;
		}

		private object ConvertToTypeName(object value, CultureInfo culture)
		{
			if (value == null)
				return string.Empty;

			Type newType = value.GetType();

			DisplayNameAttribute displayNameAttribute = newType.GetCustomAttributes(false).OfType<DisplayNameAttribute>().FirstOrDefault();

			return (displayNameAttribute == null)
			  ? newType.Name
			  : displayNameAttribute.DisplayName;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
