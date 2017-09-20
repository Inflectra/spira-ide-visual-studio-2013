















using System.Windows.Input;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.PropertyGrid.Commands
{
	public class PropertyGridCommands
	{
		private static RoutedCommand _clearFilterCommand = new RoutedCommand();
		public static RoutedCommand ClearFilter
		{
			get
			{
				return _clearFilterCommand;
			}
		}
	}
}
