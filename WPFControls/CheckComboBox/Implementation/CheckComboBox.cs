















using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Core.Utilities;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit
{
	[TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
	public class CheckComboBox : Primitives.Selector
	{
		private const string PART_Popup = "PART_Popup";

		#region Members

		private ValueChangeHelper _displayMemberPathValuesChangeHelper;
		private Popup _popup;
		private List<object> _initialValue = new List<object>();

		#endregion

		#region Constructors

		static CheckComboBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckComboBox), new FrameworkPropertyMetadata(typeof(CheckComboBox)));
		}

		public CheckComboBox()
		{
			Keyboard.AddKeyDownHandler(this, OnKeyDown);
			Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
			_displayMemberPathValuesChangeHelper = new ValueChangeHelper(this.OnDisplayMemberPathValuesChanged);
		}

		#endregion //Constructors

		#region Properties

		#region Text

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CheckComboBox), new UIPropertyMetadata(null));
		public string Text
		{
			get
			{
				return (string)GetValue(TextProperty);
			}
			set
			{
				SetValue(TextProperty, value);
			}
		}

		#endregion

		#region IsDropDownOpen

		public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(CheckComboBox), new UIPropertyMetadata(false, OnIsDropDownOpenChanged));
		public bool IsDropDownOpen
		{
			get
			{
				return (bool)GetValue(IsDropDownOpenProperty);
			}
			set
			{
				SetValue(IsDropDownOpenProperty, value);
			}
		}

		private static void OnIsDropDownOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			CheckComboBox comboBox = o as CheckComboBox;
			if (comboBox != null)
				comboBox.OnIsDropDownOpenChanged((bool)e.OldValue, (bool)e.NewValue);
		}

		protected virtual void OnIsDropDownOpenChanged(bool oldValue, bool newValue)
		{
			if (newValue)
			{
				_initialValue.Clear();
				foreach (object o in SelectedItems)
					_initialValue.Add(o);
			}
			else
			{
				_initialValue.Clear();
			}

			// TODO: Add your property changed side-effects. Descendants can override as well.
		}

		#endregion //IsDropDownOpen

		#region MaxDropDownHeight

		public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register("MaxDropDownHeight", typeof(double), typeof(CheckComboBox), new UIPropertyMetadata(SystemParameters.PrimaryScreenHeight / 3.0, OnMaxDropDownHeightChanged));
		public double MaxDropDownHeight
		{
			get
			{
				return (double)GetValue(MaxDropDownHeightProperty);
			}
			set
			{
				SetValue(MaxDropDownHeightProperty, value);
			}
		}

		private static void OnMaxDropDownHeightChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			CheckComboBox comboBox = o as CheckComboBox;
			if (comboBox != null)
				comboBox.OnMaxDropDownHeightChanged((double)e.OldValue, (double)e.NewValue);
		}

		protected virtual void OnMaxDropDownHeightChanged(double oldValue, double newValue)
		{
			// TODO: Add your property changed side-effects. Descendants can override as well.
		}

		#endregion

		#endregion //Properties

		#region Base Class Overrides

		protected override void OnSelectedValueChanged(string oldValue, string newValue)
		{
			base.OnSelectedValueChanged(oldValue, newValue);
			UpdateText();
		}

		protected override void OnDisplayMemberPathChanged(string oldDisplayMemberPath, string newDisplayMemberPath)
		{
			base.OnDisplayMemberPathChanged(oldDisplayMemberPath, newDisplayMemberPath);
			this.UpdateDisplayMemberPathValuesBindings();
		}

		protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
		{
			base.OnItemsSourceChanged(oldValue, newValue);
			this.UpdateDisplayMemberPathValuesBindings();
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_popup != null)
				_popup.Opened -= Popup_Opened;

			_popup = GetTemplateChild(PART_Popup) as Popup;

			if (_popup != null)
				_popup.Opened += Popup_Opened;
		}

		#endregion //Base Class Overrides

		#region Event Handlers

		private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
		{
			CloseDropDown(false);
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (!IsDropDownOpen)
			{
				if (KeyboardUtilities.IsKeyModifyingPopupState(e))
				{
					IsDropDownOpen = true;
					// Popup_Opened() will Focus on ComboBoxItem.
					e.Handled = true;
				}
			}
			else
			{
				if (KeyboardUtilities.IsKeyModifyingPopupState(e))
				{
					CloseDropDown(true);
					e.Handled = true;
				}
				else if (e.Key == Key.Enter)
				{
					CloseDropDown(true);
					e.Handled = true;
				}
				else if (e.Key == Key.Escape)
				{
					SelectedItems.Clear();
					foreach (object o in _initialValue)
						SelectedItems.Add(o);
					CloseDropDown(true);
					e.Handled = true;
				}
			}
		}

		private void Popup_Opened(object sender, EventArgs e)
		{
			UIElement item = ItemContainerGenerator.ContainerFromItem(SelectedItem) as UIElement;
			if ((item == null) && (Items.Count > 0))
				item = ItemContainerGenerator.ContainerFromItem(Items[0]) as UIElement;
			if (item != null)
				item.Focus();
		}

		#endregion //Event Handlers

		#region Methods

		private void UpdateDisplayMemberPathValuesBindings()
		{
			_displayMemberPathValuesChangeHelper.UpdateValueSource(ItemsCollection, this.DisplayMemberPath);
		}

		private void OnDisplayMemberPathValuesChanged()
		{
			this.UpdateText();
		}

		private void UpdateText()
		{
#if VS2008
      string newValue = String.Join( Delimiter, SelectedItems.Cast<object>().Select( x => GetItemDisplayValue( x ).ToString() ).ToArray() ); 
#else
			string newValue = String.Join(Delimiter, SelectedItems.Cast<object>().Select(x => GetItemDisplayValue(x)));
#endif

			if (String.IsNullOrEmpty(Text) || !Text.Equals(newValue))
				Text = newValue;
		}

		protected object GetItemDisplayValue(object item)
		{
			if (!String.IsNullOrEmpty(DisplayMemberPath))
			{
				var property = item.GetType().GetProperty(DisplayMemberPath);
				if (property != null)
					return property.GetValue(item, null);
			}

			return item;
		}

		private void CloseDropDown(bool isFocusOnComboBox)
		{
			if (IsDropDownOpen)
				IsDropDownOpen = false;
			ReleaseMouseCapture();

			if (isFocusOnComboBox)
				Focus();
		}

		#endregion //Methods
	}
}
