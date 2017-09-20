















using System.Windows;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit
{
	[TemplatePart(Name = PART_ActionButton, Type = typeof(Button))]
	public class SplitButton : DropDownButton
	{
		private const string PART_ActionButton = "PART_ActionButton";

		#region Constructors

		static SplitButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
		}

		#endregion //Constructors

		#region Base Class Overrides

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Button = GetTemplateChild(PART_ActionButton) as Button;
		}

		#endregion //Base Class Overrides
	}
}
