extern alias Shell10;
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
	public partial class frmDetailsIncident : UserControl
	{
		private const string CLASS = "frmDetailsIncident:";

		#region Private Data-Changed Vars
		private bool _isDescChanged;
		private bool _isResChanged;
		private bool _isFieldChanged;
		private bool _isWkfChanged;
		#endregion
		#region Private Mode Vars
		private bool _isLoadingInformation;
		private bool _isWorkflowChanging;
		#endregion

		private TreeViewArtifact _ArtifactDetails;

		#region Class Initializers
		/// <summary>Creates a new instance of our IncidentDetailsForm.</summary>
		public frmDetailsIncident()
		{
			try
			{
				InitializeComponent();
				//Temp set colors?
				//this.Foreground = Shell10.Microsoft.VisualStudio.Shell.VsBrushes.ToolboxSelectedHeadingMiddle2Key;
				//this.Background = VsBrushes.ToolWindowBackgroundKey;

				//Set custom props widts..
				this.cntCustomProps.SetColNumWidth(2, new GridLength(6, GridUnitType.Star));
				this.cntCustomProps.SetColNumWidth(4, new GridLength(6, GridUnitType.Star));

				//Load images needed..
				this.imgLoadingIncident.Source = StaticFuncs.getImage("imgInfoWPF", new Size(48, 48)).Source;
				this.imgSavingIncident.Source = StaticFuncs.getImage("imgSaveWPF", new Size(48, 48)).Source;
				this.imgLoadingError.Source = StaticFuncs.getImage("imgErrorWPF", new Size(48, 48)).Source;
				//Load strings needed..
				this.toolTxtSave.Text = StaticFuncs.getCultureResource.GetString("app_General_Save");
				this.toolTxtRefresh.Text = StaticFuncs.getCultureResource.GetString("app_General_Refresh");
				this.toolTxtLoadWeb.Text = StaticFuncs.getCultureResource.GetString("app_General_ViewBrowser");
				this.toolTxtTimer.Text = StaticFuncs.getCultureResource.GetString("app_General_StartTimer");
				this.mnuTxtActions.Text = StaticFuncs.getCultureResource.GetString("app_Incident_StatusActions") + ":";
				this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Loading");
				this.lblSavingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Saving");
				this.lblExpanderDetails.Text = StaticFuncs.getCultureResource.GetString("app_General_ExpanderDetails");
				this.btnRetryLoad.Content = StaticFuncs.getCultureResource.GetString("app_General_ButtonRetry");
				this.lblLoadingError.Text = StaticFuncs.getCultureResource.GetString("app_General_TalkingToServerErrorMessage");
				this.lblName.Text = StaticFuncs.getCultureResource.GetString("app_General_Name") + ":";
				this.lblTxtToken.Text = StaticFuncs.getCultureResource.GetString("app_General_CopyToClipboard");
				this.lblType.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Type") + ":";
				this.lblStatus.Text = StaticFuncs.getCultureResource.GetString("app_General_Status") + ":";
				this.lblDetectedBy.Text = StaticFuncs.getCultureResource.GetString("app_Incident_DetectedBy") + ":";
				this.lblOwnedBy.Text = StaticFuncs.getCultureResource.GetString("app_General_OwnedBy") + ":";
				this.lblPriority.Text = StaticFuncs.getCultureResource.GetString("app_General_Priority") + ":";
				this.lblSeverity.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Severity") + ":";
				this.lblDetectedIn.Text = StaticFuncs.getCultureResource.GetString("app_Incident_DetectedRelease") + ":";
				this.lblResolvedIn.Text = StaticFuncs.getCultureResource.GetString("app_Incident_ResolvedRelease") + ":";
				this.lblVerifiedIn.Text = StaticFuncs.getCultureResource.GetString("app_Incident_VerifiedRelease") + ":";
				this.lblDescription.Text = StaticFuncs.getCultureResource.GetString("app_General_Description") + ":";
				this.lblExpanderResolution.Text = StaticFuncs.getCultureResource.GetString("app_General_ExpanderResolution");
				this.lblExpanderSchedule.Text = StaticFuncs.getCultureResource.GetString("app_General_ExpanderSchedule");
				this.lblPerComplete.Text = StaticFuncs.getCultureResource.GetString("app_General_PerComplete") + ":";
				this.lblStartDate.Text = StaticFuncs.getCultureResource.GetString("app_General_StartDate") + ":";
				this.lblEndDate.Text = StaticFuncs.getCultureResource.GetString("app_General_EndDate") + ":";
				this.lblEstEffort.Text = StaticFuncs.getCultureResource.GetString("app_General_EstEffort") + ":";
				this.lblHours4.Text = StaticFuncs.getCultureResource.GetString("app_General_Hours");
				this.lblMins4.Text = StaticFuncs.getCultureResource.GetString("app_General_Minutes");
				this.lblProjEffort.Text = StaticFuncs.getCultureResource.GetString("app_General_ProjEffort") + ":";
				this.lblHours1.Text = StaticFuncs.getCultureResource.GetString("app_General_Hours");
				this.lblMins1.Text = StaticFuncs.getCultureResource.GetString("app_General_Minutes");
				this.lblActEffort.Text = StaticFuncs.getCultureResource.GetString("app_General_ActEffort") + ":";
				this.lblHours2.Text = StaticFuncs.getCultureResource.GetString("app_General_Hours");
				this.lblMins2.Text = StaticFuncs.getCultureResource.GetString("app_General_Minutes");
				this.lblRemEffort.Text = StaticFuncs.getCultureResource.GetString("app_General_RemEffort") + ":";
				this.lblHours3.Text = StaticFuncs.getCultureResource.GetString("app_General_Hours");
				this.lblMins3.Text = StaticFuncs.getCultureResource.GetString("app_General_Minutes");
				//this.lblExpanderCustom.Text = StaticFuncs.getCultureResource.GetString("app_General_ExpanderCustom");
				this.lblExpanderAttachments.Text = StaticFuncs.getCultureResource.GetString("app_General_Attachments");
				this.lblAddNewResolution.Text = StaticFuncs.getCultureResource.GetString("app_General_AddNewComment") + ":";
				this.btnConcurrencyMergeNo.Content = StaticFuncs.getCultureResource.GetString("app_General_Refresh");
				this.btnConcurrencyMergeYes.Content = StaticFuncs.getCultureResource.GetString("app_General_Merge");
				this.lblMergeConcurrency.Text = StaticFuncs.getCultureResource.GetString("app_Incident_AskMergeConcurrency");
				this.btnConcurrencyRefresh.Content = StaticFuncs.getCultureResource.GetString("app_General_Refresh");
				this.lblNoMergeConcurrency.Text = StaticFuncs.getCultureResource.GetString("app_Incident_NoMergeConcurrency");

				//Load set vars..
				this._WorkflowFields = this.workflow_GenerateStandardFields();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "InitializeComponent()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public frmDetailsIncident(ToolWindowPane ParentWindow)
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

		public frmDetailsIncident(TreeViewArtifact artifactDetails, ToolWindowPane parentWindow)
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

		/// <summary>Hit when the worktimer is changed from another source.</summary>
		/// <param name="sender">TreeViewArtifact</param>
		/// <param name="e">EventArgs</param>
		private void _ArtifactDetails_WorkTimerChanged(object sender, EventArgs e)
		{
			try
			{
				this.btnStartStopTimer.IsChecked = this._ArtifactDetails.IsTimed;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "_ArtifactDetails_WorkTimerChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the Type or Status dropdown is changed. Have to reload workflow and update fields.</summary>
		/// <param name="sender">cntrlType / cntrlStatus</param>
		/// <param name="e">Event Args</param>
		private void _cntrlType_Changed(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				if (!this.IsLoading && !this._isWorkflowChanging)
				{
					this._isWorkflowChanging = true;
					//Show the overlay.
					this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Incident_LoadingWorkflow");
					this.barLoadingIncident.Value = 0;
					this.barLoadingIncident.Maximum = 4;
					this.display_SetOverlayWindow(this.panelStatus, Visibility.Visible);

					//See if they want to or need to confirm..
					MessageBoxResult areTheySure = MessageBoxResult.Yes;
					if (this._isFieldChanged || this._isDescChanged || this._isResChanged)  //Not checking workflow changed.
						areTheySure = MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_WorkflowResetFieldsMessage"), StaticFuncs.getCultureResource.GetString("app_General_AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

					if (areTheySure == MessageBoxResult.Yes)
					{
						//Update window title to indicate there's a change.
						if (((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId.Value != this._Incident.IncidentTypeId.Value)
						{
							this.display_SetWindowChanged(true);
							this._isWkfChanged = true;
						}
						else
						{
							bool isChanged = (this._isDescChanged || this._isResChanged || this._isFieldChanged); //Not checking workflow changed.
							this.display_SetWindowChanged(isChanged);
							this._isWkfChanged = isChanged;
						}

						//Set the selected item..
						if (this.cntrlType.SelectedItem is RemoteIncidentType)
						{
							this._IncSelectedType = ((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId.Value;

							//Reset data and then reset combobox.
							this.loadItem_DisplayInformation(this._Incident);
							this.workflow_ClearAllRequiredHighlights();
							this.loadItem_PopulateType(this.cntrlType, this._IncSelectedType);

							//Update workflow fields..
							this.workflow_ChangeWorkflowStep();
						}
					}
					else
					{
						//Reset drop-down..
						this.loadItem_PopulateType(this.cntrlType, this._Incident.IncidentTypeId.Value);
						this.display_SetOverlayWindow(this.panelStatus, Visibility.Collapsed);
					}
					this._isWorkflowChanging = false;
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "_cntrlType_Changed()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when a date changes in a DateControl.</summary>
		/// <param name="sender">DatePicker</param>
		/// <param name="e">SelectionChangedEventArgs</param>
		private void cntrlDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				this._cntrl_TextChanged(sender, e);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "cntrlDate_SelectedDateChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when a textbox or dropdown list changes.</summary>
		/// <param name="sender">cntrlIncidentName, cntrlDetectedBy, cntrlOwnedBy, cntrlPriority, cntrlSeverity, cntrlDetectedIn, cntrlResolvedIn, cntrlVerifiedIn, cntrlDescription</param>
		/// <param name="e"></param>
		private void _cntrl_TextChanged(object sender, EventArgs e)
		{
			try
			{
				if (!this.IsLoading && !this._isWorkflowChanging)
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

		/// <summary>Hit when the user clicks the Actions menu.</summary>
		/// <param name="sender">MenuItem</param>
		/// <param name="e">RoutedEventArgs</param>
		private void mnuActions_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//Show the overlay.
				this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Incident_LoadingWorkflow");
				this.barLoadingIncident.Value = 0;
				this.barLoadingIncident.Maximum = 4;
				this.display_SetOverlayWindow(this.panelStatus, Visibility.Visible);
				this._isWorkflowChanging = true;

				//See if they want to or need to confirm..
				MessageBoxResult areTheySure = MessageBoxResult.Yes;
				if (this._isFieldChanged || this._isDescChanged || this._isResChanged) //We don't check if the workflow changed here.
					areTheySure = MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_WorkflowResetFieldsMessage"), StaticFuncs.getCultureResource.GetString("app_General_AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

				if (areTheySure == MessageBoxResult.Yes)
				{

					//Get the item they clicked.
					RemoteWorkflowIncidentTransition wkfTrans = (((MenuItem)e.OriginalSource).Header) as RemoteWorkflowIncidentTransition;
					if (wkfTrans != null)
					{
						if (wkfTrans.Name.Trim().StartsWith("»"))
						{
							//They selected a different status, update the selected index and call the workflow.
							this._IncSelectedStatus = wkfTrans.IncidentStatusId_Output;

							//Reset fields..
							this.workflow_ClearAllRequiredHighlights();
							this.loadItem_DisplayInformation(this._Incident);

							//Update the field..
							this.cntrlIncidentStatus.Text = wkfTrans.Name;

							//Clear menu items, add the 'revert' menu.
							this.mnuActions.Items.Clear();
							RemoteWorkflowIncidentTransition tempTrans = new RemoteWorkflowIncidentTransition();
							tempTrans.Name = String.Format(StaticFuncs.getCultureResource.GetString("app_Incident_Revert"), this._Incident.IncidentStatusName);
							tempTrans.IncidentStatusId_Output = this._Incident.IncidentStatusId.Value;
							tempTrans.IncidentStatusName_Output = this._Incident.IncidentStatusName;
							this.mnuActions.Items.Add(tempTrans);

							//Update workflow fields..
							this.workflow_ChangeWorkflowStep();


							//Set changed flag.
							this.display_SetWindowChanged(true);
							this._isWkfChanged = true;
						}
						else
						{
							//They reverted. Need to reset things here.
							this._IncSelectedStatus = null;
							this._IncSelectedType = null;

							//Reset fields..
							this.loadItem_DisplayInformation(this._Incident);

							//Update workflow fields..
							this.workflow_ChangeWorkflowStep();

							//Revert changed flag.
							this.display_SetWindowChanged(false);

							//No longer in workflow.
							this._isWorkflowChanging = false;
						}
					}
				}
				else
				{
					this.display_SetOverlayWindow(this.panelStatus, Visibility.Collapsed);
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "mnuActions_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the Timer button's status is changed.</summary>
		/// <param name="sender">btnStartStopTimer</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnStartStopTimer_CheckedChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				e.Handled = true;

				//Make sure the value's not null. If it is, we're defaulting to unchecked.
				if (!this.btnStartStopTimer.IsChecked.HasValue) this.btnStartStopTimer.IsChecked = false;

				if (this.btnStartStopTimer.IsChecked.Value)
				{
					this.toolTxtTimer.Text = StaticFuncs.getCultureResource.GetString("app_General_StopTimer");
					//Set the timer..
					this._ArtifactDetails.IsTimed = true;
				}
				else
				{
					this.toolTxtTimer.Text = StaticFuncs.getCultureResource.GetString("app_General_StartTimer");

					//Set the timer.
					this._ArtifactDetails.IsTimed = false;

					//Get the value and add it to the task.
					TimeSpan workedSpan = this._ArtifactDetails.WorkTime;

					//Add it to the Incident.
					int intActH = 0;
					int intActM = 0;
					if (!string.IsNullOrWhiteSpace(cntrlActEffortH.Text))
					{
						try
						{
							intActH = int.Parse(cntrlActEffortH.Text);
						}
						catch { }
					}
					if (!string.IsNullOrWhiteSpace(cntrlActEffortM.Text))
					{
						try
						{
							intActM = int.Parse(cntrlActEffortM.Text);
						}
						catch { }
					}
					intActH += (workedSpan.Days * 24) + workedSpan.Hours;
					intActM += workedSpan.Minutes;
					//Add it up again..
					TimeSpan newWorked = new TimeSpan(intActH, intActM, 0);
					//Copy new values to the temporary storage fields and the display fields.
					this._tempHoursWorked = ((newWorked.Days * 24) + newWorked.Hours);
					this._tempMinutedWorked = newWorked.Minutes;
					this.cntrlActEffortH.Text = this._tempHoursWorked.ToString();
					this.cntrlActEffortM.Text = this._tempMinutedWorked.ToString();
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnStartStopTimer_CheckedChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
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
				System.Diagnostics.Process.Start(this._IncidentUrl);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "Error launching URL: " + this._IncidentUrl);
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
				if (this._isDescChanged || this._isFieldChanged || this._isResChanged || this._isWkfChanged)
				{
					isUserSure = MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_LoseChangesMessage"), StaticFuncs.getCultureResource.GetString("app_General_AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
				}

				if (isUserSure == MessageBoxResult.Yes)
				{
					//User is sure, change the label, and launch the refresh.
					this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Refreshing");
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
			try
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
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "Hyperlink_Click()");
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
							this.barLoadingIncident.Value = 1;
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
				try
				{
					if (this._isSavingInformation != value)
					{
						if (value)
						{
							this.display_SetOverlayWindow(this.panelSaving, Visibility.Visible);
						}
						else
						{
							this.barLoadingIncident.Value = 1;
							this.display_SetOverlayWindow(this.panelSaving, Visibility.Hidden);
						}

						this._isSavingInformation = value;
					}
				}
				catch (Exception ex)
				{
					Logger.LogMessage(ex, "IsSaving.Set");
					MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
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
					this._ArtifactDetails.WorkTimerChanged += new EventHandler(_ArtifactDetails_WorkTimerChanged);
					this._Project = value.ArtifactParentProject.ArtifactTag as SpiraProject;

					//Set tab title.
					if (this.ParentWindowPane != null)
						this.ParentWindowPane.Caption = this.TabTitle;

					//Set isworking flag..
					this.btnStartStopTimer.IsChecked = value.IsTimed;

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
				return (this._isDescChanged || this._isFieldChanged || this._isResChanged || this._isWkfChanged);
			}
		}

		/// <summary>Whether or not this details screen is currently hidden.</summary>
		public bool IsHidden
		{
			get;
			set;
		}
		#endregion

		/// <summary>Use to show or hide the Status Window.</summary>
		/// <param name="visiblity">The visibility of the window.</param>
		private void display_SetOverlayWindow(Grid Panel, Visibility visiblity)
		{
			try
			{
				cntrlResolution.IsSpellcheckEnabled = true;

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
						animFadeIn.Name = Panel.Name;
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
						Panel.Tag = animFadeOut;
						storyFadeOut.Children.Add(animFadeOut);

						//Start the animation.
						storyFadeOut.Begin();

						break;
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "display_SetOverlayWindow()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

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
