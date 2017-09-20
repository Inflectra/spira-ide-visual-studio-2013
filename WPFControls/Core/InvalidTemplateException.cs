















using System;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Core
{
	public class InvalidTemplateException : Exception
	{
		#region Constructors

		public InvalidTemplateException(string message)
			: base(message)
		{
		}

		public InvalidTemplateException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		#endregion
	}
}
