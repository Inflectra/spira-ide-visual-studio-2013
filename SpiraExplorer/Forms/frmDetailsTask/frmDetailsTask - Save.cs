using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms
{
	/// <summary>Holds the saving functions for frmDetailsIncident</summary>
	public partial class frmDetailsTask : UserControl
	{
		//Are we currently saving our data?
		private bool _isSavingInformation = false;
		private int _clientNumSaving;
		private RemoteTask _TaskConcurrent;

		/// <summary>Hit when the user wants to save the requirement.</summary>
		/// <param name="sender">The save button.</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				e.Handled = true;
			}
			catch { }

			try
			{

				this.barSavingTask.Value = -5;
				this.barSavingTask.Maximum = 0;
				this.barSavingTask.Minimum = -5;

				if (this._isFieldChanged || this._isResChanged || this._isDescChanged)
				{
					//Set working flag.
					this.IsSaving = true;

					//Get the new values from the form..
					RemoteTask newTask = this.save_GetFromFields();

					if (newTask != null)
					{
						//Create a client, and save requirement and resolution..
						ImportExportClient clientSave = StaticFuncs.CreateClient(((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).ServerURL.ToString());
						clientSave.Connection_Authenticate2Completed += new EventHandler<Connection_Authenticate2CompletedEventArgs>(clientSave_Connection_Authenticate2Completed);
						clientSave.Connection_ConnectToProjectCompleted += new EventHandler<Connection_ConnectToProjectCompletedEventArgs>(clientSave_Connection_ConnectToProjectCompleted);
						clientSave.Task_UpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientSave_Task_UpdateCompleted);
						clientSave.Task_CreateCommentCompleted += new EventHandler<Task_CreateCommentCompletedEventArgs>(clientSave_Task_CreateCommentCompleted);
						clientSave.Connection_DisconnectCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientSave_Connection_DisconnectCompleted);

						//Fire off the connection.
						this._clientNumSaving = 1;
						clientSave.Connection_Authenticate2Async(((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).UserName, ((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).UserPass, StaticFuncs.getCultureResource.GetString("app_ReportName"), this._clientNum++);
					}
					else
					{
						//Display message saying that some required fields aren't filled out.
						MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_RequiredFieldsMessage"), StaticFuncs.getCultureResource.GetString("app_General_RequiredFields"), MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnSave_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}

			if (this._clientNumSaving == 0)
			{
				this.IsSaving = false;
			}
		}

		#region Client Events
		/// <summary>Hit when we're finished connecting.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void clientSave_Connection_DisconnectCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			try
			{
				const string METHOD = CLASS+"clientSave_Connection_DisconnectCompleted()";
				Logger.LogTrace(METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

				this._clientNumSaving--;
				this.barSavingTask.Value++;

				//See if it's okay to reload.
				this.save_CheckIfOkayToLoad();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientSave_Connection_DisconnectCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}

		}

		/// <summary>Hit when we're finished connecting to the project.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Connection_ConnectToProjectCompletedEventArgs</param>
		private void clientSave_Connection_ConnectToProjectCompleted(object sender, Connection_ConnectToProjectCompletedEventArgs e)
		{
			try
			{
				const string METHOD = CLASS+"clientSave_Connection_ConnectToProjectCompleted()";
				Logger.LogTrace(METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingTask.Value++;

				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						if (e.Result)
						{
							//Get the new RemoteIncident
							RemoteTask newTask = this.save_GetFromFields();

							if (newTask != null)
							{
								//Fire off our update calls.
								this._clientNumSaving++;
								client.Task_UpdateAsync(newTask, this._clientNum++);
							}
							else
							{
								//TODO: Show Error.
								//Cancel calls.
								this._clientNumSaving++;
								client.Connection_DisconnectAsync(this._clientNum++);
							}
						}
						else
						{
							//TODO: Show Error.
							//Cancel calls.
							this._clientNumSaving++;
							client.Connection_DisconnectAsync(this._clientNum++);
						}
					}
					else
					{
						//TODO: Show Error.
						//Cancel calls.
						this._clientNumSaving++;
						client.Connection_DisconnectAsync(this._clientNum++);
					}
				}

				//See if it's okay to reload.
				this.save_CheckIfOkayToLoad();

				Logger.LogTrace(METHOD + " Exit: " + this._clientNumSaving.ToString() + " left.");
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientSave_Connection_ConnectToProjectCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}

		}

		/// <summary>Hit when we're authenticated to the server.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Connection_Authenticate2CompletedEventArgs</param>
		private void clientSave_Connection_Authenticate2Completed(object sender, Connection_Authenticate2CompletedEventArgs e)
		{
			try
			{
				const string METHOD = CLASS+"clientSave_Connection_Authenticate2Completed()";
				Logger.LogTrace(METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingTask.Value++;

				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						if (e.Result)
						{
							//Connect to the project ID.
							this._clientNumSaving++;
							client.Connection_ConnectToProjectAsync(((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).ProjectID, this._clientNum++);
						}
						else
						{
							//TODO: Show Error.
							//Cancel calls.
							this._clientNumSaving++;
							client.Connection_DisconnectAsync(this._clientNum++);
						}
					}
					else
					{
						//TODO: Show Error.
						//Cancel calls.
						this._clientNumSaving++;
						client.Connection_DisconnectAsync(this._clientNum++);
					}
				}

				//See if it's okay to reload.
				this.save_CheckIfOkayToLoad();

				Logger.LogTrace(METHOD + " Exit: " + this._clientNumSaving.ToString() + " left.");
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientSave_Connection_Authenticate2Completed()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the client is finished updating the task.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void clientSave_Task_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			try
			{
				const string METHOD = CLASS+"clientSave_Incident_UpdateCompleted()";
				Logger.LogTrace(METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingTask.Value++;

				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						//See if we need to add a resolution.
						if (this._isResChanged)
						{
							//We need to save a resolution.
							RemoteComment newRes = new RemoteComment();
							newRes.CreationDate = DateTime.Now.ToUniversalTime();
							newRes.UserId = ((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).UserID;
							newRes.ArtifactId = this._ArtifactDetails.ArtifactId;
							newRes.Text = this.cntrlResolution.HTMLText;

							this._clientNumSaving++;
							client.Task_CreateCommentAsync(newRes, this._clientNum++);
						}
						else
						{
							//We're finished.
							this.barSavingTask.Value++;
							this._clientNumSaving++;
							client.Connection_DisconnectAsync(this._clientNum++);
						}
					}
					else
					{
						//Log error.
						Logger.LogMessage(e.Error, "Saving Incident Changes to Database");

						//If we get a concurrency error, get the current data.
						if (e.Error is FaultException<ServiceFaultMessage> && ((FaultException<ServiceFaultMessage>)e.Error).Detail.Type == "DataAccessConcurrencyException")
						{
							client.Task_RetrieveByIdCompleted += new EventHandler<Task_RetrieveByIdCompletedEventArgs>(clientSave_Task_RetrieveByIdCompleted);

							//Fire it off.
							this._clientNumSaving++;
							client.Incident_RetrieveByIdAsync(this._ArtifactDetails.ArtifactId, this._clientNum++);
						}
						else
						{
							//Display the error screen here.

							//Cancel calls.
							this._clientNumSaving++;
							client.Connection_DisconnectAsync(this._clientNum++);
						}
					}
				}

				//See if it's okay to reload.
				this.save_CheckIfOkayToLoad();

				Logger.LogTrace(METHOD + " Exit: " + this._clientNumSaving.ToString() + " left.");
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientSave_Incident_UpdateCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when we had a concurrency issue, and had to reload the task.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Task_RetrieveByIdCompletedEventArgs</param>
		private void clientSave_Task_RetrieveByIdCompleted(object sender, Task_RetrieveByIdCompletedEventArgs e)
		{
			try
			{
				const string METHOD = CLASS+"clientSave_Incident_RetrieveByIdCompleted()";
				Logger.LogTrace(METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingTask.Value++;


				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						//We got new information here. Let's see if it can be merged.
						bool canBeMerged = this.save_CheckIfConcurrencyCanBeMerged(e.Result);
						this._TaskConcurrent = e.Result;

						if (canBeMerged)
						{
							this.gridLoadingError.Visibility = System.Windows.Visibility.Collapsed;
							this.gridSavingConcurrencyMerge.Visibility = System.Windows.Visibility.Visible;
							this.gridSavingConcurrencyNoMerge.Visibility = System.Windows.Visibility.Collapsed;
							this.display_SetOverlayWindow(this.panelSaving, System.Windows.Visibility.Hidden);
							this.display_SetOverlayWindow(this.panelError, System.Windows.Visibility.Visible);

							//Save the client to the 'Merge' button.
							this.btnConcurrencyMergeYes.Tag = sender;
						}
						else
						{
							//TODO: Display error message here, tell users they must refresh their data.
							this.gridLoadingError.Visibility = System.Windows.Visibility.Collapsed;
							this.gridSavingConcurrencyMerge.Visibility = System.Windows.Visibility.Collapsed;
							this.gridSavingConcurrencyNoMerge.Visibility = System.Windows.Visibility.Visible;
							this.display_SetOverlayWindow(this.panelSaving, System.Windows.Visibility.Hidden);
							this.display_SetOverlayWindow(this.panelError, System.Windows.Visibility.Visible);
						}
					}
					else
					{
						//We even errored on retrieving information. Somethin's really wrong here.
						//Display error.
						Logger.LogMessage(e.Error, "Getting updated Concurrency Incident");
					}
				}

				Logger.LogTrace(METHOD + " Exit");
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientSave_Task_RetrieveByIdCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the client is finished adding a new comment.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Task_CreateCommentCompletedEventArgs</param>
		private void clientSave_Task_CreateCommentCompleted(object sender, Task_CreateCommentCompletedEventArgs e)
		{
			try
			{
				const string METHOD = CLASS+"clientSave_Incident_AddResolutionsCompleted()";
				Logger.LogTrace(METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingTask.Value++;

				if (!e.Cancelled)
				{
					if (e.Error != null)
					{
						//Log message.
						Logger.LogMessage(e.Error, "Adding Comment to Incident");
						//Display error that the item saved, but adding the new resolution didn't.
						MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_Task_AddCommentErrorMessage"), StaticFuncs.getCultureResource.GetString("app_Incident_UpdateError"), MessageBoxButton.OK, MessageBoxImage.Error);
					}

					//Regardless of what happens, we're disconnecting here.
					this._clientNumSaving++;
					client.Connection_DisconnectAsync(this._clientNum++);
				}

				//See if it's okay to reload.
				this.save_CheckIfOkayToLoad();

				Logger.LogTrace(METHOD + " Exit: " + this._clientNumSaving.ToString() + " left.");
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientSave_Task_CreateCommentCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		/// <summary>Checks if it's okay to refresh the data details.</summary>
		private void save_CheckIfOkayToLoad()
		{
			try
			{
				const string METHOD = CLASS+"save_CheckIfOkayToLoad()";
				Logger.LogTrace(METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

				//If we're down to 0, we have to reload our information.
				if (this._clientNumSaving == 0)
				{
					this.IsSaving = false;
					this.lblLoadingTask.Text = StaticFuncs.getCultureResource.GetString("app_Task_Refreshing");
					this.load_LoadItem();
				}

				Logger.LogTrace(METHOD + " Exit");
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "save_CheckIfOkayToLoad()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Returns whether the given Concurrent Incident can be safely merged with the user's values.</summary>
		/// <param name="moddedTask">The concurrent requirement.</param>
		private bool save_CheckIfConcurrencyCanBeMerged(RemoteTask moddedTask)
		{
			bool retValue = false;

			try
			{

				//Get current values..
				RemoteTask userTask = this.save_GetFromFields();

				retValue = userTask.CanBeMergedWith(this._Task, moddedTask);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "save_CheckIfConcurrencyCanBeMerged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
				retValue = false;
			}
			return retValue;
		}

		/// <summary>Copies over our values from the form into an Incident object.</summary>
		/// <returns>A new RemoteIncident, or Null if error.</returns>
		private RemoteTask save_GetFromFields()
		{
			const string METHOD = CLASS+"save_GetFromFields()";

			RemoteTask retTask = null;
			try
			{
				retTask = new RemoteTask();

				//Standard fields..
				retTask.TaskId = this._Task.TaskId;
				retTask.ProjectId = this._Task.ProjectId;
				retTask.ConcurrencyDate = this._Task.ConcurrencyDate;
				retTask.CreationDate = this._Task.CreationDate.ToUniversalTime();
				retTask.LastUpdateDate = this._Task.LastUpdateDate.ToUniversalTime();
				retTask.RequirementId = this._Task.RequirementId;
				retTask.Name = this.cntrlTaskName.Text.Trim();
				retTask.TaskPriorityId = ((TaskPriority)this.cntrlPriority.SelectedValue).PriorityId;
				retTask.CreatorId = ((RemoteUser)this.cntrlDetectedBy.SelectedItem).UserId;
				retTask.OwnerId = ((RemoteUser)this.cntrlOwnedBy.SelectedItem).UserId;
				retTask.ReleaseId = ((RemoteRelease)this.cntrlDetectedIn.SelectedItem).ReleaseId;
				if (this._isDescChanged)
					retTask.Description = this.cntrlDescription.HTMLText;
				else
					retTask.Description = this._Task.Description;
				if (this.cntrlStartDate.SelectedDate.HasValue)
					retTask.StartDate = this.cntrlStartDate.SelectedDate.Value.ToUniversalTime();
				if (this.cntrlEndDate.SelectedDate.HasValue)
					retTask.EndDate = this.cntrlEndDate.SelectedDate.Value.ToUniversalTime();
				retTask.TaskStatusId = ((TaskStatus)this.cntrlStatus.SelectedValue).StatusId.Value;
				retTask.EstimatedEffort = StaticFuncs.GetMinutesFromValues(this.cntrlEstEffortH.Text, this.cntrlEstEffortM.Text);
				retTask.ActualEffort = StaticFuncs.GetMinutesFromValues(this.cntrlActEffortH.Text, this.cntrlActEffortM.Text);
				retTask.RemainingEffort = StaticFuncs.GetMinutesFromValues(this.cntrlRemEffortH.Text, this.cntrlRemEffortM.Text);

				//Custom fields..
				retTask.CustomProperties = this.cntCustomProps.GetCustomProperties();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "save_GetFromFields()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}

			//Return
			return retTask;
		}

		#region Concurrency Button Events
		/// <summary>Hit when the user does not want to save, and is forced to refresh the loaded data.</summary>
		/// <param name="sender">btnConcurrencyMergeNo, btnConcurrencyRefresh</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnConcurrencyRefresh_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//Hide the error panel, jump to loading..
				this.display_SetOverlayWindow(this.panelError, System.Windows.Visibility.Collapsed);
				this.display_SetOverlayWindow(this.panelStatus, System.Windows.Visibility.Visible);
				this.lblLoadingTask.Text = StaticFuncs.getCultureResource.GetString("app_Task_Refreshing");

				this.load_LoadItem();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnConcurrencyRefresh_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user wants to merge their changes with the concurrent requirement.</summary>
		/// <param name="sender">btnConcurrencyMergeYes</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnConcurrencyMergeYes_Click(object sender, RoutedEventArgs e)
		{
			try
			{

				e.Handled = true;
				//Get the client.
				ImportExportClient client = ((dynamic)sender).Tag as ImportExportClient;
				if (client != null)
				{
					//Switch screens again...
					this.display_SetOverlayWindow(this.panelSaving, System.Windows.Visibility.Visible);
					this.display_SetOverlayWindow(this.panelError, System.Windows.Visibility.Hidden);
					this.barSavingTask.Value--;

					//Re-launch the saving..
					RemoteTask incMerged = StaticFuncs.MergeWithConcurrency(this.save_GetFromFields(), this._Task, this._TaskConcurrent);

					this._clientNumSaving++;
					client.Task_UpdateAsync(incMerged, this._clientNum++);
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnConcurrencyMergeYes_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion
	}
}
