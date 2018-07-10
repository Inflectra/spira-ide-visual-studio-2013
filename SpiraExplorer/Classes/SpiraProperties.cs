using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms;
using System.ComponentModel;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Classes
{
    public class SpiraProperties
    {
        protected cntlSpiraExplorer _explorerWindow;

        public SpiraProperties(cntlSpiraExplorer explorerWindow)
        {
            this._explorerWindow = explorerWindow;
        }

        #region Unused Project Properties

        /*[DisplayName("Project ID")]
        [Category("Project Properties")]
        [Description("SpiraTeam Project ID")]
        public string ProjectId
        {
            get
            {
                return "PR:" + (object)SpiraContext.ProjectId;
            }
        }

        [Category("Project Properties")]
        [Description("SpiraTeam Project Name")]
        [DisplayName("Project Name")]
        public string ProjectName
        {
            get
            {
                if (this._explorerWindow != null)
                    return this._explorerWindow.CurrentProject;
                return (string)null;
            }
        }

        [DisplayName("Spira URL")]
        [Category("Project Properties")]
        [Description("SpiraTeam URL")]
        public string SpiraUrl
        {
            get
            {
                return SpiraContext.BaseUri.ToString();
            }
        }*/

        #endregion

        [Category("Current Artifact")]
        [Description("The name of the currently selected artifact")]
        [DisplayName("Name")]
        public string ArtifactName
        {
            get
            {
                if (this._explorerWindow != null && this._explorerWindow.CurrentArtifact != null)
                    return this._explorerWindow.CurrentArtifact.ArtifactName;
                return (string)null;
            }
        }

        [Description("The ID of the currently selected artifact")]
        [DisplayName("Artifact ID")]
        [Category("Current Artifact")]
        public string ArtifactId
        {
            get
            {
                if (this._explorerWindow != null && this._explorerWindow.CurrentArtifact != null)
                    return this._explorerWindow.CurrentArtifact.ArtifactIDDisplay;
                return (string)null;
            }
        }

        [Category("Current Artifact")]
        [DisplayName("Description")]
        [Description("The description of the currently selected artifact")]
        public string ArtifactDescription
        {
            get
            {
                if (this._explorerWindow != null && this._explorerWindow.CurrentArtifact != null)
                {
                    object artifactTag = this._explorerWindow.CurrentArtifact.ArtifactTag;
                    if (artifactTag != null)
                    {
                        if (artifactTag is RemoteIncident)
                            return ((RemoteIncident)artifactTag).Description.HtmlRenderAsPlainText();
                        if (artifactTag is RemoteRequirement)
                            return ((RemoteRequirement)artifactTag).Description.HtmlRenderAsPlainText();
                        if (artifactTag is RemoteTask)
                            return ((RemoteTask)artifactTag).Description.HtmlRenderAsPlainText();
                        if (artifactTag is RemoteUser)
                            return ((RemoteUser)artifactTag).EmailAddress.HtmlRenderAsPlainText();
                    }
                }
                return (string)null;
            }
        }

        [Category("Current Artifact")]
        [DisplayName("Type")]
        [Description("The type of the currently selected artifact")]
        public string ArtifactType
        {
            get
            {
                if (this._explorerWindow != null && this._explorerWindow.CurrentArtifact != null)
                {
                    object artifactTag = this._explorerWindow.CurrentArtifact.ArtifactTag;
                    if (artifactTag != null)
                    {
                        if (artifactTag is RemoteIncident)
                            return ((RemoteIncident)artifactTag).IncidentTypeName;
                        if (artifactTag is RemoteRequirement)
                            return ((RemoteRequirement)artifactTag).RequirementTypeName;
                        if (artifactTag is RemoteTask)
                            return ((RemoteTask)artifactTag).TaskTypeName;
                        if (artifactTag is RemoteUser)
                            return "Contact";
                    }
                }
                return (string)null;
            }
        }

        [Description("The status of the currently selected artifact")]
        [DisplayName("Status")]
        [Category("Current Artifact")]
        public string ArtifactStatus
        {
            get
            {
                if (this._explorerWindow != null && this._explorerWindow.CurrentArtifact != null)
                {
                    object artifactTag = this._explorerWindow.CurrentArtifact.ArtifactTag;
                    if (artifactTag != null)
                    {
                        if (artifactTag is RemoteIncident)
                            return ((RemoteIncident)artifactTag).IncidentStatusName;
                        if (artifactTag is RemoteRequirement)
                            return ((RemoteRequirement)artifactTag).StatusName;
                        if (artifactTag is RemoteTask)
                            return ((RemoteTask)artifactTag).TaskStatusName;
                        if (artifactTag is RemoteUser)
                            return "N/A";
                    }
                }
                return (string)null;
            }
        }

        [DisplayName("Priority")]
        [Category("Current Artifact")]
        [Description("The priority of the currently selected artifact")]
        public string ArtifactPriority
        {
            get
            {
                if (this._explorerWindow != null && this._explorerWindow.CurrentArtifact != null)
                {
                    object artifactTag = this._explorerWindow.CurrentArtifact.ArtifactTag;
                    if (artifactTag != null)
                    {
                        if (artifactTag is RemoteIncident)
                            return ((RemoteIncident)artifactTag).PriorityName;
                        if (artifactTag is RemoteRequirement)
                            return ((RemoteRequirement)artifactTag).ImportanceName;
                        if (artifactTag is RemoteTask)
                            return ((RemoteTask)artifactTag).TaskPriorityName;
                        if (artifactTag is RemoteUser)
                            return "N/A";
                    }
                }
                return (string)null;
            }
        }
    }
}