using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents; 
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Controls
{
	/// <summary>
	/// Interaction logic for AnchorDialogue.xaml
	/// </summary>
	public partial class cntrlAnchorDialogue : Window
	{

		private string _linkText;
		private string _url;

		public cntrlAnchorDialogue()
			: this(string.Empty)
		{
		}

		public cntrlAnchorDialogue(string linkText)
		{
			InitializeComponent();
			this._linkText = linkText;
			this.txtLinkText.Text = linkText;
		}

		public bool UrlCheck
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the selected link text.
		/// </summary>
		public string LinkText
		{
			get
			{
				string linkText = this._linkText;
				if (string.IsNullOrEmpty(linkText))
				{
					linkText = this._url;
				}
				return linkText;
			}
		}

		/// <summary>
		/// Gets the selected url.
		/// </summary>
		public string Url
		{
			get { return this._url; }
		}

		private void txtUrl_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.btnOK.IsEnabled = (this.txtUrl.Text.Length > 0);
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			if (this.txtUrl.Text.Length > 0)
			{
				bool isOk = true;
				if (this.UrlCheck)
				{
					try
					{
						Uri test = new Uri(this.txtUrl.Text);
					}
					catch
					{
						isOk = false;
						MessageBox.Show("Invalid URL format.", "Invalid", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
				if (isOk)
				{
					this._url = this.txtUrl.Text;
					this._linkText = this.txtLinkText.Text;
					this.DialogResult = true;
				}
				else
				{ 
					e.Handled = true; 
				}
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}
	}
}
