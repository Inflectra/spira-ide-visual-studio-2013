using System;
using System.Windows;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections;
using System.ComponentModel;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Classes;

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
        private ITrackSelection trackSel;
        private SelectionContainer selContainer;

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

        #region Support for populating Properties window

        [DisplayName("Project ID")]
        [Category("Project Properties")]
        [Description("SpiraTeam Project ID")]
        public string ProjectId
        {
            get
            {
                return "PR:" + SpiraContext.ProjectId;
            }
        }

        [DisplayName("Project Name")]
        [Category("Project Properties")]
        [Description("SpiraTeam Project Name")]
        public string ProjectName
        {
            get
            {
                cntlSpiraExplorer explorerWindow = (cntlSpiraExplorer)base.Content;
                if (explorerWindow != null)
                {
                    return explorerWindow.CurrentProject;
                }
                return null;
            }
        }

        private ITrackSelection TrackSelection
        {
            get
            {
                if (trackSel == null)
                    trackSel =
                       GetService(typeof(STrackSelection)) as ITrackSelection;
                return trackSel;
            }
        }

        /// <summary>
        /// Allows the XAML control get to get a Visual Studio base shell service
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetVSService(Type serviceType)
        {
            return base.GetService(serviceType);
        }


        public void UpdateSelection()
        {
            ITrackSelection track = TrackSelection;
            if (track != null)
                track.OnSelectChange((ISelectionContainer)selContainer);
        }

        public void SelectList(ArrayList list)
        {
            selContainer = new SelectionContainer(true, false);
            selContainer.SelectableObjects = list;
            selContainer.SelectedObjects = list;
            UpdateSelection();
        }

        public override void OnToolWindowCreated()
        {
            cntlSpiraExplorer explorerWindow = (cntlSpiraExplorer)base.Content;
            if (explorerWindow != null)
            {
                SpiraProperties spiraProperties = new SpiraProperties(explorerWindow);
                ArrayList listObjects = new ArrayList();
                listObjects.Add(spiraProperties);
                SelectList(listObjects);
            }
        }

        #endregion
    }
}
