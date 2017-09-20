















using System.Windows.Input;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit
{
	public static class CalculatorCommands
	{
		private static RoutedCommand _calculatorButtonClickCommand = new RoutedCommand();

		public static RoutedCommand CalculatorButtonClick
		{
			get
			{
				return _calculatorButtonClickCommand;
			}
		}
	}
}
