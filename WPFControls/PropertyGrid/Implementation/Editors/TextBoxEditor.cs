















using System.Windows.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.PropertyGrid.Editors
{
	public class TextBoxEditor : TypeEditor<WatermarkTextBox>
	{
		protected override void SetControlProperties()
		{
			Editor.BorderThickness = new System.Windows.Thickness(0);
		}

		protected override void SetValueDependencyProperty()
		{
			ValueProperty = TextBox.TextProperty;
		}
	}
}
