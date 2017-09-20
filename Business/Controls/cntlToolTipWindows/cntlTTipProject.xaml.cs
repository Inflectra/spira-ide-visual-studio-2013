using System.Windows.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.Forms
{
	/// <summary>
	/// Interaction logic for cntlTTipIncident.xaml
	/// </summary>
	public partial class cntlTTipProject : UserControl
	{
		private SpiraProject _dataitem;

		/// <summary>Creates a new instance of the control.</summary>
		public cntlTTipProject(SpiraProject dataItem)
		{
			this._dataitem = dataItem;

			//Initialize.
			InitializeComponent();

			//Set images.
			this.imgProject.Source = StaticFuncs.getImage("imgProject", new System.Windows.Size(16, 16)).Source;
			//Set strings.
			this.txtProjName.Text = StaticFuncs.getCultureResource.GetString("app_Project_Name");
			this.txtServerUrl.Text = StaticFuncs.getCultureResource.GetString("app_Project_ServerUrl");
			this.txtServerUser.Text = StaticFuncs.getCultureResource.GetString("app_Project_ServerLogin");
		}

		///// <summary>Creates a new instance of the control, setting the data item.</summary>
		///// <param name="ArtifactData">The TreeViewArtifact data item.</param>
		//public cntlTTipProject(SpiraProject ArtifactData)
		//    : base()
		//{
		//    this.DataItem = ArtifactData;
		//}

		/// <summary>Holds a reference to the treeviewitem we're displaying.</summary>
		public SpiraProject DataItem
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
			this.dataProjectName.Text = this.DataItem.ProjectName;
			this.dataServerUrl.Inlines.Add(this.DataItem.ServerURL.ToString());
			this.dataServerUrl.NavigateUri = this.DataItem.ServerURL;
			this.dataServerUserName.Text = this.DataItem.UserName;
			this.dataServerUserId.Text = this.DataItem.UserID.ToString();
		}
	}
}
