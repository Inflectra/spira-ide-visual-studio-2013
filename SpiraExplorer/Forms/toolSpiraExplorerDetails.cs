using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Controls;
using Microsoft.VisualStudio.Shell;
using System.Windows;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using System;
using Inflectra.Global;

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
	//[Guid("76C22C24-36B6-4C0C-BF60-FFCB65D1B05B")]
	public class toolSpiraExplorerDetails : ToolWindowPane
	{
		/// <summary>Standard constructor for the tool window.</summary>
		public toolSpiraExplorerDetails() :
			base(null)
		{
			try
			{
				base.Caption = "";
				base.Content = new cntrlDetailsForm();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "toolSpiraExplorerDetails()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public toolSpiraExplorerDetails(object ContentControl)
			: this()
		{
			try
			{
				this.FormControl = ContentControl;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "ShowToolWindow()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>The contents of the tool window.</summary>
		public object FormControl
		{
			get
			{
				return base.Content;
			}
			set
			{
				base.Content = value;
			}
		}

		/// <summary>Hit when the window is attempted to be closed.</summary>
		protected override void OnClose()
		{
			try
			{
				if (((dynamic)((dynamic)this.FormControl).Content).IsUnsaved)
				{
					//Show message asking if they want to save.
					MessageBoxResult saveChanges = MessageBoxResult.No;
					saveChanges = MessageBox.Show(
						StaticFuncs.getCultureResource.GetString("app_Global_SaveChangesMessage"),
						StaticFuncs.getCultureResource.GetString("app_Global_SaveChanges"),
						MessageBoxButton.YesNo,
						MessageBoxImage.Question,
						MessageBoxResult.No);

					if (saveChanges == MessageBoxResult.Yes)
					{
						((dynamic)((dynamic)this.FormControl).Content).ExternalSave();
					}
				}
			}
			catch
			{ }
		}

		/// <summary>Whether or not the contents have unsaved changes.</summary>
		public bool IsChanged
		{
			get
			{
				try
				{
					//Regardless of the content, get whether it's changed or not.
					return (bool)((dynamic)((dynamic)this.FormControl).Content).IsUnsaved;
				}
				catch
				{
					return false;
				}
			}
		}

		/// <summary>Returns whether this specific item has been opened already or not.</summary>
		public bool IsHidden
		{
			get
			{
				try
				{
					return (bool)((dynamic)((dynamic)this.FormControl).Content).IsHidden;
				}
				catch
				{
					return true;
				}
			}
		}

		/// <summary>Whether or not the content has been set yet.</summary>
		public bool IsContentSet
		{
			get
			{
				return (((dynamic)this.FormControl).Content != null);
			}
		}
	}
}
