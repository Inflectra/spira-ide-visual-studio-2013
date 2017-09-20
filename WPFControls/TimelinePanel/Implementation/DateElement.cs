















using System;
using System.Windows;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit
{
	internal sealed class DateElement : IComparable<DateElement>
	{
		#region Members

		private readonly int _originalIndex;

		#endregion //Members

		#region Fields

		public readonly UIElement Element;
		public readonly DateTime Date;
		public readonly DateTime DateEnd;
		public Rect PlacementRectangle;

		public int Column = 0;
		public int ColumnSpan = 1;

		#endregion //Fields

		#region Constructors

		internal DateElement(UIElement element, DateTime date, DateTime dateEnd)
			: this(element, date, dateEnd, -1)
		{
		}

		internal DateElement(UIElement element, DateTime date, DateTime dateEnd, int originalIndex)
		{
			Element = element;
			Date = date;
			DateEnd = dateEnd;
			_originalIndex = originalIndex;
		}

		#endregion //Constructors

		#region Base Class Overrides

		public override string ToString()
		{
			var fe = Element as FrameworkElement;
			if (fe == null)
				return base.ToString();

			if (fe.Tag != null)
				return fe.Tag.ToString();

			return fe.Name;
		}

		#endregion //Base Class Overrides

		#region Methods

		public int CompareTo(DateElement d)
		{
			int dateCompare = Date.CompareTo(d.Date);
			if (dateCompare != 0)
				return dateCompare;

			if (_originalIndex >= 0)
				return (_originalIndex < d._originalIndex) ? -1 : 1;

			return -DateEnd.CompareTo(d.DateEnd);
		}

		#endregion
	}
}
