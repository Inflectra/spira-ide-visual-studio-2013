















using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Windows.Input;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Core.Utilities;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Zoombox
{
	public class ZoomboxCursors
	{
		#region Constructors

		static ZoomboxCursors()
		{
			try
			{
				new EnvironmentPermission(PermissionState.Unrestricted).Demand();
				_zoom = new Cursor(ResourceHelper.LoadResourceStream(Assembly.GetExecutingAssembly(), "Zoombox/Resources/Zoom.cur"));
				_zoomRelative = new Cursor(ResourceHelper.LoadResourceStream(Assembly.GetExecutingAssembly(), "Zoombox/Resources/ZoomRelative.cur"));
			}
			catch (SecurityException)
			{
				// partial trust, so just use default cursors
			}
		}

		#endregion

		#region Zoom Static Property

		public static Cursor Zoom
		{
			get
			{
				return _zoom;
			}
		}

		private static readonly Cursor _zoom = Cursors.Arrow;

		#endregion

		#region ZoomRelative Static Property

		public static Cursor ZoomRelative
		{
			get
			{
				return _zoomRelative;
			}
		}

		private static readonly Cursor _zoomRelative = Cursors.Arrow;

		#endregion
	}
}
