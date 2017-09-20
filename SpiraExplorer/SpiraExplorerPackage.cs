using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using EnvDTE;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Controls;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012
{
    /// <summary>This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.</summary>
    [PackageRegistration(UseManagedResourcesOnly = true)] // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is a package.
    [InstalledProductRegistration("#110", "#112", "3.2.1.14503", IconResourceID = 400)] // This attribute is used to register the information needed to show the this package in the Help/About dialog of Visual Studio.
    [ProvideMenuResource("Menus.ctmenu", 1)] // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideToolWindow(typeof(toolSpiraExplorer), MultiInstances = false, Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")] // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(toolSpiraExplorerDetails), MultiInstances = true, Window = "76C22C24-36B6-4C0C-BF60-FFCB65D1B05B", Transient = false)] // This attribute registers a tool window exposed by this package.
    [Guid(GuidList.guidSpiraExplorerPkgString)]
    public sealed class SpiraExplorerPackage : Package
    {
        private EnvDTE.Events _EnvironEvents;
        SolutionEvents _SolEvents;
        public static Dictionary<TreeViewArtifact, int> _windowDetails;
        static int _numWindowIds = -1;
        private static string CLASS = "SpiraExplorerPackage::";

        /// <summary>Default constructor of the package. Inside this method you can place any initialization code that does not require any Visual Studio service because at this point the package object is created but not sited yet inside Visual Studio environment. The place to do all the other initialization is the Initialize method.</summary>
        public SpiraExplorerPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));

            //Upgrade existing settings,
            Settings.Default.Upgrade();

            //Get settings ready..
            if (Settings.Default.AssignedProjects == null)
            {
                Settings.Default.AssignedProjects = new Business.SerializableDictionary<string, string>();
                Settings.Default.Save();
            }
            if (Settings.Default.AllProjects == null)
            {
                Settings.Default.AllProjects = new Business.SerializableList<string>();
                Settings.Default.Save();
            }
            if (SpiraExplorerPackage._windowDetails == null)
            {
                SpiraExplorerPackage._windowDetails = new Dictionary<TreeViewArtifact, int>();
            }

            //Initialize the Logger.
#if DEBUG
            Logger.LoggingToFile = true;
            Logger.TraceLogging = true;
