















using System;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Core
{
	public class InvalidContentException : Exception
	{
		#region Constructors

		public InvalidContentException(string message)
			: base(message)
		{
		}

		public InvalidContentException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		#endregion
	}
}
