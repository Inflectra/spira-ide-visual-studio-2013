















using System.Windows;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Panels
{
	public class ChildExitedEventArgs : RoutedEventArgs
	{
		#region Constructors

		public ChildExitedEventArgs(UIElement child)
		{
			_child = child;
		}

		#endregion

		#region Child Property

		public UIElement Child
		{
			get
			{
				return _child;
			}
		}

		private readonly UIElement _child;

		#endregion
	}
}
