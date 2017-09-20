















using System.Windows.Input;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.PropertyGrid.Commands
{
	public static class PropertyItemCommands
	{
		private static RoutedCommand _resetValueCommand = new RoutedCommand();
		public static RoutedCommand ResetValue
		{
			get
			{
				return _resetValueCommand;
			}
		}
	}
}
