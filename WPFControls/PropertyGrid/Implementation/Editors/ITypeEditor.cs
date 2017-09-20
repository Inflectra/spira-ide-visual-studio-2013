















using System.Windows;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.PropertyGrid.Editors
{
	public interface ITypeEditor
	{
		FrameworkElement ResolveEditor(PropertyItem propertyItem);
	}
}
