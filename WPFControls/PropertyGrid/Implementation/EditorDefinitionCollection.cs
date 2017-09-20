















using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.PropertyGrid
{
	public class EditorDefinitionCollection : ObservableCollection<EditorDefinition>
	{
		public EditorDefinition this[string propertyName]
		{
			get
			{
				foreach (var item in Items)
				{
					if (item.PropertiesDefinitions.Where(x => x.Name == propertyName).Any())
						return item;
				}

				return null;
			}
		}

		public EditorDefinition this[Type targetType]
		{
			get
			{
				foreach (var item in Items)
				{
					if (item.TargetType == targetType)
						return item;
				}

				return null;
			}
		}
	}
}
