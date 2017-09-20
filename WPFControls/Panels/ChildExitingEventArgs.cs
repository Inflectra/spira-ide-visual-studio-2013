















using System.Windows;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Panels
{
	public class ChildExitingEventArgs : RoutedEventArgs
	{
		#region Constructors

		public ChildExitingEventArgs(UIElement child, Rect? exitTo, Rect arrangeRect)
		{
			_child = child;
			_exitTo = exitTo;
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

		#region ExitTo Property

		public Rect? ExitTo
		{
			get
			{
				return _exitTo;
			}
			set
			{
				_exitTo = value;
			}
		}

		private Rect? _exitTo; //null

		#endregion
	}
}
