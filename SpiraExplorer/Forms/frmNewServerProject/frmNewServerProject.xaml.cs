using System;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Classes;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms
{
	/// <summary>
	/// Interaction logic for frmNewSpiraProject.xaml
	/// </summary>
	public partial class frmNewSpiraProject : Window
	{
		#region Internal Vars
		private SoapServiceClient _client;
		#endregion

		public frmNewSpiraProject()
		{
			try
			{
				InitializeComponent();

				//Set the title & icon.
				this.Title = Business.StaticFuncs.getCultureResource.GetString("strNewSpiraProject");
				try
				{
					System.Drawing.Icon ico = (System.Drawing.Icon)Business.StaticFuncs.getCultureResource.GetObject("icoLogo");
					MemoryStream icoStr = new MemoryStream();
					ico.Save(icoStr);
					icoStr.Seek(0, SeekOrigin.Begin);
					this.Icon = BitmapFrame.Create(icoStr);
				}
				catch (Exception ex)
				{
					Logger.LogMessage(ex);
				}

				//Set initial colors and form status.
				this.barProg.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(Business.StaticFuncs.getCultureResource.GetString("app_Colors_StyledBarNormal"));
				this.barProg.IsIndeterminate = false;
				this.barProg.Value = 0;
				this.grdAvailProjs.IsEnabled = false;
				this.grdEntry.IsEnabled = true;
				this.btnConnect.Tag = false;
				this.btnConnect.Click += new RoutedEventHandler(btnConnect_Click);
				int num = this.cmbProjectList.Items.Add("-- No Projects Available --");
				this.cmbProjectList.SelectedIndex = num;
				this.cmbProjectList.SelectionChanged += new SelectionChangedEventHandler(cmbProjectList_SelectionChanged);

                //See if we have initial values to load in for this solution
                if (SpiraContext.HasSolutionProps)
                {
                    this.txbServer.Text = SpiraContext.BaseUri.ToString();
                    if (!String.IsNullOrEmpty(SpiraContext.Login) && !String.IsNullOrEmpty(SpiraContext.Password))
                    {
                        this.txbUserID.Text = SpiraContext.Login;
                        this.txbUserPass.Password = SpiraContext.Password;

                        //Start the connections.
                        this.barProg.IsIndeterminate = true;
                        this.barProg.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(StaticFuncs.getCultureResource.GetString("app_Colors_StyledBarColor"));
                        this.grdEntry.IsEnabled = false;
                        this.btnConnect.Content = "_Cancel";
                        this.btnConnect.Tag = true;
                        this.txtStatus.Text = "Connecting to server...";
                        this.cmbProjectList.Items.Clear();
                        this.grdAvailProjs.IsEnabled = false;

                        //Create new client.
                        this._client = StaticFuncs.CreateClient(this.txbServer.Text.Trim());
                        this._client.Connection_Authenticate2Completed += new EventHandler<Connection_Authenticate2CompletedEventArgs>(_client_CommunicationFinished);
                        this._client.User_RetrieveByUserNameCompleted += new EventHandler<User_RetrieveByUserNameCompletedEventArgs>(_client_CommunicationFinished);
                        this._client.Project_RetrieveCompleted += new EventHandler<Project_RetrieveCompletedEventArgs>(_client_CommunicationFinished);

                        this._client.Connection_Authenticate2Async(this.txbUserID.Text, this.txbUserPass.Password, StaticFuncs.getCultureResource.GetString("app_ReportName"));

                    }
                }
            }
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "InitializeComponent()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the project list selection is changed.</summary>
		/// <param name="sender">cmbProjectList</param>
		/// <param name="e">SelectionChangedEventArgs</param>
		private void cmbProjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				//If they selected a valid project, let them save.
				if (this.cmbProjectList.SelectedItem != null)
				{
					if (this.cmbProjectList.SelectedItem.GetType() == typeof(Business.SpiraProject))
					{
						this.btnSave.IsEnabled = true;
					}
					else
					{
						this.btnSave.IsEnabled = false;
					}
				}
				else
				{
					this.btnSave.IsEnabled = false;
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "cmbProjectList_SelectionChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user user clicks the Connect button.</summary>
		/// <param name="sender">btnConnect</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnConnect_Click(object sender, RoutedEventArgs e)
		{
			if (e != null)
				e.Handled = true;

			try
			{
				bool tag = (bool)this.btnConnect.Tag;
				if (tag)
				{
					this._client = null;

					//Set form.
					this.grdEntry.IsEnabled = true;
					this.barProg.IsIndeterminate = false;
					this.barProg.Value = 0;
					this.btnConnect.Content = "_Get Projects";
					this.btnConnect.Tag = false;
					this.txtStatus.Text = "";
					this.txtStatus.ToolTip = null;
				}
				else
				{
					if (this.txbServer.Text.ToLowerInvariant().EndsWith(".asmx") || this.txbServer.Text.ToLowerInvariant().EndsWith(".aspx") || this.txbServer.Text.ToLowerInvariant().EndsWith(".svc"))
						MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_NewProject_URLErrorMessage"), StaticFuncs.getCultureResource.GetString("app_NewProject_URLError"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
					else
					{
						//Start the connections.
						this.barProg.IsIndeterminate = true;
						this.barProg.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(StaticFuncs.getCultureResource.GetString("app_Colors_StyledBarColor"));
						this.grdEntry.IsEnabled = false;
						this.btnConnect.Content = "_Cancel";
						this.btnConnect.Tag = true;
						this.txtStatus.Text = "Connecting to server...";
						this.cmbProjectList.Items.Clear();
						this.grdAvailProjs.IsEnabled = false;

						//Create new client.
						this._client = StaticFuncs.CreateClient(this.txbServer.Text.Trim());
						this._client.Connection_Authenticate2Completed += new EventHandler<Connection_Authenticate2CompletedEventArgs>(_client_CommunicationFinished);
						this._client.User_RetrieveByUserNameCompleted += new EventHandler<User_RetrieveByUserNameCompletedEventArgs>(_client_CommunicationFinished);
						this._client.Project_RetrieveCompleted += new EventHandler<Project_RetrieveCompletedEventArgs>(_client_CommunicationFinished);

						this._client.Connection_Authenticate2Async(this.txbUserID.Text, this.txbUserPass.Password, StaticFuncs.getCultureResource.GetString("app_ReportName"));
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnConnect_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when communication is finished with the server.</summary>
		/// <param name="sender">SoapServiceClient</param>
		/// <param name="e">EventArgs</param>
		private void _client_CommunicationFinished(object sender, AsyncCompletedEventArgs e)
		{
			try
			{
				if (e.Error == null)
				{
					try
					{
						if (e.GetType() == typeof(Connection_Authenticate2CompletedEventArgs))
						{
							Connection_Authenticate2CompletedEventArgs evt = e as Connection_Authenticate2CompletedEventArgs;
							if (evt.Result)
							{
								this.txtStatus.Text = "Getting user information...";
								this._client.User_RetrieveByUserNameAsync(this.txbUserID.Text, false);
							}
							else
							{
								//Failed login.
								this.btnConnect_Click(null, null);
								//Just act like they canceled the service, then set error flag.
								this.barProg.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(StaticFuncs.getCultureResource.GetString("app_Colors_StyledBarError"));
								this.barProg.Value = 1;
								this.txtStatus.Text = "Invalid username or password.";
							}
						}
						else if (e.GetType() == typeof(User_RetrieveByUserNameCompletedEventArgs))
						{
							User_RetrieveByUserNameCompletedEventArgs evt = e as User_RetrieveByUserNameCompletedEventArgs;
							if (evt != null)
							{
								this.txtStatus.Text = "Getting Projects...";
								this.txbUserNum.Text = evt.Result.UserId.ToString();
								this._client.Project_RetrieveAsync();
							}
							else
								throw new Exception("Results are null.");
						}
						else if (e.GetType() == typeof(Project_RetrieveCompletedEventArgs))
						{
							this.cmbProjectList.Items.Clear();

							Project_RetrieveCompletedEventArgs evt = e as Project_RetrieveCompletedEventArgs;

							//Load projects here.
							if (evt != null && evt.Result.Count > 0)
							{
                                SpiraProject matchingProject = null;
                                foreach (RemoteProject RemoteProj in evt.Result)
								{
									Business.SpiraProject Project = new Business.SpiraProject();
									Project.ProjectId = RemoteProj.ProjectId.Value;
									Project.ServerURL = new Uri(this.txbServer.Text);
									Project.UserName = this.txbUserID.Text;
									Project.UserPass = this.txbUserPass.Password;
									Project.UserID = int.Parse(this.txbUserNum.Text);

									this.cmbProjectList.Items.Add(Project);

                                    if (SpiraContext.ProjectId == Project.ProjectId)
                                    {
                                        matchingProject = Project;
                                    }
                                }

                                //Select one if necessary
                                if (matchingProject != null)
                                {
                                    this.cmbProjectList.SelectedItem = matchingProject;
                                }
                                else
                                {
                                    this.cmbProjectList.SelectedIndex = 0;
                                }
								this.grdAvailProjs.IsEnabled = true;
							}
							else
							{
								int num = this.cmbProjectList.Items.Add("-- No Projects Available --");
								this.cmbProjectList.SelectedIndex = num;
							}

							//Reset form.
							this.btnConnect_Click(null, null);
						}
					}
					catch (Exception ex)
					{
						Logger.LogMessage(ex);
						//Reset form.
						this.btnConnect_Click(null, null);
						//Just act like they canceled the service, then set error flag.
						this.barProg.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(StaticFuncs.getCultureResource.GetString("app_Colors_StyledBarError"));
						this.barProg.Value = 1;
						this.txtStatus.Text = "Error connecting.";
						this.txtStatus.ToolTip = ex.Message;
					}
				}
				else
				{
					Logger.LogMessage(e.Error);
					//Reset form.
					this.btnConnect_Click(null, null);
					//Just act like they canceled the service, then set error flag.
					this.barProg.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(StaticFuncs.getCultureResource.GetString("app_Colors_StyledBarError"));
					this.barProg.Value = 1;
					this.txtStatus.Text = "Could not connect!";
					this.txtStatus.ToolTip = e.Error.Message;
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "_client_CommunicationFinished()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user wants to save their selection.</summary>
		/// <param name="sender">btnSave</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				e.Handled = true;

                //Set the SpiraContext values
                SpiraContext.BaseUri = new Uri(this.txbServer.Text.Trim());
                SpiraContext.Login = this.txbUserID.Text.Trim();
                SpiraContext.Password = this.txbUserPass.Password.Trim();
                if (this.cmbProjectList.SelectedItem.GetType() == typeof(Business.SpiraProject))
                {
                    Business.SpiraProject spiraProject = (Business.SpiraProject)cmbProjectList.SelectedItem;
                    SpiraContext.ProjectId = spiraProject.ProjectId;
                }

                this.DialogResult = true;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnSave_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user wants to cancel.</summary>
		/// <param name="sender">btnCancel</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				e.Handled = true;
				this.DialogResult = false;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnCancel_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}

		}
	}
}
