using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Properties;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Classes;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms
{
	/// <summary>
	/// Interaction logic for cntlSpiraExplorer.xaml
	/// </summary>
	public partial class cntlSpiraExplorer : UserControl
	{
		private static string CLASS = "cntlSpiraExplorer::";
		private List<TreeViewArtifact> _Projects = new List<TreeViewArtifact>();
		private int _numActiveClients = 0;

        /// <summary>Loads a Spira project into the treeview.</summary>
        /// <param name="projectId">The id of the project</param>
        private void loadProject(int projectId)
		{
			try
			{
				//Get our list of projects.
				this._Projects.Clear();
				if (projectId > 0)
				{
                    //Instantiate the new project (it will query the server and get the project name, etc.)
                    SpiraProject spiraProject = new SpiraProject(SpiraContext.Login, SpiraContext.Password, SpiraContext.BaseUri, SpiraContext.ProjectId);

                    TreeViewArtifact newProj = new TreeViewArtifact(this.refreshTreeNodeServerData);
                    newProj.ArtifactTag = spiraProject;
					newProj.ArtifactId = ((Business.SpiraProject)newProj.ArtifactTag).ProjectId;
					newProj.ArtifactName = ((Business.SpiraProject)newProj.ArtifactTag).ProjectName;
					newProj.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Project;
					newProj.ArtifactIsFolder = true;
					newProj.Parent = null;

					this._Projects.Add(newProj);
				}

				//Refresh the treeview.
				this.refreshProjects();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "loadProjects()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Update items from the server.</summary>
		/// <param name="itemToRefresh">The root item (and children) to update.</param>
		private void refreshTreeNodeServerData(TreeViewArtifact itemToRefresh)
		{
			try
			{
				//Depending what is highlighted will specify what needs to be updated.
				if (itemToRefresh.ArtifactIsFolder)
				{
					//Update children..
					foreach (TreeViewArtifact subChild in itemToRefresh.Items)
					{
						//If it's a folder, call recursively.
						if (subChild.ArtifactIsFolder)
							this.refreshTreeNodeServerData(subChild);
					}

					if (itemToRefresh.ArtifactType.GetType() == typeof(Spira_ImportExport))
					{
						Spira_ImportExport clientExist = (Spira_ImportExport)itemToRefresh.ArtifactTag;

						//Kill it.
						try
						{
							clientExist.Client.Abort();
						}
						catch { }
						finally
						{
							try
							{
								clientExist.Client.Connection_Disconnect();
							}
							catch { }
						}
						clientExist = null;
						itemToRefresh.ArtifactTag = null;
					}

					//Now refresh this one if necessary.
					if (itemToRefresh.ArtifactType != TreeViewArtifact.ArtifactTypeEnum.None && itemToRefresh.ArtifactType != TreeViewArtifact.ArtifactTypeEnum.Project)
					{
						//We're spawning one off, make the bar visible.
						this.barLoading.Visibility = System.Windows.Visibility.Visible;
						this.trvProject.Cursor = System.Windows.Input.Cursors.AppStarting;

						//Generate a new client to go get data for.
						Spira_ImportExport clientRefresh = new Spira_ImportExport(((SpiraProject)itemToRefresh.ArtifactParentProject.ArtifactTag).ServerURL.ToString(), ((SpiraProject)itemToRefresh.ArtifactParentProject.ArtifactTag).UserName, ((SpiraProject)itemToRefresh.ArtifactParentProject.ArtifactTag).UserPass);
						clientRefresh.ConnectionReady += new EventHandler(_client_ConnectionReady);
						clientRefresh.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
						clientRefresh.Client.Incident_RetrieveCompleted += new EventHandler<Business.SpiraTeam_Client.Incident_RetrieveCompletedEventArgs>(_client_Incident_RetrieveCompleted);
						clientRefresh.Client.Requirement_RetrieveCompleted += new EventHandler<Requirement_RetrieveCompletedEventArgs>(_client_Requirement_RetrieveCompleted);
						clientRefresh.Client.Task_RetrieveCompleted += new EventHandler<Task_RetrieveCompletedEventArgs>(_client_Task_RetrieveCompleted);
						clientRefresh.Client.Connection_DisconnectCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_client_Connection_DisconnectCompleted);
						clientRefresh.ClientNode = itemToRefresh;
						itemToRefresh.ArtifactTag = clientRefresh;

						clientRefresh.Connect();
						this._numActiveClients++;
					}
				}
				else if (itemToRefresh.ArtifactType == TreeViewArtifact.ArtifactTypeEnum.Project)
				{
					//Loop through each child.
					foreach (TreeViewArtifact childItem in itemToRefresh.Items)
						this.refreshTreeNodeServerData(childItem);
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "refreshTreeNodeServerData()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Refreshes the display for all loaded projects.</summary>
		private void refreshProjects()
		{
			try
			{
				//All this does is create the tree structure for each project, then calls
				//  refreshTreeNodeServerData on each Project TreeNode.

				//Clear the tree and refresh data.
				this.trvProject.Items.Refresh();
				this.barLoading.Visibility = Visibility.Visible;
				this.trvProject.Cursor = System.Windows.Input.Cursors.AppStarting;

				foreach (TreeViewArtifact trvProj in this._Projects)
				{
					//Create the 'My' nodes.
					TreeViewArtifact folderIncMy = new TreeViewArtifact(this.refreshTreeNodeServerData);
					folderIncMy.ArtifactIsFolder = true;
					folderIncMy.ArtifactName = string.Format(StaticFuncs.getCultureResource.GetString("app_Tree_My"), StaticFuncs.getCultureResource.GetString("app_Tree_Incidents"));
					folderIncMy.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Incident;
					folderIncMy.ArtifactIsFolderMine = true;
					TreeViewArtifact folderReqMy = new TreeViewArtifact(this.refreshTreeNodeServerData);
					folderReqMy.ArtifactIsFolder = true;
					folderReqMy.ArtifactName = string.Format(StaticFuncs.getCultureResource.GetString("app_Tree_My"), StaticFuncs.getCultureResource.GetString("app_Tree_Requirements"));
					folderReqMy.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Requirement;
					folderReqMy.ArtifactIsFolderMine = true;
					TreeViewArtifact folderTskMy = new TreeViewArtifact(this.refreshTreeNodeServerData);
					folderTskMy.ArtifactIsFolder = true;
					folderTskMy.ArtifactName = string.Format(StaticFuncs.getCultureResource.GetString("app_Tree_My"), StaticFuncs.getCultureResource.GetString("app_Tree_Tasks"));
					folderTskMy.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Task;
					folderTskMy.ArtifactIsFolderMine = true;

					folderIncMy.Parent = trvProj;
					folderReqMy.Parent = trvProj;
					folderTskMy.Parent = trvProj;

					trvProj.Items.Add(folderIncMy);
					trvProj.Items.Add(folderReqMy);
					trvProj.Items.Add(folderTskMy);

					//Now, refresh the project.
					this.refreshTreeNodeServerData(trvProj);
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "refreshProjects()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#region Client Events
		/// <summary>Hit when the Incident clients are finished retrieving data.</summary>
		/// <param name="sender">SoapServiceClient</param>
		/// <param name="e">Incident_RetrieveCompletedEventArgs</param>
		private void _client_Incident_RetrieveCompleted(object sender, Business.SpiraTeam_Client.Incident_RetrieveCompletedEventArgs e)
		{
			string METHOD = CLASS + "_client_Incident_RetrieveCompleted";
			try
			{
				//Grab parent node.
				TreeViewArtifact parentNode = e.UserState as TreeViewArtifact;

				if (parentNode != null)
				{
					parentNode.Items.Clear();

					//We got results back. Let's do something with them!
					if (e.Error == null)
					{
						foreach (RemoteIncident incident in e.Result)
						{
							//Make new node.
							TreeViewArtifact newNode = new TreeViewArtifact(this.refreshTreeNodeServerData);
							newNode.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Incident;
							newNode.ArtifactTag = incident;
							newNode.ArtifactName = incident.Name;
							newNode.ArtifactIsFolder = false;
							newNode.ArtifactId = incident.IncidentId.Value;
							newNode.Parent = parentNode;
							newNode.DetailsOpenRequested += new EventHandler(newNode_DetailsOpenRequested);
							newNode.WorkTimerChanged += new EventHandler(newNode_WorkTimerChanged);

							parentNode.Items.Add(newNode);
						}
					}
					else
					{
						this.addErrorNode(ref parentNode, e.Error, StaticFuncs.getCultureResource.GetString("app_Error_RetrieveShort"));
					}
				}
				else
				{
					//No parent node. Log error, exit.
					Logger.LogMessage(METHOD, "Did not get a parent folder!", System.Diagnostics.EventLogEntryType.Error);
				}

				//Disconnect the client, subtract from the count.
				try
				{
					((SoapServiceClient)sender).Connection_DisconnectAsync();
				}
				catch { }
				parentNode.ArtifactTag = null;
				this._numActiveClients--;
				this.refreshTree();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "_client_Incident_RetrieveCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user updates the worktimer on an TreeViewArtifact.</summary>
		/// <param name="sender">TreeViewArtifact</param>
		/// <param name="e">EventArgs</param>
		private void newNode_WorkTimerChanged(object sender, EventArgs e)
		{
			try
			{
				//First, update the treeview.
				this.trvProject.Items.Refresh();

				//If there is no window to update, then we need to save the item.
				if (!((TreeViewArtifact)sender).IsTimed)
				{
					if (((SpiraExplorerPackage)this.Pane.Package).FindExistingToolWindow((TreeViewArtifact)sender, false) == null)
					{
						TreeViewArtifact artItem = sender as TreeViewArtifact;

						//No window, update it ourselves. Create the client.
						SoapServiceClient clientWkTime = StaticFuncs.CreateClient(((SpiraProject)artItem.ArtifactParentProject.ArtifactTag).ServerURL.ToString());
						//Set event handlers.
						clientWkTime.Connection_Authenticate2Completed += new EventHandler<Connection_Authenticate2CompletedEventArgs>(clientWkTime_Connection_Authenticate2Completed);
						clientWkTime.Connection_ConnectToProjectCompleted += new EventHandler<Connection_ConnectToProjectCompletedEventArgs>(clientWkTime_Connection_ConnectToProjectCompleted);
						clientWkTime.Incident_RetrieveByIdCompleted += new EventHandler<Incident_RetrieveByIdCompletedEventArgs>(clientWkTime_Artifact_RetrieveByIdCompleted);
						clientWkTime.Task_RetrieveByIdCompleted += new EventHandler<Task_RetrieveByIdCompletedEventArgs>(clientWkTime_Artifact_RetrieveByIdCompleted);
						clientWkTime.Incident_UpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientWkTime_Artifact_UpdateCompleted);
						clientWkTime.Task_UpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientWkTime_Artifact_UpdateCompleted);
						clientWkTime.Connection_DisconnectCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientWkTime_Connection_DisconnectCompleted);

						//Fire off the connection.
						this.barLoading.Visibility = System.Windows.Visibility.Visible;
						this.barLoading.IsIndeterminate = false;
						this.barLoading.Minimum = 0;
						this.barLoading.Maximum = 5;
						this.barLoading.Value = 0;
						this._numActiveClients++;
						clientWkTime.Connection_Authenticate2Async(
							((SpiraProject)artItem.ArtifactParentProject.ArtifactTag).UserName,
							((SpiraProject)artItem.ArtifactParentProject.ArtifactTag).UserPass,
							StaticFuncs.getCultureResource.GetString("app_ReportName"),
							sender);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "newNode_WorkTimerChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the UpdateWorkTimeClient is finished updating a Requirement, Incident, or Task.</summary>
		/// <param name="sender">SoapServiceClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void clientWkTime_Artifact_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			try
			{
				SoapServiceClient wktimeClient = sender as SoapServiceClient;
				TreeViewArtifact treeArt = (TreeViewArtifact)e.UserState;

				this.barLoading.Value += 1;
				this._numActiveClients--;

				if (e.Error != null || e.Cancelled)
				{
					//Display error here.
				}

				this._numActiveClients++;
				wktimeClient.Connection_DisconnectAsync(e.UserState);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientWkTime_Artifact_UpdateCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the UpdateWorkTimeClient is finished disconnecting from the server.</summary>
		/// <param name="sender">SoapServiceClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void clientWkTime_Connection_DisconnectCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			try
			{
				this._numActiveClients--;

				this.barLoading.Visibility = System.Windows.Visibility.Collapsed;
				this.barLoading.IsIndeterminate = true;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientWkTime_Connection_DisconnectCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the UpdateWorkTimeClient is finished retrieving the artifact from the server.</summary>
		/// <remarks>This is where the values are actually saved to the artifact.</remarks>
		/// <param name="sender">SoapServiceClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void clientWkTime_Artifact_RetrieveByIdCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			try
			{
				SoapServiceClient wktimeClient = sender as SoapServiceClient;
				TreeViewArtifact treeArt = (TreeViewArtifact)e.UserState;

				this.barLoading.Value += 1;
				this._numActiveClients--;

				if (e.Error == null && !e.Cancelled)
				{
					//Get the type of eventargs..
					string strEvent = e.GetType().ToString();
					strEvent = strEvent.Substring(strEvent.LastIndexOf(".") + 1).ToLowerInvariant();
					switch (strEvent)
					{
						#region Update Incident
						case "incident_retrievebyidcompletedeventargs":
							Incident_RetrieveByIdCompletedEventArgs evt1 = e as Incident_RetrieveByIdCompletedEventArgs;
							//Get current value and add our worked time to it.
							if (evt1 != null)
							{
								int numMinutes = (int)treeArt.WorkTime.TotalMinutes;
								if (numMinutes > 0)
								{
									if (evt1.Result.ActualEffort.HasValue)
									{
										evt1.Result.ActualEffort = evt1.Result.ActualEffort.Value + numMinutes;
									}
									else
									{
										evt1.Result.ActualEffort = numMinutes;
									}
								}

								//Okay, update the item.
								this._numActiveClients++;
								wktimeClient.Incident_UpdateAsync(evt1.Result, treeArt);
							}
							break;
						#endregion

						#region Update Task
						case "Task_RetrieveByIdCompletedEventArgs":
							Task_RetrieveByIdCompletedEventArgs evt2 = e as Task_RetrieveByIdCompletedEventArgs;
							//Get current value and add our worked time to it.
							if (evt2 != null)
							{
								int numMinutes = (int)treeArt.WorkTime.TotalMinutes;
								if (numMinutes > 0)
								{
									if (evt2.Result.ActualEffort.HasValue)
									{
										evt2.Result.ActualEffort = evt2.Result.ActualEffort.Value + numMinutes;
									}
									else
									{
										evt2.Result.ActualEffort = numMinutes;
									}
								}

								//Okay, update the item.
								this._numActiveClients++;
								wktimeClient.Task_UpdateAsync(evt2.Result, treeArt);
							}
							break;
						#endregion

						default:
							//Error, cancel operation.
							wktimeClient.Connection_DisconnectAsync(e.UserState);
							break;
					}
				}
				else
				{
					//Cancel connection.
					this._numActiveClients++;
					wktimeClient.Connection_DisconnectAsync(e.UserState);
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientWkTime_Artifact_RetrieveByIdCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the UpdateWorkTimeClient is finished connecting to the project.</summary>
		/// <param name="sender">SoapServiceClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void clientWkTime_Connection_ConnectToProjectCompleted(object sender, Connection_ConnectToProjectCompletedEventArgs e)
		{
			try
			{
				SoapServiceClient wktimeClient = sender as SoapServiceClient;
				TreeViewArtifact treeArt = (TreeViewArtifact)e.UserState;

				this.barLoading.Value += 1;
				this._numActiveClients--;

				if (e.Error == null && !e.Cancelled)
				{
					//Okay, we're good to go get our information.
					this._numActiveClients++;
					switch (treeArt.ArtifactType)
					{
						case TreeViewArtifact.ArtifactTypeEnum.Incident:
							wktimeClient.Incident_RetrieveByIdAsync(treeArt.ArtifactId, e.UserState);
							break;

						case TreeViewArtifact.ArtifactTypeEnum.Requirement:
							wktimeClient.Requirement_RetrieveByIdAsync(treeArt.ArtifactId, e.UserState);
							break;

						case TreeViewArtifact.ArtifactTypeEnum.Task:
							wktimeClient.Task_RetrieveByIdAsync(treeArt.ArtifactId, e.UserState);
							break;

						default:
							//Error, cancel operation.
							wktimeClient.Connection_DisconnectAsync(e.UserState);
							break;
					}
				}
				else
				{
					//Cancel connection.
					this._numActiveClients++;
					wktimeClient.Connection_DisconnectAsync(e.UserState);
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientWkTime_Connection_ConnectToProjectCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the UpdateWorkTimeClient is finished connecting to the server.</summary>
		/// <param name="sender">SoapServiceClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void clientWkTime_Connection_Authenticate2Completed(object sender, Connection_Authenticate2CompletedEventArgs e)
		{
			try
			{
				SoapServiceClient wktimeClient = sender as SoapServiceClient;
				TreeViewArtifact treeArt = (TreeViewArtifact)e.UserState;

				this.barLoading.Value += 1;
				this._numActiveClients--;

				if (e.Error == null && !e.Cancelled)
				{
					//Everything is okay so far, fire off connecting to the project.
					this._numActiveClients++;
					wktimeClient.Connection_ConnectToProjectAsync(((SpiraProject)treeArt.ArtifactParentProject.ArtifactTag).ProjectId, e.UserState);
				}
				else
				{
					//Cancel connection.
					this._numActiveClients++;
					wktimeClient.Connection_DisconnectAsync(e.UserState);
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientWkTime_Connection_Authenticate2Completed()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user wants to open details by clicking on the context menu.</summary>
		/// <param name="sender">TreeViewArtifact</param>
		/// <param name="e">EventArgs</param>
		private void newNode_DetailsOpenRequested(object sender, EventArgs e)
		{
			try
			{
				this.TreeNode_MouseDoubleClick(sender, null);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "newNode_DetailsOpenRequested()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when a client ran into an error while trying to connect to the server.</summary>
		/// <param name="sender">Spira_ImportExport</param>
		/// <param name="e">Spira_ImportExport.ConnectionException</param>
		private void _client_ConnectionError(object sender, Business.Spira_ImportExport.ConnectionException e)
		{
			try
			{
				//There was an error trying to connect. Mark the node as error.
				Business.Spira_ImportExport _client = sender as Business.Spira_ImportExport;
				if (_client != null)
				{
					if (_client.ClientNode != null)
					{
						TreeViewArtifact treeNode = _client.ClientNode;
						this.addErrorNode(ref treeNode, e.error, StaticFuncs.getCultureResource.GetString("app_Error_RetrieveShort"));
						_client.ClientNode = treeNode;

						//Refresh treeview.
						this._numActiveClients--;
						this.refreshTree();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "_client_ConnectionError()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when a client is connected and ready to get data for the tree node.</summary>
		/// <param name="sender">Spira_ImportExport</param>
		/// <param name="e">EventArgs</param>
		private void _client_ConnectionReady(object sender, EventArgs e)
		{
			try
			{
				//Connection ready, connect to the project.
				Business.Spira_ImportExport client = sender as Business.Spira_ImportExport;
				if (client != null)
				{
					//Get the parent project..
					TreeViewArtifact nodeProject = client.ClientNode.ArtifactParentProject;

					client.Client.Connection_ConnectToProjectCompleted += new EventHandler<Connection_ConnectToProjectCompletedEventArgs>(_client_Connection_ConnectToProjectCompleted);
					client.Client.Connection_ConnectToProjectAsync(nodeProject.ArtifactId, client);
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "_client_ConnectionReady()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the client is connected and logged into a project.</summary>
		/// <param name="sender">Spira_ImportExport</param>
		/// <param name="e">Connection_ConnectToProjectCompletedEventArgs</param>
		private void _client_Connection_ConnectToProjectCompleted(object sender, Connection_ConnectToProjectCompletedEventArgs e)
		{
			string METHOD = CLASS + "_client_Connection_ConnectToProjectCompleted";
			try
			{
				//Connection ready. Let's fire off our query.
				Business.Spira_ImportExport client = e.UserState as Business.Spira_ImportExport;

				if (e.Error == null && client != null)
				{
					//Get the parent project..
					TreeViewArtifact nodeProject = client.ClientNode.ArtifactParentProject;

					switch (client.ClientNode.ArtifactType)
					{
						case TreeViewArtifact.ArtifactTypeEnum.Incident:
							string strIncidnet = Business.StaticFuncs.getCultureResource.GetString("app_Tree_Incidents");
							string strMyIncident = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_My"), strIncidnet);
							string strUnIncident = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), strIncidnet);
							if (client.ClientNode.ArtifactName.ToLowerInvariant().Trim() == strMyIncident.ToLowerInvariant().Trim())
							{
								//Send this client off to get all incidents assigned to User.
								client.Client.Incident_RetrieveAsync(Spira_ImportExport.GenerateFilter(((SpiraProject)nodeProject.ArtifactTag).UserID, false, "IN"), Spira_ImportExport.GenerateSort(), 1, 99999, client.ClientNode);
							}
							else if (client.ClientNode.ArtifactName.ToLowerInvariant().Trim() == strUnIncident.ToLowerInvariant().Trim())
							{
								//This will only be hit if they have Unassigned incidents displayed. Otherwise,
								//  this whole section isn't run. No need to check the setting here.
								client.Client.Incident_RetrieveAsync(Spira_ImportExport.GenerateFilter(-999, false, "IN"), Spira_ImportExport.GenerateSort(), 1, 99999, client.ClientNode);
							}
							else
							{
								// Do nothing. Something wrong.
								Logger.LogMessage(METHOD,"Folder has invalid name.", System.Diagnostics.EventLogEntryType.Error);
								this._numActiveClients--;
								this.refreshTree();
							}
							break;

						case TreeViewArtifact.ArtifactTypeEnum.Requirement:
							string strRequirement = Business.StaticFuncs.getCultureResource.GetString("app_Tree_Requirements");
							string strMyRequirement = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_My"), strRequirement);
							string strUnRequirement = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), strRequirement);
							if (client.ClientNode.ArtifactName.ToLowerInvariant().Trim() == strMyRequirement.ToLowerInvariant().Trim())
							{
								//Send this client off to get all incidents assigned to User.
								client.Client.Requirement_RetrieveAsync(Spira_ImportExport.GenerateFilter(((SpiraProject)nodeProject.ArtifactTag).UserID, false, "RQ"), 1, 999999, client.ClientNode);
							}
							else if (client.ClientNode.ArtifactName.ToLowerInvariant().Trim() == strUnRequirement.ToLowerInvariant().Trim())
							{
								//This will only be hit if they have Unassigned incidents displayed. Otherwise,
								//  this whole section isn't run. No need to check the setting here.
								client.Client.Requirement_RetrieveAsync(Spira_ImportExport.GenerateFilter(-999, false, "RQ"), 1, 99999, client.ClientNode);
							}
							else
							{
								// Do nothing. Something wrong.
								Logger.LogMessage(METHOD,"Folder has invalid name.", System.Diagnostics.EventLogEntryType.Error);
								this._numActiveClients--;
								this.refreshTree();
							}
							break;

						case TreeViewArtifact.ArtifactTypeEnum.Task:
							string strTask = Business.StaticFuncs.getCultureResource.GetString("app_Tree_Tasks");
							string strMyTask = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_My"), strTask);
							string strUnTask = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), strTask);
							if (client.ClientNode.ArtifactName.ToLowerInvariant().Trim() == strMyTask.ToLowerInvariant().Trim())
							{
								//Send this client off to get all incidents assigned to User.
								client.Client.Task_RetrieveAsync(Spira_ImportExport.GenerateFilter(((SpiraProject)nodeProject.ArtifactTag).UserID, false, "TK"), Spira_ImportExport.GenerateSort(), 1, 99999, client.ClientNode);
							}
							else if (client.ClientNode.ArtifactName.ToLowerInvariant().Trim() == strUnTask.ToLowerInvariant().Trim())
							{
								//This will only be hit if they have Unassigned incidents displayed. Otherwise,
								//  this whole section isn't run. No need to check the setting here.
								client.Client.Task_RetrieveAsync(Spira_ImportExport.GenerateFilter(-999, false, "TK"), Spira_ImportExport.GenerateSort(), 1, 99999, client.ClientNode);
							}
							else
							{
								// Do nothing. Something wrong.
								Logger.LogMessage(METHOD, "Folder has invalid name.", System.Diagnostics.EventLogEntryType.Error);
								this._numActiveClients--;
								this.refreshTree();
							}
							break;
					}
				}
				else
				{
					//Add an error node.
					if (client != null)
					{
						TreeViewArtifact treeNode = client.ClientNode;
						this.addErrorNode(ref treeNode, e.Error, StaticFuncs.getCultureResource.GetString("app_Error_RetrieveShort"));
						client.ClientNode = treeNode;

						//Refresh treeview.
						this._numActiveClients--;
						this.refreshTree();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "_client_Connection_ConnectToProjectCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when a client sent to retrieve Requirements is finished with results.</summary>
		/// <param name="sender">SoapServiceClient</param>
		/// <param name="e">Requirement_RetrieveCompletedEventArgs</param>
		private void _client_Requirement_RetrieveCompleted(object sender, Requirement_RetrieveCompletedEventArgs e)
		{
			string METHOD = CLASS + "_client_Requirement_RetrieveCompleted";
			try
			{
				//Grab parent node.
				TreeViewArtifact parentNode = e.UserState as TreeViewArtifact;

				if (parentNode != null)
				{
					parentNode.Items.Clear();

					//We got results back. Let's do something with them!
					if (e.Error == null)
					{
						foreach (RemoteRequirement requirement in e.Result)
						{
							if (!requirement.Summary)
							{
								//Make new node.
								TreeViewArtifact newNode = new TreeViewArtifact(this.refreshTreeNodeServerData);
								newNode.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Requirement;
								//newNode.TreeNode = this;
								newNode.ArtifactTag = requirement;
								newNode.ArtifactName = requirement.Name;
								newNode.ArtifactIsFolder = false;
								newNode.ArtifactId = requirement.RequirementId.Value;
								newNode.Parent = parentNode;
								newNode.DetailsOpenRequested += new EventHandler(newNode_DetailsOpenRequested);

								parentNode.Items.Add(newNode);
							}
						}
					}
					else
					{
						this.addErrorNode(ref parentNode, e.Error, StaticFuncs.getCultureResource.GetString("app_Error_RetrieveShort"));
					}
				}
				else
				{
					//No parent node. Log error, exit.
					Logger.LogMessage(METHOD, "No parent folder.", System.Diagnostics.EventLogEntryType.Error);
				}

				//Disconnect the client, subtract from the count.
				try
				{
					((SoapServiceClient)sender).Connection_DisconnectAsync();
				}
				catch { }
				parentNode.ArtifactTag = null;
				this._numActiveClients--;
				this.refreshTree();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "_client_Requirement_RetrieveCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when a client sent to retrieve Tasks is finished with results.</summary>
		/// <param name="sender">SoapServiceClient</param>
		/// <param name="e">Task_RetrieveCompletedEventArgs</param>
		private void _client_Task_RetrieveCompleted(object sender, Task_RetrieveCompletedEventArgs e)
		{
			string METHOD = CLASS + "_client_Task_RetrieveCompleted";
			try
			{
				//Grab parent node.
				TreeViewArtifact parentNode = e.UserState as TreeViewArtifact;

				if (parentNode != null)
				{
					parentNode.Items.Clear();

					//We got results back. Let's do something with them!
					if (e.Error == null)
					{
						foreach (RemoteTask task in e.Result)
						{
							//Make new node.
							TreeViewArtifact newNode = new TreeViewArtifact(this.refreshTreeNodeServerData);
							newNode.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Task;
							newNode.ArtifactTag = task;
							newNode.ArtifactName = task.Name;
							newNode.ArtifactIsFolder = false;
							newNode.ArtifactId = task.TaskId.Value;
							newNode.Parent = parentNode;
							newNode.DetailsOpenRequested += new EventHandler(newNode_DetailsOpenRequested);
							newNode.WorkTimerChanged += new EventHandler(newNode_WorkTimerChanged);

							parentNode.Items.Add(newNode);
						}
					}
					else
					{
						this.addErrorNode(ref parentNode, e.Error, StaticFuncs.getCultureResource.GetString("app_Error_RetrieveShort"));
					}
				}
				else
				{
					//No parent node. Log error, exit.
					Logger.LogMessage(METHOD, "No parent node!", System.Diagnostics.EventLogEntryType.Error);
				}

				//Disconnect the client, subtract from the count.
				try
				{
					((SoapServiceClient)sender).Connection_DisconnectAsync();
				}
				catch { }
				parentNode.ArtifactTag = null;
				this._numActiveClients--;
				this.refreshTree();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "_client_Task_RetrieveCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when a client is finished connecting.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _client_Connection_DisconnectCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			try
			{
				//We're finished disconnecting. Let's null it out.
				sender = null;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "_client_Connection_DisconnectCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#endregion

		/// <summary>Will erase all nodes in the treeview and display the 'No Solution Loaded' message.</summary>
		private void noSolutionLoaded()
		{
			try
			{
				this._solutionName = null;
				this._Projects.Clear();
				this._Projects.Add(this._nodeNoSolution);
				this.barLoading.Visibility = Visibility.Collapsed;
				this.trvProject.Cursor = System.Windows.Input.Cursors.Arrow;
				this.trvProject.Items.Refresh();
				this.btnRefresh.IsEnabled = false;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "noSolutionLoaded()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Will erase all nodes in the treeview and display the 'No Projects Loaded' message.</summary>
		private void noProjectsLoaded()
		{
			try
			{
				this._Projects.Clear();
				this._Projects.Add(this._nodeNoProjects);
				this.barLoading.Visibility = Visibility.Collapsed;
				this.trvProject.Cursor = System.Windows.Input.Cursors.Arrow;
				this.trvProject.Items.Refresh();
				this.btnRefresh.IsEnabled = false;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "noProjectsLoaded()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Refreshes tree and other items when needed.</summary>
		private void refreshTree()
		{
			try
			{
				//Calls the code to refresh the tree, and hide the progress br if necessary.
				this.trvProject.Items.Refresh();

				if (this._numActiveClients == 0)
				{
					this.barLoading.Visibility = System.Windows.Visibility.Collapsed;
					this.trvProject.Cursor = System.Windows.Input.Cursors.Arrow;
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "refreshTree()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Creates an error node in the given node.</summary>
		/// <param name="nodeToAddTo">TreeViewArtifact node to add the error to.</param>
		/// <param name="exception">Exception for the error.</param>
		/// <param name="Title">The title of the error node.</param>
		private void addErrorNode(ref TreeViewArtifact nodeToAddTo, Exception exception, String Title)
		{
			try
			{
				TreeViewArtifact errorNode = new TreeViewArtifact(this.refreshTreeNodeServerData);
				errorNode.ArtifactIsError = true;
				errorNode.ArtifactName = Title;
				errorNode.ArtifactTag = exception;
				errorNode.Parent = nodeToAddTo;

				//Clear existing, add error.
				nodeToAddTo.Items.Clear();
				nodeToAddTo.Items.Add(errorNode);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientSave_Connection_ConnectToProjectCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
