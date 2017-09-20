
















namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Core.Input
{
	public interface IValidateInput
	{
		event InputValidationErrorEventHandler InputValidationError;
		bool CommitInput();
	}
}
