using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using System.Linq;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms
{
	/// <summary>Holds the saving functions for frmDetailsIncident</summary>
	public partial class frmDetailsRequirement : UserControl
	{
		//Are we currently saving our data?
		private bool _isSavingInformation = false;
		private int _clientNumSaving;
		private RemoteRequirement _RequirementConcurrent;

		/// <summary>Hit when the user wants to save the task.</summary>
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
				this.barSavingReq.Value = -5;
				this.barSavingReq.Maximum = 0;
				this.barSavingReq.Minimum = -5;

				if (this._isFieldChanged || this._isResChanged || this._isDescChanged)
				{
					//Set working flag.
					this.IsSaving = true;

					//Get the new values from the form..
					RemoteRequirement newRequirement = this.save_GetFromFields();

					if (newRequirement != null)
					{
						//Create a client, and save task and resolution..
						ImportExportClient clientSave = StaticFuncs.CreateClient(((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).ServerURL.ToString());
						clientSave.Connection_Authenticate2Completed += clientSave_Connection_Authenticate2Completed;
						clientSave.Connection_ConnectToProjectCompleted += clientSave_Connection_ConnectToProjectCompleted;
						clientSave.Requirement_UpdateCompleted += clientSave_Requirement_UpdateCompleted;
						clientSave.Requirement_CreateCommentCompleted += clientSave_Requirement_CreateCommentCompleted;
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
			const string METHOD = CLASS + "clientSave_Connection_DisconnectCompleted()";
			Logger.LogTrace_EnterMethod(METHOD + "  " + this._clientNumSaving.ToString() + " running.");

			try
			{

				this._clientNumSaving--;
				this.barSavingReq.Value++;

				//See if it's okay to reload.
				this.save_CheckIfOkayToLoad();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, METHOD);
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
			Logger.LogTrace_ExitMethod(METHOD);
		}

		/// <summary>Hit when we're finished adding a resolution.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_AddResolutionsCompletedEventArgs</param>
		private void clientSave_Requirement_CreateCommentCompleted(object sender, Requirement_CreateCommentCompletedEventArgs e)
		{
			const string METHOD = CLASS + "clientSave_Requirement_CreateCommentCompleted()";
			Logger.LogTrace_EnterMethod(METHOD + "  " + this._clientNumSaving.ToString() + " clients running.");

			try
			{
				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingReq.Value++;

				if (!e.Cancelled)
				{
					if (e.Error != null)
					{
						//Log message.
						Logger.LogMessage(e.Error, "Adding Comment to Requirement");
						//Display error that the item saved, but adding the new resolution didn't.
						MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_AddCommentErrorMessage"), StaticFuncs.getCultureResource.GetString("app_General_UpdateError"), MessageBoxButton.OK, MessageBoxImage.Error);
					}

					//Regardless of what happens, we're disconnecting here.
					this._clientNumSaving++;
					client.Connection_DisconnectAsync(this._clientNum++);
				}

				//See if it's okay to reload.
				this.save_CheckIfOkayToLoad();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, METHOD);
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}

			Logger.LogTrace(METHOD + "  " + this._clientNumSaving.ToString() + " clients left.");
		}

		/// <summary>Hit when we're finished updating the main information.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void clientSave_Requirement_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			const string METHOD = CLASS + "clientSave_Requirement_UpdateCompleted()";
			Logger.LogTrace_EnterMethod(METHOD + "  " + this._clientNumSaving.ToString() + " running.");
			try
			{
				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingReq.Value++;

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
							client.Requirement_CreateCommentAsync(newRes, this._clientNum++);
						}
						else
						{
							//We're finished.
							this.barSavingReq.Value++;
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
							client.Requirement_RetrieveByIdCompleted += new EventHandler<Requirement_RetrieveByIdCompletedEventArgs>(client_Requirement_RetrieveByIdCompleted);

							//Fire it off.
							this._clientNumSaving++;
							client.Requirement_RetrieveByIdAsync(this._ArtifactDetails.ArtifactId, this._clientNum++);
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

			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, METHOD);
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
			Logger.LogTrace_ExitMethod(METHOD + "  " + this._clientNumSaving.ToString() + " clients left.");
		}

		/// <summary>Hit if we hit a concurrency issue, and have to compare values.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrieveByIdCompletedEventArgs</param>
		private void client_Requirement_RetrieveByIdCompleted(object sender, Requirement_RetrieveByIdCompletedEventArgs e)
		{
			const string METHOD = CLASS + "client_Requirement_RetrieveByIdCompleted()";
			Logger.LogTrace_EnterMethod(METHOD + "  " + this._clientNumSaving.ToString() + " clients running.");

			try
			{
				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingReq.Value++;


				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						//We got new information here. Let's see if it can be merged.
						bool canBeMerged = this.save_CheckIfConcurrencyCanBeMerged(e.Result);
						this._RequirementConcurrent = e.Result;

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

			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, METHOD);
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
			Logger.LogTrace_ExitMethod(METHOD + "  " + this._clientNumSaving.ToString() + " clients left.");
		}

		/// <summary>Hit when we're finished connecting to the project.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Connection_ConnectToProjectCompletedEventArgs</param>
		private void clientSave_Connection_ConnectToProjectCompleted(object sender, Connection_ConnectToProjectCompletedEventArgs e)
		{
			const string METHOD = CLASS + "clientSave_Connection_ConnectToProjectCompleted()";
			Logger.LogTrace_EnterMethod(METHOD + "  " + this._clientNumSaving.ToString() + " clients running.");

			try
			{
				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingReq.Value++;

				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						if (e.Result)
						{
							//Get the new RemoteIncident
							RemoteRequirement newRequirement = this.save_GetFromFields();

							if (newRequirement != null)
							{
								//Fire off our update calls.
								this._clientNumSaving++;
								client.Requirement_UpdateAsync(newRequirement, this._clientNum++);
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

			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, METHOD);
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
			Logger.LogTrace_ExitMethod(METHOD + "  " + this._clientNumSaving.ToString() + " clients left.");
		}

		/// <summary>Hit when we're authenticated to the server.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Connection_Authenticate2CompletedEventArgs</param>
		private void clientSave_Connection_Authenticate2Completed(object sender, Connection_Authenticate2CompletedEventArgs e)
		{
			const string METHOD = CLASS + "clientSave_Connection_Authenticate2Completed()";
			Logger.LogTrace_EnterMethod(METHOD + "  " + this._clientNumSaving.ToString() + " clients running.");

			try
			{
				ImportExportClient client = (sender as ImportExportClient);
				this._clientNumSaving--;
				this.barSavingReq.Value++;

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

			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, METHOD);
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
			Logger.LogTrace_ExitMethod(METHOD + "  " + this._clientNumSaving.ToString() + " clients left.");
		}
		#endregion

		/// <summary>Checks if it's okay to refresh the data details.</summary>
		private void save_CheckIfOkayToLoad()
		{
			const string METHOD = CLASS + "save_CheckIfOkayToLoad()";
			Logger.LogTrace_EnterMethod(METHOD + "  " + this._clientNumSaving.ToString() + " clients running.");

			try
			{
				//If we're down to 0, we have to reload our information.
				if (this._clientNumSaving == 0)
				{
					Logger.LogTrace("No More Clients, loading new data from server.");
					this.IsSaving = false;
					this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Requirement_Refreshing");
					this.load_LoadItem();
				}

			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, METHOD);
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
			Logger.LogTrace_ExitMethod(METHOD);
		}

		/// <summary>Returns whether the given Concurrent Requirement can be safely merged with the user's values.</summary>
		/// <param name="moddedTask">The concurrent Requirement.</param>
		private bool save_CheckIfConcurrencyCanBeMerged(RemoteRequirement moddedRequirement)
		{
			bool retValue = false;

			try
			{

				//Get current values..
				RemoteRequirement userRequirement = this.save_GetFromFields();

				retValue = userRequirement.CanBeMergedWith(this._Requirement, moddedRequirement);
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
		private RemoteRequirement save_GetFromFields()
		{
			const string METHOD = CLASS + "save_GetFromFields()";

			RemoteRequirement retRequirement = null;
			try
			{
				retRequirement = new RemoteRequirement();

				//Standard fields..
				retRequirement.ConcurrencyDate = this._Requirement.ConcurrencyDate;
				retRequirement.CoverageCountBlocked = this._Requirement.CoverageCountBlocked;
				retRequirement.CoverageCountCaution = this._Requirement.CoverageCountCaution;
				retRequirement.CoverageCountFailed = this._Requirement.CoverageCountFailed;
				retRequirement.CoverageCountPassed = this._Requirement.CoverageCountPassed;
				retRequirement.CoverageCountTotal = this._Requirement.CoverageCountTotal;
				retRequirement.CreationDate = this._Requirement.CreationDate.ToUniversalTime();
				retRequirement.IndentLevel = this._Requirement.IndentLevel;
				retRequirement.LastUpdateDate = this._Requirement.LastUpdateDate.ToUniversalTime();
				retRequirement.ProjectId = this._Requirement.ProjectId;
				retRequirement.RequirementId = this._Requirement.RequirementId;
				retRequirement.Summary = this._Requirement.Summary;
				retRequirement.TaskActualEffort = this._Requirement.TaskActualEffort;
				retRequirement.TaskCount = this._Requirement.TaskCount;
				retRequirement.TaskEstimatedEffort = this._Requirement.TaskEstimatedEffort;
				retRequirement.AuthorId = ((RemoteUser)this.cntrlCreatedBy.SelectedItem).UserId;
				if (this._isDescChanged)
					retRequirement.Description = this.cntrlDescription.HTMLText;
				else
					retRequirement.Description = this._Requirement.Description;
				retRequirement.ImportanceId = ((RequirementPriority)this.cntrlImportance.SelectedItem).PriorityId;
				retRequirement.Name = this.cntrlName.Text.Trim();
				retRequirement.OwnerId = ((RemoteUser)this.cntrlOwnedBy.SelectedItem).UserId;
				retRequirement.PlannedEffort = StaticFuncs.GetMinutesFromValues(this.cntrlPlnEffortH.Text, this.cntrlPlnEffortM.Text);
				retRequirement.ReleaseId = ((RemoteRelease)this.cntrlRelease.SelectedItem).ReleaseId;
				retRequirement.StatusId = ((RequirementStatus)this.cntrlStatus.SelectedItem).StatusId;

				//Custom Properties
				retRequirement.CustomProperties = this.cntCustomProps.GetCustomProperties();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, METHOD);
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
				retRequirement = null;
			}

			//Return
			return retRequirement;
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
				this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Requirement_Refreshing");

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
					this.barSavingReq.Value--;

					//Re-launch the saving..
					RemoteRequirement reqMerged = StaticFuncs.MergeWithConcurrency(this.save_GetFromFields(), this._Requirement, this._RequirementConcurrent);

					this._clientNumSaving++;
					client.Requirement_UpdateAsync(reqMerged, this._clientNum++);
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
