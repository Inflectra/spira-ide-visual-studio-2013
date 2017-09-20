















namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.PropertyGrid.Editors
{
	public class PrimitiveTypeCollectionEditor : TypeEditor<PrimitiveTypeCollectionControl>
	{
		protected override void SetControlProperties()
		{
			Editor.BorderThickness = new System.Windows.Thickness(0);
			Editor.Content = "(Collection)";
		}

		protected override void SetValueDependencyProperty()
		{
			ValueProperty = PrimitiveTypeCollectionControl.ItemsSourceProperty;
		}

		protected override void ResolveValueBinding(PropertyItem propertyItem)
		{
			Editor.ItemsSourceType = propertyItem.PropertyType;
			Editor.ItemType = propertyItem.PropertyType.GetGenericArguments()[0];
			base.ResolveValueBinding(propertyItem);
		}
	}
}
