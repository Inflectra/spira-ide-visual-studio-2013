using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Controls;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Controls
{
	/// <summary>
	/// Interaction logic for cntlCustomProps.xaml
	/// </summary>
	public partial class cntlCustomProps : UserControl
	{
		/// <summary>Our remote artifact, holds the custom property values.</summary>
		private RemoteArtifact item;
		/// <summary>The defined custom properties.</summary>
		private List<RemoteCustomProperty> propertyDefinitions;
		/// <summary>All the custom lists we have defined.</summary>
		private List<RemoteCustomList> listDefinitions;
		/// <summary>List of all project users that we can select from.</summary>
		private List<RemoteProjectUser> listUsers;
		/// <summary>Holds the remote releases to display.</summary>
		private List<RemoteRelease> listReleases;
		/// <summary>Holds the numbers of the fields that are required.</summary>
		private List<int> reqdFields = new List<int>();
		/// <summary>Holds the number of the fields that are required by the workflow.</summary>
		private List<int> reqgWorkflow = new List<int>();
		/// <summary>Stores the labels with the associated control number.</summary>
		private Dictionary<int, TextBlock> LabelControls = new Dictionary<int, TextBlock>();

		/// <summary>The number of control columns.</summary>
		private int numCols = 2;

		/// <summary>Holds any user-specified column widths.</summary>
		Dictionary<int, GridLength> colWidths = new Dictionary<int, GridLength>();

		public event EventHandler<EventArgs> ControlChanged;

		/// <summary>Create an instance of the class.</summary>
		public cntlCustomProps()
		{
			InitializeComponent();
			this.reqdFields = new List<int>();

			//Set initial defaults.
			this.LabelHorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			this.LabelVerticalAlignment = System.Windows.VerticalAlignment.Top;
			this.ControlHorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
			this.ControlVerticalAlignment = System.Windows.VerticalAlignment.Top;
		}

		#region Private Functions
		/// <summary>Load the data into the control.</summary>
		private void DataBindFields()
		{
			#region Control Creation
			//Create our columns, first.
			this.ClearData();
			bool isLabelCol = true;
			for (int i = 1; i <= (this.numCols * 2); i++)
			{
				//Get the col width..
				GridLength width;
				if (this.colWidths.ContainsKey(i))
				{
					width = this.colWidths[i];
				}
				else
				{
					if (isLabelCol) width = GridLength.Auto;
					else width = new GridLength(.5, GridUnitType.Star);
				}
				this.grdContent.ColumnDefinitions.Add(new ColumnDefinition() { Width = width });

				//Add a seperator row, if needed!
				if (!isLabelCol && i < (this.numCols * 2))
				{
					GridLength sepWidth = new GridLength(10, GridUnitType.Pixel);
					if (this.SeperatorWidth.Equals(GridLength.Auto))
						sepWidth = new GridLength(10, GridUnitType.Pixel);
					else
						sepWidth = this.SeperatorWidth;

					this.grdContent.ColumnDefinitions.Add(new ColumnDefinition() { Width = sepWidth });
				}

				//Flip switch.
				isLabelCol = !isLabelCol;
			}

			//Go through the field definitions, and create the fields.
			int current_rowNum = -1;
			int current_colNum = (this.numCols * 3) - 1;
			CurrentColumnTypeEnum colType = CurrentColumnTypeEnum.Label;
			this.LabelControls.Clear();

			//Create the fields, set initial (default) values..
			foreach (RemoteCustomProperty prop in this.propertyDefinitions)
			{
				if (current_colNum >= ((this.numCols * 3) - 1))
				{
					//Advance/Reset counters..
					current_rowNum++;
					current_colNum = 0;

					//Add the new row.
					this.grdContent.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
				}

				//Create the field..
				Control propControl = null;
				TextBlock propLabel = new TextBlock();

				//If the item is required..
				if (this.isPropertyRequired(prop))
				{
					propLabel.FontWeight = FontWeights.Bold;
					if (!this.reqdFields.Contains(prop.CustomPropertyId.Value))
						this.reqdFields.Add(prop.CustomPropertyId.Value);
				}
				else
				{
					propLabel.FontWeight = FontWeights.Normal;
					if (this.reqdFields.Contains(prop.CustomPropertyId.Value))
						this.reqdFields.Remove(prop.CustomPropertyId.Value);
				}

				bool fullRowCont = false;
				switch (prop.CustomPropertyTypeId)
				{
					#region Text & URL
					case 1: //Text field.
					case 9: //URL field.
						//Check for richtext, first..
						bool isRich = false;
						if (prop.CustomPropertyTypeId == 1 &&
							prop.Options != null &&
							prop.Options.Where(op => op.CustomPropertyOptionId == 4).Count() == 1)
						{
							isRich = this.getBoolFromValue(prop.Options.Where(op => op.CustomPropertyOptionId == 4).Single());
						}

						//Create controls..
						if (isRich)
						{
							propControl = new cntrlRichTextEditor();
							((cntrlRichTextEditor)propControl).MinHeight = 150;
							((cntrlRichTextEditor)propControl).TextChanged += textBox_TextChanged;
							fullRowCont = true;
						}
						else
						{
							propControl = new TextBox();
							((TextBox)propControl).TextChanged += textBox_TextChanged;
						}

						//Set MaxLegnth
						if (!isRich)
						{
							int? max = this.getPropertyMaxLength(prop);
							if (max.HasValue && max.Value > 0)
								((TextBox)propControl).MaxLength = max.Value;
						}

						//Get the artifact's custom prop, if there is one, if not, set default.
						if (this.item != null)
						{
							RemoteArtifactCustomProperty custProp = this.getItemsCustomProp(prop);
							string value = "";
							if (custProp != null)
								value = custProp.StringValue;

							if (isRich)
								((cntrlRichTextEditor)propControl).HTMLText = value;
							else
								((TextBox)propControl).Text = value;

						}
						else
						{
							if (isRich)
								((cntrlRichTextEditor)propControl).HTMLText = (string)this.getPropertyDefaultValue(prop);
							else
								((TextBox)propControl).Text = (string)this.getPropertyDefaultValue(prop);
						}
						break;
					#endregion

					#region Integer
					case 2: //Integer field.
						propControl = new IntegerUpDown();
						((IntegerUpDown)propControl).ValueChanged += numberUpDown_ValueChanged;

						//Set Min & Max.
						((IntegerUpDown)propControl).Maximum = (int?)this.getPropertyMaxValue(prop);
						((IntegerUpDown)propControl).Minimum = (int?)this.getPropertyMinValue(prop);

						//Get the artifact's custom prop, if there is one. If not, set default.
						if (this.item != null)
						{
							RemoteArtifactCustomProperty custProp = this.getItemsCustomProp(prop);
							if (custProp != null)
								((IntegerUpDown)propControl).Value = custProp.IntegerValue;
							else
								((IntegerUpDown)propControl).Value = null;
						}
						else
						{
							((IntegerUpDown)propControl).Value = (int?)this.getPropertyDefaultValue(prop);
						}
						break;
					#endregion

					#region Decimal
					case 3: //Decimal field.
						propControl = new DecimalUpDown();
						((DecimalUpDown)propControl).ValueChanged += numberUpDown_ValueChanged;

						//Set Min & Max.
						((DecimalUpDown)propControl).Maximum = (decimal?)this.getPropertyMaxValue(prop);
						((DecimalUpDown)propControl).Minimum = (decimal?)this.getPropertyMinValue(prop);

						//Set precision & Incr. Decr. amount.
						int? precision = this.getPropertyPrecision(prop);
						if (!precision.HasValue) precision = 0;
						decimal incNum = ((precision.Value < 2) ? 1 : (decimal)Math.Pow((double)10, (double)((precision - 2) * -1)));
						((DecimalUpDown)propControl).FormatString = "F" + precision.Value.ToString();
						((DecimalUpDown)propControl).Increment = incNum;

						//Get the artifact's custom prop, if there is one..
						if (this.item != null)
						{
							RemoteArtifactCustomProperty custProp = this.getItemsCustomProp(prop);
							if (custProp != null)
								((DecimalUpDown)propControl).Value = custProp.DecimalValue;
							else
								((DecimalUpDown)propControl).Value = null;

						}
						else
						{
							((DecimalUpDown)propControl).Value = (decimal?)this.getPropertyDefaultValue(prop);
						}
						break;
					#endregion

					#region Boolean
					case 4: //Boolean (Checkbox) field.
						{
							//The control..
							propControl = new CheckBox();
							((CheckBox)propControl).Unchecked += checkBox_CheckChanged;
							((CheckBox)propControl).Checked += checkBox_CheckChanged;

							//Set item's value, or default.
							if (this.item != null)
							{
								RemoteArtifactCustomProperty custProp = this.getItemsCustomProp(prop);
								if (custProp != null)
									((CheckBox)propControl).IsChecked = custProp.BooleanValue;
								else
									((CheckBox)propControl).IsChecked = false;
							}
							else
							{
								((CheckBox)propControl).IsChecked = (bool?)this.getPropertyDefaultValue(prop);
							}
						}
						break;
					#endregion

					#region Date
					case 5: //Date field.
						{
							//The control..
							propControl = new DatePicker();
							((DatePicker)propControl).SelectedDateChanged += comboBox_SelectionChanged;

							//Set item's value, or set default.
							if (this.item != null)
							{
								RemoteArtifactCustomProperty custProp = this.getItemsCustomProp(prop);
								if (custProp != null)
									if (custProp.DateTimeValue.HasValue)
									{
										((DatePicker)propControl).SelectedDate = custProp.DateTimeValue.Value.ToLocalTime();
									}
									else
									{
										((DatePicker)propControl).SelectedDate = null;
									}
								else
								{
									((DatePicker)propControl).SelectedDate = null;
								}
							}
							else
							{
								((DatePicker)propControl).SelectedDate = (DateTime?)this.getPropertyDefaultValue(prop);
								if (((DatePicker)propControl).SelectedDate.HasValue)
									((DatePicker)propControl).SelectedDate = ((DatePicker)propControl).SelectedDate.Value.ToLocalTime();
							}
						}
						break;
					#endregion

					#region List, User, Release
					case 6: //List field.
					case 8: //User field.
					case 10: //Release field.
						{
							object itemList = null;
							propControl = new ComboBox();
							((ComboBox)propControl).SelectionChanged += comboBox_SelectionChanged;

							//Get the list..
							object itemSource = null;
							if (prop.CustomPropertyTypeId == 6)
							{
								if (prop.CustomList != null)
									itemSource = prop.CustomList.Values;
								else
									itemSource = new List<RemoteCustomListValue>();
							}
							else if (prop.CustomPropertyTypeId == 8)
							{
								itemSource = this.listUsers;
							}
							else if (prop.CustomPropertyTypeId == 10)
							{
								itemSource = this.listReleases;
							}
							((ComboBox)propControl).ItemsSource = (System.Collections.IEnumerable)itemSource;

							//Add a default entry if item is not required.
							if (!this.isPropertyRequired(prop))
							{
								if (prop.CustomPropertyTypeId == 6)
									((List<RemoteCustomListValue>)itemSource).Insert(0, new RemoteCustomListValue() { CustomPropertyListId = -1, Name = "-- None --" });
								else if (prop.CustomPropertyTypeId == 8)
									((List<RemoteProjectUser>)itemSource).Insert(0, new RemoteProjectUser() { FullName = "-- None --" });
								else if (prop.CustomPropertyTypeId == 10)
									((List<RemoteRelease>)itemSource).Insert(0, new RemoteRelease() { Name = "-- None --" });
							}

							//Load up actual values, or default, if defined.
							int? selectedIndex = null;
							RemoteArtifactCustomProperty custProp = this.getItemsCustomProp(prop);
							if (custProp != null)
								selectedIndex = custProp.IntegerValue;

							if (selectedIndex.HasValue)
							{
								if (prop.CustomPropertyTypeId == 6 && prop.CustomList.Values.Count() > 0)
									((ComboBox)propControl).SelectedItem = prop.CustomList.Values.Where(clv => clv.CustomPropertyValueId == custProp.IntegerValue).SingleOrDefault();
								else if (prop.CustomPropertyTypeId == 8)
									((ComboBox)propControl).SelectedItem = this.listUsers.Where(luv => luv.UserId == custProp.IntegerValue).SingleOrDefault();
								else if (prop.CustomPropertyTypeId == 10)
									((ComboBox)propControl).SelectedItem = this.listReleases.Where(lr => lr.ReleaseId == custProp.IntegerValue).SingleOrDefault();
								else
									((ComboBox)propControl).SelectedItem = null;
							}
						}
						break;
					#endregion

					#region Multilist
					case 7: //Multilist field.
						{
							propControl = new ListBox();
							((ListBox)propControl).SelectionMode = SelectionMode.Multiple;
							((ListBox)propControl).SelectionChanged += comboBox_SelectionChanged;
							propControl.Height = 50;

							if (prop.CustomList != null)
								((ListBox)propControl).ItemsSource = prop.CustomList.Values;
							else
								((ListBox)propControl).ItemsSource = new List<RemoteCustomListValue>();

							if (this.item != null)
							{
								((ListBox)propControl).SelectedItems.Clear();
								RemoteArtifactCustomProperty custProp = this.getItemsCustomProp(prop);
								if (custProp != null && custProp.IntegerListValue != null)
								{
									foreach (int selItem in custProp.IntegerListValue)
									{
										RemoteCustomListValue lst = prop.CustomList.Values.Where(clv => clv.CustomPropertyValueId == selItem).SingleOrDefault();
										if (lst != null)
										{
											((ListBox)propControl).SelectedItems.Add(lst);
										}
									}
								}
							}
							else
							{
								//Set default.
							}
						}
						break;
					#endregion
				}

				//Now add the control to our grid.
				if (propControl != null)
				{
					//Save the custom property definition.
					propControl.Tag = prop;

					//The label properties..
					propLabel.Text = prop.Name + ":";
					TextOptions.SetTextFormattingMode(propLabel, TextFormattingMode.Display);
					TextOptions.SetTextHintingMode(propLabel, TextHintingMode.Fixed);
					TextOptions.SetTextRenderingMode(propLabel, TextRenderingMode.ClearType);
					this.LabelControls.Add(prop.CustomPropertyId.Value, propLabel);

					//propLabel.Margin = this.LabelMargin;
					propLabel.VerticalAlignment = this.LabelVerticalAlignment;
					propLabel.HorizontalAlignment = this.LabelHorizontalAlignment;
					if (this.LabelStyle != null) propLabel.Style = this.LabelStyle;

					//The other control properties..
					//propControl.Margin = this.ControlMargin;
					//propControl.Padding = this.ControlPadding;
					propControl.VerticalAlignment = this.ControlVerticalAlignment;
					propControl.HorizontalAlignment = this.ControlHorizontalAlignment;
					if (this.ControlNormalStyle != null) propControl.Style = this.ControlNormalStyle;

					// Get a new row, if necessary.
					int useCols = ((fullRowCont) ? ((this.numCols * 3) - 1) : 3);
					if ((useCols + current_colNum) > ((this.numCols * 3)))
					{
						//need to create a new row.
						current_rowNum++;
						current_colNum = 0;
						this.grdContent.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
					}

					//Add the label.
					this.grdContent.Children.Add(propLabel);
					Grid.SetColumn(propLabel, current_colNum);
					Grid.SetRow(propLabel, current_rowNum);

					//Add the control..
					this.grdContent.Children.Add(propControl);
					Grid.SetColumn(propControl, current_colNum + 1);
					Grid.SetRow(propControl, current_rowNum);
					if (fullRowCont) Grid.SetColumnSpan(propControl, (this.numCols * 3) - 1);

					//Advance the column count..
					current_colNum += ((fullRowCont) ? ((this.numCols * 3) - 1) : 3);

					//Loop and start the next property..
				}
			}
			#endregion //Control Creation

		}

		#region Internal Events
		/// <summary>Hit when a ComboBox or ListBox's values are changed.</summary>
		/// <param name="sender">Combobox, Listbox</param>
		/// <param name="e">SelectionChangedEventArgs</param>
		private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.ControlChanged != null)
				this.ControlChanged(this, new EventArgs());
		}

		/// <summary>Hit when a CheckBox is changed.</summary>
		/// <param name="sender">CheckBox</param>
		/// <param name="e">RoutedEventArgs</param>
		private void checkBox_CheckChanged(object sender, RoutedEventArgs e)
		{
			if (this.ControlChanged != null)
				this.ControlChanged(this, new EventArgs());
		}

		/// <summary>Hit when a NumberUpDown control is changed.</summary>
		/// <param name="sender">IntegerUpDown, DecimalUpDown</param>
		/// <param name="e">RoutedPropertyChangedEventArgs</param>
		private void numberUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (this.ControlChanged != null)
				this.ControlChanged(this, new EventArgs());
		}

		/// <summary>Hit when a textbox or RichTextControl is changed.</summary>
		/// <param name="sender">TextBox, RichTextBox</param>
		/// <param name="e">EventArgs</param>
		private void textBox_TextChanged(object sender, EventArgs e)
		{
			if (this.ControlChanged != null)
				this.ControlChanged(this, new EventArgs());
		}
		#endregion

		/// <summary>Gets the boolean from the value field of the option.</summary>
		/// <param name="opt">The custom propert option.</param>
		/// <returns>Boolean</returns>
		private bool getBoolFromValue(RemoteCustomPropertyOption opt)
		{
			bool retValue = false;
			try
			{
				if (!bool.TryParse(opt.Value, out retValue))
				{
					//Check for 'Y'.
					retValue = (opt.Value.ToUpperInvariant().Trim().Equals("Y"));
				}
			}
			catch { }
			return retValue;
		}

		/// <summary>Gets the boolean from the value field of the option.</summary>
		/// <param name="opt">The custom propert option.</param>
		/// <returns>Boolean</returns>
		private int? getIntFromValue(RemoteCustomPropertyOption opt)
		{
			int? retValue = null;
			try
			{
				int test = 0;
				if (int.TryParse(opt.Value, out test))
				{
					retValue = test;
				}
			}
			catch { }
			return retValue;
		}

		/// <summary>Convert the string value to a datetime. Returns null if unparseable.</summary>
		/// <param name="opt">The option to get the value from.</param>
		/// <returns>A DateTime</returns>
		private DateTime? getDateFromValue(RemoteCustomPropertyOption opt)
		{
			DateTime? retValue = null;
			try
			{
				DateTime test = DateTime.Now;
				if (DateTime.TryParse(opt.Value, out test))
				{
					retValue = test;
				}
			}
			catch { }
			return retValue;
		}

		/// <summary>Convert the string value to a datetime. Returns null if unparseable.</summary>
		/// <param name="opt">The option to get the value from.</param>
		/// <returns>A DateTime</returns>
		private decimal? getDecimalFromValue(RemoteCustomPropertyOption opt)
		{
			decimal? retValue = null;
			try
			{
				decimal test = 0;
				if (decimal.TryParse(opt.Value, out test))
				{
					retValue = test;
				}
			}
			catch { }
			return retValue;
		}

		/// <summary>Pulls a matching Artifact Custom Prop from the artifact for the given RemoteCustomProp given.</summary>
		/// <param name="custProp">The custom property to pull from the artifact.</param>
		/// <returns>The ArtifactCustomProperty.</returns>
		private RemoteArtifactCustomProperty getItemsCustomProp(RemoteCustomProperty custProp)
		{
			RemoteArtifactCustomProperty retValue = null;

			if (this.item != null && this.item.CustomProperties.Where(cp => cp.PropertyNumber == custProp.PropertyNumber).Count() == 1)
			{
				retValue = this.item.CustomProperties.Where(cp => cp.PropertyNumber == custProp.PropertyNumber).SingleOrDefault();
			}

			return retValue;
		}

		/// <summary>Send the property to tell if it's required or not.</summary>
		/// <param name="prop">The custom property.</param>
		/// <returns>True if the item's required. False if nulls (allow empty).</returns>
		private bool isPropertyRequired(RemoteCustomProperty prop)
		{
			bool retValue = false;

			if (prop.Options!=null&&prop.Options.Count(o => o.CustomPropertyOptionId == 1) > 0)
			{
				foreach (RemoteCustomPropertyOption opt in prop.Options.Where(o => o.CustomPropertyOptionId == 1))
				{
					bool? value = !this.getBoolFromValue(opt);
					if (!retValue && value.HasValue && value.Value)
						retValue = true;
				}
			}

			return retValue;
		}

		/// <summary>Gets the default value for the given property.</summary>
		/// <param name="prop">The property to get the default value for.</param>
		/// <returns>The value, otherwise 'null' if not set.</returns>
		private object getPropertyDefaultValue(RemoteCustomProperty prop)
		{
			object retValue = null;

			if (prop.Options.Count(o => o.CustomPropertyOptionId == 5) == 1)
			{
				RemoteCustomPropertyOption opt = prop.Options.SingleOrDefault(o => o.CustomPropertyOptionId == 5);
				if (opt != null)
				{
					switch (prop.CustomPropertyTypeId)
					{
						case 1: //String (Text)
						case 9: //       (URL)
							{
								retValue = opt.Value;
								break;
							}

						case 2: //Integer (Integer)
						case 6: //        (List)
						case 8: //        (User)
						case 10: //       (Release)
							{
								int? value = this.getIntFromValue(opt);
								if (value.HasValue)
									retValue = value.Value;
								break;
							}

						case 3: //Decimal (Decimal)
							{
								decimal? value = this.getDecimalFromValue(opt);
								if (value.HasValue)
									retValue = value;
								break;
							}

						case 4: //Boolean (Boolean)
							{
								bool? value = this.getBoolFromValue(opt);
								if (value.HasValue)
									retValue = value;
								break;
							}

						case 5: //Date (Date)
							{
								DateTime? value = this.getDateFromValue(opt);
								if (value.HasValue)
									retValue = value;
								break;
							}

						case 7: //List<int> (Multilist)
							{
								break;
							}
					}
				}
			}

			return retValue;
		}

		/// <summary>Gets the MaxLegnth defined for the field, if any.</summary>
		/// <param name="prop">The property to get the vlaue for.</param>
		/// <returns>The MaxLength, if set.</returns>
		private int? getPropertyMaxLength(RemoteCustomProperty prop)
		{
			int? retValue = null;

			if (prop.Options.Count(o => o.CustomPropertyOptionId == 2) == 1)
			{
				RemoteCustomPropertyOption opt = prop.Options.SingleOrDefault(o => o.CustomPropertyOptionId == 2);
				if (opt != null)
				{
					retValue = this.getIntFromValue(opt);
				}
			}

			return retValue;
		}

		/// <summary>Returns the property's defined Maximum Value.</summary>
		/// <param name="prop">The property.</param>
		/// <returns>Integer or Decimal or null.</returns>
		private object getPropertyMaxValue(RemoteCustomProperty prop)
		{
			object retValue = null;

			if (prop.Options.Count(o => o.CustomPropertyOptionId == 6) == 1)
			{
				RemoteCustomPropertyOption opt = prop.Options.SingleOrDefault(o => o.CustomPropertyOptionId == 1);
				if (prop.CustomPropertyTypeId == 2)
				{
					int? value = this.getIntFromValue(opt);
					if (value.HasValue)
						retValue = value.Value;
				}
				else if (prop.CustomPropertyTypeId == 3)
				{
					decimal? value = this.getDecimalFromValue(opt);
					if (value.HasValue)
						retValue = value.Value;
				}
			}

			return retValue;
		}

		/// <summary>Returns the property's defined Minimum Value.</summary>
		/// <param name="prop">The property.</param>
		/// <returns>Integer or Decimal or null.</returns>
		private object getPropertyMinValue(RemoteCustomProperty prop)
		{
			object retValue = null;

			if (prop.Options.Count(o => o.CustomPropertyOptionId == 7) == 1)
			{
				RemoteCustomPropertyOption opt = prop.Options.SingleOrDefault(o => o.CustomPropertyOptionId == 7);
				if (prop.CustomPropertyTypeId == 2)
				{
					int? value = this.getIntFromValue(opt);
					if (value.HasValue)
						retValue = value.Value;
				}
				else if (prop.CustomPropertyTypeId == 3)
				{
					decimal? value = this.getDecimalFromValue(opt);
					if (value.HasValue)
						retValue = value.Value;
				}
			}

			return retValue;
		}

		/// <summary>Returns the property's defined precision.</summary>
		/// <param name="prop">The property.</param>
		/// <returns>Integer if defined.</returns>
		private int? getPropertyPrecision(RemoteCustomProperty prop)
		{
			int? retValue = null;

			if (prop.Options.Count(o => o.CustomPropertyOptionId == 8) == 1)
			{
				RemoteCustomPropertyOption opt = prop.Options.SingleOrDefault(o => o.CustomPropertyOptionId == 8);
				retValue = this.getIntFromValue(opt);
			}

			return retValue;
		}

		#endregion #Private Functions

		#region Properties
		/// <summary>The number of columns to display the controls in.</summary>
		public int NumberControlColumns
		{
			get
			{
				return this.numCols;
			}
			set
			{
				if (value < 1)
					throw new ArgumentException("Number of columns must be at least 1.");

				this.numCols = value;
			}
		}

		/// <summary>Margin for the lable control.</summary>
		public Thickness LabelMargin
		{ get; set; }

		/// <summary>Horizontal Alignmnet for Labels</summary>
		public HorizontalAlignment LabelHorizontalAlignment
		{ get; set; }

		/// <summary>Vertical alignment for labels.</summary>
		public VerticalAlignment LabelVerticalAlignment
		{ get; set; }

		/// <summary>Padding for the data control.</summary>
		public Thickness ControlPadding
		{ get; set; }

		/// <summary>Margin for the data control.</summary>
		public Thickness ControlMargin
		{ get; set; }

		/// <summary>Horizontal Alignment for controls.</summary>
		public HorizontalAlignment ControlHorizontalAlignment
		{ get; set; }

		/// <summary>Vertical Alignment for controls.</summary>
		public VerticalAlignment ControlVerticalAlignment
		{ get; set; }

		/// <summary>The Style to apply to Controls.</summary>
		public Style ControlNormalStyle
		{ get; set; }

		/// <summary>The style to apply when a field is in error.</summary>
		public Style ControlErrorStyle
		{ get; set; }

		/// <summary>The style to apply to labels.</summary>
		public Style LabelStyle
		{ get; set; }

		/// <summary>The width of the seperator columns.</summary>
		public GridLength SeperatorWidth
		{ get; set; }
		#endregion #Properties

		#region Public Functions
		/// <summary>Sets the data that we're databinding against.</summary>
		/// <param name="fieldDataSource">The remote artifact like the incident or requirement.</param>
		/// <param name="fieldDataDefinition">The full definition of custom properties.</param>
		/// <param name="fieldCustomLists">Any defined custom lists.</param>
		/// <param name="delayDisplay">Whether or not to display data now, or wait until .DataBindFields() is called.</param>
		/// <param name="projectUsers">List of project users that can be selected.</param>
		/// <param name="releases">List of remote releases to display for the user.</param>
		public void SetItemsSource(RemoteArtifact fieldDataSource, List<RemoteCustomProperty> fieldDataDefinition, List<RemoteCustomList> fieldCustomLists, List<RemoteProjectUser> projectUsers, List<RemoteRelease> releases, bool delayDisplay = false)
		{
			if (fieldDataDefinition == null)
			{ throw new ArgumentNullException("fieldDataDefinition"); }
			else if (fieldDataSource == null)
			{ throw new ArgumentNullException("fieldDataSource"); }
			else if (fieldCustomLists == null)
			{ throw new ArgumentNullException("fieldCustomLists"); }
			else if (projectUsers == null)
			{ throw new ArgumentNullException("projectUsers"); }
			else if (releases == null)
			{ throw new ArgumentNullException("releases"); }


			this.item = fieldDataSource;
			this.propertyDefinitions = fieldDataDefinition;
			this.listDefinitions = fieldCustomLists;
			this.listUsers = projectUsers;
			this.listReleases = releases;

			if (!delayDisplay)
				this.DataBindFields();

		}

		/// <summary>Set the width for the specified column. Defaults to Auto and * for Label, Control columns.</summary>
		/// <param name="colNum">The column number. 1, 3, 5 are label columns, while 2, 4, 6 are control columns.</param>
		/// <param name="width">The width. null to remove the width.</param>
		public void SetColNumWidth(int colNum, GridLength width)
		{
			if (colNum > (this.numCols * 2))
			{
				throw new ArgumentException("Column number specified (" +
					colNum.ToString() +
					") cannot be higher than the number of columns in the control (" +
					(this.numCols * 2).ToString() +
					" (" +
					this.numCols.ToString() +
					" specified.)).");
			}
			else
			{
				if (width != null && this.colWidths.ContainsKey(colNum))
				{
					this.colWidths.Remove(colNum);
				}
				else if (this.colWidths.ContainsKey(colNum))
				{
					this.colWidths[colNum] = width;
				}
				else
				{
					this.colWidths.Add(colNum, width);
				}
			}
		}

		/// <summary>Clears the loaded data.</summary>
		public void ClearData()
		{
			//Clear out our records..
			this.item = null;
			this.propertyDefinitions = new List<RemoteCustomProperty>();
			this.listDefinitions = new List<RemoteCustomList>();
			this.listUsers = new List<RemoteProjectUser>();
			this.numCols = 2;

			//Remove the controls..
			for (int i = 0; i < this.grdContent.Children.Count; )
			{
				this.grdContent.Children.Remove(this.grdContent.Children[i]);
			}

			//Remove rows and columns.
			this.grdContent.RowDefinitions.Clear();
			this.grdContent.ColumnDefinitions.Clear();
		}

		/// <summary>Checks values for validity.</summary>
		/// <returns>False if there are errors, true if there are no issues and values are valid.</returns>
		/// <param name="highlightFieldsInError">If true, checking for the fields will then highlight them as being in error.</param>
		public bool PerformValidation(bool highlightFieldsInError = false)
		{
			bool retValue = true;

			//Clear all fields in error.
			if (highlightFieldsInError)
				this.ClearErrorStyles();

			//Loop through each custom property, and make sure that fields are entered properly.
			foreach (UIElement cont in this.grdContent.Children)
			{
				if (cont is Control)
				{
					if (((Control)cont).Tag is RemoteCustomProperty)
					{
						RemoteCustomProperty prop = ((RemoteCustomProperty)((Control)cont).Tag);

						switch (prop.CustomPropertyTypeId)
						{
							case 1: // Text
							case 9: // URL
								{
									string enteredValue = "";
									if (cont is cntrlRichTextEditor)
										enteredValue = ((cntrlRichTextEditor)cont).HTMLText.Trim();
									else if (cont is TextBox)
										enteredValue = ((TextBox)cont).Text.Trim();

									//Required. (Only plain text & url)
									if ((this.reqgWorkflow.Contains(prop.CustomPropertyId.Value) ||
										this.reqdFields.Contains(prop.CustomPropertyId.Value)) &&
										string.IsNullOrWhiteSpace(enteredValue) &&
										(cont is TextBox))
									{
										retValue = false;
										if (highlightFieldsInError)
											((Control)cont).Style = this.ControlErrorStyle;
									}

									//Max Length (Only plain text & url)
									if (prop.Options.Count(op => op.CustomPropertyOptionId == 2) == 1)
									{
										int? MaxLength = this.getIntFromValue(prop.Options.Single(op => op.CustomPropertyOptionId == 2));

										if (MaxLength.HasValue && MaxLength.Value > 0)
										{
											if (enteredValue.Length > MaxLength)
											{
												retValue = false;
												if (highlightFieldsInError)
													((Control)cont).Style = this.ControlErrorStyle;
											}
										}
									}

									//Min Length (Only plain text & url)
									if (prop.Options.Count(op => op.CustomPropertyOptionId == 3) == 1)
									{
										int? MinLength = this.getIntFromValue(prop.Options.Single(op => op.CustomPropertyOptionId == 3));

										if (MinLength.HasValue && MinLength.Value > 0)
										{
											if (enteredValue.Length < MinLength)
											{
												retValue = false;
												if (highlightFieldsInError)
													((Control)cont).Style = this.ControlErrorStyle;
											}
										}
									}
								}
								break;

							case 2: // Integer
							case 6: // List
							case 8: // User
								{
									int? enteredValue = null;
									if (cont is IntegerUpDown)
										enteredValue = ((IntegerUpDown)cont).Value;
									else if (cont is ComboBox)
									{
										object selectedItem = ((ComboBox)cont).SelectedItem;
										if (selectedItem != null)
										{
											if (selectedItem is RemoteUser)
											{
												enteredValue = ((RemoteUser)selectedItem).UserId;
											}
											else if (selectedItem is RemoteCustomListValue)
											{
												enteredValue = ((RemoteCustomListValue)selectedItem).CustomPropertyValueId;
											}
										}
									}

									//Required?
									if ((this.reqgWorkflow.Contains(prop.CustomPropertyId.Value) ||
										this.reqdFields.Contains(prop.CustomPropertyId.Value)) &&
										!enteredValue.HasValue)
									{
										retValue = false;
										if (highlightFieldsInError)
											((Control)cont).Style = this.ControlErrorStyle;
									}

									//Max Value (only Integer)
									if (prop.Options.Count(op => op.CustomPropertyOptionId == 6) == 1 && prop.CustomPropertyTypeId == 2)
									{
										int? MaxValue = this.getIntFromValue(prop.Options.Single(op => op.CustomPropertyOptionId == 6));

										if (MaxValue.HasValue && MaxValue.Value > 0)
										{
											if (enteredValue.HasValue && enteredValue.Value > MaxValue)
											{
												retValue = false;
												if (highlightFieldsInError)
													((Control)cont).Style = this.ControlErrorStyle;

											}
										}
									}

									//Min Value (only Integer)
									if (prop.Options.Count(op => op.CustomPropertyOptionId == 7) == 1 && prop.CustomPropertyTypeId == 2)
									{
										int? MinValue = this.getIntFromValue(prop.Options.Single(op => op.CustomPropertyOptionId == 7));

										if (MinValue.HasValue && MinValue.Value > 0)
										{
											if (enteredValue.HasValue && enteredValue.Value < MinValue)
											{
												retValue = false;
												if (highlightFieldsInError)
													((Control)cont).Style = this.ControlErrorStyle;
											}
										}
									}
								}
								break;

							case 3: // Decimal
								{
									decimal? enteredValue = null;
									if (cont is DecimalUpDown)
										enteredValue = ((DecimalUpDown)cont).Value;

									//Required?
									if ((this.reqgWorkflow.Contains(prop.CustomPropertyId.Value) ||
										this.reqdFields.Contains(prop.CustomPropertyId.Value)) &&
										!enteredValue.HasValue)
									{
										retValue = false;
										if (highlightFieldsInError)
											((Control)cont).Style = this.ControlErrorStyle;
									}

									//Max Value (only Integer)
									if (prop.Options.Count(op => op.CustomPropertyOptionId == 6) == 1 && prop.CustomPropertyTypeId == 2)
									{
										decimal? MaxValue = this.getDecimalFromValue(prop.Options.Single(op => op.CustomPropertyOptionId == 6));

										if (MaxValue.HasValue && MaxValue.Value > 0)
										{
											if (enteredValue.HasValue && enteredValue.Value > MaxValue)
											{
												retValue = false;
												if (highlightFieldsInError)
													((Control)cont).Style = this.ControlErrorStyle;
											}
										}
									}

									//Min Value (only Integer)
									if (prop.Options.Count(op => op.CustomPropertyOptionId == 7) == 1 && prop.CustomPropertyTypeId == 2)
									{
										decimal? MinValue = this.getDecimalFromValue(prop.Options.Single(op => op.CustomPropertyOptionId == 7));

										if (MinValue.HasValue && MinValue.Value > 0)
										{
											if (enteredValue.HasValue && enteredValue.Value < MinValue)
											{
												retValue = false;
												if (highlightFieldsInError)
													((Control)cont).Style = this.ControlErrorStyle;
											}
										}
									}
								}
								break;

							case 4: // Boolean
								//Boolean has no checks.
								break;

							case 5: // Date
								{
									DateTime? enteredValue = null;
									if (cont is DatePicker)
										enteredValue = ((DatePicker)cont).SelectedDate;

									//Required?
									if ((this.reqgWorkflow.Contains(prop.CustomPropertyId.Value) ||
										this.reqdFields.Contains(prop.CustomPropertyId.Value)) &&
										!enteredValue.HasValue)
									{
										retValue = false;
										if (highlightFieldsInError)
											((Control)cont).Style = this.ControlErrorStyle;
									}
								}
								break;

							case 7: // Multilist
								{
									List<int> enteredValue = new List<int>();
									if (cont is ComboBox)
									{
										//Required?
										if ((this.reqgWorkflow.Contains(prop.CustomPropertyId.Value) ||
											this.reqdFields.Contains(prop.CustomPropertyId.Value)) &&
											((ListBox)cont).SelectedItems.Count < 1)
										{
											retValue = false;
											if (highlightFieldsInError)
												((Control)cont).Style = this.ControlErrorStyle;
										}
									}
								}
								break;
						}
					}
				}
			}

			return retValue;
		}

		/// <summary>Clears any set error styles from controls.</summary>
		public void ClearErrorStyles()
		{
			foreach (UIElement cont in this.grdContent.Children)
			{
				if (cont is Control)
				{
					((Control)cont).Style = this.ControlNormalStyle;
				}
			}
		}

		/// <summary>Used to retrieve the values from the user.</summary>
		public List<RemoteArtifactCustomProperty> GetCustomProperties()
		{
			List<RemoteArtifactCustomProperty> retList = new List<RemoteArtifactCustomProperty>();

			//Load what the user entered..
			foreach (UIElement cont in this.grdContent.Children)
			{
				if (cont is Control)
				{
					if (((Control)cont).Tag is RemoteCustomProperty)
					{
						//Get the Custom Property definition.
						RemoteCustomProperty prop = ((RemoteCustomProperty)((Control)cont).Tag);
						//Now get the Artifact Custom Property defintiion.
						RemoteArtifactCustomProperty artProp = this.item.CustomProperties.Where(cp => cp.PropertyNumber == prop.PropertyNumber).SingleOrDefault();
						if (artProp == null)
						{
							artProp = new RemoteArtifactCustomProperty();
							artProp.Definition = prop;
							artProp.PropertyNumber = prop.PropertyNumber;
						}

						switch (prop.CustomPropertyTypeId)
						{
							case 1: // Text
							case 9: // URL
								if (cont is cntrlRichTextEditor)
									artProp.StringValue = ((cntrlRichTextEditor)cont).HTMLText;
								else if (cont is TextBox)
									artProp.StringValue = ((TextBox)cont).Text;
								break;

							case 2: // Integer
							case 6: // List
							case 8: // User
								int? newValue = null;
								if (cont is IntegerUpDown)
								{
									newValue = ((IntegerUpDown)cont).Value;
								}
								else if (cont is ComboBox)
								{
									if (prop.CustomPropertyTypeId == 6) // Re
									{
										if (((ComboBox)cont).SelectedItem != null)
										{
											newValue = ((RemoteCustomListValue)((ComboBox)cont).SelectedItem).CustomPropertyValueId;
										}
									}
									else if (prop.CustomPropertyTypeId == 8)
									{
										if (((ComboBox)cont).SelectedItem != null)
										{
											newValue = ((RemoteUser)((ComboBox)cont).SelectedItem).UserId;
										}
									}
								}

								artProp.IntegerValue = newValue;
								break;

							case 3: // Decimal
								artProp.DecimalValue = ((DecimalUpDown)cont).Value;
								break;

							case 4: // Boolean
								artProp.BooleanValue = ((CheckBox)cont).IsChecked;
								break;

							case 5: // Date
								if (((DatePicker)cont).SelectedDate.HasValue)
								{
									artProp.DateTimeValue = ((DatePicker)cont).SelectedDate.Value.ToUniversalTime();
								}
								break;

							case 7: // MultiList
								artProp.IntegerListValue = new List<int>();
								foreach (RemoteCustomListValue value in ((ListBox)cont).SelectedItems)
								{
									artProp.IntegerListValue.Add(value.CustomPropertyValueId.Value);
								}
								break;

						}

						retList.Add(artProp);
					}
				}
			}

			return retList;
		}

		/// <summary>Changes the field requirements when run.</summary>
		/// <param name="workflowFields">The field statuses.</param>
		public void SetWorkflowFields(Dictionary<int, int> workflowFields)
		{
			//Loop through each custom property, and make sure that fields are entered properly.
			foreach (UIElement cont in this.grdContent.Children)
			{
				if (cont is Control)
				{
					if (((Control)cont).Tag is RemoteCustomProperty)
					{
						RemoteCustomProperty prop = ((RemoteCustomProperty)((Control)cont).Tag);

						if (workflowFields.ContainsKey(prop.CustomPropertyId.Value))
						{
							int status = workflowFields[prop.CustomPropertyId.Value];

							if (status == 2) //Required.
								this.reqgWorkflow.Add(prop.CustomPropertyId.Value);
							else if (status == 1) // Disabled.
							{
								//Disable the control, making sure it's visible.
								((Control)cont).IsEnabled = false;
								((Control)cont).Visibility = System.Windows.Visibility.Visible;
								this.LabelControls[prop.CustomPropertyId.Value].IsEnabled = false;
								this.LabelControls[prop.CustomPropertyId.Value].Visibility = System.Windows.Visibility.Visible;
							}
							else if (status == 3) //Hidden.
							{
								//Hide the control and label.
								((Control)cont).IsEnabled = true;
								((Control)cont).Visibility = System.Windows.Visibility.Collapsed;
								this.LabelControls[prop.CustomPropertyId.Value].IsEnabled = true;
								this.LabelControls[prop.CustomPropertyId.Value].Visibility = System.Windows.Visibility.Collapsed;
							}
							else if (status == 0) //Normal
							{
								//Make the control visible and 
								((Control)cont).IsEnabled = true;
								((Control)cont).Visibility = System.Windows.Visibility.Visible;
								this.LabelControls[prop.CustomPropertyId.Value].IsEnabled = true;
								this.LabelControls[prop.CustomPropertyId.Value].Visibility = System.Windows.Visibility.Visible;
							}
						}
					}
				}
			}

			//Now we have to 'combine' the CustomField required status with the Workflow Status
			foreach (KeyValuePair<int, TextBlock> kbpValue in this.LabelControls)
			{
				FontWeight weight = FontWeights.Normal;

				if (this.reqdFields.Contains(kbpValue.Key) || this.reqgWorkflow.Contains(kbpValue.Key))
					weight = FontWeights.Bold;

				kbpValue.Value.FontWeight = weight;
			}
		}
		#endregion

		private enum CurrentColumnTypeEnum : int
		{
			Label = 1,
			Data = 2,
			Spacer = 3
		}
	}
}
