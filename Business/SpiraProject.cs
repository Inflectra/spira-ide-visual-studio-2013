using System;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business
{
	public class SpiraProject
	{
		///* Record for the SpiraProject string-type:
		///* Field	Data
		///* 1 [0]	User Name
		///* 2 [1]	User Password
		///* 3 [2]	Server URI
		///* 4 [3]	Project ID
		///* 5 [4]	Project Name
		///* 6 [5]	User ID

		//Public constants..
		public const char CHAR_FIELD = '\xE';
		public const char CHAR_RECORD = '\xF';
		public const string URL_APIADD = "/Services/v2_2/ImportExport.asmx";

		//Public properties.
		public Uri ServerURL;
		public string UserName;
		public string UserPass;
		public int UserID = -1;
		public int ProjectID;
		public string ProjectName = "Project";

		#region Initializer Methods

		/// <summary>Creates a new instance of the class.</summary>
		public SpiraProject()
		{
			//In this case, they're setting them manually, don't fiddle with anything.
		}

		/// <summary>Creates a new instance of the class.</summary>
		/// <param name="UserName">User name to connect as.</param>
		/// <param name="UserPass">Password to use for the user.</param>
		/// <param name="ServerURI">URL of the Server</param>
		/// <param name="ProjectID">Project ID</param>
		public SpiraProject(string UserName, string UserPass, Uri ServerURI, int ProjectID)
		{
			//In this case, let's pull what we can.
			this.UserName = UserName;
			this.UserPass = UserPass;
			this.ServerURL = ServerURI;
			this.ProjectID = ProjectID;

			//Get the name and userID. This will take care of the rest.
			this.refreshProject();
		}

		/// <summary>Creates a new instance of the class.</summary>
		/// <param name="UserName">User name to connect as.</param>
		/// <param name="UserPass">Password to use for the user.</param>
		/// <param name="ServerURI">URL of the Server</param>
		/// <param name="ProjectID">Project ID</param>
		/// <param name="ProjectName">Project's Name</param>
		/// <param name="UserID">User ID</param>
		public SpiraProject(string UserName, string UserPass, Uri ServerURI, int ProjectID, string ProjectName, int UserID)
		{
			//In this case, let's pull what we can.
			this.UserName = UserName;
			this.UserPass = UserPass;
			this.ServerURL = ServerURI;
			this.ProjectID = ProjectID;
			this.ProjectName = ProjectName;
			this.UserID = UserID;

			//Get the name and userID. This will take care of the rest.
			this.refreshProject();
		}
		#endregion

		#region Public Methods
		/// <summary>Gets the display string for the object.</summary>
		/// <returns>String containing the server name and project name or project ID if error.</returns>
		public override string ToString()
		{
			this.refreshProject();

			string retString = this.ProjectName + " [" + this.ServerURL.Host + "]";

			return retString;
		}

		/// <summary>Compares this SpiraProject to the specified project. True if the projects are the same. False if not.</summary>
		/// <param name="inProject">SpiraProject to compare this object against.</param>
		/// <returns>True if the settings are the same, false if not.</returns>
		public bool IsEqualTo(SpiraProject inProject)
		{
			if (this.ServerURL.AbsoluteUri.Trim() == inProject.ServerURL.AbsoluteUri.Trim() &&
				this.UserName == inProject.UserName &&
				this.ProjectID == inProject.ProjectID)
				return true;
			else
				return false;
		}
		#endregion

		#region Static Members
		/// <summary>Converts an encoded string into a SpiraProject. Uses \xE for field seperation.</summary>
		/// <param name="inString">The string to decode.</param>
		/// <returns>A SpiraProject based on the given string, or null if error.</returns>
		public static SpiraProject GenerateFromString(string inString)
		{
			//Get the values.
			string[] values = inString.Split(SpiraProject.CHAR_FIELD);

			try
			{
				string userName = values[0];
				string userPass = values[1];
				Uri serverUri = new Uri(values[2]);
				int projectID = int.Parse(values[3]);
				string projectName = values[4];
				int userID = int.Parse(values[5]);

				return new SpiraProject(userName, userPass, serverUri, projectID, projectName, userID);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>Returns a savable string from a project object.</summary>
		/// <param name="inProject">The SpiraProject to convert.</param>
		/// <returns>A string of the fields seperated by the field seperator.</returns>
		public static string GenerateToString(SpiraProject inProject)
		{
			if (inProject != null)
			{
				return inProject.UserName + SpiraProject.CHAR_FIELD +
					inProject.UserPass + SpiraProject.CHAR_FIELD +
					inProject.ServerURL.AbsoluteUri + SpiraProject.CHAR_FIELD +
					inProject.ProjectID.ToString() + SpiraProject.CHAR_FIELD +
					inProject.ProjectName + SpiraProject.CHAR_FIELD +
					inProject.UserID.ToString();
			}
			else
			{
				return null;
			}
		}

		/// <summary>Compares to SpiraProjects and returns whether they are equal or not.</summary>
		/// <param name="inProject1">One of the SpiraProjects to compare.</param>
		/// <param name="inProject2">Another SpiraProject to compare.</param>
		/// <returns>True if the settings are the same, false if not.</returns>
		public bool IsEqual(SpiraProject inProject1, SpiraProject inProject2)
		{
			if (inProject2.ServerURL.AbsoluteUri.Trim() == inProject1.ServerURL.AbsoluteUri.Trim() &&
				inProject2.UserName == inProject1.UserName &&
				inProject2.ProjectID == inProject1.ProjectID)
				return true;
			else
				return false;
		}
		#endregion

		/// <summary>Refreshes project name if necessary.</summary>
		private void refreshProject()
		{
			//Only run if it's not set.
			if ((this.ProjectName == "Project" || string.IsNullOrEmpty(this.ProjectName)) || this.UserID == -1)
			{
				try
				{
					ImportExportClient client = StaticFuncs.CreateClient(this.ServerURL.ToString());

					//Connect and get the project information.
					if (client.Connection_Authenticate2(this.UserName, this.UserPass, StaticFuncs.getCultureResource.GetString("app_ReportName")))
					{
						//Connected, get project and user information.
						this.ProjectName = client.Project_RetrieveById(this.ProjectID).Name;
						this.UserID = client.User_RetrieveByUserName(this.UserName).UserId.Value;
					}
				}
				catch
				{ }
			}
		}
	}
}
