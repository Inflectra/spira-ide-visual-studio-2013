















using System.Windows.Input;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Core.Utilities
{
	internal class KeyboardUtilities
	{
		internal static bool IsKeyModifyingPopupState(KeyEventArgs e)
		{
			return ((((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) && ((e.SystemKey == Key.Down) || (e.SystemKey == Key.Up)))
				  || (e.Key == Key.F4));
		}
	}
}
