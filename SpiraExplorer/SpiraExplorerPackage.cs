using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
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
using Microsoft.VisualStudio.OLE.Interop;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Classes;
using Microsoft.VisualStudio;

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
    [ProvideSolutionProperties(_strSolutionPersistanceKey)]
    public sealed class SpiraExplorerPackage : Package, IVsPersistSolutionOpts, IVsPersistSolutionProps
    {
        private EnvDTE.Events _EnvironEvents;
        SolutionEvents _SolEvents;
        public static Dictionary<TreeViewArtifact, int> _windowDetails;
        static int _numWindowIds = -1;
        private static string CLASS = "SpiraExplorerPackage::";

        #region Constants

        // The name of the solution section used to persist provider options (should be unique)
        private const string _strSolutionPersistanceKey = "SpiraExplorerSolutionProperties";

        // The name of the section in the solution user options file used to persist user-specific options (should be unique, shorter than 31 characters and without dots)
        private const string _strSolutionUserOptionsKey = "SpiraExplorer";

        #endregion


        /// <summary>Default constructor of the package. Inside this method you can place any initialization code that does not require any Visual Studio service because at this point the package object is created but not sited yet inside Visual Studio environment. The place to do all the other initialization is the Initialize method.</summary>
        public SpiraExplorerPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));

            //Upgrade existing settings,
            Settings.Default.Upgrade();

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

        #region IVsPersistSolutionOpts methods

        /// <summary>
        /// Called by the shell when the SUO file is saved. The provider calls the shell back to let it 
        /// know which options keys it will use in the suo file.
        /// </summary>
        public int SaveUserOptions(IVsSolutionPersistence pPersistence)
        {
            // The shell will create a stream for the section of interest, and will call back the provider on 
            // IVsPersistSolutionProps.WriteUserOptions() to save specific options under the specified key.
            pPersistence.SavePackageUserOpts(this, _strSolutionUserOptionsKey);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by the shell when a solution is opened and the SUO file is read.
        /// </summary>
        public int LoadUserOptions(IVsSolutionPersistence pPersistence, uint grfLoadOpts)
        {
            // Note this can be during opening a new solution, or may be during merging of 2 solutions.
            // The provider calls the shell back to let it know which options keys from the suo file were written by this provider.
            // If the shell will find in the suo file a section that belong to this package, it will create a stream, 
            // and will call back the provider on IVsPersistSolutionProps.ReadUserOptions() to read specific options 
            // under that option key.
            pPersistence.LoadPackageUserOpts(this, _strSolutionUserOptionsKey);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by the shell to let the package write user options under the specified key.
        /// </summary>
        public int WriteUserOptions(IStream pOptionsStream, string pszKey)
        {
            // This function gets called by the shell to let the package write user options under the specified key.
            // The key was declared in SaveUserOptions(), when the shell started saving the suo file.
            Debug.Assert(pszKey.CompareTo(_strSolutionUserOptionsKey) == 0, "The shell called to read an key that doesn't belong to this package");

            //Add the Spira settings to the hashtable
            Hashtable hashSpiraUserData = new Hashtable();
            if (!String.IsNullOrEmpty(SpiraContext.Login) && !String.IsNullOrEmpty(SpiraContext.Password))
            {
                hashSpiraUserData["spiraLogin"] = SpiraContext.Login;
                hashSpiraUserData["spiraPassword"] = SpiraContext.Password;

                // The easiest way to read/write the data of interest is by using a binary formatter class
                // This way, we can write a map of information about projects with one call 
                // (each element in the map needs to be serializable though)
                // The alternative is to write binary data in any byte format you'd like using pOptionsStream.Write
                DataStreamFromComStream pStream = new DataStreamFromComStream(pOptionsStream);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(pStream, hashSpiraUserData);
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by the shell if the _strSolutionUserOptionsKey section declared in LoadUserOptions() as 
        /// being written by this package has been found in the suo file
        /// </summary>
        public int ReadUserOptions(IStream pOptionsStream, string pszKey)
        {
            // This function is called by the shell if the _strSolutionUserOptionsKey section declared
            // in LoadUserOptions() as being written by this package has been found in the suo file. 
            // Note this can be during opening a new solution, or may be during merging of 2 solutions.
            // A good source control provider may need to persist this data until OnAfterOpenSolution or OnAfterMergeSolution is called

            // The easiest way to read/write the data of interest is by using a binary formatter class
            DataStreamFromComStream pStream = new DataStreamFromComStream(pOptionsStream);
            Hashtable hashSpiraUserData = new Hashtable();
            if (pStream.Length > 0)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                hashSpiraUserData = formatter.Deserialize(pStream) as Hashtable;

                if (hashSpiraUserData.ContainsKey("spiraLogin"))
                {
                    SpiraContext.Login = (string)hashSpiraUserData["spiraLogin"];
                }
                if (hashSpiraUserData.ContainsKey("spiraPassword"))
                {
                    SpiraContext.Password = (string)hashSpiraUserData["spiraPassword"];
                }
            }

            return VSConstants.S_OK;
        }

        #endregion

        #region  IVsPersistSolutionProps

        /// <summary>
        /// This function is called by the IDE to determine if something needs to be saved in the solution.
        /// If the package returns that it has dirty properties, the shell will callback on SaveSolutionProps
        /// </summary>
        /// <param name="pHierarchy"></param>
        /// <param name="pqsspSave"></param>
        /// <returns></returns>
        public int QuerySaveSolutionProps(IVsHierarchy pHierarchy, VSQUERYSAVESLNPROPS[] pqsspSave)
        {
            if (pHierarchy == null)
            {

                VSQUERYSAVESLNPROPS result = VSQUERYSAVESLNPROPS.QSP_HasNoProps;

                if (SpiraContext.IsDirty)
                {
                    result = VSQUERYSAVESLNPROPS.QSP_HasDirtyProps;
                }
                else if (!SpiraContext.HasSolutionProps)
                {
                    result = VSQUERYSAVESLNPROPS.QSP_HasNoProps;
                }
                else
                    result = VSQUERYSAVESLNPROPS.QSP_HasNoDirtyProps;
                pqsspSave[0] = result;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// This function gets called by the shell after QuerySaveSolutionProps returned QSP_HasDirtyProps
        /// </summary>
        /// <param name="pHierarchy"></param>
        /// <param name="pPersistence"></param>
        /// <returns></returns>
        public int SaveSolutionProps(IVsHierarchy pHierarchy, IVsSolutionPersistence pPersistence)
        {
            // The package will pass in the key under which it wants to save its properties, 
            // and the IDE will call back on WriteSolutionProps

            // The properties will be saved in the Pre-Load section
            // When the solution will be reopened, the IDE will call our package to load them back before the projects in the solution are actually open
            // This could help if the source control package needs to persist information like projects translation tables, that should be read from the suo file
            // and should be available by the time projects are opened and the shell start calling IVsSccEnlistmentPathTranslation functions.
            if (pHierarchy == null) // Only save the property on the solution itself
            {
                // SavePackageSolutionProps will call WriteSolutionProps with the specified key

                if (SpiraContext.HasSolutionProps)
                    pPersistence.SavePackageSolutionProps(1 /* TRUE */, null, this, _strSolutionPersistanceKey);

                // Once we saved our props, the solution is not dirty anymore
                SpiraContext.SolutionPropsSaved();
            }

            return VSConstants.S_OK;
        }

        public int WriteSolutionProps(IVsHierarchy pHierarchy, string pszKey, IPropertyBag pPropBag)
        {
            if (pHierarchy != null)
                return VSConstants.S_OK; // Not send by our code!
            else if (pPropBag == null)
                return VSConstants.E_POINTER;

            pPropBag.Write("spiraUrl", SpiraContext.BaseUri.ToString());
            pPropBag.Write("spiraProjectId", SpiraContext.ProjectId.ToString());

            return VSConstants.S_OK;
        }

        public int ReadSolutionProps(IVsHierarchy pHierarchy, string pszProjectName, string pszProjectMk, string pszKey, int fPreLoad, IPropertyBag pPropBag)
        {
            if (pHierarchy != null)
                return VSConstants.S_OK;

            object absoluteUri;
            object projectId;
            pPropBag.Read("spiraUrl", out absoluteUri, null, 0, null);
            pPropBag.Read("spiraProjectId", out projectId, null, 0, null);
            if (absoluteUri != null)
            {
                SpiraContext.BaseUri = new Uri((string)absoluteUri);

                //We don't want it marked as dirty yet
                SpiraContext.SolutionPropsSaved();
            }
            if (projectId != null)
            {
                int intValue;
                if (Int32.TryParse((string)projectId, out intValue))
                {
                    SpiraContext.ProjectId = intValue;

                    //We don't want it marked as dirty yet
                    SpiraContext.SolutionPropsSaved();
                }
            }

            return VSConstants.S_OK;
        }

        public int OnProjectLoadFailure(IVsHierarchy pStubHierarchy, string pszProjectName, string pszProjectMk, string pszKey)
        {
            // We should save our settings again
            SpiraContext.SetUnsavedChanges();

            return VSConstants.S_OK;
        }

        #endregion
    }
}
