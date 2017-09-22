using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Classes
{
    /// <summary>
    /// Contains the connection information for the current spira connection
    /// </summary>
    public static class SpiraContext
    {
        private static bool isDirty = false;
        private static Uri baseUri = null;
        private static int projectId = 0;

        /// <summary>
        /// The base Url for SpiraTeam (stored in the .sln file)
        /// </summary>
        public static Uri BaseUri
        {
            get
            {
                return baseUri;
            }
            set
            {
                baseUri = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// The login for SpiraTeam (stored in the .suo file)
        /// </summary>
        public static string Login
        {
            get;
            set;
        }

        /// <summary>
        /// The password for SpiraTeam (stored in the .suo file)
        /// </summary>
        public static string Password
        {
            get;
            set;
        }

        /// <summary>
        /// The project ID for SpiraTeam (stored in the .sln file)
        /// </summary>
        public static int ProjectId
        {
            get
            {
                return projectId;
            }
            set
            {
                projectId = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// Do we have unsaved .SLN property changes
        /// </summary>
        public static bool IsDirty
        {
            get
            {
                return isDirty;
            }
        }

        /// <summary>
        /// Do we have the values for project and/or URL initialized
        /// </summary>
        public static bool HasSolutionProps
        {
            get
            {
                return (baseUri != null || projectId > 0);
            }
        }

        /// <summary>
        /// Tells us that the data has been saved
        /// </summary>
        public static void SolutionPropsSaved()
        {
            isDirty = false;
        }

        /// <summary>
        /// Tells us that we have unsaved changes
        /// </summary>
        public static void SetUnsavedChanges()
        {
            isDirty = true;
        }
    }
}
