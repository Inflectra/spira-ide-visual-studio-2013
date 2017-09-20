using System.Windows;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Controls
{
	/// <summary>
	/// Interaction logic for wpfDiscussionFrame.xaml
	/// </summary>
	public partial class cntlDiscussionFrame : UserControl
	{
		public cntlDiscussionFrame(string HeaderLine, string MessageBody)
		{
			InitializeComponent();

			this.txtHeader.Text = HeaderLine;
			this.txtMessage.BorderThickness = new Thickness(0);

			if (string.IsNullOrEmpty(MessageBody))
				this.txtMessage.Visibility = Visibility.Collapsed;
			else
				this.txtMessage.HTMLText = MessageBody;
		}
	}
}
