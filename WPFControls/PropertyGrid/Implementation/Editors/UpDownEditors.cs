















using System;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Primitives;
namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.PropertyGrid.Editors
{
	public class UpDownEditor<TEditor, TType> : TypeEditor<TEditor> where TEditor : UpDownBase<TType>, new()
	{
		protected override void SetControlProperties()
		{
			Editor.BorderThickness = new System.Windows.Thickness(0);
		}
		protected override void SetValueDependencyProperty()
		{
			ValueProperty = UpDownBase<TType>.ValueProperty;
		}
	}

	public class ByteUpDownEditor : UpDownEditor<ByteUpDown, byte?> { }

	public class DecimalUpDownEditor : UpDownEditor<DecimalUpDown, decimal?> { }

	public class DoubleUpDownEditor : UpDownEditor<DoubleUpDown, double?>
	{
		protected override void SetControlProperties()
		{
			base.SetControlProperties();
			Editor.AllowInputSpecialValues = AllowedSpecialValues.Any;
		}
	}

	public class IntegerUpDownEditor : UpDownEditor<IntegerUpDown, int?> { }

	public class LongUpDownEditor : UpDownEditor<LongUpDown, long?> { }

	public class ShortUpDownEditor : UpDownEditor<ShortUpDown, short?> { }

	public class SingleUpDownEditor : UpDownEditor<SingleUpDown, float?>
	{
		protected override void SetControlProperties()
		{
			base.SetControlProperties();
			Editor.AllowInputSpecialValues = AllowedSpecialValues.Any;
		}
	}

	public class DateTimeUpDownEditor : UpDownEditor<DateTimeUpDown, DateTime?> { }

}
