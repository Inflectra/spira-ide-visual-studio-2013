using System;
using System.Windows;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Classes;
using Microsoft.VisualStudio;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;
using System.ComponentModel;
using System.Collections.Generic;

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
                SolutionEventsListener l = new SolutionEventsListener(explorerWindow);
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

        /*[DisplayName("Project ID")]
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
        }*/

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

    internal class SolutionEventsListener : IVsSolutionEvents, IDisposable
    {
        private IVsSolution solution;
        private SoapServiceClient _client;
        private uint solutionEventsCookie;

        public event Action OnAfterOpenSolution;

        private string address, id, password;
        private cntlSpiraExplorer spiraExplorer;

        private List<TreeViewArtifact> _Projects = new List<TreeViewArtifact>();


        public SolutionEventsListener(cntlSpiraExplorer explorer)
        {
            this.spiraExplorer = explorer;
            InitNullEvents();

            solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;

            if (solution != null)
            {
                solution.AdviseSolutionEvents(this, out solutionEventsCookie);
            }
        }

        private void InitNullEvents()
        {
            OnAfterOpenSolution += () => {
                address = SpiraContext.BaseUri.ToString();
                id = SpiraContext.Login;
                password = SpiraContext.Password;

                //Create new client.
                this._client = StaticFuncs.CreateClient(address.Trim());
                this._client.Connection_Authenticate2Completed += new EventHandler<Connection_Authenticate2CompletedEventArgs>(_client_CommunicationFinished);
                this._client.User_RetrieveByUserNameCompleted += new EventHandler<User_RetrieveByUserNameCompletedEventArgs>(_client_CommunicationFinished);
                this._client.Project_RetrieveCompleted += new EventHandler<Project_RetrieveCompletedEventArgs>(_client_CommunicationFinished);

                this._client.Connection_Authenticate2Async(id, password, StaticFuncs.getCultureResource.GetString("app_ReportName"));
            };
        }

        private void _client_CommunicationFinished(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    try
                    {
                        if (e.GetType() == typeof(Connection_Authenticate2CompletedEventArgs))
                        {
                            Connection_Authenticate2CompletedEventArgs evt = e as Connection_Authenticate2CompletedEventArgs;
                            if (evt.Result)
                            {
                                this._client.User_RetrieveByUserNameAsync(this.id, false);
                            }
                        }
                        else if (e.GetType() == typeof(User_RetrieveByUserNameCompletedEventArgs))
                        {
                            User_RetrieveByUserNameCompletedEventArgs evt = e as User_RetrieveByUserNameCompletedEventArgs;
                            if (evt != null)
                            {
                                this._client.Project_RetrieveAsync();
                            }
                            else
                                throw new Exception("Results are null.");
                        }
                        else if (e.GetType() == typeof(Project_RetrieveCompletedEventArgs))
                        {
                            _Projects.Clear();

                            Project_RetrieveCompletedEventArgs evt = e as Project_RetrieveCompletedEventArgs;

                            //Load projects here.
                            if (evt != null && evt.Result.Count > 0)
                            {
                                SpiraProject matchingProject = null;
                                foreach (RemoteProject RemoteProj in evt.Result)
                                {
                                    Business.SpiraProject Project = new Business.SpiraProject();
                                    Project.ProjectId = RemoteProj.ProjectId.Value;
                                    Project.ServerURL = new Uri(this.address);
                                    Project.UserName = this.id;
                                    Project.UserPass = this.password;
                                    Project.UserID = int.Parse(this.id);

                                    TreeViewArtifact nProject = new TreeViewArtifact(spiraExplorer.refresh);
                                    nProject.ArtifactTag = Project;
                                    nProject.ArtifactId = ((Business.SpiraProject)nProject.ArtifactTag).ProjectId;
                                    nProject.ArtifactName = ((Business.SpiraProject)nProject.ArtifactTag).ProjectName;
                                    nProject.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Project;
                                    nProject.ArtifactIsFolder = true;
                                    nProject.Parent = null;

                                    _Projects.Add(nProject);

                                    if (SpiraContext.ProjectId == Project.ProjectId)
                                    {
                                        matchingProject = Project;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage(ex);
                    }
                }
                else
                {
                    Logger.LogMessage(e.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex, "_client_CommunicationFinished()");
                MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            //Access the SLN/SUO file to get the associated Spira URL, credentials and project
            if (SpiraContext.HasSolutionProps)
            {
                spiraExplorer.loadProject(SpiraContext.ProjectId);
                spiraExplorer.refresh(null);
            }
            
        }

        private void old(AsyncCompletedEventArgs e)
        {
            
        }



        #region IVsSolutionEvents Members

        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            OnAfterOpenSolution();
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (solution != null && solutionEventsCookie != 0)
            {
                GC.SuppressFinalize(this);
                solution.UnadviseSolutionEvents(solutionEventsCookie);
                OnAfterOpenSolution = null;
                solutionEventsCookie = 0;
                solution = null;
            }
        }

        #endregion
    }

}
