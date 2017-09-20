using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Properties;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms
{
	/// <summary>
	/// Interaction logic for wpfServerProject.xaml
	/// </summary>
	public partial class frmAssignProject : Window
	{
		#region Internal Vars
		private bool _hasChanged = false;
		private string _solname;
		#endregion

		/// <summary>Creates a new instance of the form. Should call setSpiraProjects() and setSoltion() after calling this.</summary>
		internal frmAssignProject()
		{
			try
			{
				InitializeComponent();

				//Title
				this.Title = Business.StaticFuncs.getCultureResource.GetString("strAssignProjectTitle");
				//Icon
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

				//Load logos and images.
				this.imgLogo.Source = Business.StaticFuncs.getImage("imgLogo", new Size()).Source;
				this.imgLogo.Height = imgLogo.Source.Height;
				this.imgLogo.Width = imgLogo.Source.Width;
				this.btnNew.Content = Business.StaticFuncs.getImage("imgAdd", new Size(16, 16));
				this.btnEdit.Content = Business.StaticFuncs.getImage("imgEdit", new Size(16, 16));
				this.btnDelete.Content = Business.StaticFuncs.getImage("imgDelete", new Size(16, 16));

				//Set events.
				this.btnEdit.IsEnabledChanged += new DependencyPropertyChangedEventHandler(btn_IsEnabledChanged);
				this.btnDelete.IsEnabledChanged += new DependencyPropertyChangedEventHandler(btn_IsEnabledChanged);
				this.btnNew.Click += new RoutedEventHandler(btnNewEdit_Click);
				this.btnEdit.Click += new RoutedEventHandler(btnNewEdit_Click);
				this.btnAdd.Click += new RoutedEventHandler(btnAdd_Click);
				this.btnRemove.Click += new RoutedEventHandler(btnRemove_Click);
				this.btnSave.Click += new RoutedEventHandler(btnSave_Click);
				this.btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
				this.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
				this.btn_IsEnabledChanged(this.btnEdit, new DependencyPropertyChangedEventArgs());
				this.btn_IsEnabledChanged(this.btnDelete, new DependencyPropertyChangedEventArgs());

				//Get the solution name & load items.
				if (Business.StaticFuncs.GetEnvironment.Solution.IsOpen)
					this._solname = (string)Business.StaticFuncs.GetEnvironment.Solution.Properties.Item("Name").Value;
				else
					this._solname = null;
				this.loadSolution();

				//Set the caption.
				this.setRTFCaption();

				//Load available projects into Selection box.

			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, ".ctor()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#region Form Events
		/// <summary>Hit when the user decides they want to delete a project.</summary>
		/// <param name="sender">btnDelete</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string sureMsg = "Are you sure you want to delete project:" + Environment.NewLine + ((Business.SpiraProject)this.lstAvailProjects.SelectedItem).ToString();
				MessageBoxResult userSure = MessageBox.Show(sureMsg, "Remove Project?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

				if (userSure == MessageBoxResult.Yes)
				{
					this._hasChanged = true;
					this.lstAvailProjects.Items.RemoveAt(this.lstAvailProjects.SelectedIndex);
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnDelete_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the Cencel button on the form is clicked.</summary>
		/// <param name="sender">btnCancel</param>
		/// <param name="e">Event Args</param>
		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (this._hasChanged)
				{
					MessageBoxResult OKtoClose = MessageBox.Show("Lose changes made to settings?", "Close?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Cancel);
					if (OKtoClose == MessageBoxResult.Yes)
					{
						this.DialogResult = false;
					}
				}
				else
				{
					this.DialogResult = false;
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnCancel_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the Save button is clicked.</summary>
		/// <param name="sender">btnSave</param>
		/// <param name="e">Event Args</param>
		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//The user wanted to save. Save our settings and raise the closeform event.
				string selProjects = "";
				SerializableList<string> availProjects = new SerializableList<string>();

				foreach (Business.SpiraProject proj in this.lstAvailProjects.Items)
				{
					availProjects.Add(Business.SpiraProject.GenerateToString(proj));
				}
				foreach (Business.SpiraProject proj in this.lstSelectProjects.Items)
				{
					string projstr = Business.SpiraProject.GenerateToString(proj);
					availProjects.Add(projstr);
					selProjects += projstr + SpiraProject.CHAR_RECORD;
				}
				selProjects = selProjects.Trim(SpiraProject.CHAR_RECORD);

				//Save the selected projects to the settings.
				if (!string.IsNullOrWhiteSpace(this._solname))
				{
					if (Settings.Default.AssignedProjects.ContainsKey(this._solname))
						Settings.Default.AssignedProjects[this._solname] = selProjects;
					else
						Settings.Default.AssignedProjects.Add(this._solname, selProjects);
				}

				Settings.Default.AllProjects = availProjects;
				Settings.Default.Save();

				this.DialogResult = true;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnSave_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Remove the selected projects from the 'Selected' list.</summary>
		/// <param name="sender">The btnRemove</param>
		/// <param name="e">Event Args</param>
		private void btnRemove_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//Like removing duplicates, but in reverse.
				List<Business.SpiraProject> SelProjs = new List<Business.SpiraProject>();
				foreach (Business.SpiraProject proj in this.lstSelectProjects.SelectedItems)
				{
					this.lstAvailProjects.Items.Add(proj);
					SelProjs.Add(proj);
				}

				foreach (Business.SpiraProject proj in SelProjs)
				{
					for (int i = 0; i < this.lstSelectProjects.Items.Count; )
					{
						if (proj.IsEqualTo((Business.SpiraProject)this.lstSelectProjects.Items[i]))
						{
							this.lstSelectProjects.Items.RemoveAt(i);
						}
						else
						{
							i++;
						}
					}
				}

				this._hasChanged = true;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnRemove_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user wants to assign a serverproject.</summary>
		/// <param name="sender">btnAdd</param>
		/// <param name="e">Event Args</param>
		private void btnAdd_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//Add selected items to the solution project.
				foreach (object selItem in this.lstAvailProjects.SelectedItems)
				{
					//Add it to the selected items panel.
					this.lstSelectProjects.Items.Add(selItem);
				}

				//Remove duplicates.
				this.removeDuplicates();

				this._hasChanged = true;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnAdd_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user wants to add/edit a serverproject.</summary>
		/// <param name="sender">btnNew / btnEdit</param>
		/// <param name="e">Event Args</param>
		private void btnNewEdit_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Button button = (Button)sender;

				//Create the form.
				frmNewSpiraProject frmAddProject = new frmNewSpiraProject();
				frmAddProject.Owner = this;

				if (button.Name == "btnEdit")
				{
					//Get the item selected.
					Business.SpiraProject proj = (Business.SpiraProject)this.lstAvailProjects.SelectedItem;
					frmAddProject.txbServer.Text = proj.ServerURL.AbsoluteUri;
					frmAddProject.txbUserID.Text = proj.UserName;
					frmAddProject.txbUserPass.Password = proj.UserPass;
					int projnum = frmAddProject.cmbProjectList.Items.Add(proj);
					frmAddProject.cmbProjectList.SelectedIndex = projnum;
				}

				if (frmAddProject.ShowDialog().Value)
				{
					if (frmAddProject.cmbProjectList.SelectedItem != null)
					{
						Business.SpiraProject selProject = (Business.SpiraProject)frmAddProject.cmbProjectList.SelectedItem;

						//Add it to the available list if there's no existing ones.
						bool AddToSelected = false;
						for (int i = 0; i < this.lstAvailProjects.Items.Count; )
						{
							if (((Business.SpiraProject)this.lstAvailProjects.Items[i]).IsEqualTo(selProject))
							{
								this.lstAvailProjects.Items.RemoveAt(i);
							}
							else
							{
								i++;
							}
						}
						for (int i = 0; i < this.lstSelectProjects.Items.Count; )
						{
							if (((Business.SpiraProject)this.lstSelectProjects.Items[i]).IsEqualTo(selProject))
							{
								this.lstSelectProjects.Items.RemoveAt(i);
								AddToSelected = true;
							}
							else
							{
								i++;
							}
						}

						if (AddToSelected)
						{
							this.lstSelectProjects.Items.Add(selProject);
						}
						else
						{
							this.lstAvailProjects.Items.Add(selProject);
						}
					}
					this._hasChanged = true;
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btnNewEdit_Click()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when a button IsEnabled is changed.</summary>
		/// <param name="sender">btnEdit / btnDelete</param>
		/// <param name="e">Event Args</param>
		private void btn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				Button button = (Button)sender;
				((Image)button.Content).Opacity = ((button.IsEnabled) ? 1 : .5);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "btn_IsEnabledChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when either listbox's selection changes. Sets required button states.</summary>
		/// <param name="sender">Control that sent the event.</param>
		/// <param name="e">SelectionChangedEventArgs</param>
		private void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				ListBox userInteract = (ListBox)sender;

				switch (userInteract.Name)
				{
					case "lstAvailProjects":
						{
							if (userInteract.SelectedItems.Count > 0)
							{
								this.btnAdd.IsEnabled = this.lstSelectProjects.IsEnabled;
								this.btnEdit.IsEnabled = (userInteract.SelectedItems.Count == 1);
								this.btnDelete.IsEnabled = (userInteract.SelectedItems.Count == 1);
							}
							else
							{
								this.btnAdd.IsEnabled = false;
								this.btnEdit.IsEnabled = false;
								this.btnDelete.IsEnabled = false;
							}
						}
						break;

					case "lstSelectProjects":
						{
							if (userInteract.SelectedItems.Count > 0)
							{
								this.btnRemove.IsEnabled = this.lstSelectProjects.IsEnabled;
							}
							else
							{
								this.btnRemove.IsEnabled = false;
							}
						}
						break;
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "listbox_SelectionChanged()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		/// <summary>Removes any entries in the Available list that are also in the Selected list.</summary>
		private void removeDuplicates()
		{
			try
			{
				foreach (Business.SpiraProject proj in this.lstSelectProjects.Items)
				{
					//Loop through the ones available..
					for (int i = 0; i < this.lstAvailProjects.Items.Count; )
					{
						if (((Business.SpiraProject)this.lstAvailProjects.Items[i]).IsEqualTo(proj))
						{
							this.lstAvailProjects.Items.RemoveAt(i);
						}
						else
						{
							i++;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "removeDuplicates()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Converts a string into a stream for the RTFTextBox</summary>
		private void setRTFCaption()
		{
			try
			{
				string rtfText = "";
				if (string.IsNullOrEmpty(this._solname))
				{
					rtfText = Business.StaticFuncs.getCultureResource.GetString("flowSelectNoSolution");
				}
				else
				{
					rtfText = Business.StaticFuncs.getCultureResource.GetString("flowSelectSolution");
					rtfText = rtfText.Replace("%solution%", this._solname);
				}

				this.headerCaption.Document = (FlowDocument)XamlReader.Load(new XmlTextReader(new StringReader(rtfText)));
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "setRTFCaption()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Sets the solution name for configuring the form's display.</summary>
		/// <param name="solName">The name of the currently-loaded solution, null if none open.</param>
		private void loadSolution()
		{
			try
			{
				//Load up all projects into the first listbox.
				this.lstAvailProjects.Items.Clear();
				foreach (string proj in Settings.Default.AllProjects)
					this.lstAvailProjects.Items.Add(Business.SpiraProject.GenerateFromString(proj.Trim(new char[] { SpiraProject.CHAR_RECORD, SpiraProject.CHAR_FIELD }).Trim()));

				//We have the solution name, load up the projects associated, and remove them from the available.
				if (!string.IsNullOrEmpty(this._solname))
				{
					this.lstSelectProjects.Items.Clear();
					if (Settings.Default.AssignedProjects.ContainsKey(this._solname))
					{
						string strProjs = Settings.Default.AssignedProjects[this._solname];
						if (!string.IsNullOrWhiteSpace(strProjs))
						{
							foreach (string strProj in strProjs.Split(Business.SpiraProject.CHAR_RECORD))
							{
								Business.SpiraProject Project = Business.SpiraProject.GenerateFromString(strProj);
								this.lstSelectProjects.Items.Add(Project);
							}
							//remove duplicates.
							this.removeDuplicates();
						}
					}
					this.lstSelectProjects.IsEnabled = true;
				}
				this.setRTFCaption();
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "loadSolution()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
