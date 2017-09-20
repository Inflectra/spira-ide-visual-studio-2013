using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms
{
	/// <summary>Holds the saving functions for frmDetailsIncident</summary>
	public partial class frmDetailsIncident : UserControl
	{
		//Are we currently saving our data?
		private bool _isSavingInformation = false;
		private int _clientNumSaving;
		private RemoteIncident _IncidentConcurrent;

		/// <summary>Hit when the user wants to save the task.</summary>
		/// <param name="sender">The save button.</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			if (e != null)
			{
				e.Handled = true;
			}

			try
			{
				this.barSavingIncident.Value = -5;
				this.barSavingIncident.Maximum = 0;
				this.barSavingIncident.Minimum = -5;

				if (this._isFieldChanged || this._isWkfChanged || this._isResChanged || this._isDescChanged)
				{
					//Clear highlights.
					this.workflow_ClearAllRequiredHighlights();

					//Set working flag.
					this.IsSaving = true;

					//Get the new values from the form..
					RemoteIncident newIncident = this.save_GetFromFields();

					if (newIncident != null && this.workflow_CheckRequiredFields())
					{
						//Create a client, and save task and resolution..
						ImportExportClient clientSave = StaticFuncs.CreateClient(((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).ServerURL.ToString());
						clientSave.Connection_Authenticate2Completed += clientSave_Connection_Authenticate2Completed;
						clientSave.Connection_ConnectToProjectCompleted += clientSave_Connection_ConnectToProjectCompleted;
						clientSave.Incident_UpdateCompleted += clientSave_Incident_UpdateCompleted;
						clientSave.Incident_AddCommentsCompleted += clientSave_Incident_AddCommentsCompleted;
						clientSave.Connection_DisconnectCompleted += clientSave_Connection_DisconnectCompleted;

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
				this.barSavingIncident.Value++;

				//See if it's okay to reload.
				this.save_CheckIfOkayToLoad();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientSave_Connection_DisconnectCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when we're finished adding a resolution.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_AddResolutionsCompletedEventArgs</param>
		private void clientSave_Incident_AddCommentsCompleted(object sender, Incident_AddCommentsCompletedEventArgs e)
		{
			try
			{
				const string METHOD = CLASS+"clientSave_Incident_AddResolutionsCompleted()";
				Logger.LogTrace(METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingIncident.Value++;

				if (!e.Cancelled)
				{
					if (e.Error != null)
					{
						//Log message.
						Logger.LogMessage(e.Error, "Adding Comment to Incident");
						//Display error that the item saved, but adding the new resolution didn't.
						MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_AddCommentErrorMessage"), StaticFuncs.getCultureResource.GetString("app_Incident_UpdateError"), MessageBoxButton.OK, MessageBoxImage.Error);
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
				Logger.LogMessage(ex, "clientSave_Incident_AddResolutionsCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when we're finished updating the main information.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void clientSave_Incident_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			try
			{
				const string METHOD = CLASS+"clientSave_Incident_UpdateCompleted()";
				Logger.LogTrace(METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingIncident.Value++;

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
							client.Incident_AddCommentsAsync(new List<RemoteComment>() { newRes }, this._clientNum++);
						}
						else
						{
							//We're finished.
							this.barSavingIncident.Value++;
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
							client.Incident_RetrieveByIdCompleted += new EventHandler<Incident_RetrieveByIdCompletedEventArgs>(clientSave_Incident_RetrieveByIdCompleted);

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

		/// <summary>Hit if we hit a concurrency issue, and have to comapre values.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrieveByIdCompletedEventArgs</param>
		private void clientSave_Incident_RetrieveByIdCompleted(object sender, Incident_RetrieveByIdCompletedEventArgs e)
		{
			try
			{
				const string METHOD = CLASS+"clientSave_Incident_RetrieveByIdCompleted()";
				Logger.LogTrace(METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingIncident.Value++;


				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						//We got new information here. Let's see if it can be merged.
						bool canBeMerged = this.save_CheckIfConcurrencyCanBeMerged(e.Result);
						this._IncidentConcurrent = e.Result;

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
				Logger.LogMessage(ex, "clientSave_Incident_RetrieveByIdCompleted()");
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
				this.barSavingIncident.Value++;

				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						if (e.Result)
						{
							//Get the new RemoteIncident
							RemoteIncident newIncident = this.save_GetFromFields();

							if (newIncident != null)
							{
								//Fire off our update calls.
								this._clientNumSaving++;
								client.Incident_UpdateAsync(newIncident, this._clientNum++);
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
				this.barSavingIncident.Value++;

				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						if (e.Result)
						{
							//Connect to the progect ID.
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
					this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Refreshing");
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
		/// <param name="moddedTask">The concurrent task.</param>
		private bool save_CheckIfConcurrencyCanBeMerged(RemoteIncident moddedIncident)
		{
			bool retValue = false;
			try
			{

				//Get current values..
				RemoteIncident userIncident = this.save_GetFromFields();

				retValue = userIncident.CanBeMergedWith(this._Incident, moddedIncident);
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
		private RemoteIncident save_GetFromFields()
		{
			const string METHOD = CLASS+"save_GetFromFields()";

			RemoteIncident retIncident = null;
			try
			{
				retIncident = new RemoteIncident();

				//*Fixed fields..
				retIncident.IncidentId = this._Incident.IncidentId;
				retIncident.ProjectId = this._Incident.ProjectId;
				if (this._Incident.CreationDate.HasValue)
					retIncident.CreationDate = this._Incident.CreationDate.Value.ToUniversalTime();
				retIncident.LastUpdateDate = this._Incident.LastUpdateDate.ToUniversalTime();
				retIncident.ConcurrencyDate = this._Incident.ConcurrencyDate;

				//*Standard fields..
				retIncident.Name = this.cntrlIncidentName.Text.Trim();
				retIncident.IncidentTypeId = ((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId;
				retIncident.IncidentStatusId = ((this._IncSelectedStatus.HasValue) ? this._IncSelectedStatus.Value : this._IncCurrentStatus.Value);
				retIncident.OpenerId = ((RemoteUser)this.cntrlDetectedBy.SelectedItem).UserId;
				retIncident.OwnerId = ((RemoteUser)this.cntrlOwnedBy.SelectedItem).UserId;
				retIncident.PriorityId = ((RemoteIncidentPriority)this.cntrlPriority.SelectedItem).PriorityId;
				retIncident.SeverityId = ((RemoteIncidentSeverity)this.cntrlSeverity.SelectedItem).SeverityId;
				retIncident.DetectedReleaseId = ((RemoteRelease)this.cntrlDetectedIn.SelectedItem).ReleaseId;
				retIncident.ResolvedReleaseId = ((RemoteRelease)this.cntrlResolvedIn.SelectedItem).ReleaseId;
				retIncident.VerifiedReleaseId = ((RemoteRelease)this.cntrlVerifiedIn.SelectedItem).ReleaseId;
				if (this._isDescChanged)
					retIncident.Description = this.cntrlDescription.HTMLText;
				else
					retIncident.Description = this._Incident.Description;

				//*Schedule fields..
				if (this.cntrlStartDate.SelectedDate.HasValue)
					retIncident.StartDate = this.cntrlStartDate.SelectedDate.Value.ToUniversalTime();
				if (this.cntrlEndDate.SelectedDate.HasValue)
					retIncident.ClosedDate = this.cntrlEndDate.SelectedDate.Value.ToUniversalTime();
				retIncident.EstimatedEffort = StaticFuncs.GetMinutesFromValues(this.cntrlEstEffortH.Text, this.cntrlEstEffortM.Text);
				retIncident.ActualEffort = StaticFuncs.GetMinutesFromValues(this.cntrlActEffortH.Text, this.cntrlActEffortM.Text);
				retIncident.RemainingEffort = StaticFuncs.GetMinutesFromValues(this.cntrlRemEffortH.Text, this.cntrlRemEffortM.Text);

				//Custom Properties
				retIncident.CustomProperties = this.cntCustomProps.GetCustomProperties();

			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, METHOD);
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
				retIncident = null;
			}

			//Return
			return retIncident;
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
				this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Refreshing");

				this.load_LoadItem();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnConcurrencyRefresh_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user wants to merge their changes with the concurrent task.</summary>
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
					this.barSavingIncident.Value--;

					//Re-launch the saving..
					RemoteIncident incMerged = StaticFuncs.MergeWithConcurrency(this.save_GetFromFields(), this._Incident, this._IncidentConcurrent);

					this._clientNumSaving++;
					client.Incident_UpdateAsync(incMerged, this._clientNum++);
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
