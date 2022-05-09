using CrypTool.PluginBase;
using System.ComponentModel;

namespace Transposition
{
    internal class TranspositionSettings : ISettings
    {
        # region private variables

        private int selectedAction = 0;

        private ReadInMode selectedReadIn = ReadInMode.byRow;
        private PermutationMode selectedPermutation = PermutationMode.byColumn;
        private ReadOutMode selectedReadOut = ReadOutMode.byColumn;
        private int Presentation_Speed = 100;
        private NumberMode selectedNumberMode = NumberMode.asChar;
        private InternalNumberMode selectedInternalNumberMode = InternalNumberMode.asChar;

        #endregion

        #region public enums

        public enum ReadInMode { byRow = 0, byColumn = 1 };
        public enum PermutationMode { byRow = 0, byColumn = 1 };
        public enum ReadOutMode { byRow = 0, byColumn = 1 };
        public enum NumberMode { asChar = 0, asHex = 1 };
        public enum InternalNumberMode { asChar = 0, asHex = 1 };

        # endregion

        # region Settings

        [PropertySaveOrder(1)]
        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get => selectedAction;
            set
            {
                if (value != selectedAction)
                {
                    selectedAction = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [PropertySaveOrder(2)]
        [ContextMenu("ReadInCaption", "ReadInTooltip", 2, ContextMenuControlType.ComboBox, null, new string[] { "ReadInList1", "ReadInList2" })]
        [TaskPane("ReadInCaption", "ReadInTooltip", null, 2, false, ControlType.ComboBox, new string[] { "ReadInList1", "ReadInList2" })]
        public int ReadIn
        {
            get => (int)selectedReadIn;
            set
            {
                if ((ReadInMode)value != selectedReadIn)
                {
                    selectedReadIn = (ReadInMode)value;
                    OnPropertyChanged("ReadIn");
                }
            }
        }

        /// <summary>
        /// Method to change ReadIn without firing the Property Changed event
        /// Thus, Transposition Analyzer may change settings without making workspace "dirty"
        /// </summary>
        /// <param name="value"></param>
        public void SetReadIn(int value)
        {
            selectedReadIn = (ReadInMode)value;
        }

        [PropertySaveOrder(3)]
        [ContextMenu("PermutationCaption", "PermutationTooltip", 3, ContextMenuControlType.ComboBox, null, new string[] { "PermutationList1", "PermutationList2" })]
        [TaskPane("PermutationCaption", "PermutationTooltip", null, 3, false, ControlType.ComboBox, new string[] { "PermutationList1", "PermutationList2" })]
        public int Permutation
        {
            get => (int)selectedPermutation;
            set
            {
                if ((PermutationMode)value != selectedPermutation)
                {
                    selectedPermutation = (PermutationMode)value;
                    OnPropertyChanged("Permutation");
                }
            }
        }

        /// <summary>
        /// Method to change Permutation without firing the Property Changed event
        /// Thus, Transposition Analyzer may change settings without making workspace "dirty"
        /// </summary>
        /// <param name="value"></param>
        public void SetPermutation(int value)
        {
            selectedPermutation = (PermutationMode)value;
        }

        [PropertySaveOrder(4)]
        [ContextMenu("ReadOutCaption", "ReadOutTooltip", 4, ContextMenuControlType.ComboBox, null, new string[] { "ReadOutList1", "ReadOutList2" })]
        [TaskPane("ReadOutCaption", "ReadOutTooltip", null, 4, false, ControlType.ComboBox, new string[] { "ReadOutList1", "ReadOutList2" })]
        public int ReadOut
        {
            get => (int)selectedReadOut;
            set
            {
                if ((ReadOutMode)value != selectedReadOut)
                {
                    selectedReadOut = (ReadOutMode)value;
                    OnPropertyChanged("ReadOut");
                }
            }
        }

        /// <summary>
        /// Method to change ReadOut without firing the Property Changed event
        /// Thus, Transposition Analyzer may change settings without making workspace "dirty"
        /// </summary>
        /// <param name="value"></param>
        public void SetReadOut(int value)
        {
            selectedReadOut = (ReadOutMode)value;
        }

        [PropertySaveOrder(5)]
        [TaskPane("PresentationSpeedCaption", "PresentationSpeedTooltip", "PresentationGroup", 6, true, ControlType.Slider, 1, 1000)]
        public int PresentationSpeed
        {
            get => Presentation_Speed;
            set
            {
                if ((value) != Presentation_Speed)
                {
                    Presentation_Speed = value;
                    OnPropertyChanged("PresentationSpeed");
                }
            }
        }

        [PropertySaveOrder(6)]
        [ContextMenu("NumberCaption", "NumberTooltip", 7, ContextMenuControlType.ComboBox, null, new string[] { "NumberList1", "NumberList2" })]
        [TaskPane("NumberCaption", "NumberTooltip", "PresentationGroup", 7, false, ControlType.ComboBox, new string[] { "NumberList1", "NumberList2" })]
        public int Number
        {
            get => (int)selectedNumberMode;
            set
            {
                if ((NumberMode)value != selectedNumberMode)
                {
                    selectedNumberMode = (NumberMode)value;
                    OnPropertyChanged("NumberMode");
                }
            }
        }

        [PropertySaveOrder(7)]
        [ContextMenu("NumberModeCaption", "NumberTooltip", 8, ContextMenuControlType.ComboBox, null, new string[] { "ModeList2", "ModeList1" })]
        [TaskPane("NumberModeCaption", "NumberTooltip", null, 8, false, ControlType.ComboBox, new string[] { "ModeList2", "ModeList1" })]
        public int InternalNumber
        {
            get => (int)selectedInternalNumberMode;
            set
            {
                if ((InternalNumberMode)value != selectedInternalNumberMode)
                {
                    selectedInternalNumberMode = (InternalNumberMode)value;
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
