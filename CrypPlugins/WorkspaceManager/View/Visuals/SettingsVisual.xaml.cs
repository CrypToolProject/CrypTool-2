
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.PluginBase.Validation;
using Microsoft.Win32;
using Microsoft.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WorkspaceManager.View.Base;

namespace WorkspaceManager.View.Visuals
{
    public partial class SettingsVisual : UserControl
    {
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private readonly Thickness CONTROL_DEFAULT_MARGIN = new Thickness(4, 0, 0, 0);
        private readonly Dictionary<ISettings, Dictionary<string, List<RadioButton>>> dicRadioButtons = new Dictionary<ISettings, Dictionary<string, List<RadioButton>>>();
        private readonly IPlugin plugin;
        private readonly EntryGroup entgrou;
        private readonly ComponentVisual bcv;
        private readonly TabControl tbC;
        public string myConnectorName;
        public bool noSettings;
        private readonly bool isSideBar;

        public SettingsVisual(PluginSettingsContainer pluginSettingsContainer, ComponentVisual bcv, bool isMaster, bool isSideBar)
        {
            bcv.Model.ConnectorPlugstateChanged += new EventHandler<Model.ConnectorPlugstateChangedEventArgs>(Model_ConnectorPlugstateChanged);
            Loaded += new RoutedEventHandler(BinSettingsVisual_Loaded);

            noSettings = false;
            this.isSideBar = isSideBar;
            Resources.Add("isSideBarResource", this.isSideBar);

            this.bcv = bcv;
            plugin = pluginSettingsContainer.Plugin;
            entgrou = new EntryGroup();
            entgrou = createContentSettings(plugin);

            if (entgrou.entryList.Count != 0)
            {
                ((WorkspaceManagerClass)bcv.Model.WorkspaceModel.MyEditor).executeEvent += new EventHandler(excuteEventHandler);

                InitializeComponent();

                if (isMaster)
                {
                    bcv.IControlCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChangedHandler);

                    tbC = new TabControl
                    {
                        Name = "TabControl",

                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent
                    };

                    DataTrigger dt = new DataTrigger
                    {
                        Value = 1
                    };

                    Binding dataBinding = new Binding("Items.Count")
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(TabControl), 1)
                    };
                    dt.Binding = dataBinding;

                    Setter sett = new Setter
                    {
                        Property = VisibilityProperty,
                        Value = Visibility.Collapsed
                    };
                    dt.Setters.Add(sett);

                    Style stu = new Style
                    {
                        TargetType = typeof(TabItem)
                    };
                    stu.Triggers.Add(dt);

                    tbC.ItemContainerStyle = stu;

                    myGrid.Children.Remove(MyScrollViewer);

                    myGrid.Children.Add(tbC);
                    TabItem tbI = new TabItem
                    {
                        Header = bcv.Model.Plugin.GetPluginInfoAttribute().Caption,
                        Content = MyScrollViewer
                    };

                    tbC.Items.Add(tbI);

                    myConnectorName = "None, I'm the master!";

                    foreach (IControlMasterElement control in bcv.IControlCollection)
                    {
                        control.PluginModelChanged += new EventHandler(icm_PluginModelChanged);
                        HandleControlMasterElementChange(control);
                    }
                }

                drawList(entgrou);

