using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Controls
{
	/// <summary>Interaction logic for cntrlRichTextEditor.xaml</summary>
	public partial class cntrlRichTextEditor : UserControl
	{
		/// <summary>Raised when text or style is changed.</summary>
		public event EventHandler TextChanged;

		#region Private Vars
		private bool updSelectionProcessing = false;
		#endregion

		public cntrlRichTextEditor()
		{
			InitializeComponent();

			//Populate the Font Family combobox. (Filter out bad fonts.)
			List<FontFamily> fontFamilies = new List<FontFamily>();
			foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
			{
				if (!fontFamily.Source.Contains("ExtraGlyphlets")) fontFamilies.Add(fontFamily);
			}
			this._toolFont.ItemsSource = fontFamilies;

			//Update toolbar.
			this._TextBox_Selection_Changed(this._TextBox, new EventArgs());
		}

		#region Control Events
		/// <summary>Hit when the text inside the textbox change.</summary>
		/// <param name="sender">RichTextBox</param>
		/// <param name="e">EventArgs</param>
		private void _TextBox_Selection_Changed(object sender, EventArgs e)
		{
			if (!this.updSelectionProcessing)
			{
				this.updSelectionProcessing = true;
				Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(UpdateToolbarSelection), null);
			}
		}

		/// <summary>Hit whenever text in the box is changed.</summary>
		/// <param name="sender">RichTextBox</param>
		/// <param name="e">TextChangedEventArgs</param>
		private void _TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			//Throw the external event.
			if (this.TextChanged != null)
			{
				this.TextChanged(this, new EventArgs());
			}
			e.Handled = true;
		}

		/// <summary>Hit when the user changes font faces.</summary>
		/// <param name="sender">ComboBox</param>
		/// <param name="e">SelectionChangedEventArgs</param>
		private void _toolFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!this.updSelectionProcessing)
			{
				if (this._toolFont.SelectedItem != null)
					this._TextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, (FontFamily)this._toolFont.SelectedItem);

				this._TextBox_Selection_Changed(this._TextBox, new EventArgs());

				if (this._TextBox.Focusable)
					this._TextBox.Focus();

			}
		}

		/// <summary>Hit when the user tabs out of the Font Size box.</summary>
		/// <param name="sender">ComboBox</param>
		/// <param name="e">KeyboardFocusChangedEventArgs</param>
		private void _toolSize_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (!this.updSelectionProcessing)
			{
				double tryNum;
				if (double.TryParse(this._toolSize.Text, out tryNum))
				{
					this._TextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, tryNum);
					this._TextBox_Selection_Changed(this._TextBox, new EventArgs());
				}
			}
		}

		/// <summary>Hit when the user changes the selection of the FontSize.</summary>
		/// <param name="sender">ComboBox</param>
		/// <param name="e">SelectionChangedEventArgs</param>
		private void _toolSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!this.updSelectionProcessing && this.IsLoaded)
			{
				try
				{
					this._TextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, (string)((ComboBoxItem)this._toolSize.SelectedItem).Content);
					this._TextBox_Selection_Changed(this._TextBox, new EventArgs());
					if (this._TextBox.Focusable)
						this._TextBox.Focus();
				}
				catch { }
			}
		}

		/// <summary>Hit when the user manually enters in a font size.</summary>
		/// <param name="sender">ComboBox</param>
		/// <param name="e">KeyEventArgs</param>
		private void _toolSize_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (!(e.Key >= Key.D0 && e.Key <= Key.D9))
			{
				e.Handled = true;
				if (e.Key == Key.Return || e.Key == Key.System)
				{
					try
					{
						this._TextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, (string)this._toolSize.Text);
						this._TextBox_Selection_Changed(this._TextBox, new EventArgs());
						if (this._TextBox.Focusable)
							this._TextBox.Focus();
					}
					catch
					{
					}
				}
			}
		}

		/// <summary>Hit when the user clicks on the Toolbar button FontColor.</summary>
		/// <param name="sender">Button</param>
		/// <param name="e">RoutedEventArgs</param>
		private void _toolFontColor_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			Color? c = this.DisplayColourDialogue();
			if (c.HasValue)
			{
				SolidColorBrush colorBrush = new SolidColorBrush(c.Value);

				if (!this.updSelectionProcessing)
					this._TextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, colorBrush);
			}
		}

		/// <summary>Hit when the user clicks on the Toolbar button FontHighlight.</summary>
		/// <param name="sender">Button</param>
		/// <param name="e">RoutedEventArgs</param>
		private void _toolFontHighlight_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

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

		/// <summary>Hit when the user clicks on the Toolbar button InsertLink.</summary>
		/// <param name="sender">Button</param>
		/// <param name="e">RoutedEventArgs</param>
		private void _toolInsertLink_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			//Create the dialog with selected text.
			bool replaceText = !(this._TextBox.Selection.Start == this._TextBox.Selection.End);

			cntrlAnchorDialogue dlg = new cntrlAnchorDialogue(this._TextBox.Selection.Text);

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
		#endregion

		#region Internal Functions
		/// <summary>Called to update the status of the comboboxes and togglebuttons depending on the selected text in the textbox.</summary>
		/// <param name="args"></param>
		/// <returns></returns>
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

		/// <summary>Displays the color picker, and returns the selected color.</summary>
		/// <returns>Nullable Color</returns>
		private Color? DisplayColourDialogue()
		{
			Color? c = null;
			cntrlColorPicker form = new cntrlColorPicker();
			if (form.ShowDialog() == true)
			{
				c = form.SelectedColor;
			}
			return c;
		}
		#endregion

		#region Public Properties
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
				return Business.HTMLandXAML.HtmlFromXamlConverter.ConvertXamlToHtml(XAMLText);
			}
			set
			{
				string strFlow = Business.HTMLandXAML.HtmlToXamlConverter.ConvertHtmlToXaml(value, true);

				if (!string.IsNullOrEmpty(strFlow))
				{
					FlowDocument doc = null;

					try
					{
						using (StringReader strRead = new StringReader(strFlow))
						{
							doc = (XamlReader.Load(new XmlTextReader(strRead))) as FlowDocument;
						}
					}
					catch { }

					this._TextBox.Document = doc;
				}
				this._TextBox.CaretPosition = this._TextBox.Selection.Start.DocumentStart;
			}
		}

		/// <summary>Whether the textbox is ReadOnly or not.</summary>
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

		/// <summary>Sets the BorderThickness on the textbox. Used for when the control is in ReadONly mode.</summary>
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

		/// <summary>Gets/Sets whether the Spellcheck feature is enabled or disabled.</summary>
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
		#endregion

	}
}
