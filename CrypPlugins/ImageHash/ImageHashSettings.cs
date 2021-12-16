/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using ImageHash.Properties;
using System.ComponentModel;

namespace CrypTool.Plugins.ImageHash
{
    public class ImageHashSettings : ISettings
    {
        #region Private Variables

        private int outputFileFormat = 0;
        private int size = 16;
        private int presentationStep = 5;
        private static string stepName = Resources.Step4Caption;
        private bool showEachStep = false;

        #endregion

        #region TaskPane Settings

        [TaskPane("SizeCaption", "SizeTooltip", null, 1, false, ControlType.TextBox, ValidationType.RangeInteger, 0, 5000)]
        public int Size
        {
            get => size;
            set
            {
                if (size != value)
                {
                    if (size > 128)
                    {
                        size = 128;
                    }
                    else
                    {
                        size = value;
                    }
                    OnPropertyChanged("size");
                }
            }
        }

        [TaskPane("OutputFileFormatCaption", "OutputFileFormatTooltip", null, 2, true, ControlType.ComboBox, new string[] {
            "Bmp",
            "Png",
            "Tiff" })]
        public int OutputFileFormat
        {
            get => outputFileFormat;
            set
            {
                if (value != outputFileFormat)
                {
                    outputFileFormat = value;
                    OnPropertyChanged("OutputFileFormat");
                }
            }
        }


        /// <summary>
        /// Boolean that sets if every step of the image processing is directly shown.
        /// </summary>
        [TaskPane("StepsCaption", "StepsTooltip", "SliderGroup", 3, false, ControlType.CheckBox)]
        public bool ShowEachStep
        {
            get => showEachStep;
            set => showEachStep = value;
        }


        [TaskPane("", "PresentationStepTooltip", "SliderGroup", 4, false, ControlType.TextBoxReadOnly)]
        public string StepName
        {
            get => stepName;
            set
            {
                if ((value) != stepName)
                {
                    stepName = value;
                    OnPropertyChanged("StepName");
                }
            }
        }

        [TaskPane("", "PresentationStepTooltip", "SliderGroup", 5, true, ControlType.Slider, 1, 5)]
        public int PresentationStep
        {
            get => presentationStep;
            set
            {
                if ((value) != presentationStep)
                {
                    presentationStep = value;
                    switch (presentationStep)
                    {
                        case 1:
                            StepName = Resources.Step0Caption;
                            break;
                        case 2:
                            StepName = Resources.Step1Caption;
                            break;
                        case 3:
                            StepName = Resources.Step2Caption;
                            break;
                        case 4:
                            StepName = Resources.Step3Caption;
                            break;
                        case 5:
                            StepName = Resources.Step4Caption;
                            break;
                    }
                    OnPropertyChanged("StepName");
                    OnPropertyChanged("PresentationStep");
                }
            }
        }

        #endregion

        #region Events

        public void Initialize()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
