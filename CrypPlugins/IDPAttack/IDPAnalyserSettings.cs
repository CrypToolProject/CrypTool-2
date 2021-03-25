using CrypTool.PluginBase;
using System.ComponentModel;
using System.Windows;

namespace IDPAnalyser
{
    class IDPAnalyserSettings : ISettings
    {
        #region settings
        private int selected_method = 0;

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            switch (selected_method)
            {
                case 0:
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Key1Min", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Key1Max", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Key2Min", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Key2Max", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Key1Size", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Key2Size", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Repeatings", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Iterations", Visibility.Collapsed)));
                    break;
                case 1:
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Key1Min", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Key1Max", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Key2Min", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Key2Max", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Key1Size", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Key2Size", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Repeatings", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Iterations", Visibility.Visible)));
                    break;
            }
        }

        //[PropertySaveOrder(1)]
        [TaskPane("Analysis_methodCaption", "Analysis_methodTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Analysis_methodList1", "Analysis_methodList2" })]
        public int Analysis_method
        {
            get
            {
                return this.selected_method;
            }

            set
            {
                if (value != selected_method)
                {
                    this.selected_method = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Analysis_method");
                }
            }
        }

        private int language = 0;
        [TaskPane("LanguageCaption", "LanguageTooltip", null, 2, false, ControlType.ComboBox, new string[] { "LanguageList1", "LanguageList2", "LanguageList3", "LanguageList4" })]
        public int Language
        {
            get
            {
                return this.language;
            }

            set
            {
                if (value != language)
                {
                    this.language = value;
                    OnPropertyChanged("Language");
                }
            }
        }

        private int key1Size = 8;
        [PropertySaveOrder(3)]
        [TaskPaneAttribute("Key1SizeCaption", "Key1SizeTooltip", "KeyGroup", 3, false, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Key1Size
        {
            get { return key1Size; }
            set
            {
                if (value != this.key1Size)
                {
                    key1Size = value;
                    OnPropertyChanged("Key1Size");
                }
            }
        }

        private int key2Size = 8;
        [PropertySaveOrder(3)]
        [TaskPaneAttribute("Key2SizeCaption", "Key2SizeTooltip", "KeyGroup", 4, false, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Key2Size
        {
            get { return key2Size; }
            set
            {
                if (value != this.key2Size)
                {
                    key2Size = value;
                    OnPropertyChanged("Key2Size");
                }
            }
        }

        private int key1Min = 8;
        [PropertySaveOrder(3)]
        [TaskPaneAttribute("Key1MinCaption", "Key1MinTooltip", "KeyGroup", 5, false, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Key1Min
        {
            get { return key1Min; }
            set
            {
                if (value != this.key1Min)
                {
                    key1Min = value;
                    OnPropertyChanged("Key1Min");
                }
            }
        }

        private int key1Max = 10;
        [PropertySaveOrder(3)]
        [TaskPaneAttribute("Key1MaxCaption", "Key1MaxTooltip", "KeyGroup", 6, false, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Key1Max
        {
            get { return key1Max; }
            set
            {
                if (value != this.key1Max)
                {
                    key1Max = value;
                    OnPropertyChanged("Key1Max");
                }
            }
        }

        private int key2Min = 8;
        [PropertySaveOrder(3)]
        [TaskPaneAttribute("Key2MinCaption", "Key2MinTooltip", "KeyGroup", 7, false, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Key2Min
        {
            get { return key2Min; }
            set
            {
                if (value != this.key2Min)
                {
                    key2Min = value;
                    OnPropertyChanged("Key2Min");
                }
            }
        }

        private int key2Max = 10;
        [PropertySaveOrder(3)]
        [TaskPaneAttribute("Key2MaxCaption", "Key2MaxTooltip", "KeyGroup", 8, false, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Key2Max
        {
            get { return key2Max; }
            set
            {
                if (value != this.key2Max)
                {
                    key2Max = value;
                    OnPropertyChanged("Key2Max");
                }
            }
        }

        private int repeatings = 10;
        [PropertySaveOrder(8)]
        [TaskPaneAttribute("RepeatingsCaption", "RepeatingsTooltip", null, 9, true, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Repeatings
        {
            get { return repeatings; }
            set
            {
                if (value != this.repeatings)
                {
                    repeatings = value;
                    OnPropertyChanged("Repeatings");
                }
            }
        }

        private int iterations = 5000;
        [PropertySaveOrder(9)]
        [TaskPaneAttribute("IterationsCaption", "IterationsTooltip", null, 10, true, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Iterations
        {
            get { return iterations; }
            set
            {
                if (value != this.iterations)
                {
                    iterations = value;
                    OnPropertyChanged("Iterations");
                }
            }
        }

        #endregion

        #region Events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            UpdateTaskPaneVisibility();
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}