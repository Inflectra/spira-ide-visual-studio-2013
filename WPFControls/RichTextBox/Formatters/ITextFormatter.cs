















using System.Windows.Documents;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit
{
	public interface ITextFormatter
	{
		string GetText(FlowDocument document);
		void SetText(FlowDocument document, string text);
	}
}