                pluginSettingsContainer.TaskPaneAttributeChanged += HandleTaskPaneAttributeChanges;
                //"Replay" all current task pane attributes:
                DispatchTaskPaneAttributeChanges(pluginSettingsContainer.CurrentTaskPaneAttributes);
                SetExecutionMode();
            }
            else
            {
                InitializeComponent();
                TextBlock tb = new TextBlock
                {
                    Text = Properties.Resources.BinSettingsVisual_BinSettingsVisual_No_Settings_available_
                };
                MyScrollViewer.Content = tb;
                noSettings = true;
            }
        }

        private void BinSettingsVisual_Loaded(object sender, RoutedEventArgs e)
        {
            InvalidateVisual();
        }

        private void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
        {
            foreach (IControlMasterElement icm in args.NewItems.OfType<IControlMasterElement>())
            {
                icm.PluginModelChanged += new EventHandler(icm_PluginModelChanged);
            }
        }

        private void icm_PluginModelChanged(object sender, EventArgs e)
        {
            HandleControlMasterElementChange(sender as IControlMasterElement);
        }

        private void HandleControlMasterElementChange(IControlMasterElement master)
        {
            if (master.PluginModel != null)
            {
                bool b = true;
                foreach (TabItem vtbI in tbC.Items)
                {
                    if (vtbI.Uid == master.ConnectorModel.PropertyName)
                    {

                        vtbI.Content = new SettingsVisual(master.PluginSettingsContainer, bcv, false, isSideBar);
                        vtbI.Header = master.PluginModel.GetName();
                        b = false;
                    }
                }

                if (b)
                {
                    TabItem tbI = new TabItem
                    {
                        Uid = master.ConnectorModel.PropertyName,
                        Content = new SettingsVisual(master.PluginSettingsContainer, bcv, false, isSideBar),
                        Header = master.PluginModel.Plugin.GetPluginInfoAttribute().Caption
                    };
                    tbC.Items.Add(tbI);
                }
            }
            else
            {
                TabItem tbI = null;
                foreach (TabItem vtbI in tbC.Items)
                {
                    if (vtbI.Uid == master.ConnectorModel.PropertyName)
                    {
                        tbI = vtbI;
                    }
                }
                if (tbI != null)
                {
                    tbC.Items.Remove(tbI);
                }
            }
        }

        private void HandleTaskPaneAttributeChanges(object sender, TaskPaneAttributeChangedEventArgs args)
        {
            DispatchTaskPaneAttributeChanges(args.ListTaskPaneAttributeContainer);
        }

        private void DispatchTaskPaneAttributeChanges(IEnumerable<TaskPaneAttribteContainer> attributeChanges)
        {
            if (Dispatcher.CheckAccess())
            {
                HandleTaskPaneAttributeChanges(attributeChanges);
            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    HandleTaskPaneAttributeChanges(attributeChanges);
                }, null);
            }
        }

        private void HandleTaskPaneAttributeChanges(IEnumerable<TaskPaneAttribteContainer> attributeChanges)
        {
            Dictionary<string, TaskPaneAttribteContainer> attributeChangesDict = attributeChanges.ToDictionary(att => att.Property, att => att);
            if (!attributeChangesDict.Any())
            {
                return;
            }

            foreach (List<ControlEntry> cel in entgrou.entryList)
            {
                entgrou.groupPanel[entgrou.entryList.IndexOf(cel)].Visibility = Visibility.Visible;
                bool allInGroupInvisible = true;

                foreach (ControlEntry ce in cel)
                {
                    if (attributeChangesDict.TryGetValue(ce.tpa.PropertyName, out TaskPaneAttribteContainer tpac))
                    {
                        ce.element.Visibility = tpac.Visibility;
                        ce.caption.Visibility = tpac.Visibility;
                    }
                    if (ce.element.Visibility == Visibility.Visible)
                    {
                        allInGroupInvisible = false;
                    }
                }

                if (allInGroupInvisible)
                {
                    entgrou.groupPanel[entgrou.entryList.IndexOf(cel)].Visibility = Visibility.Collapsed;
                }
            }
        }

        private void excuteEventHandler(object sender, EventArgs args)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                SetExecutionMode();
            }, null);
        }

        private void SetExecutionMode()
        {
            foreach (List<ControlEntry> cel in entgrou.entryList)
            {
                foreach (ControlEntry ce in cel)
                {
                    if (((WorkspaceManagerClass)bcv.Model.WorkspaceModel.MyEditor).isExecuting())
                    {
                        if (!ce.tpa.ChangeableWhileExecuting)
                        {
                            ce.element.IsEnabled = false;
                            if (ce.element is IntegerUpDown)
                            {
                                IntegerUpDown nud = ce.element as IntegerUpDown;
                                nud.Opacity = 0.80;
                                nud.Foreground = Brushes.Gray;
                            }
                            if (ce.caption != null)
                            {
                                TextBlock cap = ce.caption as TextBlock;
                                cap.Opacity = 0.80;
                                cap.Foreground = Brushes.Gray;
                            }
                        }
                    }
                    else
                    {
                        if (!ce.tpa.ChangeableWhileExecuting)
                        {
                            ce.element.IsEnabled = true;
                            if (ce.element is IntegerUpDown)
                            {
                                IntegerUpDown nud = ce.element as IntegerUpDown;
                                nud.Opacity = 1;
                                nud.Foreground = Brushes.Black;
                            }
                            if (ce.caption != null)
                            {
                                TextBlock cap = ce.caption as TextBlock;
                                cap.Opacity = 1;
                                cap.Foreground = Brushes.Black;
                            }
                        }
                    }
                }
            }
        }

        private readonly List<string> groups = new List<string>();

        public static double getComboBoxMaxSize(ComboBox child)
        {
            double x = 0;
            ComboBox cb = child as ComboBox;
            for (int i = 0; i < cb.Items.Count; i++)
            {
                string s = cb.Items[i] as string;
                FormattedText ft = new FormattedText(s, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(cb.FontFamily, cb.FontStyle, cb.FontWeight, cb.FontStretch), cb.FontSize, Brushes.Black)
                {
                    MaxLineCount = 1
                };
                if (x < ft.WidthIncludingTrailingWhitespace)
                {
                    x = ft.WidthIncludingTrailingWhitespace;
                }
            }

            return cb.Width = x + 28; // 28 pixel are an approximation of the rendersize of the dropdown button
        }

        private void drawList(EntryGroup entgrou)
        {
            bool isFirst = true;

            foreach (List<ControlEntry> cel in entgrou.entryList)
            {
                ParameterPanel parameterPanel;
                ParameterPanel noVerticalGroupParameterPanel;

                Expander testexpander = new Expander();

                Expander noverticalgroupexpander = new Expander();

                noVerticalGroupParameterPanel = new ParameterPanel(isSideBar);

                Border noVerticalGroupBodi = new Border
                {
                    Style = (Style)FindResource("border1"),

                    Child = noVerticalGroupParameterPanel
                };

                noverticalgroupexpander.Content = noVerticalGroupBodi;

                Border bodi = new Border();

                testexpander.IsExpanded = true;

                parameterPanel = new ParameterPanel(isSideBar);

                entgrou.groupPanel.Add(testexpander);

                parameterPanel.Name = "border1";

                parameterPanel.Margin = new Thickness(5);

                Binding dataBinding = new Binding("ActualWidth")
                {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.OneWay,
                    Source = parameterPanel
                };

                if (!string.IsNullOrEmpty(cel[0].tpa.groupName))
                {
                    testexpander.Header = cel[0].tpa.GroupName;
                }

                StackPanel contentPanel = new StackPanel();
                List<string> grouplist = new List<string>();
                List<Grid> gridlist = new List<Grid>();
                List<TextBlock> tebo = new List<TextBlock>();

                foreach (ControlEntry ce in cel)
                {
                    addToConnectorSettingsHide(ce);
                    TextBlock title = new TextBlock();
                    ce.caption = title;


                    if (ce.sfa == null)
                    {

                        title.Text = ce.tpa.Caption;
                        title.TextWrapping = TextWrapping.Wrap;

                        if (ce.element is CheckBox || ce.element is Button)
                        {
                            Label l = new Label
                            {
                                Height = 0
                            };
                            parameterPanel.Children.Add(ce.element);
                            parameterPanel.Children.Add(l);
                        }

                        else
                        {
                            parameterPanel.Children.Add(title);
                            if (ce.element is ComboBox)
                            {
                                ComboBox cb = ce.element as ComboBox;
                                cb.MaxWidth = getComboBoxMaxSize(cb);
                                parameterPanel.Children.Add(cb);

                            }
                            else
                            {
                                parameterPanel.Children.Add(ce.element);
                            }
                        }
                    }
                    else
                    {
                        if (ce.sfa.VerticalGroup != null)
                        {
                            bool groupExists = grouplist.Contains(ce.sfa.VerticalGroup);
                            Grid controlGrid = groupExists ? gridlist[grouplist.IndexOf(ce.sfa.VerticalGroup)] : new Grid();

                            ColumnDefinition coldef1 = new ColumnDefinition
                            {
                                Width = ce.sfa.WidthCol1
                            };
                            controlGrid.ColumnDefinitions.Add(coldef1);

                            ColumnDefinition coldef2 = new ColumnDefinition
                            {
                                Width = ce.sfa.WidthCol2
                            };
                            controlGrid.ColumnDefinitions.Add(coldef2);

                            title.Text = ce.tpa.Caption;
                            ce.caption = title;
                            title.HorizontalAlignment = HorizontalAlignment.Center;
                            title.VerticalAlignment = VerticalAlignment.Center;

                            Grid.SetColumn(title, controlGrid.ColumnDefinitions.Count - 2);

                            controlGrid.Children.Add(title);
                            Grid.SetColumn(ce.element, controlGrid.ColumnDefinitions.Count - 1);

                            if (ce.element is ComboBox)
                            {
                                ComboBox cb = ce.element as ComboBox;
                                cb.Width = getComboBoxMaxSize(cb);
                                controlGrid.Children.Add(cb);
                                controlGrid.MaxWidth += cb.Width;
                                controlGrid.MaxWidth += title.DesiredSize.Width; ;
                            }
                            else
                            {
                                controlGrid.Children.Add(ce.element);
                            }

                            if (!groupExists)
                            {
                                grouplist.Add(ce.sfa.VerticalGroup);
                                controlGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

                                Label dummy = new Label
                                {
                                    Height = 0
                                };

                                controlGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                                controlGrid.Arrange(new Rect(controlGrid.DesiredSize));
                                parameterPanel.Children.Add(controlGrid);
                                parameterPanel.Children.Add(dummy);

                                controlGrid.Width = parameterPanel.Width;

                                gridlist.Add(controlGrid);
                            }
                        }
                        else
                        {
                            if (!parameterPanel.IsAncestorOf(noverticalgroupexpander))
                            {
                                Label l = new Label
                                {
                                    Width = 1,
                                    Height = 0
                                };
                                parameterPanel.Children.Add(noverticalgroupexpander);
                                parameterPanel.Children.Add(l);
                            }


                            title.Text = ce.tpa.Caption;
                            ce.caption = title;
                            title.TextWrapping = TextWrapping.Wrap;

                            if (ce.element is CheckBox || ce.element is Button)
                            {
                                Label l = new Label
                                {
                                    Width = 1,
                                    Height = 0
                                };
                                noVerticalGroupParameterPanel.Children.Add(ce.element);
                                noVerticalGroupParameterPanel.Children.Add(l);

                            }
                            else if (ce.element is ComboBox)
                            {
                                ComboBox cb = ce.element as ComboBox;
                                noVerticalGroupParameterPanel.Children.Add(cb);
                            }
                            else
                            {
                                noVerticalGroupParameterPanel.Children.Add(title);
                                noVerticalGroupParameterPanel.Children.Add(ce.element);
                            }
                        }

                    }

                }
                parameterPanel.HorizontalAlignment = HorizontalAlignment.Left;

                bodi.Child = parameterPanel;

                bodi.Style = (Style)FindResource("border1");

                testexpander.Content = bodi;
                if (!isFirst)
                {
                    testexpander.Margin = new Thickness(0, 15, 0, 0);
                }

                if (isSideBar)
                {
                    myStack.Children.Add(testexpander);
                }
                else
                {
                    myWrap.Children.Add(testexpander);
                }

                parameterPanel.setMaxSizes(true);
                noVerticalGroupParameterPanel.setMaxSizes(true);

                isFirst = false;
            }

            BeginInit();

        }

        private EntryGroup createContentSettings(IPlugin plugin)
        {
            EntryGroup entgrou = new EntryGroup();
            foreach (TaskPaneAttribute tpa in plugin.Settings.GetSettingsProperties(plugin))
            {
                SettingsFormatAttribute sfa = plugin.Settings.GetSettingsFormat(tpa.PropertyName);
                if (sfa != null)
                {
                    if (!groups.Contains(sfa.VerticalGroup))
                    {
                        groups.Add(sfa.VerticalGroup);
                    }
                }

                Binding dataBinding = new Binding(tpa.PropertyName)
                {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay,
                    Source = plugin.Settings
                };

                bool b = (bcv.Model.GetOutputConnectors().Union(bcv.Model.GetInputConnectors())).Any(x => tpa.PropertyName == x.GetName());
                switch (tpa.ControlType)
                {
                    #region TextBox
                    case ControlType.TextBox:

                        TextBox textbox = new TextBox
                        {
                            Tag = tpa.ToolTip,
                            ToolTip = tpa.ToolTip
                        };
                        textbox.MouseEnter += Control_MouseEnter;
                        if (tpa.RegularExpression != null && tpa.RegularExpression != string.Empty)
                        {
                            ControlTemplate validationTemplate = Application.Current.Resources["validationTemplate"] as ControlTemplate;
                            RegExRule regExRule = new RegExRule
                            {
                                RegExValue = tpa.RegularExpression
                            };
                            Validation.SetErrorTemplate(textbox, validationTemplate);
                            dataBinding.ValidationRules.Add(regExRule);
                            dataBinding.NotifyOnValidationError = true;
                        }
                        textbox.SetBinding(TextBox.TextProperty, dataBinding);
                        textbox.TextWrapping = TextWrapping.Wrap;
                        entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(textbox, tpa, sfa, b, bcv.Model));
                        break;

                    #endregion TextBox

                    #region NumericUpDown
                    case ControlType.NumericUpDown:
                        if (tpa.ValidationType == ValidationType.RangeInteger)
                        {
                            IntegerUpDown intInput = new IntegerUpDown
                            {
                                SelectAllOnGotFocus = true,
                                Tag = tpa.ToolTip,
                                ToolTip = tpa.ToolTip
                            };
                            intInput.MouseEnter += Control_MouseEnter;
                            intInput.Maximum = tpa.IntegerMaxValue;
                            intInput.Minimum = tpa.IntegerMinValue;
                            string s = tpa.IntegerMaxValue + "";
                            FormattedText ft = new FormattedText(s, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(intInput.FontFamily, intInput.FontStyle, intInput.FontWeight, intInput.FontStretch), intInput.FontSize, Brushes.Black);
                            intInput.MaxWidth = ft.WidthIncludingTrailingWhitespace + 30;
                            intInput.Width = ft.WidthIncludingTrailingWhitespace + 30;
                            intInput.SetBinding(IntegerUpDown.ValueProperty, dataBinding);
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(intInput, tpa, sfa, b, bcv.Model));
                            intInput.IsEnabled = true;
                        }
                        else if (tpa.ValidationType == ValidationType.RangeDouble)
                        {
                            DoubleUpDown doubleInput = new DoubleUpDown
                            {
                                SelectAllOnGotFocus = true,
                                Tag = tpa.ToolTip,
                                ToolTip = tpa.ToolTip
                            };
                            doubleInput.MouseEnter += Control_MouseEnter;
                            doubleInput.Maximum = tpa.DoubleMaxValue;
                            doubleInput.Minimum = tpa.DoubleMinValue;
                            doubleInput.Increment = tpa.DoubleIncrement;
                            string s = double.MaxValue + "";
                            FormattedText ft = new FormattedText(s, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(doubleInput.FontFamily, doubleInput.FontStyle, doubleInput.FontWeight, doubleInput.FontStretch), doubleInput.FontSize, Brushes.Black);
                            doubleInput.MaxWidth = ft.WidthIncludingTrailingWhitespace + 30;
                            doubleInput.Width = ft.WidthIncludingTrailingWhitespace + 30;
                            doubleInput.SetBinding(DoubleUpDown.ValueProperty, dataBinding);
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(doubleInput, tpa, sfa, b, bcv.Model));
                            doubleInput.IsEnabled = true;
                        }
                        break;
                    #endregion NumericUpDown

                    #region ComboBox
                    case ControlType.ComboBox:
                        ComboBox comboBox = new ComboBox
                        {
                            Tag = tpa.ToolTip
                        };
                        comboBox.MouseEnter += Control_MouseEnter;

                        object value = plugin.Settings.GetType().GetProperty(tpa.PropertyName).GetValue(plugin.Settings, null);
                        bool isEnum = value is Enum;

                        if (isEnum) // use generic enum<->int converter
                        {
                            dataBinding.Converter = EnumToIntConverter.GetInstance();
                        }

                        if (tpa.ControlValues != null) // show manually passed entries in ComboBox
                        {
                            comboBox.ItemsSource = tpa.ControlValues;
                        }
                        else if (isEnum) // show automatically derived enum entries in ComboBox
                        {
                            comboBox.ItemsSource = Enum.GetValues(value.GetType());
                        }
                        else // nothing to show
                        {
                            GuiLogMessage("No ComboBox entries given", NotificationLevel.Error);
                        }
                        comboBox.ToolTip = tpa.ToolTip;
                        comboBox.SetBinding(ComboBox.SelectedIndexProperty, dataBinding);
                        //controlList.Add(new ControlEntry(comboBox, tpa, sfa));
                        entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(comboBox, tpa, sfa, b, bcv.Model));
                        break;

                    #endregion ComboBox

                    #region LanguageSelector
                    case ControlType.LanguageSelector:
                        ComboBox comboBox1 = new ComboBox
                        {
                            Tag = tpa.ToolTip
                        };
                        comboBox1.MouseEnter += Control_MouseEnter;
                        comboBox1.ItemsSource = CrypTool.PluginBase.Utils.LanguageStatistics.SupportedLanguages;
                        comboBox1.ToolTip = tpa.ToolTip;
                        comboBox1.SetBinding(ComboBox.SelectedIndexProperty, dataBinding);
                        entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(comboBox1, tpa, sfa, b, bcv.Model));
                        break;
                    #endregion LanguageSelector

                    #region RadioButton
                    case ControlType.RadioButton:
                        if (!dicRadioButtons.ContainsKey(plugin.Settings))
                        {
                            dicRadioButtons.Add(plugin.Settings, new Dictionary<string, List<RadioButton>>());
                        }
                        List<RadioButton> list = new List<RadioButton>();
                        StackPanel panelRadioButtons = new StackPanel
                        {
                            ToolTip = tpa.ToolTip
                        };
                        panelRadioButtons.MouseEnter += Control_MouseEnter;
                        panelRadioButtons.Margin = CONTROL_DEFAULT_MARGIN;

                        string groupNameExtension = Guid.NewGuid().ToString();

                        for (int i = 0; i < tpa.ControlValues.Length; i++)
                        {
                            RadioButton radio = new RadioButton
                            {
                                IsChecked = false
                            };

                            string stringValue = tpa.ControlValues[i];

                            Binding dataBinding1 = new Binding(plugin.Settings.GetType().GetProperty(tpa.PropertyName).Name)
                            {
                                Converter = new RadioBoolToIntConverter(),
                                Mode = BindingMode.TwoWay,
                                Source = plugin.Settings,
                                ConverterParameter = i
                            };

                            radio.GroupName = tpa.PropertyName + groupNameExtension;
                            radio.Content = stringValue;

                            radio.Tag = new RadioButtonListAndBindingInfo(list, plugin, tpa);
                            radio.ToolTip = tpa.ToolTip;
                            radio.SetBinding(RadioButton.IsCheckedProperty, dataBinding1);
                            panelRadioButtons.Children.Add(radio);
                            list.Add(radio);
                        }
                        dicRadioButtons[plugin.Settings].Add(tpa.PropertyName, list);
                        entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(panelRadioButtons, tpa, sfa, b, bcv.Model));

                        break;
                    #endregion RadioButton

                    #region CheckBox
                    case ControlType.CheckBox:
                        CheckBox checkBox = new CheckBox
                        {
                            Margin = CONTROL_DEFAULT_MARGIN
                        };
                        TextBlock wrapBlock = new TextBlock
                        {
                            Text = tpa.Caption,
                            TextWrapping = TextWrapping.Wrap
                        };
                        checkBox.Content = wrapBlock;
                        checkBox.Tag = tpa.ToolTip;
                        checkBox.ToolTip = tpa.ToolTip;
                        checkBox.MouseEnter += Control_MouseEnter;
                        checkBox.SetBinding(CheckBox.IsCheckedProperty, dataBinding);
                        entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(checkBox, tpa, sfa, b, bcv.Model));

                        break;
                    #endregion CheckBox

                    #region DynamicComboBox
                    case ControlType.DynamicComboBox:
                        PropertyInfo pInfo = plugin.Settings.GetType().GetProperty(tpa.ControlValuesNotInterpolated[0]);

                        ObservableCollection<string> coll = pInfo.GetValue(plugin.Settings, null) as ObservableCollection<string>;

                        if (coll != null)
                        {
                            ComboBox comboBoxDyn = new ComboBox
                            {
                                Tag = tpa.ToolTip,
                                ToolTip = tpa.ToolTip
                            };
                            comboBoxDyn.MouseEnter += Control_MouseEnter;
                            comboBoxDyn.ItemsSource = coll;
                            comboBoxDyn.SetBinding(ComboBox.SelectedIndexProperty, dataBinding);
                            //inputControl = comboBoxDyn;
                            //bInfo.CaptionGUIElement = comboBoxDyn;

                            //controlList.Add(new ControlEntry(comboBoxDyn, tpa, sfa));
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(comboBoxDyn, tpa, sfa, b, bcv.Model));
                        }
                        break;
                    #endregion DynamicComboBox

                    #region FileDialog
                    case ControlType.SaveFileDialog:
                    case ControlType.OpenFileDialog:
                        StackPanel sp = new StackPanel
                        {
                            Uid = "FileDialog",
                            Orientation = Orientation.Vertical
                        };

                        TextBox fileTextBox = new TextBox
                        {
                            TextWrapping = TextWrapping.Wrap,
                            Background = Brushes.LightGray,
                            IsReadOnly = true,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        fileTextBox.TextChanged += fileDialogTextBox_TextChanged;
                        fileTextBox.SetBinding(TextBox.TextProperty, dataBinding);
                        //fileTextBox.SetBinding(TextBox.ToolTipProperty, dataBinding);

                        fileTextBox.Tag = tpa;
                        if (fileTextBox.ToolTip == null || (string)fileTextBox.ToolTip == string.Empty)
                        {
                            fileTextBox.ToolTip = tpa.ToolTip;
                        }
                        fileTextBox.MouseEnter += fileTextBox_MouseEnter;
                        sp.Children.Add(fileTextBox);

                        Button btn = new Button
                        {
                            Tag = fileTextBox
                        };
                        if (tpa.ControlType == ControlType.SaveFileDialog)
                        {
                            btn.Content = Properties.Resources.Save_File;
                        }
                        else
                        {
                            btn.Content = Properties.Resources.Open_File;
                        }

                        btn.Click += FileDialogClick;
                        sp.Children.Add(btn);
                        entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(sp, tpa, sfa, b, bcv.Model));

                        break;
                    #endregion FileDialog

                    #region Button
                    case ControlType.Button:
                        Button taskPaneButton = new Button
                        {
                            Margin = new Thickness(0),
                            Tag = tpa,
                            ToolTip = tpa.ToolTip
                        };
                        taskPaneButton.MouseEnter += TaskPaneButton_MouseEnter;
                        TextBlock contentBlock = new TextBlock
                        {
                            Text = tpa.Caption,
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center
                        };
                        taskPaneButton.Content = contentBlock;
                        taskPaneButton.Click += TaskPaneButton_Click;

                        entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(taskPaneButton, tpa, sfa, b, bcv.Model));
                        break;
                    #endregion Button

                    #region Slider
                    case ControlType.Slider:
                        Slider slider = new Slider
                        {
                            Margin = CONTROL_DEFAULT_MARGIN,
                            Orientation = Orientation.Horizontal,
                            Minimum = tpa.DoubleMinValue,
                            Maximum = tpa.DoubleMaxValue,
                            Tag = tpa.ToolTip,
                            ToolTip = tpa.ToolTip
                        };
                        slider.MouseEnter += Control_MouseEnter;


                        slider.SetBinding(Slider.ValueProperty, dataBinding);

                        slider.MinWidth = 0;

                        entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(slider, tpa, sfa, b, bcv.Model));
                        break;
                    #endregion Slider

                    #region TextBoxReadOnly
                    case ControlType.TextBoxReadOnly:
                        TextBox textBoxReadOnly = new TextBox
                        {
                            MinWidth = 0,
                            TextWrapping = TextWrapping.Wrap,
                            IsReadOnly = true,
                            BorderThickness = new Thickness(0),
                            Background = Brushes.Transparent,
                            Tag = tpa.ToolTip,
                            ToolTip = tpa.ToolTip
                        };
                        textBoxReadOnly.MouseEnter += Control_MouseEnter;
                        dataBinding.Mode = BindingMode.OneWay; // read-only strings do not need a setter
                        textBoxReadOnly.SetBinding(TextBox.TextProperty, dataBinding);
                        entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(textBoxReadOnly, tpa, sfa, b, bcv.Model));
                        break;
                    #endregion TextBoxReadOnly

                    #region TextBoxHidden
                    case ControlType.TextBoxHidden:
                        PasswordBox passwordBox = new PasswordBox
                        {
                            MinWidth = 0,

                            Tag = tpa,
                            ToolTip = tpa.ToolTip
                        };
                        passwordBox.MouseEnter += Control_MouseEnter;
                        passwordBox.Password = plugin.Settings.GetType().GetProperty(tpa.PropertyName).GetValue(plugin.Settings, null) as string;
                        //textBoxReadOnly.SetBinding(PasswordBox.property , dataBinding);
                        passwordBox.PasswordChanged += TextBoxHidden_Changed;
                        entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(passwordBox, tpa, sfa, b, bcv.Model));
                        break;
                    #endregion TextBoxHidden

                    #region KeyTextBox
                    case ControlType.KeyTextBox:
                        KeyTextBox.KeyTextBox keyTextBox = new KeyTextBox.KeyTextBox();

                        KeyTextBox.IKeyManager keyManager = plugin.Settings.GetType().GetProperty(tpa.AdditionalPropertyName).GetValue(plugin.Settings, null) as KeyTextBox.IKeyManager;
                        keyTextBox.KeyManager = keyManager;
                        keyTextBox.Tag = tpa;
                        keyTextBox.ToolTip = tpa.ToolTip;
                        keyTextBox.MouseEnter += Control_MouseEnter;
                        keyTextBox.SetBinding(KeyTextBox.KeyTextBox.CurrentKeyProperty, dataBinding);
                        entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(keyTextBox, tpa, sfa, b, bcv.Model));
                        break;
                        #endregion KeyTextBox

                }

            }
            entgrou.sort();
            return entgrou;
        }

        private void checkIfPlugged(Model.PlugState state, Model.ConnectorModel model)
        {
            connectorSettingElements.TryGetValue(model.GetName(), out ControlEntry ele);
            if (state == Model.PlugState.Plugged)
            {
                if (ele != null)
                {
                    ele.Visibility = System.Windows.Visibility.Collapsed;

                    foreach (Expander expander in myStack.Children)
                    {
                        ((expander.Content as Border).Child as ParameterPanel).setMaxSizes(false);
                    }
                    foreach (Expander expander in myWrap.Children)
                    {
                        ((expander.Content as Border).Child as ParameterPanel).setMaxSizes(false);
                    }
                }
            }
            if (state == Model.PlugState.Unplugged && model.GetInputConnections().Count == 0)
            {
                if (ele != null)
                {
                    ele.Visibility = System.Windows.Visibility.Visible;

                    foreach (Expander expander in myStack.Children)
                    {
                        ((expander.Content as Border).Child as ParameterPanel).setMaxSizes(true);
                    }
                    foreach (Expander expander in myWrap.Children)
                    {
                        ((expander.Content as Border).Child as ParameterPanel).setMaxSizes(true);
                    }
                }
            }
        }

        private void Model_ConnectorPlugstateChanged(object sender, Model.ConnectorPlugstateChangedEventArgs e)
        {
            checkIfPlugged(e.PlugState, e.ConnectorModel);
        }

        private void addToConnectorSettingsHide(ControlEntry element)
        {
            if (element.hide)
            {
                connectorSettingElements.Add(element.tpa.PropertyName, element);
            }
        }

        private readonly Dictionary<string, ControlEntry> connectorSettingElements = new Dictionary<string, ControlEntry>();
        private void TextBoxHidden_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                PasswordBox pwBox = sender as PasswordBox;
                if (pwBox != null)
                {
                    TaskPaneAttribute tpa = pwBox.Tag as TaskPaneAttribute;
                    if (tpa != null)
                    {
                        plugin.Settings.GetType().GetProperty(tpa.PropertyName).SetValue(plugin.Settings, pwBox.Password, null);
                    }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        private void fileDialogTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ((TextBox)sender).ScrollToHorizontalOffset(int.MaxValue);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void fileTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is TextBox)
                {
                    SetHelpText(((sender as TextBox).Tag as TaskPaneAttribute).ToolTip);
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void FileDialogClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                TextBox tb = btn.Tag as TextBox;
                TaskPaneAttribute tpAtt = tb.Tag as TaskPaneAttribute;

                if (tpAtt.ControlType == ControlType.OpenFileDialog)
                {
                    OpenFileDialog ofd = new OpenFileDialog
                    {
                        Filter = tpAtt.FileExtension,
                        Multiselect = false
                    };
                    bool? test = ofd.ShowDialog();
                    if (test.HasValue && test.Value)
                    {
                        tb.Text = ofd.FileName;
                    }
                }
                else if (tpAtt.ControlType == ControlType.SaveFileDialog)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = tpAtt.FileExtension
                    };
                    bool? test = saveFileDialog.ShowDialog();
                    if (test.HasValue && test.Value)
                    {
                        tb.Text = saveFileDialog.FileName;
                    }
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void Control_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is IntegerUpDown)
                {
                    SetHelpText((sender as IntegerUpDown).Tag as string);
                }

                if (sender is TextBox)
                {
                    SetHelpText((sender as TextBox).Tag as string);
                }
                //if (sender is PasswordBox) SetHelpText(((BindingInfo)(sender as PasswordBox).Tag).TaskPaneSettingsAttribute.ToolTip as string);
                if (sender is CheckBox)
                {
                    SetHelpText((sender as CheckBox).Tag as string);
                }

                if (sender is ComboBox)
                {
                    SetHelpText((sender as ComboBox).Tag as string);
                }

                if (sender is Slider)
                {
                    SetHelpText((sender as Slider).Tag as string);
                }

                if (sender is Button)
                {
                    SetHelpText((sender as Button).Tag as string);
                }

                if (sender is KeyTextBox.KeyTextBox)
                {
                    SetHelpText((sender as KeyTextBox.KeyTextBox).Tag as string);
                }
            }
            catch (Exception)
            {
                // GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void SetHelpText(string text)
        {
            try
            {
                textBoxTooltip.Text = text;
                textBoxTooltip.Foreground = Brushes.Black;
            }
            catch (Exception)
            {
                //GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }

        private void TaskPaneButton_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                SetHelpText(((sender as Button).Tag as TaskPaneAttribute).ToolTip);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void TaskPaneButton_Click(object sender, RoutedEventArgs e)
        {
            TaskPaneAttribute tpa = (sender as Button).Tag as TaskPaneAttribute;
            if (tpa != null && plugin.Settings != null && tpa.Method != null)
            {
                tpa.Method.Invoke(plugin.Settings, null);
            }
        }

    }

    public class EntryGroup
    {

        public List<string> listAdmin = new List<string>();
        public List<List<ControlEntry>> entryList = new List<List<ControlEntry>>();
        public List<Expander> groupPanel = new List<Expander>();


        public void AddNewEntry(string groupname, ControlEntry entry)
        {

            if (string.IsNullOrEmpty(groupname))
            { groupname = null; }

            if (listAdmin.Contains(groupname))
            {
                listAdmin.IndexOf(groupname);
                entryList[listAdmin.IndexOf(groupname)].Add(entry);

            }
            else
            {
                List<ControlEntry> dummyList = new List<ControlEntry>
                {
                    entry
                };
                listAdmin.Add(groupname);
                entryList.Add(dummyList);
            }
        }
        public void sort()
        {
            foreach (List<ControlEntry> dummyList in entryList)
            {
                dummyList.Sort(new BindingInfoComparer());
            }
        }
    }

    public class RadioButtonListAndBindingInfo
    {
        public readonly List<RadioButton> List = null;
        public readonly IPlugin plugin = null;
        public readonly TaskPaneAttribute tpa = null;

        public RadioButtonListAndBindingInfo(List<RadioButton> list, IPlugin plugin, TaskPaneAttribute tpa)
        {
            if (list == null)
            {
                throw new ArgumentException("list");
            }

            if (plugin == null)
            {
                throw new ArgumentException("bInfo");
            }

            if (tpa == null)
            {
                throw new ArgumentException("tpa");
            }

            this.tpa = tpa;
            List = list;
            this.plugin = plugin;
        }
    }

    public class ControlEntry
    {
        public UIElement element;
        public TaskPaneAttribute tpa;
        public SettingsFormatAttribute sfa;
        public bool hide;

        private FrameworkElement captionx;
        public FrameworkElement caption
        {
            get => captionx;
            set
            {
                captionx = value;
                captionx.Visibility = element.Visibility;
            }
        }

        private Visibility visibility;
        public Visibility Visibility
        {
            get => visibility;
            set
            {
                visibility = value;
                if (caption != null)
                {
                    caption.Visibility = value;
                }

                element.Visibility = value;
            }
        }

        public ControlEntry(UIElement element, TaskPaneAttribute tpa, SettingsFormatAttribute sfa)
        {
            this.element = element;
            this.sfa = sfa;
            this.tpa = tpa;

        }

        public ControlEntry(UIElement element, TaskPaneAttribute tpa, SettingsFormatAttribute sfa, bool hide, Model.PluginModel model)
        {
            this.element = element;
            this.sfa = sfa;
            this.tpa = tpa;
            this.hide = hide;
            if (hide)
            {
                Model.ConnectorModel conModel = model.GetOutputConnectors().Union(model.GetInputConnectors()).First(x => x.GetName() == tpa.PropertyName);
                bool a = conModel.GetInputConnections().Count > 0;
                bool b = conModel.GetOutputConnections().Count > 0;
                Visibility = a || b ? Visibility.Collapsed : Visibility.Visible;
            }

        }

    }

    public class ParameterPanel : Panel
    {
        private readonly bool isSideBar;
        private double maxSize = 0;
        private double maxSizeContent = 0;
        private double maxSizeCaption = 0;
        private double maxSizeCB = 0;
        private Grid maxGrid = new Grid();

        public ParameterPanel(bool isSideBar)
        {
            this.isSideBar = isSideBar;
            VerticalAlignment = System.Windows.VerticalAlignment.Top;
            SizeChanged += new SizeChangedEventHandler(TestPanel_SizeChanged);
        }

        public void setMaxSizes(bool overRun)
        {
            maxSizeCaption = 0;
            maxSizeContent = 0;
            maxSizeCB = 0;
            maxSize = 0;
            foreach (UIElement child in Children)
            {

                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                child.Arrange(new Rect(child.DesiredSize));

                if (child is TextBlock)
                {
                    TextBlock caption = child as TextBlock;
                    if (child.Visibility == Visibility.Visible || overRun)
                    {
                        if (caption != null)
                        {
                            FormattedText formattedText = new FormattedText(
                                "1234567890123456789012345678901234567890",
                                CultureInfo.GetCultureInfo("en-us"),
                                FlowDirection.LeftToRight,
                                new Typeface(caption.FontFamily.ToString()),
                                caption.FontSize,
                                Brushes.Black);

                            if (formattedText.WidthIncludingTrailingWhitespace > maxSizeCaption)
                            {
                                maxSizeCaption = formattedText.WidthIncludingTrailingWhitespace + 10;
                            }
                        }
                    }
                }
                else if (child is KeyTextBox.KeyTextBox)
                {
                    KeyTextBox.KeyTextBox keyTextBlock = child as KeyTextBox.KeyTextBox;
                    if (child.Visibility == Visibility.Visible || overRun)
                    {
                        if (keyTextBlock != null)
                        {
                            FormattedText formattedText = new FormattedText(
                                "1234567890123456789012345678901234567890",
                                CultureInfo.GetCultureInfo("en-us"),
                                FlowDirection.LeftToRight,
                                new Typeface(keyTextBlock.FontFamily.ToString()),
                                keyTextBlock.FontSize,
                                Brushes.Black);

                            if (formattedText.WidthIncludingTrailingWhitespace > maxSizeCaption)
                            {
                                maxSizeCaption = formattedText.WidthIncludingTrailingWhitespace + 10;
                            }
                        }
                    }
                }
                else if (child is Grid)
                {

                    if (maxGrid.Width < (child as Grid).DesiredSize.Width)
                    {
                        maxGrid = child as Grid;

                    }

                }
                else if (child is CheckBox)
                {
                    if (child.DesiredSize.Width > maxSizeCB)
                    {
                        if (child.DesiredSize.Width != 0)
                        {
                            maxSizeCB = child.DesiredSize.Width;
                        }
                    }

                }
                else if (child is ComboBox)
                {
                    double comboSize = SettingsVisual.getComboBoxMaxSize(child as ComboBox);
                    if (comboSize > maxSizeContent)
                    {
                        if (comboSize != 0)
                        {
                            maxSizeContent = comboSize;
                        }
                    }
                }

                else if (child is IntegerUpDown)
                {
                    IntegerUpDown intUD = child as IntegerUpDown;
                    string s = intUD.Maximum + "";
                    FormattedText ft = new FormattedText(s, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(intUD.FontFamily, intUD.FontStyle, intUD.FontWeight, intUD.FontStretch), intUD.FontSize, Brushes.Black);

                    if (ft.WidthIncludingTrailingWhitespace > maxSizeContent)
                    {
                        if (ft.WidthIncludingTrailingWhitespace != 0)
                        {
                            maxSizeContent = ft.WidthIncludingTrailingWhitespace;
                        }
                    }
                }
                else
                {
                    child.Measure(new Size(10, 10));
                    if (child.DesiredSize.Width > maxSizeContent)
                    {

                        if (child.DesiredSize.Width != 0)
                        {
                            maxSizeContent = child.DesiredSize.Width;
                        }
                    }
                }
            }

            if (maxSizeContent < 20)
            {
                maxSizeContent = 100;
            }

            maxSize = maxSizeCaption + maxSizeContent;

            if (maxSizeCB > maxSize)
            {
                maxSize = maxSizeCB;
                maxSizeContent = maxSizeCB - maxSizeCaption - 1;
            }


            if (maxSize < maxGrid.DesiredSize.Width)
            {
                maxSize = maxGrid.DesiredSize.Width;
            }
            if (!isSideBar)
            {
                MaxWidth = maxSize + 10;
            }

            foreach (UIElement child in Children)
            {
                if (child is IntegerUpDown)
                {
                    IntegerUpDown dummyTextBox = child as IntegerUpDown;
                    dummyTextBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                }

                if (child is ComboBox)
                {
                    ComboBox dummyTextBox = child as ComboBox;
                    dummyTextBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                }

                if (child is TextBox)
                {
                    TextBox dummyTextBox = child as TextBox;
                    dummyTextBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    dummyTextBox.MaxWidth = maxSizeContent;

                }

                if (child is PasswordBox)
                {
                    PasswordBox dummyTextBox = child as PasswordBox;
                    dummyTextBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    dummyTextBox.MaxWidth = maxSizeContent;

                }

                if (child is KeyTextBox.KeyTextBox)
                {
                    KeyTextBox.KeyTextBox dummyKeyTextBox = child as KeyTextBox.KeyTextBox;
                    dummyKeyTextBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    dummyKeyTextBox.Arrange(new Rect(dummyKeyTextBox.DesiredSize));
                    dummyKeyTextBox.MaxWidth = maxSizeContent;

                }

                if (child is TextBlock)
                {
                    TextBlock dummyTextBox = child as TextBlock;
                    dummyTextBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    dummyTextBox.MaxWidth = maxSizeCaption;

                }

                if (child is Button)
                {
                    Button dummyTextBox = child as Button;
                    dummyTextBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    dummyTextBox.MaxWidth = maxSize;

                }

                if (child is CheckBox)
                {
                    CheckBox dummyTextBox = child as CheckBox;
                    dummyTextBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    dummyTextBox.MaxWidth = maxSize;

                }

                if (child is StackPanel)
                {
                    StackPanel dummyTextBox = child as StackPanel;
                    dummyTextBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    if (dummyTextBox.Uid == "FileDialog")
                    {
                        dummyTextBox.MaxWidth = maxSize;
                    }
                    else
                    {
                        dummyTextBox.MaxWidth = dummyTextBox.DesiredSize.Width;
                    }
                }

                if (child is Slider)
                {
                    Slider dummyTextBox = child as Slider;
                    dummyTextBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    dummyTextBox.MaxWidth = maxSizeContent;

                }
            }
        }

        private void TestPanel_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            foreach (UIElement child in Children)
            {
                child.Measure(new Size(ActualWidth, double.PositiveInfinity));
                FrameworkElement element = child as FrameworkElement;

                if (child is StackPanel || child is Button || child is TextBlock || child is CheckBox || child is Grid)
                {
                    FrameworkElement dummyTextBox = child as FrameworkElement;
                    dummyTextBox.MinWidth = 0;
                    dummyTextBox.Width = ActualWidth;
                    continue;
                }

                if (child is Expander)
                {
                    Expander dummyTextBox = child as Expander;
                    dummyTextBox.Width = ActualWidth;
                    continue;
                }

                if (element != null)
                {
                    element.MinWidth = 0;
                    element.MaxWidth = double.MaxValue;
                    element.Width = ActualWidth;
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            double curX = 0, curY = 0, curLineHeight = 0;
            foreach (UIElement child in Children)
            {

                child.Measure(infiniteSize);
                curY += curLineHeight + 2;
                curX = 0;
                curLineHeight = 0;
                curX += maxSize;
                if (child.DesiredSize.Height > curLineHeight)
                {
                    curLineHeight = child.DesiredSize.Height;
                }
            }

            curY += curLineHeight;
            curY += 0;

            Size resultSize = new Size
            {
                Width = double.IsPositiveInfinity(availableSize.Width) ? curX : availableSize.Width,
                Height = double.IsPositiveInfinity(availableSize.Height) ? curY : availableSize.Height
            };
            Height = resultSize.Height;

            return resultSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children == null || Children.Count == 0)
            {
                return finalSize;
            }

            TranslateTransform trans = null;
            double curX = 0, curY = 0, curLineHeight = 0;

            foreach (UIElement child in Children)
            {
                trans = child.RenderTransform as TranslateTransform;
                if (trans == null)
                {
                    child.RenderTransformOrigin = new Point(0, 0);
                    trans = new TranslateTransform();
                    child.RenderTransform = trans;
                }

                curY += curLineHeight + 2;
                curX = 0;
                curLineHeight = 0;

                child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));

                trans.X = curX;
                trans.Y = curY;

                curX += maxSizeCaption;

                if (child.DesiredSize.Height > curLineHeight)
                {
                    curLineHeight = child.DesiredSize.Height;
                }
            }

            curY += curLineHeight;

            Height = curY;
            return finalSize;
        }
    }

    public class BindingInfoComparer : IComparer<ControlEntry>
    {
        public int Compare(ControlEntry x, ControlEntry y)
        {
            if (x.tpa.Order != y.tpa.Order)
            {
                return x.tpa.Order.CompareTo(y.tpa.Order);
            }
            else
            {
                return x.tpa.Caption.CompareTo(y.tpa.Caption);
            }
        }
    }


    public class RadioBoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            int integer = (int)value;
            if (integer == int.Parse(parameter.ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool checkedBool = (bool)value;
            if (checkedBool)
            {
                if (targetType.Name != "Int32")
                {
                    string[] targetlist = targetType.GetEnumNames();
                    return Enum.Parse(targetType, targetlist[(int)parameter]);
                }
                else
                {
                    return parameter;
                }

            }
            else
            {
                return null;
            }
        }
    }

    public class EnumToIntConverter : IValueConverter
    {
        private static EnumToIntConverter instance;

        private EnumToIntConverter() { }

        // singleton
        public static EnumToIntConverter GetInstance()
        {
            if (instance == null)
            {
                instance = new EnumToIntConverter();
            }

            return instance;
        }

        // enum -> int
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value;
        }

        // int -> enum
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.ToObject(targetType, value);

        }
    }

    public class EnumBooleanConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
            {
                return DependencyProperty.UnsetValue;
            }

            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return DependencyProperty.UnsetValue;
            }

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return Enum.Parse(targetType, parameterString);
        }
        #endregion
    }


}

