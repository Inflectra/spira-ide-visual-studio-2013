using System;
using System.Windows;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.Forms
{
	/// <summary>
	/// Interaction logic for cntlTTipTask.xaml
	/// </summary>
	public partial class cntlTTipTask : UserControl
	{
		private TreeViewArtifact _dataitem;

		public cntlTTipTask(TreeViewArtifact dataItem)
		{
			this._dataitem = dataItem;

			//Initialize.
			InitializeComponent();

			//Set images.
			this.imgProject.Source = StaticFuncs.getImage("imgProject", new System.Windows.Size(16, 16)).Source;
			this.imgIncident.Source = StaticFuncs.getImage("imgIncident", new System.Windows.Size(16, 16)).Source;
			this.imgTaskTime.Source = StaticFuncs.getImage("imgTaskTime", new System.Windows.Size(16, 16)).Source;
			//Set strings.
			this.txtItemId.Text = StaticFuncs.getCultureResource.GetString("app_Task_ID") + ":";
			this.txtProject.Text = StaticFuncs.getCultureResource.GetString("app_Project") + ":";
			this.txtOwner.Text = StaticFuncs.getCultureResource.GetString("app_General_Owner") + ":";
			this.txtPriority.Text = StaticFuncs.getCultureResource.GetString("app_General_Priority") + ":";
			this.txtRelease.Text = StaticFuncs.getCultureResource.GetString("app_Global_AssociatedRelease") + ":";
			this.txtStatus.Text = StaticFuncs.getCultureResource.GetString("app_General_Status") + ":";
			this.txtStartDate.Text = StaticFuncs.getCultureResource.GetString("app_General_StartDate") + ":";
			this.txtEndDate.Text = StaticFuncs.getCultureResource.GetString("app_General_EndDate") + ":";
			this.tstProjEff.Text = StaticFuncs.getCultureResource.GetString("app_General_ProjEffort") + ":";
			this.tstActEff.Text = StaticFuncs.getCultureResource.GetString("app_General_ActEffort") + ":";
			this.tstRemEff.Text = StaticFuncs.getCultureResource.GetString("app_General_RemEffort") + ":";
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
					this.dataProjectName.Text = this.DataItem.ArtifactParentProject.ArtifactName;
					this.dataOwnerName.Text = ((dynamic)this.DataItem.ArtifactTag).OwnerName;
					this.dataPriorityName.Text = ((dynamic)this.DataItem.ArtifactTag).TaskPriorityName;
					this.dataRel.Text = ((dynamic)this.DataItem.ArtifactTag).ReleaseVersionNumber + " " + this.getIdNumber(((dynamic)this.DataItem.ArtifactTag).ReleaseId, "RL");
					this.dataStatusPerName.Text = ((dynamic)this.DataItem.ArtifactTag).TaskStatusName + " " + this.getPercent(((dynamic)this.DataItem.ArtifactTag).CompletionPercent);
					this.dataStartDate.Text = this.getDate(((dynamic)this.DataItem.ArtifactTag).StartDate);
					this.dataEndDate.Text = this.getDate(((dynamic)this.DataItem.ArtifactTag).EndDate);
					this.dataPrjEffort.Text = this.getTime(((dynamic)this.DataItem.ArtifactTag).ProjectedEffort);
					this.dataActEffort.Text = this.getTime(((dynamic)this.DataItem.ArtifactTag).ActualEffort);
					this.dataRemEffort.Text = this.getTime(((dynamic)this.DataItem.ArtifactTag).RemainingEffort);

					//Set any flag colors.
					//- Start Date
					if (this.isDateTimePast(((dynamic)this.DataItem.ArtifactTag).StartDate))
					{
						if ((int)(((dynamic)this.DataItem.ArtifactTag).TaskStatusId) == 1)
						{
							this.dataStartDate.FontWeight = FontWeights.Bold;
							this.dataStartDate.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkRed);
						}
					}
					//- End Date
					if (this.isDateTimePast(((dynamic)this.DataItem.ArtifactTag).EndDate))
					{
						this.dataEndDate.FontWeight = FontWeights.Bold;
						this.dataEndDate.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkRed);
					}
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