#endif
            new Logger(StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"));
        }

        /// <summary>This function is called when the user clicks the menu item that shows the tool window. See the Initialize method to see how the menu item is associated to this function using the OleMenuCommandService service and the MenuCommand class.</summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            try
            {
                // Get the instance number 0 of this tool window. This window is single instance so this instance
                // is actually the only one.
                // The last flag is set to true so that if the tool window does not exists it will be created.
                ToolWindowPane window = this.FindToolWindow(typeof(toolSpiraExplorer), 0, true);

                if ((null == window) || (null == window.Frame))
                {
                    throw new NotSupportedException(StaticFuncs.getCultureResource.GetString("app_General_CreateWindowError"));
                }
                IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex, "ShowToolWindow()");
                MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Package Members
        /// <summary>Initialization of the package; this method is called right after the package is sited, so this is the place where you can put all the initialization code that rely on services provided by VisualStudio.</summary>
        protected override void Initialize()
        {
            try
            {
                Logger.LogTrace(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
                base.Initialize();

                // Add our command handlers for menu (commands must exist in the .vsct file)
                OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                if (mcs != null)
                {
                    // Create the command for the tool window
                    CommandID toolwndCommandID = new CommandID(GuidList.guidSpiraExplorerCmdSet, (int)PkgCmdIDList.cmdViewExplorerWindow);
                    MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                    mcs.AddCommand(menuToolWin);

                    //DEBUG: Log info..
                    if (toolwndCommandID == null)
                        Logger.LogTrace("Initialize(): CommandID- was null!");
                    else
                        Logger.LogTrace("Initialize(): CommandID- " + toolwndCommandID.Guid.ToString() + " -- " + toolwndCommandID.ID);
                    if (menuToolWin == null)
                        Logger.LogTrace("Initialize(): MenuCommand- was null!");
                    else
                        Logger.LogTrace("Initialize(): MenuCommand- " + menuToolWin.OleStatus + " -- " + menuToolWin.Enabled + " -- " + menuToolWin.Supported + " -- " + menuToolWin.Visible);
                }
                else
                {
                    Logger.LogTrace("Initialize(): OleMenuCommandService was null!");
                }

                //Attach to the environment to get events..
                this._EnvironEvents = Business.StaticFuncs.GetEnvironment.Events;
                this._SolEvents = Business.StaticFuncs.GetEnvironment.Events.SolutionEvents;
                if (this._EnvironEvents != null && this._SolEvents != null)
                {
                    this._SolEvents.Opened += new EnvDTE._dispSolutionEvents_OpenedEventHandler(SolutionEvents_Opened);
                    this._SolEvents.AfterClosing += new EnvDTE._dispSolutionEvents_AfterClosingEventHandler(SolutionEvents_AfterClosing);
                    this._SolEvents.Renamed += new EnvDTE._dispSolutionEvents_RenamedEventHandler(SolutionEvents_Renamed);
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex, "Initialize()");
                MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Environment Events
        /// <summary>Hit when an open solution is renamed.</summary>
        /// <param name="OldName">The old name of the solution.</param>
        private void SolutionEvents_Renamed(string OldName)
        {
            try
            {
                //Get the new name of the solution..
                if (Business.StaticFuncs.GetEnvironment.Solution.IsOpen)
                {
                    string NewName = (string)Business.StaticFuncs.GetEnvironment.Solution.Properties.Item("Name").Value;
                    if (!string.IsNullOrWhiteSpace(NewName))
                    {
                        //Modify the settings to transfer over projects.
                        if (Settings.Default.AssignedProjects.ContainsKey(OldName))
                        {
                            string strAssignedProjects = Settings.Default.AssignedProjects[OldName];
                            Settings.Default.AssignedProjects.Remove(OldName);
                            if (Settings.Default.AssignedProjects.ContainsKey(NewName))
                                Settings.Default.AssignedProjects[NewName] = strAssignedProjects;
                            else
                                Settings.Default.AssignedProjects.Add(NewName, strAssignedProjects);
                            Settings.Default.Save();
                        }

                        //Reload projects..
                        ToolWindowPane window = this.FindToolWindow(typeof(toolSpiraExplorer), 0, false);
                        if (window != null)
                        {
                            cntlSpiraExplorer toolWindow = (cntlSpiraExplorer)window.Content;
                            toolWindow.loadSolution(NewName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex, "SolutionEvents_Renamed()");
                MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>Hit when the open solution is closed.</summary>
        private void SolutionEvents_AfterClosing()
        {
            try
            {
                //Get the window.
                ToolWindowPane window = this.FindToolWindow(typeof(toolSpiraExplorer), 0, false);
                if (window != null)
                {
                    cntlSpiraExplorer toolWindow = (cntlSpiraExplorer)window.Content;
                    toolWindow.loadSolution(null);
                }

                //Close all open details windows.
                lock (SpiraExplorerPackage._windowDetails)
                {
                    foreach (KeyValuePair<TreeViewArtifact, int> detailWindow in SpiraExplorerPackage._windowDetails)
                    {
                        ToolWindowPane windowDetail = this.FindToolWindow(typeof(toolSpiraExplorerDetails), detailWindow.Value, false);
                        if (windowDetail != null)
                            ((IVsWindowFrame)windowDetail.Frame).Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex, "SolutionEvents_AfterClosing()");
                MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>Hit when a solution is opened.</summary>
        private void SolutionEvents_Opened()
        {
            try
            {
                if (Business.StaticFuncs.GetEnvironment.Solution.IsOpen)
                {
                    //Get the window.
                    ToolWindowPane window = this.FindToolWindow(typeof(toolSpiraExplorer), 0, false);
                    if (window != null)
                    {
                        cntlSpiraExplorer toolWindow = (cntlSpiraExplorer)window.Content;
                        toolWindow.loadSolution((string)Business.StaticFuncs.GetEnvironment.Solution.Properties.Item("Name").Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex, "SolutionEvents_Opened()");
                MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        public void OpenDetailsToolWindow(TreeViewArtifact Artifact)
        {
            string METHOD = CLASS + "OpenDetailsToolWindow";
            try
            {
                //Find the existing window..
                toolSpiraExplorerDetails window = this.FindExistingToolWindow(Artifact, true);

                if (window != null)
                {
                    //If the window is hidden and is not in an unsaved state, reload window contents.
                    //   If window is not hidden, simply bring it to foreground.
                    //   If window is not created, load window contents.
                    //   If window is hidden and in an unsaved state, simply bring it to foreground.

                    //See if we need to reset the content.
                    if (!window.IsContentSet || (window.IsHidden && !window.IsChanged))
                    {
                        //Generate the details screen.
                        object detailContent = null;
                        switch (Artifact.ArtifactType)
                        {
                            case TreeViewArtifact.ArtifactTypeEnum.Incident:
                                frmDetailsIncident detIncident = new frmDetailsIncident(Artifact, window);
                                detailContent = detIncident;
                                break;

                            case TreeViewArtifact.ArtifactTypeEnum.Requirement:
                                frmDetailsRequirement detRequirement = new frmDetailsRequirement(Artifact, window);
                                detailContent = detRequirement;
                                break;

                            case TreeViewArtifact.ArtifactTypeEnum.Task:
                                frmDetailsTask detTask = new frmDetailsTask(Artifact, window);
                                detailContent = detTask;
                                break;
                        }
                        //Set toolwindow's content.
                        if (detailContent != null)
                        {
                            ((cntrlDetailsForm)window.FormControl).Content = detailContent;

                        }
                    }

                    //Get the frame.
                    IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
                    windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_MdiChild);
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
                    ((dynamic)((cntrlDetailsForm)window.FormControl).Content).IsHidden = false;
                }
                else
                {
                    //Log an error.
                    Logger.LogMessage(METHOD, "Could not create window.", EventLogEntryType.Error);
                    MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_WindowOpenErrorMessage"), StaticFuncs.getCultureResource.GetString("app_General_WindowOpenError"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex, "OpenDetailsToolWindow()");
                MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>Returns the details window if it exists, otherwise null.</summary>
        /// <param name="Artifact">The TreeViewArtifact to get it's detailswindow.</param>
        /// <returns>A toolSpiraExplorerDetails if found, null otherwise.</returns>
        public toolSpiraExplorerDetails FindExistingToolWindow(TreeViewArtifact Artifact, bool create = false)
        {
            toolSpiraExplorerDetails retWindow = null;

            try
            {
                if (SpiraExplorerPackage._windowDetails == null)
                {
                    SpiraExplorerPackage._windowDetails = new Dictionary<TreeViewArtifact, int>();
                }

                //Get the window ID if it already exists.
                int NextId = -1;
                if (SpiraExplorerPackage._windowDetails.ContainsKey(Artifact)) //Get the ID if it exists.
                    NextId = SpiraExplorerPackage._windowDetails[Artifact];
                else //Figure out the next ID.
                {
                    SpiraExplorerPackage._numWindowIds++;
                    NextId = SpiraExplorerPackage._numWindowIds;
                    SpiraExplorerPackage._windowDetails.Add(Artifact, SpiraExplorerPackage._numWindowIds);
                }

                //Now try to grab the window..
                retWindow = this.FindToolWindow(typeof(toolSpiraExplorerDetails), NextId, false) as toolSpiraExplorerDetails;

                if (retWindow == null && create)
                    retWindow = this.FindToolWindow(typeof(toolSpiraExplorerDetails), NextId, true) as toolSpiraExplorerDetails;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex, "Error creating window.");
                retWindow = null;
            }

            return retWindow;
        }
    }
}
