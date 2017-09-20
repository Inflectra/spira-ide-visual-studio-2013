using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Inflectra.Global;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business
{
	public partial class Spira_ImportExport : IDisposable
	{
		#region Internal Vars
		private bool _isConnected = false;
		private SpiraTeam_Client.ImportExportClient _client;
		private Exception _lastException;
		private ClientStateEnum _state = ClientStateEnum.Idle;
		private string _server;
		private string _user;
		private string _password;
		#endregion

		#region Events
		public event EventHandler<ConnectionException> ConnectionError;
		public event EventHandler ConnectionReady;
		#endregion

		public Spira_ImportExport(string server, string user, string password)
		{
			//Assign internal vars..
			this._server = server;
			this._user = user;
			this._password = password;

			this._client = Business.StaticFuncs.CreateClient(this._server);

			//Hook up events.
			this._client.Connection_Authenticate2Completed += new EventHandler<SpiraTeam_Client.Connection_Authenticate2CompletedEventArgs>(_client_Connection_Authenticate2Completed);
		}

		/// <summary>Call to initiate connection to server.</summary>
		public void Connect()
		{
			//Connect.
			this._state = ClientStateEnum.Working;
			this._lastException = null;

			this._client.Connection_Authenticate2Async(this._user, this._password, Business.StaticFuncs.getCultureResource.GetString("app_ReportName"), this);
		}

		/// <summary>Hit when the client's finished connecting.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _client_Connection_Authenticate2Completed(object sender, SpiraTeam_Client.Connection_Authenticate2CompletedEventArgs e)
		{
			if (e.Error == null)
			{
				this._isConnected = true;
				if (this.ConnectionReady != null)
					this.ConnectionReady(this, new EventArgs());
			}
			else
			{
				this._lastException = e.Error;
				this._state = ClientStateEnum.Idle;
				if (this.ConnectionError != null)
					this.ConnectionError(this, new ConnectionException(e.Error));
			}
		}

		#region Properties
		/// <summary>The client to communicate with the server with.</summary>
		public SpiraTeam_Client.ImportExportClient Client
		{
			get
			{
				return this._client;
			}
		}

		/// <summary>The last error that occured.</summary>
		public Exception ClientLastError
		{
			get
			{
				return this._lastException;
			}
		}

		/// <summary>The current status of the client.</summary>
		public ClientStateEnum ClientStatus
		{
			get
			{
				if (this._client.State == CommunicationState.Opened)
					return ClientStateEnum.Working;
				else if (this._client.State == CommunicationState.Faulted)
					return ClientStateEnum.Error;
				else
					return ClientStateEnum.Idle;
			}
		}

		/// <summary>Additional storage for reference on the client.</summary>
		public TreeViewArtifact ClientNode
		{
			get;
			set;
		}
		#endregion

		#region Event Classes
		public class ConnectionException : EventArgs
		{
			private Exception _exception;

			public ConnectionException(Exception ex)
			{
				this._exception = ex;
			}

			public Exception error
			{
				get
				{
					return this._exception;
				}
			}
		}
		#endregion

		#region IDisposable Members

		/// <summary>Called when we're finished with the client. Verifies that the client is disconnected.</summary>
		void IDisposable.Dispose()
		{
			if (this._client != null)
			{
				try
				{
					this._client.Connection_Disconnect();
				}
				catch { }
				finally
				{
					this._client = null;
				}
			}
		}

		#endregion

		public enum ClientStateEnum
		{
			Idle = 0,
			Working = 1,
			Error = 2
		}

		/// <summary>Allows the use of Self-Signed SSL certificates</summary>
		public class PermissiveCertificatePolicy
		{
			string subjectName = "";
			static PermissiveCertificatePolicy currentPolicy;

			public PermissiveCertificatePolicy(string subjectName)
			{
				this.subjectName = subjectName;
				ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertValidate);
			}

			public static void Enact(string subjectName)
			{
				currentPolicy = new PermissiveCertificatePolicy(subjectName);
			}

			public bool RemoteCertValidate(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
			{
				if (cert.Subject == subjectName || subjectName == "")
				{
					return true;
				}

				return false;
			}
		}

		#region Static Functions
		/// <summary>Generates a filter for pulling information from the Spira server.</summary>
		/// <param name="UserNum">The user number of the data to pull for.</param>
		/// <param name="IncludeComplete">Whether or not to include completed/closed items.</param>
		/// <param name="IncTypeCode">The string type code of the artifact. "TK", "IN", "RQ"</param>
		/// <returns>A RemoteFilter set.</returns>
		public static List<RemoteFilter> GenerateFilter(int UserNum, bool IncludeComplete, string IncTypeCode)
		{
			try
			{
				RemoteFilter userFilter = new RemoteFilter() { PropertyName = "OwnerId", IntValue = UserNum };
				RemoteFilter statusFilter = new RemoteFilter();
				if (!IncludeComplete)
				{
					switch (IncTypeCode.ToUpperInvariant())
					{
						case "IN":
							{
								statusFilter = new RemoteFilter() { PropertyName = "IncidentStatusId", IntValue = -2 };
							}
							break;

						case "TK":
							{
								MultiValueFilter multiValue = new MultiValueFilter();
								multiValue.Values = new List<int> { 1, 2, 4, 5 };
								statusFilter = new RemoteFilter() { PropertyName = "TaskStatusId", MultiValue = multiValue };
							}
							break;

						case "RQ":
							{
								MultiValueFilter multiValue = new MultiValueFilter();
								multiValue.Values = new List<int> { 1, 2, 3, 5, 7 };
								statusFilter = new RemoteFilter() { PropertyName = "ScopeLevelId", MultiValue = multiValue };
							}
							break;
					}
				}

				return new List<RemoteFilter> { userFilter, statusFilter };
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex);
				return null;
			}
		}

		/// <summary>Generates a RemoteSort to be used on client calls.</summary>
		/// <returns>RemoteSort</returns>
		public static RemoteSort GenerateSort()
		{
			RemoteSort sort = new RemoteSort();
			sort.PropertyName = "UserId";
			sort.SortAscending = true;

			return sort;
		}
		#endregion

	}
}
