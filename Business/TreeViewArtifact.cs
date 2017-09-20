using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.Forms;
using Microsoft.VisualStudio.Shell;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business
{
	public class TreeViewArtifact
	{
		private bool _isTimed;
		private DateTime _startTime;
		private DateTime _endTime;
		private RefreshMethod _method;
		public delegate void RefreshMethod(TreeViewArtifact item);

		public event EventHandler WorkTimerChanged;
		public event EventHandler DetailsOpenRequested;

		/// <summary>Creates a new instance of the class, unable to refresh itself.</summary>
		public TreeViewArtifact()
		{
			this.ArtifactType = ArtifactTypeEnum.None;
			this.Items = new List<object>();
		}

		/// <summary>Creates a new instance of the class, with the pointer to the method to refresh itself.</summary>
		/// <param name="method">The refreshTreeNodeServerData method.</param>
		public TreeViewArtifact(RefreshMethod method)
		{
			this._method = method;
			this.ArtifactType = ArtifactTypeEnum.None;
			this.Items = new List<object>();
		}

		/// <summary>Default indexer.</summary>
		/// <param name="index">The number of the item.</param>
		/// <returns>Child at index.</returns>
		public object this[int index]
		{
			get
			{
				return this.Items[index];
			}
			set
			{
				this.Items[index] = value;
			}
		}

		#region Artifact Properties
		/// <summary>The artifact's name.</summary>
		public string ArtifactName
		{
			get;
			set;
		}

		/// <summary>Used for additional storage. (Like a client, for example.) Can hold various items:
		/// null						- No data associated with this item.
		/// boolean						- For a folder, determines whether or not this folder contains
		///								  MY artifact (true) or Unassigned artifacts (false). After the
		///								  associated client is finished pulling data.
		/// Spira_ImportExportClient	- For a folder, is the client that is actively going out to get
		///								  information for populating itself. After this client is 
		///								  finished, will be changed to the boolean above.
		///	RemoteArtifact				- For individual items, the populated RemoteArtifact.
		///	SpiraProject				- For projects, the associated SpiraProject.
		/// </summary>
		public object ArtifactTag
		{
			get;
			set;
		}

		/// <summary>The ID of the artifact.</summary>
		public int ArtifactId
		{
			get;
			set;
		}

		/// <summary>The Type of the Artifact.</summary>
		public ArtifactTypeEnum ArtifactType
		{
			get;
			set;
		}
		#endregion

		#region Properties for TreeNode
		/// <summary>The parent object (TreeViewArtifact) of this item.</summary>
		public TreeViewArtifact Parent
		{
			get;
			set;
		}

		/// <summary>Readonly. Returns the parent project node (or null if not found.)</summary>
		public TreeViewArtifact ArtifactParentProject
		{
			get
			{
				if (this.ArtifactType == ArtifactTypeEnum.Project)
					return this;
				else
				{
					TreeViewArtifact retNode = null;

					if (this.Parent != null)
					{
						TreeViewArtifact checkNode = this.Parent as TreeViewArtifact;
						while (checkNode != null && checkNode.ArtifactType != ArtifactTypeEnum.Project)
						{
							checkNode = checkNode.Parent as TreeViewArtifact;
						}
						retNode = checkNode;
					}
					return retNode;
				}
			}
		}

		/// <summary>Items contained within the current item.</summary>
		public List<object> Items
		{
			get;
			set;
		}

		#endregion

		#region Properties for Display
		/// <summary>Readonly. header name with formatting of the artifact.</summary>
		private UIElement ArtifactDisplayName
		{
			get
			{
				string retName = this.ArtifactName;

				if (this.ArtifactType == ArtifactTypeEnum.Project)
				{
					retName += " [";

					SpiraProject projTag = this.ArtifactTag as SpiraProject;
					if (projTag != null)
						retName += projTag.ServerURL.Host.Trim();
					else
						retName += StaticFuncs.getCultureResource.GetString("app_General_Unknown");

					retName += "]";
				}
				else if (this.ArtifactIsFolder)
				{
					if (this.ArtifactType != ArtifactTypeEnum.None && this.Items.Count > 0)
						retName += " (" + this.Items.Count.ToString() + ")";
				}
				else
				{
					retName += " " + this.ArtifactIDDisplay;
				}

				//Add it to a textblock.
				TextBlock txtName = new TextBlock();
				txtName.SetResourceReference(TextBlock.ForegroundProperty, VsBrushes.ToolWindowTextKey);
				if (this.Style_IsInError)
				{
					txtName.Foreground = new SolidColorBrush(Color.FromArgb(255, 224, 64, 0));
					txtName.FontStyle = FontStyles.Italic;
				}
				if (this.Style_IsCompleted)
				{
					txtName.Foreground = new SolidColorBrush(Colors.Gray);
					txtName.FontStyle = FontStyles.Normal;
					txtName.TextDecorations = new TextDecorationCollection() { TextDecorations.Strikethrough };
				}
				txtName.Text = retName;

				return txtName;
			}
		}

		/// <summary>Readonly. Returns the image source for displaying the appropriate image in the TreeView.</summary>
		private ImageSource ArtifactImageSource
		{
			get
			{
				if (this.ArtifactType == ArtifactTypeEnum.Project)
					return StaticFuncs.getImage("imgProject", new System.Windows.Size(16, 16)).Source;
				else if (this.ArtifactIsError)
					return StaticFuncs.getImage("imgError", new System.Windows.Size(16, 16)).Source;
				else if (this.ArtifactIsNo)
					return StaticFuncs.getImage("imgNo", new System.Windows.Size(16, 16)).Source;
				else if (this.ArtifactIsFolder)
					switch (this.ArtifactType)
					{
						case ArtifactTypeEnum.Incident:
							return StaticFuncs.getImage("imgFolderIncident", new System.Windows.Size(16, 16)).Source;
						case ArtifactTypeEnum.Requirement:
							return StaticFuncs.getImage("imgFolderRequirement", new System.Windows.Size(16, 16)).Source;
						case ArtifactTypeEnum.Task:
							return StaticFuncs.getImage("imgFolderTask", new System.Windows.Size(16, 16)).Source;
						default:
							return StaticFuncs.getImage("imgFolder", new System.Windows.Size(16, 16)).Source;
					}
				else
					switch (this.ArtifactType)
					{
						case ArtifactTypeEnum.Incident:
							return StaticFuncs.getImage("imgIncident", new System.Windows.Size(16, 16)).Source;
						case ArtifactTypeEnum.Requirement:
							return StaticFuncs.getImage("imgRequirement", new System.Windows.Size(16, 16)).Source;
						case ArtifactTypeEnum.Task:
							return StaticFuncs.getImage("imgTask", new System.Windows.Size(16, 16)).Source;
						default:
							return null;
					}
			}
		}

		/// <summary>Sets whether of not this item is a folder e(containing other items of a type) or an actual artifact.</summary>
		public bool ArtifactIsFolder
		{
			get;
			set;
		}

		/// <summary>Used for XAML to determine if the item is in error (late).</summary>
		private bool Style_IsInError
		{
			get
			{
				bool retValue = false;

				if (this.ArtifactTag != null)
				{
					if (this.ArtifactTag.GetType() == typeof(SpiraTeam_Client.RemoteIncident))
					{
						SpiraTeam_Client.RemoteIncident item = (SpiraTeam_Client.RemoteIncident)this.ArtifactTag;
						if ((item.StartDate.HasValue && item.StartDate < DateTime.Now) && item.CompletionPercent == 0)
							retValue = true;
					}
					else if (this.ArtifactTag.GetType() == typeof(SpiraTeam_Client.RemoteTask))
					{
						SpiraTeam_Client.RemoteTask item = (SpiraTeam_Client.RemoteTask)this.ArtifactTag;
						if (item.StartDate.HasValue && item.StartDate < DateTime.Now && item.TaskStatusId == 1)
							retValue = true;
						if (item.EndDate.HasValue && item.EndDate < DateTime.Now)
							retValue = true;
					}
				}

				return retValue;
			}
		}

		/// <summary>Used for XAML to determine if the item is completed.</summary>
		private bool Style_IsCompleted
		{
			get
			{
				bool retValue = false;

				if (this.ArtifactTag != null)
				{
					if (this.ArtifactTag.GetType() == typeof(SpiraTeam_Client.RemoteIncident))
					{
						//HACK: At this time there is no property stating whether the issue is 'closed' or not. So, basing it on the ClosedDate.
						SpiraTeam_Client.RemoteIncident item = (SpiraTeam_Client.RemoteIncident)this.ArtifactTag;
						if (item.ClosedDate < DateTime.Now)
							retValue = true;
					}
					else if (this.ArtifactTag.GetType() == typeof(SpiraTeam_Client.RemoteTask))
					{
						SpiraTeam_Client.RemoteTask item = (SpiraTeam_Client.RemoteTask)this.ArtifactTag;
						if (item.TaskStatusId != 1 && item.TaskStatusId != 2 && item.TaskStatusId != 4 && item.TaskStatusId != 5)
							retValue = true;
					}
					else if (this.ArtifactTag.GetType() == typeof(SpiraTeam_Client.RemoteRequirement))
					{
						SpiraTeam_Client.RemoteRequirement item = (SpiraTeam_Client.RemoteRequirement)this.ArtifactTag;
						if (item.StatusId != 1 && item.StatusId != 2 && item.StatusId != 3 && item.StatusId != 5 && item.StatusId != 7)
							retValue = true;
					}
				}

				return retValue;
			}
		}

		/// <summary>Readonly. Returns a UI element that is used for the tooltip.</summary>
		private UIElement ArtifactTooltip
		{
			get
			{
				UIElement tipReturn = null;

				if (this.ArtifactIsError)
				{
					TextBlock txtMessage = new TextBlock();
					txtMessage.TextWrapping = TextWrapping.Wrap;
					txtMessage.Width = 200;
					txtMessage.Foreground = new SolidColorBrush(Colors.DarkRed);

					if (this.ArtifactTag.GetType() == typeof(Exception))
					{
						txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_General_CommunicationErrorMessage") + Environment.NewLine;
						txtMessage.Text += ((Exception)this.ArtifactTag).Message;
					}
					else
						txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_General_CommunicationErrorMessage");
					tipReturn = txtMessage;
				}
				else if (!this.ArtifactIsFolder)
				#region Individual Artifacts
				{
					switch (this.ArtifactType)
					{
						case ArtifactTypeEnum.Incident:
							tipReturn = new cntlTTipIncident(this);
							break;

						case ArtifactTypeEnum.Requirement:
							tipReturn = new cntlTTipRequirement(this);
							break;

						case ArtifactTypeEnum.Task:
							tipReturn = new cntlTTipTask(this);
							break;
					}
				}
				#endregion
				else
				#region Folder Items
				{
					TextBlock txtMessage = new TextBlock();
					txtMessage.TextWrapping = TextWrapping.Wrap;
					txtMessage.Width = 200;

					//Set the internal textblock to something silly.
					switch (this.ArtifactType)
					{
						case ArtifactTypeEnum.Incident:
							if (this.ArtifactTag == null)
							{
								if (this.ArtifactIsFolderMine)
									txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_Tree_FolderMyIncidents");
								else
									txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_Tree_FolderUnIncidents");
								txtMessage.Text += Environment.NewLine + StaticFuncs.getCultureResource.GetString("app_Tree_FolderHiIncidents");
							}
							else
								txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_Tree_FolderGettingData");
							tipReturn = txtMessage;
							break;

						case ArtifactTypeEnum.Requirement:
							if (this.ArtifactTag == null)
							{
								if (this.ArtifactIsFolderMine)
									txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_Tree_FolderMyRequirements");
								else
									txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_Tree_FolderUnRequirements");
							}
							else
								txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_Tree_FolderGettingData");
							tipReturn = txtMessage;
							break;

						case ArtifactTypeEnum.Task:
							if (this.ArtifactTag == null)
							{
								if (this.ArtifactIsFolderMine)
									txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_Tree_FolderMyTasks");
								else
									txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_Tree_FolderUnTasks");
								txtMessage.Text += Environment.NewLine + StaticFuncs.getCultureResource.GetString("app_Tree_FolderHiTasks");
							}
							else
								txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_Tree_FolderGettingData");
							tipReturn = txtMessage;
							break;

						case ArtifactTypeEnum.None:
							if (this.ArtifactName == StaticFuncs.getCultureResource.GetString("app_Tree_Incidents"))
								txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_Tree_FolderIncidents");
							else if (this.ArtifactName == StaticFuncs.getCultureResource.GetString("app_Tree_Requirements"))
								txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_Tree_FolderRequirements");
							else if (this.ArtifactName == StaticFuncs.getCultureResource.GetString("app_Tree_Tasks"))
								txtMessage.Text = StaticFuncs.getCultureResource.GetString("app_Tree_FolderTasks");
							tipReturn = txtMessage;
							break;

						case ArtifactTypeEnum.Project:
							if (this.ArtifactTag.GetType() == typeof(SpiraProject))
							{
								tipReturn = new cntlTTipProject((SpiraProject)this.ArtifactTag);
							}
							break;
					}
				}
				#endregion
				return tipReturn;
			}
		}

		/// <summary>Readonly. Returns a UIElement for the TreeView header.</summary>
		public UIElement ArtifactHeaderDisplay
		{
			get
			{
				///The containing grid.
				Grid gridHeader = new Grid();
				gridHeader.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(16) });
				gridHeader.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
				gridHeader.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
				gridHeader.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
				gridHeader.Margin = new Thickness(0, 2, 0, 2);

				//The first image, the type.
				Image imgType = new Image();
				imgType.Height = 16;
				imgType.Width = 16;
				imgType.Source = this.ArtifactImageSource;
				gridHeader.Children.Add(imgType);
				Grid.SetRow(imgType, 0);
				Grid.SetColumn(imgType, 0);

				//The second image, if necessary.
				if (this.IsTimed)
				{
					Image imgTimed = StaticFuncs.getImage("imgTaskTime", new System.Windows.Size(16, 16));
					imgTimed.Margin = new Thickness(5, 0, 0, 0);
					gridHeader.Children.Add(imgTimed);
					Grid.SetRow(imgTimed, 0);
					Grid.SetColumn(imgTimed, 1);
				}

				//The name!
				TextBlock txtName = (TextBlock)this.ArtifactDisplayName;
				txtName.Margin = new Thickness(5, 0, 0, 0);
				gridHeader.Children.Add(txtName);
				Grid.SetRow(txtName, 0);
				Grid.SetColumn(txtName, 2);

				//The tooltip:
				GroupBox grpItem = new GroupBox();
				grpItem.Content = this.ArtifactTooltip;
				grpItem.Header = this.ArtifactDisplayName;
				gridHeader.ToolTip = grpItem;


				return gridHeader;
			}
		}

		/// <summary>Readonly. Returns the UIElement that is the context menu.</summary>
		public ContextMenu ArtifactContextMenu
		{
			get
			{
				ContextMenu retMenu = null;
				retMenu = new ContextMenu();
				retMenu.HasDropShadow = true;
				retMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;

				if (!this.ArtifactIsFolder)
				{
					//Create the menu items..
					// - Open in VS.
					MenuItem mnuDetails = new MenuItem();
					mnuDetails.Header = StaticFuncs.getCultureResource.GetString("app_General_ViewDetails");
					mnuDetails.FontWeight = FontWeights.Bold;
					mnuDetails.Click += new RoutedEventHandler(mnuDetails_Click);
					// - Open in Browser
					MenuItem mnuOpenWeb = new MenuItem();
					mnuOpenWeb.Header = StaticFuncs.getCultureResource.GetString("app_General_ViewBrowser");
					mnuOpenWeb.Click += new RoutedEventHandler(mnuOpenWeb_Click);
					// - Start (or End) Work
					MenuItem mnuStartWork = new MenuItem();
					mnuStartWork.Header = ((this._isTimed) ? StaticFuncs.getCultureResource.GetString("app_General_StopTimer") : StaticFuncs.getCultureResource.GetString("app_General_StartTimer"));
					mnuStartWork.Click += new RoutedEventHandler(mnuStartWork_Click);
					// - Copy ID to Clipboard
					MenuItem mnuCopyHead = new MenuItem();
					mnuCopyHead.Header = StaticFuncs.getCultureResource.GetString("app_General_CopyToClipboard");
					mnuCopyHead.Click += new RoutedEventHandler(mnuCopyHead_Click);

					//Add to the context..
					retMenu.Items.Add(mnuDetails);
					retMenu.Items.Add(mnuOpenWeb);
					retMenu.Items.Add(new Separator());
					if (this.ArtifactType != ArtifactTypeEnum.Requirement) retMenu.Items.Add(mnuStartWork);
					retMenu.Items.Add(mnuCopyHead);
				}
				else
				{
					//Create the menu items..
					// - Refresh
					MenuItem mnuRefresh = new MenuItem();
					mnuRefresh.Header = "_Refresh List";
					mnuRefresh.Click += new RoutedEventHandler(mnuRefresh_Click);
					// - Open in Browser
					MenuItem mnuOpenWeb = new MenuItem();
					mnuOpenWeb.Header = "Open in _Browser";
					mnuOpenWeb.Click += new RoutedEventHandler(mnuOpenWeb_Click);

					retMenu.Items.Add(mnuRefresh);
					if (this.ArtifactType == ArtifactTypeEnum.Project)
					{
						retMenu.Items.Add(new Separator());
						retMenu.Items.Add(mnuOpenWeb);
					}
				}

				return retMenu;
			}
		}

		public string ArtifactIDDisplay
		{
			get
			{
				if (!this.ArtifactIsFolder)
				{
					switch (this.ArtifactType)
					{
						case ArtifactTypeEnum.Incident:
							return "[IN:" + this.ArtifactId.ToString() + "]";
						case ArtifactTypeEnum.Requirement:
							return "[RQ:" + this.ArtifactId.ToString() + "]";
						case ArtifactTypeEnum.Task:
							return "[TK:" + this.ArtifactId.ToString() + "]";
					}
				}
				return null;
			}
		}

		/// <summary>Whether or not this specific treeview item is in an error state.</summary>
		public bool ArtifactIsError
		{
			get;
			set;
		}

		/// <summary>Whether or not this is an 'empty' error message.</summary>
		public bool ArtifactIsNo
		{
			get;
			set;
		}

		/// <summary>Whether or not the user opened this treenode up.</summary>
		public bool IsExpanded
		{
			get;
			set;
		}

		/// <summary>Sets wither or not the folder is containing 'my' items, or 'unassigned' items.</summary>
		public bool ArtifactIsFolderMine
		{
			get;
			set;
		}
		#endregion

		#region Context Menu Events
		/// <summary>Hit when the user wants to refresh a treenode.</summary>
		/// <param name="sender">MenuItem</param>
		/// <param name="e">RoutedEventArgs</param>
		private void mnuRefresh_Click(object sender, RoutedEventArgs e)
		{
			//Refresh the display..
			e.Handled = true;

			//TODO: Act like they pressed refresh button on this item.
			if (this._method != null)
				this._method(this);
		}

		/// <summary>Hit when the user wants to start or stop work.</summary>
		/// <param name="sender">MenuItem</param>
		/// <param name="e">RoutedEventArgs</param>
		private void mnuStartWork_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			this.IsTimed = !this._isTimed;
		}

		/// <summary>Hit when the user selects to launch the URL in the browser.</summary>
		/// <param name="sender">MenuItem</param>
		/// <param name="e">RoutedEventArgs</param>
		private void mnuOpenWeb_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			//We need to get the URL, and then launch it.
			string strUrl = ((SpiraProject)this.ArtifactParentProject.ArtifactTag).ServerURL.ToString();

			Business.SpiraTeam_Client.ImportExportClient client = StaticFuncs.CreateClient(strUrl);

			if (this.ArtifactType != ArtifactTypeEnum.None)
			{
				string strArtUrl = client.System_GetArtifactUrl((int)this.ArtifactType, this.ArtifactParentProject.ArtifactId, this.ArtifactId, null);

				//In case the API hasn't been updated to return the full URL..
				if (strArtUrl.StartsWith("~"))
					strUrl = strArtUrl.Replace("~", strUrl);

				try
				{
					System.Diagnostics.Process.Start(strUrl);
				}
				catch (Exception ex)
				{
					Logger.LogMessage(ex);
					MessageBox.Show("Error launching browser.", "Launch URL", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
		}

		/// <summary>Hit when the user wants to open the details screen of an item.</summary>
		/// <param name="sender">MenuItem</param>
		/// <param name="e">RoutedEventArgs</param>
		private void mnuDetails_Click(object sender, RoutedEventArgs e)
		{
			//This is the same as double-clicking.
			//TODO: Fire off a details window.
			e.Handled = true;

			this.DetailsOpenRequested(this, new EventArgs());
		}

		/// <summary>Hit when the user wants to copy the artifact ID to the clipboard.</summary>
		/// <param name="sender">menuItem</param>
		/// <param name="e">RoutedEventArgs</param>
		private void mnuCopyHead_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			Clipboard.SetText(this.ArtifactIDDisplay);
		}

		#endregion

		#region Timing Properties
		/// <summary>Whether or not the item is being timed or not.</summary>
		public bool IsTimed
		{
			get
			{
				bool retValue = this._isTimed;
				//See if any children are being timed. If children are, this is inherited.
				if (!retValue)
				{
					foreach (TreeViewArtifact childItem in this.Items)
					{
						if (childItem.IsTimed)
							retValue = true;
					}
				}

				return retValue;
			}
			set
			{
				if (this.ArtifactType != ArtifactTypeEnum.Requirement) //Requirements can't be worked on.
				{
					if (value != this._isTimed)
					{
						if (value)
						{
							//They're starting the timer, set the starttime.
							this._startTime = DateTime.Now;
						}
						else
						{
							//They're ending the timer, set the finish time.
							this._endTime = DateTime.Now;
						}
					}
					this._isTimed = value;

					this.WorkTimerChanged(this, new EventArgs());
				}
			}
		}

		/// <summary>Readonly. The time work started on the item.</summary>
		public DateTime StartWorkTime
		{
			get
			{
				return this._startTime;
			}
		}

		/// <summary>Readonly. The time work ended on the item.</summary>
		public DateTime EndWorkTime
		{
			get
			{
				return this._endTime;
			}
		}

		/// <summary>Readonly. The amount of time worked since the last timer start.</summary>
		public TimeSpan WorkTime
		{
			get
			{
				TimeSpan retSpan = new TimeSpan(0);

				if (this._startTime != null)
				{
					if (this._endTime != null)
					{
						if (this._startTime <= this._endTime)
						{
							retSpan = this._endTime - this._startTime;
						}
					}
					else
					{
						if (this._startTime <= DateTime.Now)
						{
							retSpan = DateTime.Now - this._startTime;
						}
					}
				}

				return retSpan;
			}
		}
		#endregion

		/// <summary>Available types of TreeNodes.</summary>
		public enum ArtifactTypeEnum
		{
			None = 0,
			Task = 6,
			Incident = 3,
			Requirement = 1,
			Project = -4
		}
	}
}
