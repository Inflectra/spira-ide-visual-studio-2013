using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.newTask
{
    /// <summary>
    /// Interaction logic for newTask.xaml
    /// </summary>
    public partial class NewTaskWindow : Window
    {
        public NewTaskWindow()
        {
            InitializeComponent();
            SpiraProject project = new SpiraProject(SpiraContext.Login, SpiraContext.Password, SpiraContext.BaseUri, SpiraContext.ProjectId);
            this.Title.Text = "Create new task in " + project.ProjectName;

        }

        /// <summary>
        /// Hit when the user clicks on the 'create' button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.taskName.Text.Length > 0)
            {
                e.Handled = true;
                createNewTask();
                this.DialogResult = true;
            }
            else
            {
                e.Handled = false;
            }

        }

        /// <summary>creates the new task in the system</summary>
        private void createNewTask()
        {
            try
            {
                string taskName = this.taskName.Text;
                int taskStatus = 1, taskType = 1;

                //create new task and set its properties
                RemoteTask newTask = new RemoteTask();
                newTask.ProjectId = SpiraContext.ProjectId;
                newTask.Name = taskName;
                newTask.TaskStatusId = taskStatus;
                newTask.TaskTypeId = taskType;
                newTask.CompletionPercent = 0;
                newTask.CreationDate = DateTime.UtcNow;
                
                //create a client object
                Spira_ImportExport s = new Spira_ImportExport(SpiraContext.BaseUri.ToString(), SpiraContext.Login, SpiraContext.Password);
                s.Connect();
                s.Client.Connection_Authenticate2(SpiraContext.Login, SpiraContext.Password, "Visual Studio");
                s.Client.Connection_ConnectToProject(SpiraContext.ProjectId);

                //get the user ID of the user
                RemoteUser user = s.Client.User_RetrieveByUserName(SpiraContext.Login, false);
                int userId = (int)user.UserId;
                newTask.OwnerId = userId;

                s.Client.Task_Create(newTask);
                cntlSpiraExplorer.refresh();


            }
            catch (Exception e)
            {
                MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (this.taskName.Text.Length > 0)
            {
                createNewTask();
            }
        }
    }
}
