using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Controls;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms
{
	/// <summary>
	/// Interaction logic for wpfDetailsIncident.xaml
	/// </summary>
	public partial class frmDetailsRequirement : UserControl
	{
		private const string CLASS = "frmDetailsIncident:";

		#region Private Data-Changed Vars
		private bool _isDescChanged;
		private bool _isResChanged;
		private bool _isFieldChanged;
		#endregion
		#region Private Mode Vars
		private bool _isLoadingInformation;
		#endregion

		private TreeViewArtifact _ArtifactDetails;

		#region Class Initializers
		/// <summary>Creates a new instance of our IncidentDetailsForm.</summary>
		public frmDetailsRequirement()
		{
			try
			{
				InitializeComponent();

				//Load images needed..
				this.imgLoadingIncident.Source = StaticFuncs.getImage("imgInfoWPF", new Size(48, 48)).Source;
				this.imgSavingIncident.Source = StaticFuncs.getImage("imgSaveWPF", new Size(48, 48)).Source;
				this.imgLoadingError.Source = StaticFuncs.getImage("imgErrorWPF", new Size(48, 48)).Source;
				//Load strings needed..
				this.toolTxtSave.Text = StaticFuncs.getCultureResource.GetString("app_General_Save");
				this.toolTxtRefresh.Text = StaticFuncs.getCultureResource.GetString("app_General_Refresh");
				this.toolTxtLoadWeb.Text = StaticFuncs.getCultureResource.GetString("app_General_ViewBrowser");
				this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Requirement_Loading");
				this.lblSavingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Requirement_Saving");
				this.btnRetryLoad.Content = StaticFuncs.getCultureResource.GetString("app_General_ButtonRetry");
				this.lblLoadingError.Text = StaticFuncs.getCultureResource.GetString("app_General_TalkingToServerErrorMessage");
				this.lblExpanderDetails.Text = StaticFuncs.getCultureResource.GetString("app_General_ExpanderDetails");
				this.lblName.Text = StaticFuncs.getCultureResource.GetString("app_General_Name") + ":";
				this.lblTxtToken.Text = StaticFuncs.getCultureResource.GetString("app_General_CopyToClipboard");
				this.lblStatus.Text = StaticFuncs.getCultureResource.GetString("app_General_Status") + ":";
				this.lblCreatedBy.Text = StaticFuncs.getCultureResource.GetString("app_Requirement_CreatedBy") + ":";
				this.lblOwnedBy.Text = StaticFuncs.getCultureResource.GetString("app_General_OwnedBy") + ":";
				this.lblImportance.Text = StaticFuncs.getCultureResource.GetString("app_General_Priority") + ":";
				this.lblRelease.Text = StaticFuncs.getCultureResource.GetString("app_General_AssociatedRequirement") + ":";
				this.lblDescription.Text = StaticFuncs.getCultureResource.GetString("app_General_Description") + ":";
				this.lblExpanderResolution.Text = StaticFuncs.getCultureResource.GetString("app_General_ExpanderResolution");
				this.lblPlnEffort.Text = StaticFuncs.getCultureResource.GetString("app_General_EstEffort") + ":";
				this.lblHours.Text = StaticFuncs.getCultureResource.GetString("app_General_Hours");
				this.lblMins.Text = StaticFuncs.getCultureResource.GetString("app_General_Minutes");
				this.lblExpanderTasks.Text = StaticFuncs.getCultureResource.GetString("app_Requirement_ExpanderTask");
				//this.lblExpanderCustom.Text = StaticFuncs.getCultureResource.GetString("app_General_ExpanderCustom");
				this.lblExpanderAttachments.Text = StaticFuncs.getCultureResource.GetString("app_General_Attachments");
				this.lblAddNewResolution.Text = StaticFuncs.getCultureResource.GetString("app_General_AddNewComment") + ":";
				this.btnConcurrencyMergeNo.Content = StaticFuncs.getCultureResource.GetString("app_General_Refresh");
				this.btnConcurrencyMergeYes.Content = StaticFuncs.getCultureResource.GetString("app_General_Merge");
				this.lblMergeConcurrency.Text = StaticFuncs.getCultureResource.GetString("app_Requirement_AskMergeConcurrency");
				this.btnConcurrencyRefresh.Content = StaticFuncs.getCultureResource.GetString("app_General_Refresh");
				this.lblNoMergeConcurrency.Text = StaticFuncs.getCultureResource.GetString("app_Requirement_NoMergeConcurrency");

				//Load fixed-option dropdowns.
				// -- Importance
				for (int j = 0; j <= 4; j++)
				{
					//HACK: We use the same values as the Task Priority.
					RequirementPriority newImp = new RequirementPriority();
					newImp.PriorityId = ((j == 0) ? new int?() : j);
					switch (j)
					{
						case 0:
							newImp.Name = StaticFuncs.getCultureResource.GetString("app_Task_Priority_0");
							break;
						case 1:
							newImp.Name = StaticFuncs.getCultureResource.GetString("app_Task_Priority_1");
							break;
						case 2:
							newImp.Name = StaticFuncs.getCultureResource.GetString("app_Task_Priority_2");
							break;
						case 3:
							newImp.Name = StaticFuncs.getCultureResource.GetString("app_Task_Priority_3");
							break;
						case 4:
							newImp.Name = StaticFuncs.getCultureResource.GetString("app_Task_Priority_4");
							break;
					}
					this.cntrlImportance.Items.Add(newImp);
				}
				this.cntrlImportance.SelectedIndex = 0;
				// -- Status
				for (int i = 1; i <= 8; i++)
				{
					RequirementStatus newStatus = new RequirementStatus();
					newStatus.StatusId = i;
					switch (i)
					{
						case 1:
							newStatus.Name = StaticFuncs.getCultureResource.GetString("app_Requirement_Status_Requested");
							break;
						case 2:
							newStatus.Name = StaticFuncs.getCultureResource.GetString("app_Requirement_Status_Planned");
							break;
						case 3:
							newStatus.Name = StaticFuncs.getCultureResource.GetString("app_Requirement_Status_InProgress");
							break;
						case 4:
							newStatus.Name = StaticFuncs.getCultureResource.GetString("app_Requirement_Status_Completed");
							break;
						case 5:
							newStatus.Name = StaticFuncs.getCultureResource.GetString("app_Requirement_Status_Accepted");
							break;
						case 6:
							newStatus.Name = StaticFuncs.getCultureResource.GetString("app_Requirement_Status_Rejected");
							break;
						case 7:
							newStatus.Name = StaticFuncs.getCultureResource.GetString("app_Requirement_Status_Evaluated");
							break;
						case 8:
							newStatus.Name = StaticFuncs.getCultureResource.GetString("app_Requirement_Status_Obsolete");
							break;
					}
					this.cntrlStatus.Items.Add(newStatus);
				}
				this.cntrlStatus.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, ".ctor()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public frmDetailsRequirement(ToolWindowPane ParentWindow)
			: this()
		{
			try
			{
				this.ParentWindowPane = ParentWindow;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, ".ctor()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public frmDetailsRequirement(TreeViewArtifact artifactDetails, ToolWindowPane parentWindow)
			: this(parentWindow)
		{
			try
			{
				this.ArtifactDetail = artifactDetails;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, ".ctor()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#endregion

		#region Control Event Handlers

		/// <summary>Hit when a textbox or dropdown list changes.</summary>
		/// <param name="sender">cntrlIncidentName, cntrlDetectedBy, cntrlOwnedBy, cntrlPriority, cntrlSeverity, cntrlDetectedIn, cntrlResolvedIn, cntrlVerifiedIn, cntrlDescription</param>
		/// <param name="e"></param>
		private void _cntrl_TextChanged(object sender, EventArgs e)
		{
			try
			{
				if (!this.IsLoading)
				{
					this.display_SetWindowChanged(true);
					this._isFieldChanged = true;

					if (sender is cntrlRichTextEditor)
					{
						if (((cntrlRichTextEditor)sender).Name == "cntrlDescription")
						{
							this._isDescChanged = true;
						}
						else if (((cntrlRichTextEditor)sender).Name == "cntrlResolution")
						{
							this._isResChanged = true;
						}
					}

					//Unset required error backgrounds.
					Control contField = sender as Control;
					if (contField != null)
					{
						if (contField.Tag is string)  //Normal Field
						{
							contField.Tag = null;
						}
						else if (contField.Tag is RemoteCustomProperty)  //Custom Property
						{
							contField.Style = (Style)this.FindResource("PaddedControl");
						}

						if (contField is cntrlRichTextEditor) //Description & Resolution
						{
							if (((cntrlRichTextEditor)sender).Name == "cntrlDescription")
							{
								this.grpDescription.Tag = null;
							}
							else if (((cntrlRichTextEditor)sender).Name == "cntrlResolution")
							{
								this.grpResolution.Tag = null;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "_cntrl_TextChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when a toolbar button's Enabled property is changed, to 'grey' out the button images.</summary>
		/// <param name="sender">UIElement</param>
		/// <param name="e">DependencyPropertyChangedEventArgs</param>
		private void toolButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				UIElement control = sender as UIElement;
				if (control != null)
					control.Opacity = ((control.IsEnabled) ? 1 : .5);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "toolButton_IsEnabledChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when a toolbar is loaded. Hides the overflow arrow.</summary>
		/// <param name="sender">ToolBar</param>
		/// <param name="e">RoutedEventArgsparam>
		private void _toolbar_Loaded(object sender, EventArgs e)
		{
			try
			{
				ToolBar toolBar = sender as ToolBar;
				var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
				if (overflowGrid != null)
				{
					overflowGrid.Visibility = Visibility.Collapsed;
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "_toolbar_Loaded()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the fadeout is complete.</summary>
		/// <param name="sender">DoubleAnimation</param>
		/// <param name="e">EventArgs</param>
		private void animFadeOut_Completed(object sender, EventArgs e)
		{
			string METHOD = CLASS + "animFadeOut_Completed()";
			Logger.LogTrace_EnterMethod(METHOD);

			try
			{
				//Try to get the name..
				string contName = ((AnimationClock)sender).Timeline.Name;
				if (!string.IsNullOrWhiteSpace(contName))
				{
					Logger.LogTrace("Fading out on panel '" + contName + "' done.");
					object cont = this.FindName(contName);
					if (cont != null && cont is Panel)
					{
						((Panel)cont).Visibility = System.Windows.Visibility.Collapsed;
					}
					else
					{
						Logger.LogTrace("Fading out on panel '" + contName + "' done, but panel couldn't be found.");
						this.panelStatus.Visibility = System.Windows.Visibility.Collapsed;
						this.panelError.Visibility = System.Windows.Visibility.Collapsed;
						this.panelSaving.Visibility = System.Windows.Visibility.Collapsed;
					}
				}
				else
				{
					Logger.LogTrace("Fading out done, but animation had no name.");
					this.panelStatus.Visibility = System.Windows.Visibility.Collapsed;
					this.panelError.Visibility = System.Windows.Visibility.Collapsed;
					this.panelSaving.Visibility = System.Windows.Visibility.Collapsed;
				}
			}
			catch (Exception ex)
			{
				//Error occurred, clear them all.
				Logger.LogMessage(ex, METHOD);
				this.panelStatus.Visibility = System.Windows.Visibility.Collapsed;
				this.panelError.Visibility = System.Windows.Visibility.Collapsed;
				this.panelSaving.Visibility = System.Windows.Visibility.Collapsed;
			}

			Logger.LogTrace_ExitMethod(METHOD);
		}

		/// <summary>Hit when a masked text box's text is changed.</summary>
		/// <param name="sender">MaskedTextBox</param>
		/// <param name="e">TextChangedEventArgs</param>
		private void cntrlMasked_TextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				//Simply call the real one.
				this._cntrl_TextChanged(sender, e);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "cntrlMasked_TextChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the View in Web button is clicked.</summary>
		/// <param name="sender">btnLoadWeb</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnLoadWeb_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			//Fire off the url.
			try
			{
				System.Diagnostics.Process.Start(this._RequirementUrl);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "Error launching URL: " + this._RequirementUrl);
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_ErrorLaunchingUrlMessage"), StaticFuncs.getCultureResource.GetString("app_General_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user clicks the Refresh button.</summary>
		/// <param name="sender">btRefresh</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				e.Handled = true;

				MessageBoxResult isUserSure = MessageBoxResult.Yes;
				if (this._isDescChanged || this._isFieldChanged || this._isResChanged)
				{
					isUserSure = MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_LoseChangesMessage"), StaticFuncs.getCultureResource.GetString("app_General_AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
				}

				if (isUserSure == MessageBoxResult.Yes)
				{
					//User is sure, change the label, and launch the refresh.
					this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Requirement_Refreshing");
					this.load_LoadItem();
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnRefresh_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user want to copy the token ID to the clipboard.</summary>
		/// <param name="sender">TextBlock</param>
		/// <param name="e">MouseButtonEventArgs</param>
		private void lblToken_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			try
			{
				e.Handled = true;

				Clipboard.SetText(this._ArtifactDetails.ArtifactIDDisplay);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "lblToken_MouseDown()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when a HyperLink object is clicked.</summary>
		/// <param name="sender">Hyperlink</param>
		/// <param name="e">RoutedEventArgs</param>
		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			if (sender is Hyperlink)
			{
				try
				{
					System.Diagnostics.Process.Start(((Hyperlink)sender).NavigateUri.ToString());
				}
				catch (Exception ex)
				{
					Logger.LogMessage(ex, "Could not launch URL.");
					MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_ErrorLaunchingUrlMessage"), StaticFuncs.getCultureResource.GetString("app_General_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		/// <summary>Hit when the user wants to open up a related Task</summary>
		/// <param name="sender">Hyperlink</param>
		/// <param name="e">RoutedEventArgs</param>
		private void Hyperlink_OpenTask_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Hyperlink link = sender as Hyperlink;
				if (link != null)
				{
					TreeViewArtifact taskArt = link.Tag as TreeViewArtifact;
					if (taskArt != null)
					{
						((SpiraExplorerPackage)this.ParentWindowPane.Package).OpenDetailsToolWindow(taskArt);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "Hyperlink_OpenTask_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#endregion

		#region Properties
		/// <summary>The parent windowframe of the control, for accessing window settings.</summary>
		public ToolWindowPane ParentWindowPane
		{
			get;
			set;
		}

		/// <summary>This specifies whether or not we are in the process of loading data for display.</summary>
		private bool IsLoading
		{
			get
			{
				return this._isLoadingInformation;
			}
			set
			{
				try
				{
					if (this._isLoadingInformation != value)
					{
						if (value)
						{
							this.display_SetOverlayWindow(this.panelStatus, Visibility.Visible);
						}
						else
						{
							this.barLoadingReq.Value = 1;
							this.display_SetOverlayWindow(this.panelStatus, Visibility.Hidden);
						}

						this._isLoadingInformation = value;
					}
				}
				catch (Exception ex)
				{
					Logger.LogMessage(ex, "IsLoading.Set");
					MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		/// <summary>This specifies whether or not we are in the process of saving data.</summary>
		private bool IsSaving
		{
			get
			{
				return this._isSavingInformation;
			}
			set
			{
				if (this._isSavingInformation != value)
				{
					if (value)
					{
						this.display_SetOverlayWindow(this.panelSaving, Visibility.Visible);
					}
					else
					{
						this.barLoadingReq.Value = 1;
						this.display_SetOverlayWindow(this.panelSaving, Visibility.Hidden);
					}

					this._isSavingInformation = value;
				}
			}
		}

		/// <summary>Returns the string that it to be displayed in the docked tab.</summary>
		public string TabTitle
		{
			get
			{
				try
				{
					if (this._ArtifactDetails != null)
						return this._ArtifactDetails.ArtifactName + " " + this._ArtifactDetails.ArtifactIDDisplay;
					else
						return "";
				}
				catch (Exception ex)
				{
					Logger.LogMessage(ex, "TabTitle.Get");
					MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
					return "";
				}
			}
		}

		/// <summary>The detail item for this display.</summary>
		public TreeViewArtifact ArtifactDetail
		{
			get
			{
				return this._ArtifactDetails;
			}
			set
			{
				try
				{
					//See if they've made any changes..
					this._ArtifactDetails = value;
					this._Project = value.ArtifactParentProject.ArtifactTag as SpiraProject;

					//Set tab title.
					if (this.ParentWindowPane != null)
						this.ParentWindowPane.Caption = this.TabTitle;

					//Load details.
					this.load_LoadItem();
				}
				catch (Exception ex)
				{
					Logger.LogMessage(ex, "ArtifactDetail.Set");
					MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		/// <summary>Whether or not this artifact has unsaved changes.</summary>
		public bool IsUnsaved
		{
			get
			{
				return (this._isDescChanged || this._isFieldChanged || this._isResChanged);
			}
		}

		/// <summary>Whether or not this details screen is currently hidden.</summary>
		public bool IsHidden
		{
			get;
			set;
		}
		#endregion

		/// <summary>Shows the error panel, with the appropriate message.</summary>
		/// <param name="Message">Optional Message to show.</param>
		private void display_ShowErrorPanel(string Message = null)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(Message))
					this.lblLoadingError.Text = Message;

				//Display the error panel.
				this.gridSavingConcurrencyMerge.Visibility = System.Windows.Visibility.Collapsed;
				this.gridSavingConcurrencyNoMerge.Visibility = System.Windows.Visibility.Collapsed;
				this.gridLoadingError.Visibility = System.Windows.Visibility.Visible;
				this.display_SetOverlayWindow(this.panelStatus, System.Windows.Visibility.Collapsed);
				this.display_SetOverlayWindow(this.panelError, System.Windows.Visibility.Visible);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "display_ShowErrorPanel()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Use to show or hide the Status Window.</summary>
		/// <param name="visiblity">The visibility of the window.</param>
		private void display_SetOverlayWindow(Grid Panel, Visibility visiblity)
		{
			string METHOD = CLASS + "display_SetOverlayWindow()";
			Logger.LogTrace_EnterMethod(METHOD + "  For panel: '" + Panel.Name + "'; Visibility: '" + visiblity.ToString() + "'");

			try
			{
				//Fade in or out the status window...
				switch (visiblity)
				{
					case System.Windows.Visibility.Visible:
						//Set initial values..
						Panel.Opacity = 0;
						Panel.Visibility = System.Windows.Visibility.Visible;

						Storyboard storyFadeIn = new Storyboard();
						DoubleAnimation animFadeIn = new DoubleAnimation(0, 1, new TimeSpan(0, 0, 0, 0, 150));
						Storyboard.SetTarget(animFadeIn, Panel);
						Storyboard.SetTargetProperty(animFadeIn, new PropertyPath(Control.OpacityProperty));
						storyFadeIn.Name = Panel.Name;
						storyFadeIn.Children.Add(animFadeIn);

						//Start the animation.
						storyFadeIn.Begin();

						break;

					case System.Windows.Visibility.Collapsed:
					case System.Windows.Visibility.Hidden:
					default:
						//Fade it out.
						Storyboard storyFadeOut = new Storyboard();
						DoubleAnimation animFadeOut = new DoubleAnimation(1, 0, new TimeSpan(0, 0, 0, 0, 250));
						Storyboard.SetTarget(animFadeOut, Panel);
						Storyboard.SetTargetProperty(animFadeOut, new PropertyPath(Control.OpacityProperty));
						animFadeOut.Name = Panel.Name;
						animFadeOut.Completed += animFadeOut_Completed;
						storyFadeOut.Children.Add(animFadeOut);

						//Start the animation.
						storyFadeOut.Begin();

						break;
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, METHOD);
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
			Logger.LogTrace_ExitMethod(METHOD);
		}

		/// <summary>Sets whether the window has changed or not.</summary>
		/// <param name="IsChanged">True for if fields are changed, false if not.</param>
		private void display_SetWindowChanged(bool IsChanged = true)
		{
			try
			{
				if (IsChanged)
				{
					if (this.ParentWindowPane != null)
					{
						if (this.ParentWindowPane.Frame != null) ((IVsWindowFrame)this.ParentWindowPane.Frame).SetProperty((int)__VSFPROPID2.VSFPROPID_OverrideDirtyState, true);
						if (!this.ParentWindowPane.Caption.EndsWith("*"))
							this.ParentWindowPane.Caption = this.ParentWindowPane.Caption + " *";
						if (this.btnSave != null) this.btnSave.IsEnabled = true;
					}
				}
				else
				{
					if (this.ParentWindowPane != null)
					{
						if (this.ParentWindowPane.Frame != null) ((IVsWindowFrame)this.ParentWindowPane.Frame).SetProperty((int)__VSFPROPID2.VSFPROPID_OverrideDirtyState, false);
						if (this.ParentWindowPane.Caption.EndsWith("*"))
							this.ParentWindowPane.Caption = this.ParentWindowPane.Caption.Trim(new char[] { ' ', '*' });
						if (this.btnSave != null) this.btnSave.IsEnabled = false;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "display_SetWindowChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Class to hold the fixed Priority types.</summary>
		private class RequirementPriority
		{
			public int? PriorityId
			{
				get;
				set;
			}

			public string Name
			{
				get;
				set;
			}
		}

		/// <summary>Class to hold the fixed Status types.</summary>
		private class RequirementStatus
		{
			public int? StatusId
			{
				get;
				set;
			}
			public string Name
			{
				get;
				set;
			}
		}

		/// <summary>Got a save command from an external source.</summary>
		public void ExternalSave()
		{
			try
			{
				this.btnSave_Click(this.btnSave, null);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "ExternalSave()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

	}
}