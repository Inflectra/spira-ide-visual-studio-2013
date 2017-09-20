















namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit
{
	public interface IRichTextBoxFormatBar
	{
		/// <summary>
		/// Represents the RichTextBox that will be the target for all text manipulations in the format bar.
		/// </summary>
		System.Windows.Controls.RichTextBox Target
		{
			get;
			set;
		}

		/// <summary>
		/// Represents the Method that will be used to update the format bar values based on the Selection.
		/// </summary>
		void Update();
	}
}
