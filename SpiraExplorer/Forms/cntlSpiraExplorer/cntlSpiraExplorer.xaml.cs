using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Properties;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Diagnostics;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Classes;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms
{
	/// <summary>Interaction logic for cntlSpiraExplorer.xaml</summary>
	public partial class cntlSpiraExplorer : UserControl, IVsPersistSolutionOpts
    {
		#region Internal Vars
		string _solutionName;
		private TreeViewArtifact _nodeNoSolution;
		private TreeViewArtifact _nodeNoProjects;
		#endregion
		#region Public Events
		public event EventHandler<OpenItemEventArgs> OpenDetails;
        #endregion

        #region Constants

        // The name of the solution section used to persist provider options (should be unique)
        private const string _strSolutionPersistanceKey = "SpiraExplorerSolutionProperties";

        // The name of the section in the solution user options file used to persist user-specific options (should be unique, shorter than 31 characters and without dots)
        private const string _strSolutionUserOptionsKey = "SpiraExplorer";

        #endregion

        #region Constructor

        /// <summary>Creates a new instance of the control.</summary>
        public cntlSpiraExplorer()
		{
			try
			{
				//Overall initialization.
				InitializeComponent();

				//Set button images and events.
				// - Config button
				Image btnConfigImage = Business.StaticFuncs.getImage("imgProject", new Size(16, 16));
				btnConfigImage.Stretch = Stretch.None;
				this.btnConfig.Content = btnConfigImage;
				// - Show Completed button
				Image btnCompleteImage = Business.StaticFuncs.getImage("imgShowCompleted", new Size(16, 16));
				btnCompleteImage.Stretch = Stretch.None;
				this.btnShowClosed.Content = btnCompleteImage;
				this.btnShowClosed.IsChecked = Settings.Default.ShowCompleted;
				// - Refresh Button
				Image btnRefreshImage = Business.StaticFuncs.getImage("imgRefresh", new Size(16, 16));
				btnRefreshImage.Stretch = Stretch.None;
				this.btnRefresh.Content = btnRefreshImage;
				// - Set bar color.
				this.barLoading.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(StaticFuncs.getCultureResource.GetString("app_Colors_StyledBarColor"));

				//Set datasource.
				this._Projects = new List<TreeViewArtifact>();
				this.trvProject.Items.Clear();
				this.trvProject.ItemsSource = this._Projects;

				//Load nodes.
				this.CreateStandardNodes();

				//If a solution is loaded now, get the loaded solution.
				if (Business.StaticFuncs.GetEnvironment.Solution.IsOpen)
					this.loadSolution((string)Business.StaticFuncs.GetEnvironment.Solution.Properties.Item("Name").Value);
				else
					this.loadSolution(null);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, ".ctor()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

        #endregion

        #region Control Events

        /// <summary>Hit when the user double-clicks on a tree node.</summary>
        /// <param name="sender">treeView</param>
        /// <param name="evt">EventArgs</param>
        private void tree_NodeDoubleClick(object sender, EventArgs evt)
		{
			try
			{
				string itemTag = (string)((TreeViewItem)sender).Tag;

				this.OpenDetails(this, new OpenItemEventArgs(itemTag));
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "tree_NodeDoubleClick()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the selected item changes in the treeview.</summary>
		/// <param name="sender">trvProject</param>
		/// <param name="e">RoutedPropertyChangedEventArgs</param>
		private void trvProject_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			try
			{
				e.Handled = true;

				//If it's a TreeViewArtifact item.
				if (this.trvProject.SelectedItem != null && this.trvProject.SelectedItem.GetType() == typeof(TreeViewArtifact))
				{
					//Only if it's NOT not a folder.
					TreeViewArtifact selItem = this.trvProject.SelectedItem as TreeViewArtifact;
					this.btnRefresh.IsEnabled = (selItem != null && selItem.ArtifactIsFolder);
				}
				else
					this.btnRefresh.IsEnabled = false;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "trvProject_SelectedItemChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user wants to refresh the list.</summary>
		/// <param name="sender">btnRefresh, btnShowClosed</param>
		/// <param name="e">Event Args</param>
		private void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				TreeViewArtifact selItem = this.trvProject.SelectedItem as TreeViewArtifact;
				if (selItem != null) this.refreshTreeNodeServerData(selItem);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnRefresh_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user wants to show/not show closed items.</summary>
		/// <param name="sender">TobbleButton</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnShowClosed_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				e.Handled = true;

				//We need to save the setting here.
				Settings.Default.ShowCompleted = this.btnShowClosed.IsChecked.Value;
				Settings.Default.Save();

				//Clear out all children..
				foreach (TreeViewArtifact trvProject in this._Projects)
				{
					trvProject.Items.Clear();
				}

				//Refresh the item list.
				this.refreshProjects();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnShowClosed_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when a toolbar button IsEnabled is changed, for greying out icons.</summary>
		/// <param name="sender">toolButton</param>
		/// <param name="e">DependencyPropertyChangedEventArgs</param>
		private void toolButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				UIElement btnChanged = sender as UIElement;
				if (btnChanged != null)
					btnChanged.Opacity = ((btnChanged.IsEnabled) ? 1 : .5);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "toolButton_IsEnabledChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user clicks on the Configuration button/</summary>
		/// <param name="sender">btnConfig</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnConfig_Click(object sender, RoutedEventArgs e)
		{
			try
			{
                frmNewSpiraProject frmAddProject = new frmNewSpiraProject();

                if (frmAddProject.ShowDialog().Value)
				{
					//If a solution is loaded now, get the loaded solution.
					if (Business.StaticFuncs.GetEnvironment.Solution.IsOpen)
						this.loadSolution((string)Business.StaticFuncs.GetEnvironment.Solution.Properties.Item("Name").Value, true);
					else
						this.loadSolution(null);
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnConfig_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
			e.Handled = true;
		}

		/// <summary>Possibly hit when the user double-clicks on an item in the treenode.</summary>
		/// <param name="sender">Object</param>
		/// <param name="e">MouseButtonEventArgs</param>
		/// <remarks>Must be public so the TreeNodeArtifact can access the funtion.</remarks>
		private void TreeNode_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			try
			{
				//If it's not a folder and an artifact, open a details screen.
				//Try to get the data item.
				ContentControl trvContainer = sender as ContentControl;
				if (trvContainer != null)
				{
					Grid trvGrid = trvContainer.Content as Grid;
					if (trvGrid != null)
					{
						TreeViewArtifact trvArtifact = trvGrid.DataContext as TreeViewArtifact;
						if (trvArtifact != null)
						{
							if (!trvArtifact.ArtifactIsFolder &&
								(trvArtifact.ArtifactType == TreeViewArtifact.ArtifactTypeEnum.Incident ||
								trvArtifact.ArtifactType == TreeViewArtifact.ArtifactTypeEnum.Requirement ||
								trvArtifact.ArtifactType == TreeViewArtifact.ArtifactTypeEnum.Task))
							{
								//Okay then, let's open up the details.
								((SpiraExplorerPackage)this.Pane.Package).OpenDetailsToolWindow(trvArtifact);
							}
						}
					}
				}
				else
				{
					if (sender is TreeViewArtifact)
					{
						((SpiraExplorerPackage)this.Pane.Package).OpenDetailsToolWindow((sender as TreeViewArtifact));
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "TreeNode_MouseDoubleClick()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#endregion

		/// <summary>Tells the control that a new solution was loaded.</summary>
		/// <param name="solName">The current Solution name.</param>
		public void loadSolution(string solName, bool force = false)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(solName))
				{
					this.noSolutionLoaded();
				}
				else
				{
					//Only get the projects if the solution name changed. (Avoid refreshing when solution name is unchanged.)
					if (this._solutionName != solName || force)
					{
                        //Access the SUO file to get the associated Spira URL, credentials and project
                        //If none available, display message
                        //TODO: ADAM
						//Get projects associated with this solution.
						SerializableDictionary<string, string> availProjects = Settings.Default.AssignedProjects;
						if (availProjects != null && availProjects.ContainsKey(solName) && !string.IsNullOrWhiteSpace(availProjects[solName]))
							this.loadProjects(availProjects[solName]);
						else
						{
							this.noProjectsLoaded();
						}
						this._solutionName = solName;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "loadSolution()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#region Tree Node Methods
		/// <summary>Creates the standard nodes. Run at class creation.</summary>
		private void CreateStandardNodes()
		{
			try
			{
				//Define our standard nodes here.
				// - No Projects
				this._nodeNoProjects = new TreeViewArtifact(this.refreshTreeNodeServerData);
				this._nodeNoProjects.ArtifactName = "No projects selected for this solution.";
				this._nodeNoProjects.ArtifactIsNo = true;

				// - No Solution
				this._nodeNoSolution = new TreeViewArtifact(this.refreshTreeNodeServerData);
				this._nodeNoSolution.ArtifactName = "No solution open.";
				this._nodeNoSolution.ArtifactIsNo = true;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "CreateStandardNodes()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

        #endregion

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
            hashSpiraUserData["spiraLogin"] = SpiraContext.Login;
            hashSpiraUserData["spiraPassword"] = SpiraContext.Password;

            // The easiest way to read/write the data of interest is by using a binary formatter class
            // This way, we can write a map of information about projects with one call 
            // (each element in the map needs to be serializable though)
            // The alternative is to write binary data in any byte format you'd like using pOptionsStream.Write
            DataStreamFromComStream pStream = new DataStreamFromComStream(pOptionsStream);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(pStream, hashSpiraUserData);

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
                    hashSpiraUserData["spiraLogin"] = SpiraContext.Login;
                }
                if (hashSpiraUserData.ContainsKey("spiraPassword"))
                {
                    hashSpiraUserData["spiraPassword"] = SpiraContext.Password;
                }
            }

            return VSConstants.S_OK;
        }

        #endregion

        #region Helper Classes
        /// <summary>Class for Opening the details of a new item.</summary>
        public class OpenItemEventArgs : EventArgs
		{
			public string ItemTag;

			public OpenItemEventArgs(string itemTag)
			{
				this.ItemTag = itemTag;
			}
		}
		#endregion

		public ToolWindowPane Pane
		{
			get;
			set;
		}
	}
}
