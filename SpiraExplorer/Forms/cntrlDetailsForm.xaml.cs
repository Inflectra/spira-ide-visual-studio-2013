using System.Windows.Controls;
using System;
using Inflectra.Global;
using System.Windows;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Controls
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class cntrlDetailsForm : UserControl
	{
		public cntrlDetailsForm()
		{
			try
			{
				InitializeComponent();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "InitializeComponent()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}

		}

		public new object Content
		{
			get
			{
				return this.cntrlContent.Content;
			}
			set
			{
				this.cntrlContent.Content = value;
			}
		}
	}
}
