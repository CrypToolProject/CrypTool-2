/*
   Copyright 2008 - 2022 CrypTool Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;

namespace CrypTool.PluginBase
{

    public enum Direction
    {
        InputData,
        OutputData,
        ControlSlave,
        ControlMaster
    }

    [Obsolete("DisplayLevel is no longer used, see #122")]
    public enum DisplayLevel
    {
        Beginner,
        Experienced,
        Professional,
        Expert
    }

    public enum QuickWatchFormat
    {
        None,
        Text,
        Hex,
        Base64
    }

    public enum ControlType
    {
        TextBox,
        ComboBox,
        RadioButton,
        CheckBox,
        OpenFileDialog,
        SaveFileDialog,
        NumericUpDown,
        Button,
        Slider,
        TextBoxReadOnly,
        DynamicComboBox,
        TextBoxHidden,
        KeyTextBox,
        LanguageSelector
    }

    public enum ContextMenuControlType
    {
        ComboBox,
        CheckBox
    }

    public enum NotificationLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Balloon
    }

    public enum ValidationType
    {
        RegEx,
        RangeInteger,
        RangeDouble
    }

    public enum EncodingTypes
    {
        Default = 0,
        Binary = 1,
        Unicode = 2,
        UTF7 = 3,
        UTF8 = 4,
        UTF32 = 5,
        ASCII = 6,
        BigEndianUnicode = 7
    }

    public enum DisplayPluginMode
    {
        Normal, Disabled
    }

    public enum StatusChangedMode
    {
        ImageUpdate, Presentation, QuickWatchPresentation
    }
}
