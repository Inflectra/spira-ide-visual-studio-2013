using System;
using System.Windows;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.Forms
{
	/// <summary>
	/// Interaction logic for cntlTTipUser.xaml
	/// </summary>
	public partial class cntlTTipUser : UserControl
	{
		private TreeViewArtifact _dataitem;

        public cntlTTipUser(TreeViewArtifact dataItem)
		{
			this._dataitem = dataItem;

			//Initialize.
			InitializeComponent();

			//Set images.
			this.imgUser.Source = StaticFuncs.getImage("imgUser", new System.Windows.Size(16, 16)).Source;
		}

		///// <summary>Creates a new instance of the control, setting the data item.</summary>
		///// <param name="ArtifactData">The TreeViewArtifact data item.</param>
		//public cntlTTipTask(TreeViewArtifact ArtifactData)
		//    : base()
		//{
		//    this.DataItem = ArtifactData;
		//}

		/// <summary>Holds a reference to the treeviewitem we're displaying.</summary>
		public TreeViewArtifact DataItem
		{
			get
			{
				return this._dataitem;
			}
			set
			{
				this._dataitem = value;
				this.loadDisplayData();
			}
		}

		/// <summary>Loads values from our Artifact item into the display fields.</summary>
		private void loadDisplayData()
		{
			if (this.DataItem != null)
			{
				try
				{
					this.dataArtifactId.Text = this.DataItem.ArtifactId.ToString();
                    this.dataName.Text = ((SpiraTeam_Client.RemoteUser)this.DataItem.ArtifactTag).FullName;
                    this.dataLogin.Text = ((SpiraTeam_Client.RemoteUser)this.DataItem.ArtifactTag).UserName;
                    this.dataEmailAddress.Text = ((SpiraTeam_Client.RemoteUser)this.DataItem.ArtifactTag).EmailAddress;
                    this.dataDepartment.Text = ((SpiraTeam_Client.RemoteUser)this.DataItem.ArtifactTag).Department;
				}
				catch { }
			}
		}

		/// <summary>Catches when we're actually ready to display the data.</summary>
		/// <param name="e">EventArgs</param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			this.loadDisplayData();
		}

		/// <summary>Takes a nullable integer, and returs a useful time-string.</summary>
		/// <param name="Minutes">The number of minutes.</param>
		/// <returns>String formatted with the # of hours.</returns>
		private string getTime(int? Minutes)
		{
			if (Minutes.HasValue)
			{
				return "~" + Math.Round(((decimal)Minutes / 60), 0).ToString() + " " + StaticFuncs.getCultureResource.GetString("app_General_HoursAbbr");
			}
			else
				return "";
		}

		/// <summary>Takes a nullable integer, and returns just the number in string format.</summary>
		/// <param name="Number">The number to convert.</param>
		/// <returns>A string containing the number.</returns>
		private string getIdNumber(int? Number, string Prefix)
		{
			if (Number.HasValue)
				return "[" + Prefix.ToUpperInvariant().Trim() + ":" + Number.Value.ToString() + "]";
			else
				return "";
		}

		/// <summary>Takes a nullable integer, and returns just the number in string format.</summary>
		/// <param name="Number">The number to convert.</param>
		/// <returns>A string containing the number.</returns>
		private string getPercent(int? Number)
		{
			if (Number.HasValue)
				return "(" + Number.Value.ToString() + "%)";
			else
				return "";
		}

		/// <summary>Takes a nullable DateTime, and returns just the number in string format.</summary>
		/// <param name="Number">The number to convert.</param>
		/// <returns>A string containing the number.</returns>
		private string getDate(DateTime? Date)
		{
			if (Date.HasValue)
				return Date.Value.ToLongDateString();
			else
				return "";
		}

		/// <summary>Takes a nullable DateTime, and returns a bool whether or not the time has passed. Null dates always return false.</summary>
		/// <param name="Number">The date to check.</param>
		/// <returns>Boolean.</returns>
		private bool isDateTimePast(DateTime? Date)
		{
			if (Date.HasValue)
				return (Date.Value < DateTime.Now);
			else
				return false;
		}
	}
}
