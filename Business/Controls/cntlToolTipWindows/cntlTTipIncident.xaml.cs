using System.Windows.Controls;
using System;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.Forms
{
	/// <summary>
	/// Interaction logic for cntlTTipIncident.xaml
	/// </summary>
	public partial class cntlTTipIncident : UserControl
	{
		private TreeViewArtifact _dataitem;

		/// <summary>Creates a new instance of the control.</summary>
		public cntlTTipIncident(TreeViewArtifact dataItem)
		{
			//Initialize.
			InitializeComponent();

			//Load the item's data.
			this.DataItem = dataItem;

			//Set images.
			this.imgProject.Source = StaticFuncs.getImage("imgProject", new System.Windows.Size(16, 16)).Source;
			this.imgIncident.Source = StaticFuncs.getImage("imgIncident", new System.Windows.Size(16, 16)).Source;
			this.imgRelease.Source = StaticFuncs.getImage("imgRelease", new System.Windows.Size(16, 16)).Source;
			//Set strings.
			this.txtItemId.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Id") + ":";
			this.txtProject.Text = StaticFuncs.getCultureResource.GetString("app_Project") + ":";
			this.txtOwner.Text = StaticFuncs.getCultureResource.GetString("app_General_Owner") + ":";
			this.txtStatusType.Text = StaticFuncs.getCultureResource.GetString("app_Incident_StatusType") + ":";
			this.txtEstimate.Text = StaticFuncs.getCultureResource.GetString("app_General_EstEffort") + ":";
			this.txtProjected.Text = StaticFuncs.getCultureResource.GetString("app_General_ProjEffort") + ":";
			this.txtPriority.Text = StaticFuncs.getCultureResource.GetString("app_General_Priority") + ":";
			this.txtSeverity.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Severity") + ":";
			this.txtDetected.Text = StaticFuncs.getCultureResource.GetString("app_Incident_DetectedRelease") + ":";
			this.txtResolved.Text = StaticFuncs.getCultureResource.GetString("app_Incident_ResolvedRelease") + ":";
			this.txtVerified.Text = StaticFuncs.getCultureResource.GetString("app_Incident_VerifiedRelease") + ":";
		}

		///// <summary>Creates a new instance of the control, setting the data item.</summary>
		///// <param name="ArtifactData">The TreeViewArtifact data item.</param>
		//public cntlTTipIncident(TreeViewArtifact ArtifactData)
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
			this.dataArtifactId.Text = this.DataItem.ArtifactId.ToString();
			this.dataProjectName.Text = this.DataItem.ArtifactParentProject.ArtifactName;
			this.dataOwnerName.Text = ((dynamic)this.DataItem.ArtifactTag).OwnerName;
			this.dataStatusName.Text = ((dynamic)this.DataItem.ArtifactTag).IncidentStatusName;
			this.dataTypeName.Text = ((dynamic)this.DataItem.ArtifactTag).IncidentTypeName;
			this.dataEstEffort.Text = this.getTime(((dynamic)this.DataItem.ArtifactTag).EstimatedEffort);
			this.dataProjEffort.Text = this.getTime(((dynamic)this.DataItem.ArtifactTag).ProjectedEffort);
			this.dataPriorityName.Text = ((dynamic)this.DataItem.ArtifactTag).PriorityName;
			this.dataSeverityName.Text = ((dynamic)this.DataItem.ArtifactTag).SeverityName;
			this.dataDetVer.Text = ((dynamic)this.DataItem.ArtifactTag).DetectedReleaseVersionNumber + " " + this.getVersionIdNumber(((dynamic)this.DataItem.ArtifactTag).DetectedReleaseId);
			this.dataResVer.Text = ((dynamic)this.DataItem.ArtifactTag).ResolvedReleaseVersionNumber + " " + this.getVersionIdNumber(((dynamic)this.DataItem.ArtifactTag).ResolvedReleaseId);
			this.dataVerVer.Text = ((dynamic)this.DataItem.ArtifactTag).VerifiedReleaseVersionNumber + " " + this.getVersionIdNumber(((dynamic)this.DataItem.ArtifactTag).VerifiedReleaseId);
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
		private string getVersionIdNumber(int? Number)
		{
			if (Number.HasValue)
				return "[RL:" + Number.Value.ToString() + "]";
			else
				return "";
		}
	}
}
