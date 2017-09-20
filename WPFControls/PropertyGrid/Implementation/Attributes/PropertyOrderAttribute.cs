















using System;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.PropertyGrid.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class PropertyOrderAttribute : Attribute
	{
		public int Order
		{
			get;
			set;
		}

		public PropertyOrderAttribute(int order)
		{
			Order = order;
		}
	}
}
