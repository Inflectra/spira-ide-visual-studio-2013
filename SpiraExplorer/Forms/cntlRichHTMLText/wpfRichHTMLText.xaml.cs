using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Controls
{
	/// <summary>
	/// Interaction logic for wpfRichHTMLText.xaml
	/// </summary>
	public partial class wpfRichHTMLText : UserControl
	{
		public event EventHandler TextChanged;
		bool updSelectionProcessing = false;

		/// <summary>Creates a new instance of the control.</summary>
		public wpfRichHTMLText()
		{
			InitializeComponent();

			//Hook up internal events. (Font changes, text changes.
			this._TextBox.Selection.Changed += new EventHandler(Selection_Changed);
			this._toolSize.SelectionChanged += new SelectionChangedEventHandler(_toolSize_SelectionChanged);
			this._toolSize.PreviewLostKeyboardFocus += new KeyboardFocusChangedEventHandler(_toolSize_PreviewLostKeyboardFocus);
			this._toolSize.PreviewKeyDown += new KeyEventHandler(_toolSize_PreviewKeyDown);
			this._toolFont.SelectionChanged += new SelectionChangedEventHandler(_toolFont_SelectionChanged);

			//Populate the Font Family combobox. (Filter out bad fonts.)
			List<FontFamily> fontFamilies = new List<FontFamily>();
			foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
			{
				if (!fontFamily.Source.Contains("ExtraGlyphlets"))
					fontFamilies.Add(fontFamily);
			}
			this._toolFont.ItemsSource = fontFamilies;

			this.Selection_Changed(null, null);
		}

		void _toolSize_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (!(e.Key >= Key.D0 && e.Key <= Key.D9))
			{
				e.Handled = true;
				if (e.Key == Key.Return || e.Key == Key.System)
				{
					try
					{
						this._TextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, (string)this._toolSize.Text);
						this.Selection_Changed(null, null);
						if (this._TextBox.Focusable)
							this._TextBox.Focus();
					}
					catch
					{
					}
				}
			}
		}

		void _toolFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!this.updSelectionProcessing)
			{
				if (this._toolFont.SelectedItem != null)
					this._TextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, (FontFamily)this._toolFont.SelectedItem);
				this.Selection_Changed(null, null);
				if (this._TextBox.Focusable)
					this._TextBox.Focus();

			}
		}

		private object UpdateToolbarSelection(object args)
		{
			try
			{
				TextSelection selText = this._TextBox.Selection;

				object value;
				//Get the style for the current selection.
				//  - Font size.
				value = selText.GetPropertyValue(TextElement.FontSizeProperty);
				this._toolSize.Text = (value == DependencyProperty.UnsetValue) ? "" : value.ToString();
				// - Font typeface.
				value = selText.GetPropertyValue(TextElement.FontFamilyProperty);
				if (value != DependencyProperty.UnsetValue)
					if (this._toolFont.Items.Contains((FontFamily)value))
						this._toolFont.SelectedIndex = this._toolFont.Items.IndexOf(value);
					else
						this._toolFont.Text = ((FontFamily)value).Source;
				else
					this._toolFont.Text = "";
				// - Font weight. (Bold)
				value = selText.GetPropertyValue(TextElement.FontWeightProperty);
				this._toolBold.IsChecked = (value == DependencyProperty.UnsetValue) ? false : ((FontWeight)value != FontWeights.Normal);
				// - Font Style. (Italic)
				value = selText.GetPropertyValue(TextElement.FontStyleProperty);
				this._toolItalic.IsChecked = (value == DependencyProperty.UnsetValue) ? false : ((FontStyle)value != FontStyles.Normal);
				// - Font underline.
				value = selText.GetPropertyValue(Inline.TextDecorationsProperty);
				bool isUnder = false;
				if (value != DependencyProperty.UnsetValue)
					foreach (TextDecoration text in (TextDecorationCollection)value)
					{
						if (text.Location == TextDecorationLocation.Underline)
							isUnder = true;
					}
				this._toolUnderline.IsChecked = isUnder;
				// - Font alignment
				value = selText.GetPropertyValue(Paragraph.TextAlignmentProperty);
				this._toolJusLeft.IsChecked = (value == DependencyProperty.UnsetValue) ? true : (TextAlignment)value == TextAlignment.Left;
				this._toolJusCenter.IsChecked = (value == DependencyProperty.UnsetValue) ? false : (TextAlignment)value == TextAlignment.Center;
				this._toolJusRight.IsChecked = (value == DependencyProperty.UnsetValue) ? false : (TextAlignment)value == TextAlignment.Right;
				// - Marker # Numbering
				Paragraph startParagraph = selText.Start.Paragraph;
				Paragraph endParagraph = selText.End.Paragraph;
				if (startParagraph != null && endParagraph != null &&
					(startParagraph.Parent is ListItem) && (endParagraph.Parent is ListItem) &&
					((ListItem)startParagraph.Parent).List == ((ListItem)endParagraph.Parent).List)
				{
					TextMarkerStyle markerStyle = ((ListItem)startParagraph.Parent).List.MarkerStyle;
					this._toolBullet.IsChecked = (markerStyle == TextMarkerStyle.Disc ||
											   markerStyle == TextMarkerStyle.Circle ||
											   markerStyle == TextMarkerStyle.Square ||
											   markerStyle == TextMarkerStyle.Box);
					this._toolNumber.IsChecked = (markerStyle == TextMarkerStyle.LowerRoman ||
												 markerStyle == TextMarkerStyle.UpperRoman ||
												 markerStyle == TextMarkerStyle.LowerLatin ||
												 markerStyle == TextMarkerStyle.UpperLatin ||
												 markerStyle == TextMarkerStyle.Decimal);
				}
				else
				{
					this._toolBullet.IsChecked = false;
					this._toolNumber.IsChecked = false;
				}
			}
			finally
			{
				this.updSelectionProcessing = false;
			}
			return null;
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			if (!this.updSelectionProcessing)
			{
				this.updSelectionProcessing = true;
				Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(UpdateToolbarSelection), null);
			}
		}

		void _toolSize_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (!this.updSelectionProcessing)
			{
				double tryNum;
				if (double.TryParse(this._toolSize.Text, out tryNum))
				{
					this._TextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, tryNum);
					this.Selection_Changed(null, null);
				}
			}
		}

		void _toolSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!this.updSelectionProcessing)
			{
				this._TextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, (string)((ComboBoxItem)this._toolSize.SelectedItem).Content);
				this.Selection_Changed(null, null);
				if (this._TextBox.Focusable)
					this._TextBox.Focus();
			}
		}

		/// <summary>Contents of the text box in HTML.</summary>
		public string HTMLText
		{
			get
			{
				TextRange doc = new TextRange(this._TextBox.Document.ContentStart.DocumentStart, this._TextBox.Document.ContentEnd.DocumentEnd);
				System.IO.Stream stream = new System.IO.MemoryStream();
				doc.Save(stream, DataFormats.Xaml);
				stream.Seek(0, System.IO.SeekOrigin.Begin);
				string XAMLText = "<FlowDocument>" + new System.IO.StreamReader(stream).ReadToEnd() + "</FlowDocument>";
				//string XAMLtext = new TextRange(this._TextBox.Document.ContentStart.DocumentStart, this._TextBox.Document.ContentEnd.DocumentEnd).Text;
				return HtmlFromXamlConverter.ConvertXamlToHtml(XAMLText);
			}
			set
			{
				string strFlow = HtmlToXamlConverter.ConvertHtmlToXaml(value, false);
				this._TextBox.Document.Blocks.Clear();
				if (!string.IsNullOrEmpty(strFlow))
				{
					this._TextBox.Selection.Load(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(strFlow)), DataFormats.Xaml);
				}
				this._TextBox.CaretPosition = this._TextBox.Selection.Start.DocumentStart;
			}
		}

		/// <summary>Whether the textbox is readonly or not.</summary>
		public bool IsReadOnly
		{
			get
			{
				return this._TextBox.IsReadOnly;
			}
			set
			{
				this._TextBox.IsReadOnly = value;
			}
		}

		/// <summary>Whether or not the toolbar is visible.</summary>
		public bool IsToolbarVisible
		{
			get
			{
				return (this._MenuBar.Visibility == Visibility.Visible);
			}
			set
			{
				this._MenuBar.Visibility = ((value) ? Visibility.Visible : Visibility.Collapsed);
			}
		}

		/// <summary>Sets the borderthickness on the textbox. Used for when the control is in readonly mode.</summary>
		public new Thickness BorderThickness
		{
			get
			{
				return this._TextBox.BorderThickness;
			}
			set
			{
				this._TextBox.BorderThickness = value;
			}
		}

		/// <summary>Hit whenever text in the box is changed.</summary>
		/// <param name="sender">Textbox</param>
		/// <param name="e">Event args</param>
		private void _TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			//Throw the external event.
			if (this.TextChanged != null)
			{
				this.TextChanged(this, new EventArgs());
			}
			e.Handled = true;
		}

		public bool IsSpellcheckEnabled
		{
			get
			{
				return this._TextBox.SpellCheck.IsEnabled;
			}
			set
			{
				this._TextBox.SpellCheck.IsEnabled = value;
			}
		}

		private void _toolFontColor_Click(object sender, RoutedEventArgs e)
		{
			Color? c = this.DisplayColourDialogue();
			if (c.HasValue)
			{
				SolidColorBrush colorBrush = new SolidColorBrush(c.Value);

				if (!this.updSelectionProcessing)
					this._TextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, colorBrush);
			}
		}

		private void _toolFontHighlight_Click(object sender, RoutedEventArgs e)
		{
			Color? c = this.DisplayColourDialogue();
			if (c.HasValue)
			{
				SolidColorBrush colorBrush = new SolidColorBrush(c.Value);

				if (!this.updSelectionProcessing)
				{
					this._TextBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, colorBrush);
				}
			}
		}

		private void _toolInsertLink_Click(object sender, RoutedEventArgs e)
		{
			//Create the dialog with selected text.
			bool replaceText = !(this._TextBox.Selection.Start == this._TextBox.Selection.End);

			wpfAnchorDialogue dlg = new wpfAnchorDialogue(this._TextBox.Selection.Text);

			//Show the dialog.
			if (dlg.ShowDialog().Value)
			{
				try
				{
					TextPointer insert = this._TextBox.Selection.Start;
					if (replaceText)
					{
						this._TextBox.Selection.Load(new System.IO.MemoryStream(Encoding.UTF8.GetBytes("")), DataFormats.Text);
						this._TextBox.CaretPosition = insert;
					}

					this._TextBox.BeginChange();
					//Create the link. 
					Hyperlink hl = new Hyperlink(new Run(dlg.LinkText), insert);
					hl.NavigateUri = new Uri(dlg.Url);

					//Insert the link. 
					this._TextBox.EndChange();
				}
				catch
				{ }
			}
		}

		/// <summary>Displays the colour picker, and returns the selected colour.</summary>
		/// <returns>Nullable Color</returns>
		private Color? DisplayColourDialogue()
		{
			Color? c = null;
			wpfColorPickerDialog form = new wpfColorPickerDialog();
			if (form.ShowDialog() == true)
			{
				c = form.SelectedColor;
			}
			return c;
		}
	}
}
