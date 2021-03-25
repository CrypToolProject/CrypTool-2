using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using System.Windows.Threading;
using System.Threading;
using System.Globalization;
using System.Xml.Schema;
using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Editor;
using CrypTool.PluginBase.Miscellaneous;
using KeyTextBox;
using WorkspaceManager.Model;
using Path = System.IO.Path;
using ValidationType = System.Xml.ValidationType;

namespace Wizard
{
    /// <summary>
    /// Interaction logic for WizardControl.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Wizard.Properties.Resources")]
    public partial class WizardControl : UserControl
    {
        private class PageInfo
        {
            public string name;
            public string description;
            public string image;
            public XElement tag;
            public string headline;
        }

        private class ConditionalTextSetter
        {
            public Action<XElement> TextSetter { get; set; }
            public XElement XElement { get; set; }
        }

        ObservableCollection<PageInfo> currentHistory = new ObservableCollection<PageInfo>();
        private readonly RecentFileList _recentFileList = RecentFileList.GetSingleton();
        private List<ConditionalTextSetter> conditionalTextSetters;
        private Dictionary<string, bool> selectedCategories = new Dictionary<string, bool>();
        private SolidColorBrush selectionBrush = new SolidColorBrush();
        private const string configXMLPath = "Wizard.Config.wizard.config.start.xml";
        private const string defaultLang = "en";
        private XElement wizardConfigXML;
        private Dictionary<string, List<PluginPropertyValue>> propertyValueDict = new Dictionary<string, List<PluginPropertyValue>>();
        private HashSet<TextBox> boxesWithWrongContent = new HashSet<TextBox>();
        private HistoryTranslateTransformConverter historyTranslateTransformConverter = new HistoryTranslateTransformConverter();
        private List<TextBox> currentOutputBoxes = new List<TextBox>();
        private List<ProgressBar> currentProgressBars = new List<ProgressBar>();
        private List<TextBox> currentInputBoxes = new List<TextBox>();
        private List<ContentControl> currentPresentations = new List<ContentControl>();
        private WorkspaceManager.WorkspaceManagerClass currentManager = null;
        private bool canStopOrExecute = false;
        private string _title;

        internal event OpenEditorHandler OnOpenEditor;
        internal event OpenTabHandler OnOpenTab;
        internal event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        internal string SamplesDir { set; private get; }
        
        public WizardControl()
        {
            InitializeComponent();
            OuterScrollViewer.Focus();
            Loaded += delegate { Keyboard.Focus(this); };
        }

        public void Initialize()
        {
            try
            {
                // DEBUG HELP string[] names = this.GetType().Assembly.GetManifestResourceNames();

                XElement xml = GetXml(configXMLPath);
                GenerateXML(xml);
                
                currentHistory.CollectionChanged += delegate
                {
                    CreateHistory();
                };

                selectionBrush.Color = Color.FromArgb(255, 200, 220, 245);
                SetupPage(wizardConfigXML);
                AddToHistory(wizardConfigXML);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Couldn't create wizard: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        public XElement WizardConfigXML
        {
            get { return wizardConfigXML; }
        }

        private XElement GetXml(string xmlPath)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.ValidationType = ValidationType.DTD;
            settings.ValidationEventHandler += delegate(object sender, ValidationEventArgs e)
                                                   {
                                                       GuiLogMessage(string.Format("Error validating wizard XML file {0}: {1}", xmlPath, e.Message), NotificationLevel.Error);
                                                   };
            settings.XmlResolver = new ResourceDTDResolver();

            return LoadXMLFromAssembly(xmlPath, settings);
        }

        public static XElement LoadXMLFromAssembly(string xmlPath, XmlReaderSettings settings)
        {
            XmlReader xmlReader = XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream(xmlPath), settings);
            return XElement.Load(xmlReader);
        }

        internal class ResourceDTDResolver : XmlResolver
        {
            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                if (Path.GetFileName(absoluteUri.LocalPath) == "wizard.dtd")
                    return Assembly.GetExecutingAssembly().GetManifestResourceStream("Wizard.Config.wizard.dtd");
                return null;
            }

            public override ICredentials Credentials
            {
                set { }
            }
        }

