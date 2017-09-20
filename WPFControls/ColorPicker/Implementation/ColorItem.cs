















using System.Windows.Media;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit
{
	public class ColorItem
	{
		public Color Color
		{
			get;
			set;
		}
		public string Name
		{
			get;
			set;
		}

		public ColorItem(Color color, string name)
		{
			Color = color;
			Name = name;
		}
	}
}
