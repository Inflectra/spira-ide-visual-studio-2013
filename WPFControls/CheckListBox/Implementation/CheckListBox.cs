















using System.Windows;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Primitives;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit
{
	public class CheckListBox : Selector
	{
		#region Constructors

		static CheckListBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckListBox), new FrameworkPropertyMetadata(typeof(CheckListBox)));
		}

		public CheckListBox()
		{

		}

		#endregion //Constructors
	}
}
