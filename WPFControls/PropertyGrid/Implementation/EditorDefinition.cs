















using System;
using System.Windows;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.PropertyGrid
{
	public class EditorDefinition
	{
		public DataTemplate EditorTemplate
		{
			get;
			set;
		}

		private PropertyDefinitionCollection _properties = new PropertyDefinitionCollection();
		public PropertyDefinitionCollection PropertiesDefinitions
		{
			get
			{
				return _properties;
			}
			set
			{
				_properties = value;
			}
		}

		public Type TargetType
		{
			get;
			set;
		}
	}
}
