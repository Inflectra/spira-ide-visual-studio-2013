using System;
using System.Windows;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Microsoft.VisualStudio.Shell;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms
{
	/// <summary>
	/// This class implements the tool window exposed by this package and hosts a user control.
	///
	/// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
	/// usually implemented by the package implementer.
	///
	/// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
	/// implementation of the IVsUIElementPane interface.
	/// </summary>
	//[Guid("3ae79031-e1bc-11d0-8f78-00a0c9110057")]
	public class toolSpiraExplorer : ToolWindowPane
	{
		/// <summary>Standard constructor for the tool window.</summary>
		public toolSpiraExplorer() :
			base(null)
		{
			try
			{
				// Set the window title reading it from the resources.
				this.Caption = StaticFuncs.getCultureResource.GetString("app_Tree_Name");
				// Set the image that will appear on the tab of the window frame
				// when docked with an other window
				// The resource ID correspond to the one defined in the resx file
				// while the Index is the offset in the bitmap strip. Each image in
				// the strip being 16x16.
				this.BitmapResourceID = 301;
				this.BitmapIndex = 0;

				// This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
				// we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
				// the object returned by the Content property.

				cntlSpiraExplorer explorerWindow = new cntlSpiraExplorer();
				explorerWindow.Pane = this;

				base.Content = explorerWindow;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "ShowToolWindow()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}

		}
	}
}
