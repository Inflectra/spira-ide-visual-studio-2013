using System.Windows.Controls;
using System;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.Forms
{
	/// <summary>
	/// Interaction logic for cntlTTipRequirement.xaml
	/// </summary>
	public partial class cntlTTipRequirement : UserControl
	{
		private TreeViewArtifact _dataitem;

		public cntlTTipRequirement(TreeViewArtifact dataItem)
		{
			this._dataitem = dataItem;

			//Initialize.
			InitializeComponent();

			//Set images.
			this.imgProject.Source = StaticFuncs.getImage("imgProject", new System.Windows.Size(16, 16)).Source;
			this.imgIncident.Source = StaticFuncs.getImage("imgIncident", new System.Windows.Size(16, 16)).Source;
			this.imgRelease.Source = StaticFuncs.getImage("imgRelease", new System.Windows.Size(16, 16)).Source;
			//Set strings.
			this.txtItemId.Text = StaticFuncs.getCultureResource.GetString("app_Requirement_ID") + ":";
			this.txtProject.Text = StaticFuncs.getCultureResource.GetString("app_Project") + ":";
			this.txtOwner.Text = StaticFuncs.getCultureResource.GetString("app_General_Owner") + ":";
			this.txtStatus.Text = StaticFuncs.getCultureResource.GetString("app_General_Status") + ":";
			this.txtPlanned.Text = StaticFuncs.getCultureResource.GetString("app_Requirement_PlannedEff") + ":";
			this.txtAssRel.Text = StaticFuncs.getCultureResource.GetString("app_General_Release") + ":";
		}

		///// <summary>Creates a new instance of the control, setting the data item.</summary>
		///// <param name="ArtifactData">The TreeViewArtifact data item.</param>
		//public cntlTTipRequirement(TreeViewArtifact ArtifactData)
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
			this.dataStatusName.Text = ((dynamic)this.DataItem.ArtifactTag).StatusName;
			this.dataImportanceName.Text = ((dynamic)this.DataItem.ArtifactTag).ImportanceName;
			this.dataPlannedEffort.Text = this.getTime(((dynamic)this.DataItem.ArtifactTag).PlannedEffort);
			this.dataVer.Text = ((dynamic)this.DataItem.ArtifactTag).ReleaseVersionNumber + " " + this.getVersionIdNumber(((dynamic)this.DataItem.ArtifactTag).ReleaseId);
		}

		/// <summary>Takes a nullable integer, and returns a useful time-string.</summary>
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
