















using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.PropertyGrid
{
	internal interface IPropertyDefinition
	{
		ImageSource AdvancedOptionsIcon { get; }
		object AdvancedOptionsTooltip { get; }
		string Category { get; }
		string CategoryValue { get; }
		IEnumerable<IPropertyDefinition> ChildrenDefinitions { get; }
		IEnumerable<CommandBinding> CommandBindings { get; }
		string Description { get; }
		string DisplayName { get; }
		int DisplayOrder { get; }
		bool IsExpandable { get; }
		IPropertyParent PropertyParent { get; }
		object Value { get; set; }

		FrameworkElement GenerateEditorElement(PropertyItem propertyItem);
	}
}
