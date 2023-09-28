using CrypTool.PluginBase;
using System.ComponentModel;

namespace WebService
{
    public class WebServiceSettings : ISettings
    {
        //   public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private string methodName;
        [TaskPane("MethodNameCaption", "MethodNameTooltip", "Konfigurieren", 1, true, ControlType.TextBox, "")]
        public string MethodName
        {
            get => methodName;
            set
            {
                methodName = value;
                OnPropertyChanged("MethodName");
            }
        }

        private int paramString;
        [TaskPane("StringCaption", "StringTooltip", "Konfigurieren", 3, true, ControlType.ComboBox, new string[] { "0", "1", "2" })]
        public int String
        {
            get => paramString;
            set
            {
                paramString = value;
                OnPropertyChanged("String");
            }
        }
        private int paramDouble;
        [TaskPane("DoubleCaption", "DoubleTooltip", "Konfigurieren", 4, true, ControlType.ComboBox, new string[] { "0", "1", "2" })]
        public int Double
        {
            get => paramDouble;
            set
            {
                paramDouble = value;
                OnPropertyChanged("Double");
            }
        }
        private int integer;
        //  [ContextMenu( "IntegerCaption", "IntegerTooltip", 0, ContextMenuControlType.ComboBox, null, "True", "False")]
        [TaskPane("IntegerTPCaption", "IntegerTPTooltip", "Konfigurieren", 2, true, ControlType.ComboBox, new string[] { "IntegerList1", "IntegerList2", "IntegerList3" })]
        public int Integer
        {
            get => integer;
            set
            {
                integer = value;
                OnPropertyChanged("Integer");
            }
        }

        private int test;
        [TaskPane("TestCaption", "TestTooltip", "Konfigurieren", 5, true, ControlType.ComboBox, new string[] { "void", "int", "string", "float", "double" })]
        public int Test
        {
            get => test;
            set
            {
                test = value;
                OnPropertyChanged("Test");
            }
        }
        [TaskPane("exportWSDLCaption", "exportWSDLTooltip", "", 0, true, ControlType.Button)]
        public void exportWSDL()
        {
            OnPropertyChanged("exportWSDL");
        }

        //[TaskPane( "createKeyCaption", "createKeyTooltip", "Key Management", 0, false, ControlType.Button)]
        //public void createKey()
        //{
        //    OnPropertyChanged("createKey");
        //    {

        //    }
        //}

        [TaskPane("publishKeyCaption", "publishKeyTooltip", "Key Management", 1, true, ControlType.Button)]
        public void publishKey()
        {
            OnPropertyChanged("publishKey");
        }

        [TaskPane("MethodenStubCaption", "MethodenStubTooltip", "Konfigurieren", 0, true, ControlType.Button)]
        public void MethodenStub()
        {
            OnPropertyChanged("MethodenStub");

        }

        private string targetFileName;
        [TaskPane("TargetFilenameCaption", "TargetFilenameTooltip", null, 0, true, ControlType.SaveFileDialog, FileExtension = "CrypTool Alphabet (*.cta)|*.cta")]
        public string TargetFilename
        {
            get => targetFileName;
            set
            {
                targetFileName = value;
                OnPropertyChanged("TargetFilename");
            }
        }
        private string userCode;

        public string UserCode
        {
            get => userCode;
            set
            {
                userCode = value;
                OnPropertyChanged("UserCode");
            }
        }
        //private int animationSpeed = 3;
        //[TaskPane( "AnimationSpeedCaption", "AnimationSpeedTooltip", "Animation", 9, false, ControlType.NumericUpDown, CrypTool.PluginBase.ValidationType.RangeInteger, 1, 5)]
        //public int AnimationSpeed
        //{
        //    get
        //    {
        //        return animationSpeed;
        //    }
        //    set
        //    {
        //        animationSpeed = value;
        //        OnPropertyChanged("AnimationSpeed");
        //    }
        //}
        private bool compiled;

        public bool Compiled
        {
            get => compiled;
            set
            {
                compiled = value;
                OnPropertyChanged("Compiled");

            }
        }
        #region INotifyPropertyChanged Member


        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
