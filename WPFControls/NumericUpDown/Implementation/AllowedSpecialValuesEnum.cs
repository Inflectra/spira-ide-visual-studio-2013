















using System;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit
{
	[Flags]
	public enum AllowedSpecialValues
	{
		None = 0,
		NaN = 1,
		PositiveInfinity = 2,
		NegativeInfinity = 4,
		AnyInfinity = PositiveInfinity | NegativeInfinity,
		Any = NaN | AnyInfinity
	}
}
