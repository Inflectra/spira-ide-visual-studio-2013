















using System.Windows;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Panels
{
	public class ChildEnteredEventArgs : RoutedEventArgs
	{
		#region Constructors

		public ChildEnteredEventArgs(UIElement child, Rect arrangeRect)
		{
			_child = child;
			_arrangeRect = arrangeRect;
		}

		#endregion

		#region ArrangeRect Property

		public Rect ArrangeRect
		{
			get
			{
				return _arrangeRect;
			}
		}

		private readonly Rect _arrangeRect;

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
