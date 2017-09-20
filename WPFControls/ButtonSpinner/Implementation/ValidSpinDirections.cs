















using System;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit
{
	/// <summary>
	/// Represents spin directions that are valid.
	/// </summary>
	[Flags]
	public enum ValidSpinDirections
	{
		/// <summary>
		/// Can not increase nor decrease.
		/// </summary>
		None = 0,

		/// <summary>
		/// Can increase.
		/// </summary>
		Increase = 1,

		/// <summary>
		/// Can decrease.
		/// </summary>
		Decrease = 2
	}
}
