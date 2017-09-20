















using System;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Core;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Zoombox
{
	public class ZoomboxViewChangedEventArgs : PropertyChangedEventArgs<ZoomboxView>
	{
		#region Constructors

		public ZoomboxViewChangedEventArgs(
		  ZoomboxView oldView,
		  ZoomboxView newView,
		  int oldViewStackIndex,
		  int newViewStackIndex)
			: base(Zoombox.CurrentViewChangedEvent, oldView, newView)
		{
			_newViewStackIndex = newViewStackIndex;
			_oldViewStackIndex = oldViewStackIndex;
		}

		#endregion

		#region NewViewStackIndex Property

		public int NewViewStackIndex
		{
			get
			{
				return _newViewStackIndex;
			}
		}

		private readonly int _newViewStackIndex = -1;

		#endregion

		#region NewViewStackIndex Property

		public int OldViewStackIndex
		{
			get
			{
				return _oldViewStackIndex;
			}
		}

		private readonly int _oldViewStackIndex = -1;

		#endregion

		#region NewViewStackIndex Property

		public bool IsNewViewFromStack
		{
			get
			{
				return _newViewStackIndex >= 0;
			}
		}

		#endregion

		#region NewViewStackIndex Property

		public bool IsOldViewFromStack
		{
			get
			{
				return _oldViewStackIndex >= 0;
			}
		}

		#endregion

		protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
		{
			((ZoomboxViewChangedEventHandler)genericHandler)(genericTarget, this);
		}
	}
}
