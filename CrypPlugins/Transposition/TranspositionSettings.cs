using CrypTool.PluginBase;
using System.ComponentModel;

namespace Transposition
{
    class TranspositionSettings : ISettings
    {
        # region private variables

        private int selectedAction = 0;

        private ReadInMode selectedReadIn = ReadInMode.byRow;
        private PermutationMode selectedPermutation = PermutationMode.byColumn;
        private ReadOutMode selectedReadOut = ReadOutMode.byColumn;
        private int Presentation_Speed = 100;
        private NumberMode selectedNumberMode = NumberMode.asChar;
        private InternalNumberMode selectedInternalNumberMode = InternalNumberMode.asChar;
        
        # endregion

        #region public enums

        public enum ReadInMode { byRow = 0, byColumn = 1};
        public enum PermutationMode { byRow = 0, byColumn = 1 };
        public enum ReadOutMode { byRow = 0, byColumn = 1 };
        public enum NumberMode {asChar = 0, asHex = 1};
        public enum InternalNumberMode { asChar = 0, asHex = 1 };

        # endregion

        # region Settings

        [PropertySaveOrder(1)]
        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get { return this.selectedAction; }
            set
            {
                if (value != selectedAction)
                {
                    this.selectedAction = value;
                    OnPropertyChanged("Action");   
                }
            }
        }

        [PropertySaveOrder(2)]
        [ContextMenu("ReadInCaption", "ReadInTooltip", 2, ContextMenuControlType.ComboBox, null, new string[] { "ReadInList1", "ReadInList2" })]
        [TaskPane("ReadInCaption", "ReadInTooltip", null, 2, false, ControlType.ComboBox, new string[] { "ReadInList1", "ReadInList2" })]
        public int ReadIn
        {
            get { return (int) this.selectedReadIn; }
            set
            {
                if ((ReadInMode)value != selectedReadIn)
                {
                    this.selectedReadIn = (ReadInMode)value;
                    OnPropertyChanged("ReadIn");   
                }
            }
        }

        [PropertySaveOrder(3)]
        [ContextMenu("PermutationCaption", "PermutationTooltip", 3, ContextMenuControlType.ComboBox, null, new string[] { "PermutationList1", "PermutationList2" })]
        [TaskPane("PermutationCaption", "PermutationTooltip", null, 3, false, ControlType.ComboBox, new string[] { "PermutationList1", "PermutationList2" })]
        public int Permutation
        {
            get { return (int)this.selectedPermutation; }
            set
            {
                if ((PermutationMode)value != selectedPermutation)
                {
                    this.selectedPermutation = (PermutationMode)value;
                    OnPropertyChanged("Permutation");   
                }
            }
        }

        [PropertySaveOrder(4)]
        [ContextMenu("ReadOutCaption", "ReadOutTooltip", 4, ContextMenuControlType.ComboBox, null, new string[] { "ReadOutList1", "ReadOutList2" })]
        [TaskPane("ReadOutCaption", "ReadOutTooltip", null, 4, false, ControlType.ComboBox, new string[] { "ReadOutList1", "ReadOutList2" })]
        public int ReadOut
        {
            get { return (int)this.selectedReadOut; }
            set
            {
                if ((ReadOutMode)value != selectedReadOut)
                {
                    this.selectedReadOut = (ReadOutMode)value;
                    OnPropertyChanged("ReadOut");   
                }
            }
        }

        

        [PropertySaveOrder(5)]
        [TaskPane( "PresentationSpeedCaption", "PresentationSpeedTooltip", "PresentationGroup", 6, true, ControlType.Slider, 1, 1000)]
        public int PresentationSpeed
        {
            get { return (int)Presentation_Speed; }
            set
            {
                if ((value) != Presentation_Speed)
                {
                    this.Presentation_Speed = value;
                    OnPropertyChanged("PresentationSpeed");   
                }
            }
        }

        [PropertySaveOrder(6)]
        [ContextMenu("NumberCaption", "NumberTooltip", 7, ContextMenuControlType.ComboBox, null, new string[] { "NumberList1", "NumberList2" })]
        [TaskPane("NumberCaption", "NumberTooltip", "PresentationGroup", 7, false, ControlType.ComboBox, new string[] { "NumberList1", "NumberList2" })]
        public int Number
        {
            get { return (int)this.selectedNumberMode;}
            set
            {
                if ((NumberMode)value != selectedNumberMode)
                {
                    this.selectedNumberMode = (NumberMode)value;
                    OnPropertyChanged("NumberMode");   
                }
            }
        }

        [PropertySaveOrder(7)]
        [ContextMenu("NumberModeCaption", "NumberTooltip", 8, ContextMenuControlType.ComboBox, null, new string[] { "ModeList2", "ModeList1" })]
        [TaskPane("NumberModeCaption", "NumberTooltip", null, 8, false, ControlType.ComboBox, new string[] { "ModeList2", "ModeList1" })]
        public int InternalNumber
        {
            get { return (int)this.selectedInternalNumberMode; }
            set
            {
                if ((InternalNumberMode)value != selectedInternalNumberMode)
                {
                    this.selectedInternalNumberMode = (InternalNumberMode)value;
                    OnPropertyChanged("InternalNumberMode");
                }
            }
        }


        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
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
