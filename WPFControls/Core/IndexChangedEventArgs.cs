















using System;
using System.Windows;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Core
{
	public class IndexChangedEventArgs : PropertyChangedEventArgs<int>
	{
		#region Constructors

		public IndexChangedEventArgs(RoutedEvent routedEvent, int oldIndex, int newIndex)
			: base(routedEvent, oldIndex, newIndex)
		{
		}

		#endregion

		protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
		{
			((IndexChangedEventHandler)genericHandler)(genericTarget, this);
		}
	}
}
