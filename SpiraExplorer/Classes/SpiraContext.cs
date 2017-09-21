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
        /// <summary>
        /// The base Url for SpiraTeam (stored in the .sln file)
        /// </summary>
        public static Uri BaseUri
        {
            get;
            set;
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
            get;
            set;
        }

    }
}