        // generate the full XML tree for the wizard (recursive)
        private void GenerateXML(XElement xml)
        {
            try
            {
                IEnumerable<XElement> allFiles = xml.Elements("file");
                foreach(var ele in allFiles)
                {
                    XAttribute att = ele.Attribute("resource");
                    if (att != null)
                    {
                        string path = att.Value;
                        XElement sub = GetXml(path);
                        ele.AddAfterSelf(sub);
                    }
                }

                IEnumerable<XElement> allElements = xml.Elements();
                if (allElements.Any())
                {
                    foreach (XElement ele in allElements)
                    {
                        if (ele.Name != "file")
                        {
                            GenerateXML(ele);
                        }
                    }
                }

                wizardConfigXML = xml;

            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Could not GenerateXML: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        #region WidthConverter

        [ValueConversion(typeof(Double), typeof(Double))]
        private class WidthConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (double)value * (double)parameter;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private WidthConverter widthConverter = new WidthConverter();

        #endregion

        #region HistoryTranslateTransformConverter

        [ValueConversion(typeof(double), typeof(double))]
        class HistoryTranslateTransformConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (double)value - 2;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        private void SetupPage(XElement element)
        {
            StopCurrentWorkspaceManager();
            nextButton.IsEnabled = true;
            CreateProjectButton.Visibility = Visibility.Hidden;
            HideErrorLabel();

            currentOutputBoxes.Clear();
            currentProgressBars.Clear();
            currentInputBoxes.Clear();
            currentPresentations.Clear();
            boxesWithWrongContent.Clear();
            conditionalTextSetters = new List<ConditionalTextSetter>();
            SaveContent();

            if ((element.Name == "loadSample") && (element.Attribute("file") != null) && (element.Attribute("title") != null))
            {
                if (!LoadSample(element.Attribute("file").Value, element.Attribute("title").Value, true, element))
                {
                    ErrorLabel.Visibility = Visibility.Visible;
                }
                abortButton_Click(null, null);
                return;
            }

            XElement parent = element.Parent;
            if (parent == null)
            {
                backButton.IsEnabled = false;
                abortButton.IsEnabled = false;
            }
            else
            {
                backButton.IsEnabled = true;
                abortButton.IsEnabled = true;
            }

            //nextButton.IsEnabled = false;

            //set headline
            XElement headline = FindElementsInElement(element, "headline").First();
            if (headline != null)
            {
                SetTextFromXElement(headline, delegate(XElement el) { taskHeader.Content = el.Value.Trim().ToUpper(); });
            }

            //set task description label
            XElement task = FindElementsInElement(element, "task").First();
            if (task != null)
            {
                SetTextFromXElement(task, delegate(XElement el) { descHeader.Text = el.Value.Trim(); });
            }


            if (element.Name == "input" || element.Name == "sampleViewer")
            {
                categoryGrid.Visibility = Visibility.Hidden;
                inputPanel.Visibility = Visibility.Visible;
                
                var inputs = from el in element.Elements()
                             where el.Name == "inputBox" || el.Name == "comboBox" || el.Name == "checkBox" || el.Name == "outputBox" 
                             || el.Name == "keyTextBox" || el.Name == "progressBar" || el.Name == "label" || el.Name == "presentation" 
                             || el.Name == "pluginSetter"
                             select el;

                inputStack.Children.Clear();
                
                var allNexts = (from el in element.Elements()
                                 where el.Name == "input" || el.Name == "category" || el.Name == "loadSample" || el.Name == "sampleViewer"
                                 select el);
                if (allNexts.Count() > 0)
                {
                    inputPanel.Tag = allNexts.First();
                    if (allNexts.First().Name == "loadSample")
                        SwitchNextButtonContent();
                }
                else
                {
                    var dummy = new XElement("loadSample");
                    element.Add(dummy);
                    inputPanel.Tag = dummy;
                    SwitchNextButtonContent();
                }

                FillInputStack(inputs, element.Name.ToString(), (element.Name == "input"));

                if (element.Name == "sampleViewer" && (element.Attribute("file") != null))
                {
                    nextButton.IsEnabled = false;
                    if (element.Attribute("showCreateButton") == null || element.Attribute("showCreateButton").Value.ToLower() != "false")
                    {
                        CreateProjectButton.Visibility = Visibility.Visible;
                    }
                    
                    if (!LoadSample(element.Attribute("file").Value, null, false, element))
                    {
                        ErrorLabel.Visibility = Visibility.Visible;
                    }
                }

                string id = GetElementID((XElement)inputPanel.Tag);

            }
            else if (element.Name == "category")
            {
                categoryGrid.Visibility = Visibility.Visible;
                inputPanel.Visibility = Visibility.Hidden;
                
                radioButtonStackPanel.Children.Clear();

                //generate radio buttons
                var options = from el in element.Elements()
                              where el.Name == "category" || el.Name == "input" || el.Name == "loadSample" || el.Name == "sampleViewer"
                              select el;

                if (options.Any())
                {
                    bool isSelected = false;

                    foreach (XElement ele in options)
                    {
                        Border border = new Border();
                        Label l = new Label();
                        Image i = new Image();
                        StackPanel sp = new StackPanel();

                        border.Child = sp;
                        border.VerticalAlignment = VerticalAlignment.Stretch;
                        border.CornerRadius = new CornerRadius(5, 0, 0, 5);
                        border.BorderBrush = Brushes.LightSeaGreen;

                        l.Height = 35;
                        l.HorizontalAlignment = HorizontalAlignment.Stretch;
                        XElement label = FindElementsInElement(ele, "name").First();
                        if (label != null)
                        {
                            SetTextFromXElement(label, delegate(XElement el) { l.Content = el.Value.Trim(); });
                        }

                        i.Width = 26;
                        string image = ele.Attribute("image").Value;
                        if (image != null)
                        {
                            ImageSource ims = (ImageSource) TryFindResource(image);
                            if (ims != null)
                            {
                                i.Source = ims;
                                sp.Children.Add(i);
                            }
                            else
                                GuiLogMessage(string.Format("Could not find ressource image {0}!",image), NotificationLevel.Warning);
                        }

                        sp.VerticalAlignment = VerticalAlignment.Stretch;
                        sp.HorizontalAlignment = HorizontalAlignment.Stretch;
                        sp.Orientation = Orientation.Horizontal;
                        sp.Children.Add(l);

                        RadioButton rb = new RadioButton();
                        rb.Focusable = false;
                        string id = GetElementID(ele);
                        rb.Checked += rb_Checked;
                        rb.MouseDoubleClick += rb_MouseDoubleClick;
                        rb.HorizontalAlignment = HorizontalAlignment.Stretch;
                        rb.VerticalAlignment = VerticalAlignment.Stretch;
                        rb.VerticalContentAlignment = VerticalAlignment.Center;
                        rb.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                        rb.Content = border;
                        rb.Tag = ele;
                        if (ele.Name == "loadSample")
                        {
                            RoutedEventHandler rbEvent = delegate
                                              {
                                                  SwitchNextButtonContent();
                                              };
                            rb.Checked += rbEvent;
                            rb.Unchecked += rbEvent;
                        }

                        radioButtonStackPanel.Children.Add(rb);
                        bool wasSelected = false;
                        selectedCategories.TryGetValue(GetElementID(ele), out wasSelected);
                        if (wasSelected)
                        {
                            rb.IsChecked = true;
                            isSelected = true;
                        }
                    }

                    if (!isSelected)
                    {
                        RadioButton b = (RadioButton)radioButtonStackPanel.Children[0];
                        b.IsChecked = true;
                        selectedCategories.Remove(GetElementID((XElement)b.Tag));
                        selectedCategories.Add(GetElementID((XElement)b.Tag), true);
                    }

                }
            }
        }

        private void HideErrorLabel()
        {
            if ((ErrorLabel.Tag != null) && ((bool)(ErrorLabel.Tag) == true))
            {
                ErrorLabel.Visibility = Visibility.Collapsed;
                ErrorLabel.Tag = false;
            }
            else
            {
                ErrorLabel.Tag = true;
            }
        }

        void rb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            nextButton_Click(sender, e);
        }

        private void FillInputStack(IEnumerable<XElement> inputs, string type, bool isInput)
        {
            var inputFieldStyle = (Style)FindResource("InputFieldStyle");

            var groups = from input in inputs where input.Attribute("group") != null select input.Attribute("group").Value;
            groups = groups.Distinct();

            var inputGroups = new List<StackPanel>();
            var otherInputs = new List<StackPanel>();

            foreach (var group in groups)
            {
                var sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Tag = group;
                inputGroups.Add(sp);
            }

            foreach (var input in inputs)
            {
                try
                {
                    var stack = new StackPanel();

                    var descriptionTextBlock = new TextBlock() { FontWeight = FontWeights.Normal };
                    var description = new Label() { Content = descriptionTextBlock };
                    var descEle = FindElementsInElement(input, "description");
                    if (descEle != null && descEle.Any())
                    {
                        SetTextFromXElement(descEle.First(), el =>
                                                                 {
                                                                     descriptionTextBlock.Inlines.Clear();
                                                                     var inline = TrimInline(XMLHelper.ConvertFormattedXElement(el));
                                                                     if (inline != null)
                                                                     {
                                                                         descriptionTextBlock.Inlines.Add(inline);
                                                                     }
                                                                 });
                    }
                    description.HorizontalAlignment = HorizontalAlignment.Left;
                    description.FontWeight = FontWeights.Bold;
                    stack.Children.Add(description);

                    if (input.Name != "label")
                    {
                        Control inputElement = CreateInputElement(input, inputFieldStyle, isInput);

                        //TODO add controls to same "level" if they have the same group

                        //Set width:
                        if (inputElement != null && input.Attribute("width") != null)
                        {
                            string width = input.Attribute("width").Value.Trim();
                            if (width.EndsWith("%"))
                            {
                                double percentage;
                                if (Double.TryParse(width.Substring(0, width.Length - 1), out percentage))
                                {
                                    percentage /= 100;
                                    Binding binding = new Binding("ActualWidth");
                                    binding.Source = inputStack;
                                    binding.Converter = widthConverter;
                                    binding.ConverterParameter = percentage;
                                    inputElement.SetBinding(FrameworkElement.WidthProperty, binding);
                                }
                            }
                            else
                            {
                                double widthValue;
                                if (Double.TryParse(width, out widthValue))
                                {
                                    inputElement.Width = widthValue;
                                }
                            }
                        }

                        //Set alignment
                        if (inputElement != null && input.Attribute("alignment") != null)
                        {
                            switch (input.Attribute("alignment").Value.Trim().ToLower())
                            {
                                case "right":
                                    inputElement.HorizontalAlignment = HorizontalAlignment.Right;
                                    break;
                                case "left":
                                    inputElement.HorizontalAlignment = HorizontalAlignment.Left;
                                    break;
                                case "center":
                                    inputElement.HorizontalAlignment = HorizontalAlignment.Center;
                                    break;
                                case "stretch":
                                    inputElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                                    break;
                            }
                        }

                        stack.Children.Add(inputElement);
                    }

                    if (input.Attribute("group") != null && inputGroups.Any())
                    {
                        var sp = from g in inputGroups where (string)g.Tag == input.Attribute("group").Value select g;
                        if (sp.Any())
                        {
                            var group = sp.First();
                            group.Children.Add(stack);
                        }
                    }
                    else
                    {
                        stack.Tag = input;
                        otherInputs.Add(stack);
                    }
                }
                catch (Exception e)
                {
                    GuiLogMessage(string.Format("Error while creating wizard element {0}: {1}", input, e.Message), NotificationLevel.Error);
                }
            }

            foreach (var input in inputs)
            {
                if (input.Attribute("group") != null && inputGroups.Any())
                {
                    var sp = from g in inputGroups where (string)g.Tag == input.Attribute("group").Value select g;
                    if (sp.Any())
                    {
                        var group = sp.First();
                        if (!inputStack.Children.Contains(group))
                            inputStack.Children.Add(group);
                    }
                }
                else
                {
                    var p = from g in otherInputs where (XElement)g.Tag == input select g;
                    if (p.Any())
                    {
                        var put = p.First();
                        inputStack.Children.Add(put);
                    }
                }
            }

        }

        private Inline TrimInline(Inline inline, bool left = true, bool right = true)
        {
            if (inline is Run)
            {
                if (left)
                {
                    ((Run) inline).Text = ((Run) inline).Text.TrimStart();
                }
                if (right)
                {
                    ((Run) inline).Text = ((Run) inline).Text.TrimEnd();
                }
            }
            else if (inline is Span)
            {
                var inlines = ((Span) inline).Inlines;
                TrimInline(inlines.First(), true, false);
                TrimInline(inlines.Last(), false, true);
            }
            return inline;
        }

        private Control CreateInputElement(XElement input, Style inputFieldStyle, bool isInput)
        {
            Control element = null;

            string key = null;
            if (input.Name != "presentation" && input.Name != "progressBar")
                key = GetElementPluginPropertyKey(input);

            var pluginPropertyValue = GetPropertyValue(key, input.Parent);

            XElement xel;
            switch (input.Name.ToString())
            {
                case "pluginSetter":
                    var pluginInputBox = new TextBox();
                    pluginInputBox.Visibility = Visibility.Collapsed;
                    pluginInputBox.Tag = input;
                    pluginInputBox.Text = input.Value;
                    element = pluginInputBox;
                    break;

                case "inputBox":
                    var inputBox = new TextBox();
                    inputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    inputBox.Tag = input;
                    inputBox.AcceptsReturn = true;
                    inputBox.TextWrapping = TextWrapping.Wrap;
                    if (input.Attribute("visibleLines") != null)
                    {
                        int visibleLines;
                        if (Int32.TryParse(input.Attribute("visibleLines").Value.Trim(), out visibleLines))
                        {
                            inputBox.MinLines = visibleLines;
                            inputBox.MaxLines = visibleLines;
                        }
                    }
                    inputBox.Style = inputFieldStyle;

                    if (input.Attribute("regex") != null)
                    {
                        var regex = new Regex(input.Attribute("regex").Value, RegexOptions.Compiled);

                        inputBox.TextChanged += delegate
                        {
                            CheckRegex(inputBox, regex);
                        };
                    }

                    if (key != null && pluginPropertyValue != null)
                        inputBox.Text = (string) pluginPropertyValue.Value;
                    else
                    {
                        var defaultvalues = FindElementsInElement(input, "defaultvalue");
                        SetTextFromXElement(defaultvalues.First(), delegate(XElement el)
                                                                       {
                                                                           if (!string.IsNullOrEmpty(el.Value))
                                                                               inputBox.Text = el.Value.Trim();
                                                                       });
                    }

                    if (!isInput)
                        currentInputBoxes.Add(inputBox);

                    xel = input.Element("storage");
                    if (xel != null)
                    {
                        var storageContainer = new StorageContainer(ShowStorageOverlay);
                        bool showStorageButton;
                        bool showLoadAddButtons;
                        GetStorageAttributes(xel, out showStorageButton, out showLoadAddButtons);
                        storageContainer.AddContent(inputBox, xel.Attribute("key").Value, showStorageButton, showLoadAddButtons, showLoadAddButtons);
                        storageContainer.SetValueMethod(delegate(string s) { inputBox.Text = s; });
                        storageContainer.GetValueMethod(() => inputBox.Text);
                        element = storageContainer;
                    }
                    else
                    {
                        element = inputBox;
                    }
                    break;

                case "comboBox":
                    var comboBox = new ComboBox();
                    comboBox.Style = inputFieldStyle;
                    comboBox.Tag = input;
                    comboBox.SelectionChanged += InputComboBoxSelectionChanged;

                    var items = FindElementsInElement(input, "item");
                    foreach (var item in items)
                    {
                        var cbi = new ComboBoxItem();
                        if (item.Attribute("content") != null)
                        {
                            cbi.Content = item.Attribute("content").Value;
                        }
                        comboBox.Items.Add(cbi);
                    }

                    if (key != null && pluginPropertyValue != null)
                    {
                        if (pluginPropertyValue.Value is int)
                        {
                            var cbi = (ComboBoxItem) comboBox.Items.GetItemAt((int)pluginPropertyValue.Value);
                            cbi.IsSelected = true;
                        }
                    }
                    else if (input.Attribute("defaultValue") != null)
                    {
                        int i;
                        if (Int32.TryParse(input.Attribute("defaultValue").Value.Trim(), out i))
                        {
                            var cbi = (ComboBoxItem) comboBox.Items.GetItemAt(i);
                            cbi.IsSelected = true;
                        }
                    }
                    else
                    {
                        ((ComboBoxItem) comboBox.Items.GetItemAt(0)).IsSelected = true;
                    }

                    element = comboBox;
                    break;

                case "checkBox":
                    CheckBox checkBox = new CheckBox();
                    checkBox.Tag = input;
                    checkBox.Style = inputFieldStyle;

                    var contents = FindElementsInElement(input, "content");
                    SetTextFromXElement(contents.First(), delegate(XElement el)
                                                   {
                                                       if (!string.IsNullOrEmpty(el.Value))
                                                           checkBox.Content = el.Value.Trim();
                                                   });

                    if (key != null && pluginPropertyValue != null)
                    {
                        string value = (string)pluginPropertyValue.Value;
                        if (value.ToLower() == "true")
                            checkBox.IsChecked = true;
                        else
                            checkBox.IsChecked = false;
                    }
                    else if (input.Attribute("defaultValue") != null)
                    {
                        string value = input.Attribute("defaultValue").Value;
                        if (value.ToLower() == "true")
                            checkBox.IsChecked = true;
                        else
                            checkBox.IsChecked = false;
                    }

                    element = checkBox;
                    break;

                case "outputBox":
                    if (isInput)
                        break;

                    var outputBox = new TextBox();
                    outputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    outputBox.Tag = input;
                    outputBox.AcceptsReturn = true;
                    outputBox.TextWrapping = TextWrapping.Wrap;
                    if (input.Attribute("visibleLines") != null)
                    {
                        int visibleLines;
                        if (Int32.TryParse(input.Attribute("visibleLines").Value.Trim(), out visibleLines))
                        {
                            outputBox.MinLines = visibleLines;
                            outputBox.MaxLines = visibleLines;
                        }
                    }
                    outputBox.Style = inputFieldStyle;

                    if (input.Attribute("regex") != null)
                    {
                        var regex = new Regex(input.Attribute("regex").Value, RegexOptions.Compiled);

                        outputBox.TextChanged += delegate
                        {
                            CheckRegex(outputBox, regex);
                        };
                    }
                    
                    outputBox.IsReadOnly = true;
                    currentOutputBoxes.Add(outputBox);

                    xel = input.Element("storage");
                    if (xel != null)
                    {
                        var storageContainer = new StorageContainer(ShowStorageOverlay);
                        bool showLoadAddButtons;
                        bool showStorageButton;
                        GetStorageAttributes(xel, out showStorageButton, out showLoadAddButtons);
                        storageContainer.AddContent(outputBox, xel.Attribute("key").Value, showStorageButton, false, showLoadAddButtons);
                        storageContainer.SetValueMethod(null);
                        storageContainer.GetValueMethod(() => outputBox.Text);
                        element = storageContainer;
                    }
                    else
                    {
                        element = outputBox;
                    }
                    break;

                case "progressBar":
                    if (isInput)
                        break;

                    var progressBar = new ProgressBar();
                    progressBar.Tag = input;
                    progressBar.Style = inputFieldStyle;
                    progressBar.Height = 30;
                    currentProgressBars.Add(progressBar);
                    element = progressBar;
                    break;

                case "keyTextBox":
                    var keyTextBox = new KeyTextBox.KeyTextBox();
                    var keyManager = new SimpleKeyManager(input.Attribute("format").Value);
                    keyTextBox.KeyManager = keyManager;
                    if (key != null && pluginPropertyValue != null)
                    {
                        keyManager.SetKey((string) pluginPropertyValue.Value);
                    }
                    else if (input.Attribute("defaultkey") != null)
                    {
                        keyManager.SetKey(input.Attribute("defaultkey").Value);
                    }
                    keyTextBox.Tag = input;
                    keyTextBox.Style = inputFieldStyle;

                    xel = input.Element("storage");
                    if (xel != null)
                    {
                        var storageContainer = new StorageContainer(ShowStorageOverlay);
                        bool showLoadAddButtons;
                        bool showStorageButton;
                        GetStorageAttributes(xel, out showStorageButton, out showLoadAddButtons);
                        storageContainer.AddContent(keyTextBox, xel.Attribute("key").Value, showStorageButton, showLoadAddButtons, showLoadAddButtons);
                        storageContainer.SetValueMethod(delegate(string s) { keyTextBox.CurrentKey = s; });
                        storageContainer.GetValueMethod(() => keyTextBox.CurrentKey);
                        element = storageContainer;
                    }
                    else
                    {
                        element = keyTextBox;
                    }
                    break;

                case "presentation":
                    if (isInput)
                        break;

                    var cc = new ContentControl();

                    //Set height:
                    if (input.Attribute("height") != null)
                    {
                        string height = input.Attribute("height").Value.Trim();
                        if (height.EndsWith("%"))
                        {
                            double percentage;
                            if (Double.TryParse(height.Substring(0, height.Length - 1), out percentage))
                            {
                                percentage /= 100;
                                Binding binding = new Binding("ActualHeight");
                                binding.Source = inputStack;
                                binding.Converter = widthConverter;
                                binding.ConverterParameter = percentage;
                                cc.SetBinding(FrameworkElement.HeightProperty, binding);
                            }
                        }
                        else
                        {
                            double heightValue;
                            if (Double.TryParse(height, out heightValue))
                            {
                                cc.Height = heightValue;
                            }
                        }
                    }

                    cc.Style = inputFieldStyle;
                    cc.Tag = input;

                    currentPresentations.Add(cc);
                    element = cc;
                    break;
            }

            return element;
        }

        private static void GetStorageAttributes(XElement xel, out bool showStorageButton, out bool showLoadAddButtons)
        {
            showStorageButton = false;
            if (xel.Attribute("showStorageButton") != null)
            {
                showStorageButton = xel.Attribute("showStorageButton").Value == "true";
            }
            showLoadAddButtons = false;
            if (xel.Attribute("showLoadAddButtons") != null)
            {
                showLoadAddButtons = xel.Attribute("showLoadAddButtons").Value == "true";
            }
        }

        private void ShowStorageOverlay(StorageControl control)
        {
            control.CloseEvent += delegate { Overlay.Visibility = Visibility.Collapsed; };
            StorageGrid.Children.Clear();
            StorageGrid.Children.Add(control);
            Overlay.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// If a combobox on an input page is changed, this method checks if some conditional texts on the same page have to be changed.
        /// </summary>
        private void InputComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox) sender;
            var ele = (XElement) comboBox.Tag;

            var pluginSetters = (from i in ele.Elements("item")
                                 group i by i.Attribute("lang").Value
                                 into ig
                                 select ig.ElementAt(comboBox.SelectedIndex)).SelectMany(x => x.Elements("pluginSetter")).ToList();

            string pluginName = null;
            string propertyName = null;
            if (ele.Attribute("plugin") != null && ele.Attribute("property") != null)
            {
                pluginName = ele.Attribute("plugin").Value;
                propertyName = ele.Attribute("property").Value;
            }

            foreach (var conditionalTextSetter in conditionalTextSetters)
            {
                foreach (var condition in conditionalTextSetter.XElement.Elements("condition"))
                {
                    var condPlugin = condition.Attribute("plugin").Value;
                    var condProperty = condition.Attribute("property").Value;
                    int condValue;
                    if (!int.TryParse(condition.Attribute("value").Value, out condValue))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(pluginName) && !string.IsNullOrEmpty(propertyName))
                    {
                        if (condPlugin == pluginName && condProperty == propertyName)
                        {
                            if (condValue == comboBox.SelectedIndex)
                            {
                                conditionalTextSetter.TextSetter(condition);
                            }
                        }
                    }

                    foreach (var pluginSetter in pluginSetters)
                    {
                        if (condPlugin == pluginSetter.Attribute("plugin").Value && condProperty == pluginSetter.Attribute("property").Value)
                        {
                            int setterValue;
                            if (int.TryParse(pluginSetter.Value, out setterValue))
                            {
                                if (condValue == setterValue)
                                {
                                    conditionalTextSetter.TextSetter(condition);
                                }
                            }
                        }
                    }
                }
            }
        }

        private PluginPropertyValue GetPropertyValue(string key, XElement path)
        {
            if (key != null && propertyValueDict.ContainsKey(key))
            {
                foreach (var pv in propertyValueDict[key])
                {
                    if (IsSamePath(pv.Path, path))
                    {
                        return pv;
                    }
                }
            }
            return null;
        }

        private bool IsSamePath(XElement path, XElement path2)
        {
            if (path == path2 || path.Descendants().Contains(path2) || path2.Descendants().Contains(path))
            {
                return true;
            }
            return false;
        }

        private string GetElementPluginPropertyKey(XElement element)
        {
            string key = null;
            if (element.Attribute("plugin") != null && element.Attribute("property") != null)
            {
                var plugin = element.Attribute("plugin").Value;
                var property = element.Attribute("property").Value;
                key = string.Format("{0}.{1}", plugin, property);
            }
            return key;
        }

        private void CreateHistory()
        {
            StackPanel historyStack = new StackPanel();
            historyStack.Orientation = Orientation.Horizontal;

            foreach (var page in currentHistory)
            {
                var p = new ContentControl();
                p.Focusable = false;
                var bg = selectionBrush.Clone();
                bg.Opacity = 1 - (historyStack.Children.Count / (double)currentHistory.Count);
                var sp = new StackPanel { Orientation = Orientation.Horizontal, Background = bg };
                p.Content = sp;
                p.Tag = page.tag;
                p.MouseDoubleClick += new MouseButtonEventHandler(page_MouseDoubleClick);

                Polygon triangle = new Polygon();
                triangle.Points = new PointCollection();
                triangle.Points.Add(new Point(0, 0));
                triangle.Points.Add(new Point(0, 10));
                triangle.Points.Add(new Point(10, 5));
                triangle.Fill = bg;
                triangle.Stretch = Stretch.Uniform;
                triangle.Width = 32;
                sp.Children.Add(triangle); 
                if (page.image != null && FindResource(page.image) != null)
                {
                    var im = new Image { Source = (ImageSource)FindResource(page.image), Width = 32 };
                    sp.Children.Add(im);
                }
                var nameLabel = new Label { Content = page.name };
                sp.Children.Add(nameLabel);
                p.ToolTip = page.headline;
                var translateTranform = new TranslateTransform();
                triangle.RenderTransform = translateTranform;
                Binding binding = new Binding("ActualWidth");
                binding.Source = p;
                binding.Converter = historyTranslateTransformConverter;
                BindingOperations.SetBinding(translateTranform, TranslateTransform.XProperty, binding);

                historyStack.Children.Add(p);
            }

            history.Content = historyStack;
            history.ScrollToRightEnd();
        }

        void page_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SwitchButtonWhenNecessary();
            canStopOrExecute = false;

            var cc = (ContentControl)sender;
            var hs = (StackPanel)history.Content;
            int i = hs.Children.IndexOf(cc);
            history.Content = null;

            while (currentHistory.Count > i+1)
            {
                currentHistory.RemoveAt(currentHistory.Count - 1);
            }

            CreateHistory();

            XElement parent = (XElement)cc.Tag;

            if (parent == null)
                parent = wizardConfigXML;

            SetupPage(parent);
        }

        private void CheckRegex(TextBox textBox, Regex regex)
        {
            var match = regex.Match(textBox.Text);
            if (!match.Success || match.Index != 0 || match.Length != textBox.Text.Length)
            {
                textBox.Style = (Style) FindResource("TextInputInvalid");
                GuiLogMessage(string.Format("Content of textbox does not fit regular expression {0}.", regex.ToString()), NotificationLevel.Error);
                boxesWithWrongContent.Add(textBox);
                nextButton.IsEnabled = false;
            }
            else
            {
                textBox.Style = null;
                boxesWithWrongContent.Remove(textBox);
                if (boxesWithWrongContent.Count == 0)
                    nextButton.IsEnabled = true;
            }
        }

        private string GetElementID(XElement element)
        {
            if (element != null && element.Parent != null)
            {
                return GetElementID(element.Parent) + "[" + element.Parent.Nodes().ToList().IndexOf(element) + "]." + element.Name;
            }
            else
                return "";
        }

        private bool LoadSample(string file, string title, bool openTab, XElement element)
        {
            try
            {
                _title = title;
                file = Path.Combine(SamplesDir, file);

                var persistance = new ModelPersistance();
                persistance.OnGuiLogNotificationOccured += delegate(IPlugin sender, GuiLogEventArgs args)
                                                        {
                                                            OnGuiLogNotificationOccured(sender, args);
                                                        };
                var model = persistance.loadModel(file,false);
                model.OnGuiLogNotificationOccured += delegate(IPlugin sender, GuiLogEventArgs args)
                                                         {
                                                             OnGuiLogNotificationOccured(sender, args);
                                                         };

                foreach (PluginModel pluginModel in model.GetAllPluginModels())
                {
                    pluginModel.Plugin.Initialize();
                }

                if (!openTab)
                {
                    CreateProjectButton.Tag = element;
                    RegisterEventsForLoadedSample(model);
                }

                FillDataToModel(model, element);

                //load sample:
                if (openTab)
                {
                    ImageSource ims = null;
                    currentManager = (WorkspaceManager.WorkspaceManagerClass) OnOpenEditor(typeof (WorkspaceManager.WorkspaceManagerClass), null);
                    currentManager.Open(model);
                    persistance.HandleTemplateReplacement(file, model);
                    if (CrypTool.PluginBase.Properties.Settings.Default.Wizard_RunTemplate)
                    {
                        currentManager.OnFileLoaded += NewEditorOnFileLoaded;
                    }
                    
                    if (element.Attribute("image") != null)
                    {
                        ims = (ImageSource)TryFindResource(element.Attribute("image").Value);
                    }
                    currentManager.Presentation.ToolTip = _title;
                    var tooltip = new Span();
                    tooltip.Inlines.Add(new Run(_title));
                    OnOpenTab(currentManager, new TabInfo() { Title = _title,  Tooltip = tooltip, Icon = ims }, null);
                }
                else
                {
                    currentManager = new WorkspaceManager.WorkspaceManagerClass();
                    currentManager.Open(model);
                    canStopOrExecute = true;
                    currentManager.OnFileLoaded += OnFileLoaded;
                }

                _recentFileList.AddRecentFile(file);
            }
            catch(Exception ex)
            {
                GuiLogMessage(string.Format("Error loading sample {0}: {1}", file,ex.Message),NotificationLevel.Error);
                return false;
            }
            return true;
        }

        private void NewEditorOnFileLoaded(IEditor editor, string filename)
        {
            if (CrypTool.PluginBase.Properties.Settings.Default.Wizard_RunTemplate && currentManager.CanExecute)
                currentManager.Execute();
            currentManager.OnFileLoaded -= NewEditorOnFileLoaded;
            OnOpenTab(currentManager, new TabInfo() { Title = _title }, null);
        }

        private void OnFileLoaded(IEditor editor, string filename)
        {
            currentManager.Execute();
            currentManager.OnFileLoaded -= OnFileLoaded;
        }

        private void FillDataToModel(WorkspaceModel model, XElement element)
        {
            //Fill in all data from wizard to sample:
            foreach (var c in propertyValueDict)
            {
                foreach (var ppv in c.Value)
                {
                    if (IsSamePath(element, ppv.Path))
                    {
                        try
                        {
                            var plugins = ppv.PluginName.Split(';');
                            foreach (var plugin in model.GetAllPluginModels().Where(x => plugins.Contains(x.GetName())))
                            {
                                if (SetPluginProperty(ppv, plugin))    
                                    plugin.Plugin.Initialize();
                                else
                                    GuiLogMessage(string.Format("Failed settings plugin property {0}.{1} to \"{2}\"!", plugin.GetName(), ppv.PropertyName, ppv.Value), NotificationLevel.Error);
                            }
                        }
                        catch (Exception)
                        {
                            GuiLogMessage(string.Format("Failed settings plugin property {0}.{1} to \"{2}\"!", ppv.PluginName, ppv.PropertyName, ppv.Value), NotificationLevel.Error);
                        }
                    }
                }
            }
        }

        internal static bool SetPluginProperty(PluginPropertyValue ppv, PluginModel plugin)
        {
            var settings = plugin.Plugin.Settings;

            object propertyObject = null;
            PropertyInfo property;
            if (!ppv.PropertyName.StartsWith(".") && plugin.Plugin.GetType().GetProperty(ppv.PropertyName) != null)
            {
                property = plugin.Plugin.GetType().GetProperty(ppv.PropertyName);
                propertyObject = plugin.Plugin;
            }
            else
            {
                if (ppv.PropertyName.StartsWith("."))
                {
                    property = settings.GetType().GetProperty(ppv.PropertyName.Substring(1, ppv.PropertyName.Length - 1));
                    propertyObject = settings;
                }
                else
                {
                    property = settings.GetType().GetProperty(ppv.PropertyName);
                    propertyObject = settings;
                }
            }
            
            if (property != null)
            {
                if (ppv.Value is string)
                    SetPropertyToString(ppv, propertyObject, property);
                else if (ppv.Value is int)
                    property.SetValue(propertyObject, (int)ppv.Value, null);
                else if (ppv.Value is bool)
                    property.SetValue(propertyObject, (bool)ppv.Value, null);
                return true;
            }
            else
            {
                //if (ppv.Value is object)
                //{
                //    return true;
                //}
                return false;
            }
        }

        private static void SetPropertyToString(PluginPropertyValue ppv, object settings, PropertyInfo property)
        {
            if (property.PropertyType == typeof(string))
            {
                property.SetValue(settings, (string) ppv.Value, null);
            }
            else if (property.PropertyType == typeof(int))
            {
                property.SetValue(settings, Int32.Parse((string)ppv.Value), null);
            }
            else if (property.PropertyType == typeof(double))
            {
                property.SetValue(settings, Double.Parse((string)ppv.Value), null);
            }
        }

        private void RegisterEventsForLoadedSample(WorkspaceModel model)
        {
            //check if the addressed plugins are present in the template
            var referencedPlugins = currentInputBoxes.Cast<Control>().Concat<Control>(currentOutputBoxes).Concat<Control>(currentPresentations).Concat<Control>(currentProgressBars);
            foreach (var plugin in referencedPlugins)
            {
                XElement ele = (XElement)plugin.Tag;
                var pluginName = ele.Attribute("plugin").Value;
                if (pluginName != null)
                {
                    int n = model.GetAllPluginModels().Where(x => x.GetName() == pluginName).Count();
                    if (n == 0)
                    {
                        GuiLogMessage("Could not find plugin '"+pluginName+"'.", NotificationLevel.Warning);
                    }
                    else if (n > 1)
                    {
                        GuiLogMessage("Found more than one plugin '" + pluginName + "'.", NotificationLevel.Warning);
                    }
                }
            }

            //Register events for output boxes:
            foreach (var outputBox in currentOutputBoxes)
            {
                XElement ele = (XElement)outputBox.Tag;
                var pluginName = ele.Attribute("plugin").Value;
                var propertyName = ele.Attribute("property").Value;
                if (pluginName != null && propertyName != null)
                {
                    var plugin = model.GetAllPluginModels().Where(x => x.GetName() == pluginName).First().Plugin;
                    var settings = plugin.Settings;
                    object theObject = null;

                    var property = plugin.GetType().GetProperty(propertyName);
                    EventInfo propertyChangedEvent = null;
                    if (property != null)
                    {
                        propertyChangedEvent = plugin.GetType().GetEvent("PropertyChanged");
                        theObject = plugin;
                    }
                    else    //Use property from settings
                    {
                        property = settings.GetType().GetProperty(propertyName);
                        propertyChangedEvent = settings.GetType().GetEvent("PropertyChanged");
                        theObject = settings;
                    }

                    if (property != null && propertyChangedEvent != null)
                    {
                        TextBox box = outputBox;
                        propertyChangedEvent.AddEventHandler(theObject, (PropertyChangedEventHandler)delegate(Object sender, PropertyChangedEventArgs e)
                                                                                                         {
                                                                                                             if (e.PropertyName == propertyName)
                                                                                                             {
                                                                                                                 UpdateOutputBox(box, property, theObject);
                                                                                                             }
                                                                                                         });
                    }
                }
            }

            //Register events for progress bars:
            foreach (var progressBar in currentProgressBars)
            {
                XElement ele = (XElement) progressBar.Tag;
                var pluginName = ele.Attribute("plugin").Value;
                var plugin = model.GetAllPluginModels().Where(x => x.GetName() == pluginName).First().Plugin;
                ProgressBar bar = progressBar;
                plugin.OnPluginProgressChanged += delegate(IPlugin sender, PluginProgressEventArgs args)
                                                      {
                                                          Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                                          {
                                                              bar.Maximum = args.Max;
                                                              bar.Value = args.Value;
                                                          }, null);
                                                      };

            }

            //fill presentations
            foreach (var presentation in currentPresentations)
            {
                var ele = (XElement)presentation.Tag;
                var pluginName = ele.Attribute("plugin").Value;
                if (!string.IsNullOrEmpty(pluginName))
                {
                    var plugin = model.GetAllPluginModels().Where(x => x.GetName() == pluginName).First().Plugin;
                    if (presentation.Content == null)
                    {
                        presentation.Content = plugin.Presentation;
                        if (presentation.Content.GetType().GetProperty("Text") != null)
                        {
                            var defaultvalues = FindElementsInElement(ele, "defaultvalue");
                            ContentControl presentation1 = presentation;
                            SetTextFromXElement(defaultvalues.First(), delegate(XElement el)
                                                                           {
                                                                               if (!string.IsNullOrEmpty(el.Value))
                                                                               {
                                                                                   presentation1.Content.GetType().GetProperty("Text").SetValue(presentation1.Content, el.Value.Trim(), null);
                                                                               }
                                                                           });
                        }
                    }
                }
            }

            //Register events for input boxes:
            foreach (var inputBox in currentInputBoxes)
            {
                XElement ele = (XElement)inputBox.Tag;
                var pluginName = ele.Attribute("plugin").Value;
                var propertyName = ele.Attribute("property").Value;
                if (pluginName != null && propertyName != null)
                {
                    var pluginNames = pluginName.Split(';');
                    foreach (var plugin in model.GetAllPluginModels().Where(x => pluginNames.Contains(x.GetName())))
                    {
                        var settings = plugin.Plugin.Settings;
                        object theObject = null;

                        var property = plugin.Plugin.GetType().GetProperty(propertyName);
                        //we check, if the property name starts with a "."; thus, we assume we want to target the settings of a component
                        if (!propertyName.StartsWith(".") && property != null)
                        {
                            theObject = plugin.Plugin;
                        }
                        else    //Use property from settings
                        {
                            if (propertyName.StartsWith("."))
                            {
                                property = settings.GetType().GetProperty(propertyName.Substring(1, propertyName.Length - 1));
                                theObject = settings;
                            }
                            else
                            {
                                property = settings.GetType().GetProperty(propertyName);
                                theObject = settings;
                            }                            
                        }

                        if (property != null)
                        {
                            TextBox box = inputBox;
                            PluginModel plugin1 = plugin;
                            inputBox.TextChanged += delegate
                                                        {
                                                            property.SetValue(settings, box.Text, null);
                                                            plugin1.Plugin.Initialize();
                                                        };
                        }
                    }
                }
            }
        }

        private void UpdateOutputBox(TextBox box, PropertyInfo property, object theObject)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                                             {
                                                                 box.Text = (string) property.GetValue(theObject, null);
                                                             }, null);
        }

        void rb_Checked(object sender, RoutedEventArgs e)
        {
            ResetSelectionDependencies();
            RadioButton b = (RadioButton)sender;
            b.Background = Brushes.LightSeaGreen;
            Border c = (Border)b.Content;
            c.BorderThickness = new Thickness(1, 1, 0, 1);
            c.Background = selectionBrush;
            XElement ele = (XElement)b.Tag;
            selectedCategories.Remove(GetElementID(ele));
            selectedCategories.Add(GetElementID(ele), true);
            XElement desc = FindElementsInElement(ele, "description").First();
            if (desc != null)
            {
                SetTextFromXElement(desc, el =>
                                              {
                                                  CategoryDescription.Inlines.Clear();
                                                  var inline = TrimInline(XMLHelper.ConvertFormattedXElement(el));
                                                  if (inline != null)
                                                  {
                                                      CategoryDescription.Inlines.Add(inline);
                                                  }
                                              });
            }
            nextButton.IsEnabled = true;
        }

        /// <summary>
        /// Gets the elements text with respect to possible condition statements and calls the setter action delegate.
        /// </summary>
        private void SetTextFromXElement(XElement element, Action<XElement> textSetter)
        {
            if (element.Element("condition") != null)
            {
                conditionalTextSetters.Add(new ConditionalTextSetter() {TextSetter = textSetter, XElement = element});
                foreach (var condition in element.Elements("condition"))
                {
                    var key = GetElementPluginPropertyKey(condition);
                    if (propertyValueDict.ContainsKey(key))
                    {
                        foreach (var pair in propertyValueDict[key])
                        {
                            if (IsSamePath(pair.Path, element))
                            {
                                if (condition.Attribute("value").Value == pair.Value.ToString())
                                {
                                    textSetter(condition);
                                    return;
                                }
                            }
                        }
                    }
                }
                //if no condition holds, just set the content of the first one:
                textSetter(element.Element("condition"));
            }
            else
            {
                textSetter(element);
            }
        }

        private void ResetSelectionDependencies()
        {
            for (int i = 0; i < radioButtonStackPanel.Children.Count; i++)
            {
                RadioButton b = (RadioButton)radioButtonStackPanel.Children[i];
                XElement ele = (XElement)b.Tag;
                selectedCategories.Remove(GetElementID(ele));
                selectedCategories.Add(GetElementID(ele), false);
                b.Background = Brushes.Transparent;
                Border c = (Border)b.Content;
                c.BorderThickness = new Thickness(0);
                c.Background = Brushes.Transparent;
            }
        }

        //finds elements according to the current language
        private IEnumerable<XElement> FindElementsInElement(XElement element, string xname)
        {
            CultureInfo currentLang = CultureInfo.CurrentUICulture;

            IEnumerable<XElement> allElements = element.Elements(xname);
            IEnumerable<XElement> foundElements = null;

            if (allElements.Any())
            {
                foundElements = from descln in allElements where descln.Attribute("lang").Value == currentLang.TextInfo.CultureName select descln;
                if (!foundElements.Any())
                {
                    foundElements = from descln in allElements where descln.Attribute("lang").Value == currentLang.TwoLetterISOLanguageName select descln;
                    if (!foundElements.Any())
                        foundElements = from descln in allElements where descln.Attribute("lang").Value == defaultLang select descln;
                }
            }

            if (foundElements == null || !foundElements.Any() || !allElements.Any())
            {
                if (!allElements.Any())
                {
                    List<XElement> fe = new List<XElement>();
                    fe.Add(new XElement("dummy"));
                    return fe;
                }
                else
                    return allElements;
            }

            return foundElements;
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (CrypTool.PluginBase.Properties.Settings.Default.Wizard_ShowAnimations)
            {
                Storyboard mainGridStoryboardLeft = (Storyboard) FindResource("MainGridStoryboardNext1");
                mainGridStoryboardLeft.Begin();
            }
            else
            {
                SetNextContent(sender, e);
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            canStopOrExecute = false;
            if (CrypTool.PluginBase.Properties.Settings.Default.Wizard_ShowAnimations)
            {
                Storyboard mainGridStoryboardLeft = (Storyboard) FindResource("MainGridStoryboardBack1");
                mainGridStoryboardLeft.Begin();
            }
            else
            {
                SetLastContent(sender, e);
            }
        }

        private void abortButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchButtonWhenNecessary();
            canStopOrExecute = false;

            foreach (RadioButton rb in radioButtonStackPanel.Children)
            {
                if (rb.IsChecked != null && (bool)rb.IsChecked)
                    rb.IsChecked = false;
            }

            history.Content = null;
            currentHistory.Clear();
            AddToHistory(wizardConfigXML);
            propertyValueDict.Clear();
            ResetSelectionDependencies();
            radioButtonStackPanel.Children.Clear();
            selectedCategories.Clear();
            CategoryDescription.Text = "";
            SetupPage(wizardConfigXML);
        }

        internal void StopCurrentWorkspaceManager()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
            {
                if (currentManager != null && currentManager.CanStop)
                    currentManager.Stop();
            }, null);
        }

        internal void ExecuteCurrentWorkspaceManager()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (currentManager != null && currentManager.CanExecute)
                    currentManager.Execute();
            }, null);
        }

        internal bool WizardCanStop()
        {
            if (!canStopOrExecute || currentManager == null)
                return false;
            else
                return currentManager.CanStop;
        }

        internal bool WizardCanExecute()
        {
            if (!canStopOrExecute || currentManager == null)
                return false;
            else
                return currentManager.CanExecute;
        }

        private void SwitchButtonWhenNecessary()
        {
            if (inputPanel.Visibility == Visibility.Visible)
            {
                if (inputPanel.Tag != null && ((XElement)inputPanel.Tag).Name == "loadSample")
                    SwitchNextButtonContent();
            }
        }

        private void SetNextContent(object sender, EventArgs e)
        {
            if (categoryGrid.Visibility == Visibility.Visible)
            {
                for (int i = 0; i < radioButtonStackPanel.Children.Count; i++)
                {
                    RadioButton b = (RadioButton) radioButtonStackPanel.Children[i];
                    if (b.IsChecked != null && (bool) b.IsChecked)
                    {
                        var ele = (XElement) b.Tag;
                        AddToHistory(ele);
                        SetupPage(ele);
                        break;
                    }
                }
            }
            else if (inputPanel.Visibility == Visibility.Visible)
            {
                var nextElement = (XElement) inputPanel.Tag;
                AddToHistory(nextElement);
                SetupPage(nextElement);
            }

            if (CrypTool.PluginBase.Properties.Settings.Default.Wizard_ShowAnimations)
            {
                Storyboard mainGridStoryboardLeft = (Storyboard) FindResource("MainGridStoryboardNext2");
                mainGridStoryboardLeft.Begin();
            }
        }

        private void SaveContent()
        {
            if (inputPanel.Visibility == Visibility.Visible)
            {
                foreach (var child in inputStack.Children)
                {
                    SaveControlContent(child);
                }
            }
        }

        private void AddToHistory(XElement ele)
        {
            try
            {
                var page = new PageInfo() { tag = ele };
                SetTextFromXElement(FindElementsInElement(ele, "name").First(), delegate(XElement el) { page.name = el.Value.Trim(); });
                SetTextFromXElement(FindElementsInElement(ele, "description").First(), delegate(XElement el) { page.description = el.Value.Trim(); });
                SetTextFromXElement(FindElementsInElement(ele, "headline").First(), delegate(XElement el) { page.headline = el.Value.Trim(); });

                if (ele.Attribute("image") != null)
                {
                    page.image = ele.Attribute("image").Value;
                }

                currentHistory.Add(page);
            }
            catch (Exception)
            {
                GuiLogMessage("Error adding page to history", NotificationLevel.Error);
            }
        }

        private void SaveControlContent(object o)
        {
            var sp = (StackPanel)o;

            foreach (var inp in sp.Children)
            {
                var input = inp;
                if (input is StorageContainer)
                {
                    input = ((StorageContainer)input).GetContent();
                }

                if (input is TextBox || input is ComboBox || input is CheckBox || input is KeyTextBox.KeyTextBox)
                {
                    Control c = (Control)input;
                    XElement ele = (XElement)c.Tag;
                    if (ele.Name == "outputBox")
                        continue;

                    var key = GetElementPluginPropertyKey(ele);
                    PluginPropertyValue newEntry = null;
                    
                    if (input is TextBox)
                    {
                        if (ele.Attribute("plugin") != null && ele.Attribute("property") != null)
                        {
                            TextBox textBox = (TextBox) input;
                            newEntry = new PluginPropertyValue()
                                           {
                                               PluginName = ele.Attribute("plugin").Value,
                                               PropertyName = ele.Attribute("property").Value,
                                               Value = textBox.Text,
                                               Path = ele.Parent
                                           };
                        }
                    }
                    else if (input is KeyTextBox.KeyTextBox)
                    {
                        if (ele.Attribute("plugin") != null && ele.Attribute("property") != null)
                        {
                            var keyTextBox = (KeyTextBox.KeyTextBox) input;
                            newEntry = new PluginPropertyValue()
                                           {
                                               PluginName = ele.Attribute("plugin").Value,
                                               PropertyName = ele.Attribute("property").Value,
                                               Value = keyTextBox.CurrentKey,
                                               Path = ele.Parent
                                           };
                        }
                    }
                    else if (input is ComboBox)
                    {
                        var comboBox = (ComboBox)input;
                        var pluginSetters = (from i in ele.Elements("item")
                                             group i by i.Attribute("lang").Value into ig
                                             select ig.ElementAt(comboBox.SelectedIndex)).SelectMany(x => x.Elements("pluginSetter"));

                        foreach (var pluginSetter in pluginSetters)
                        {
                            int setterValue;
                            if (int.TryParse(pluginSetter.Value, out setterValue))
                            {
                                var pluginSetterKey = GetElementPluginPropertyKey(pluginSetter);
                                AddPluginPropertyEntry(new PluginPropertyValue()
                                                            {
                                                                PluginName = pluginSetter.Attribute("plugin").Value,
                                                                PropertyName = pluginSetter.Attribute("property").Value,
                                                                Value = setterValue,
                                                                Path = ele.Parent
                                                            }, pluginSetterKey, ele);
                            }
                        }

                        if (key != null)
                        {
                            newEntry = new PluginPropertyValue()
                                            {
                                                PluginName = ele.Attribute("plugin").Value,
                                                PropertyName = ele.Attribute("property").Value,
                                                Value = comboBox.SelectedIndex,
                                                Path = ele.Parent
                                            };
                        }
                    }
                    else if (input is CheckBox)
                    {
                        if (ele.Attribute("plugin") != null && ele.Attribute("property") != null)
                        {
                            CheckBox checkBox = (CheckBox) input;
                            if (checkBox.IsChecked != null)
                            {
                                newEntry = new PluginPropertyValue()
                                               {
                                                   PluginName = ele.Attribute("plugin").Value,
                                                   PropertyName = ele.Attribute("property").Value,
                                                   Value = (bool) checkBox.IsChecked,
                                                   Path = ele.Parent
                                               };
                            }
                        }
                    }

                    if (newEntry != null)
                    {
                        AddPluginPropertyEntry(newEntry, key, ele);
                    }
                }
                else if (input is StackPanel)
                {
                    SaveControlContent(input);
                }
            }
        }

        private void AddPluginPropertyEntry(PluginPropertyValue newEntry, string key, XElement ele)
        {
            if (newEntry != null && key != null)
            {
                var pluginPropertyValue = GetPropertyValue(key, ele.Parent);
                if (pluginPropertyValue != null)
                {
                    pluginPropertyValue.Value = newEntry.Value;
                }
                else
                {
                    if (propertyValueDict.ContainsKey(key))
                    {
                        propertyValueDict[key].Add(newEntry);
                    }
                    else
                    {
                        propertyValueDict.Add(key, new List<PluginPropertyValue>() {newEntry});
                    }
                }
            }
        }

        private void SetLastContent(object sender, EventArgs e)
        {
            canStopOrExecute = false;

            XElement ele = null;
            if (categoryGrid.Visibility == Visibility.Visible && radioButtonStackPanel.Children.Count > 0)
            {
                RadioButton b = (RadioButton) radioButtonStackPanel.Children[0];
                ele = (XElement) b.Tag;

                foreach (RadioButton rb in radioButtonStackPanel.Children)
                {
                    if (rb.IsChecked != null && (bool)rb.IsChecked)
                        rb.IsChecked = false;
                }

            }
            else if (inputPanel.Visibility == Visibility.Visible)
            {
                ele = (XElement) inputPanel.Tag;
                if (ele != null && ((XElement)inputPanel.Tag).Name == "loadSample")
                    SwitchNextButtonContent();
            }

            if (ele != null)
            {
                XElement grandParent = ele.Parent.Parent;
                if (grandParent == null)
                    grandParent = wizardConfigXML;

                if (currentHistory.Count > 0)
                    currentHistory.RemoveAt(currentHistory.Count - 1);

                SetupPage(grandParent);
            }

            if (CrypTool.PluginBase.Properties.Settings.Default.Wizard_ShowAnimations)
            {
                Storyboard mainGridStoryboardLeft = (Storyboard) FindResource("MainGridStoryboardBack2");
                mainGridStoryboardLeft.Begin();
            }
        }

        private void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(null, new GuiLogEventArgs(message, null, loglevel));
        }

        private void SwitchNextButtonContent()
        {
            var tmp = nextButton.Content;
            nextButton.Content = nextButton.Tag;
            nextButton.Tag = tmp;
        }

        private void history_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point dir = e.GetPosition(history);
                if (dir.X < history.ActualWidth / 2)
                    history.LineRight();
                else if (dir.X > history.ActualWidth / 2)
                    history.LineLeft();
            }
        }

        private void KeyPressedDown(Key key)
        {
            switch (key)
            {
                case Key.Up:
                case Key.Down:
                    if (categoryGrid.Visibility == Visibility.Visible)
                    {
                        if (radioButtonStackPanel.Children.Count != 0)
                        {
                            int i = 0;
                            while (((RadioButton)radioButtonStackPanel.Children[i]).IsChecked == false)
                                i++;
                            ((RadioButton)radioButtonStackPanel.Children[i]).IsChecked = false;

                            if (key == Key.Down)
                            {
                                if (radioButtonStackPanel.Children.Count > i + 1)
                                    ((RadioButton)radioButtonStackPanel.Children[i + 1]).IsChecked = true;
                                else
                                    ((RadioButton)radioButtonStackPanel.Children[0]).IsChecked = true;
                            }
                            else   //Up
                            {
                                if (i - 1 >= 0)
                                    ((RadioButton)radioButtonStackPanel.Children[i - 1]).IsChecked = true;
                                else
                                    ((RadioButton)radioButtonStackPanel.Children[radioButtonStackPanel.Children.Count - 1]).IsChecked = true;
                            }
                        }
                    }
                    break;

                case Key.Left:
                    if (backButton.IsEnabled)
                        backButton_Click(null, null);
                    break;

                case Key.Right:
                    if (nextButton.IsEnabled)
                        nextButton_Click(null, null);
                    break;
            }
        }

        private void ScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (OuterScrollViewer.IsKeyboardFocused || descScroll.IsKeyboardFocused || inputPanel.IsKeyboardFocused || history.IsKeyboardFocused)
            {
                KeyPressedDown(e.Key);
                e.Handled = true;
            }
        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            SaveControlContent(inputStack);
            var element = (XElement)CreateProjectButton.Tag;
            if ((element == null) || !LoadSample(element.Attribute("file").Value, Properties.Resources.LoadedSampleTitle, true, element))
            {
                ErrorLabel.Visibility = Visibility.Visible;
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CategoryDescription.Text != null)
                Clipboard.SetText(CategoryDescription.Text);
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            if (e.Source is RadioButton)
            {
                RadioButton rb = (RadioButton) e.Source;
                if (rb.IsChecked.HasValue && rb.IsChecked.Value)
                {
                    Storyboard sb = (Storyboard) Resources["NextButtonAttentionAnimation"];
                    sb.Begin();
                }
            }
        }

        private void nextButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                SwitchNextButtonContent();
            }
        }
    }

    internal class PluginPropertyValue
    {
        public string PluginName;
        public string PropertyName;
        public object Value;
        public XElement Path;
    }
}
