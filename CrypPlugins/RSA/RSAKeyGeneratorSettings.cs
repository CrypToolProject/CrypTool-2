/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Plugins.RSA
{
    /// <summary>
    /// Settings class for the RSAKeyGenerator plugin
    /// </summary>
    internal class RSAKeyGeneratorSettings : ISettings
    {
        #region private members
        private int source;
        private int e_or_d;

        private string p = "23";
        private string q = "13";
        private string n = "299";
        private string e = "23";
        private string d = "23";
        private string certificateFile;
        private string password = "";
        private string range = "100";
        private int rangeType = 0;  // interpret range as: 0=number of bits, 1=upper limit
        private bool generateSafePrimes = false;
        #endregion

        #region events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            UpdateTaskPaneVisibility();
        }

        #endregion

        #region public

        /// <summary>
        /// Getter/Setter for the source of the Key Data
        /// </summary>
        [TaskPane("SourceCaption", "SourceTooltip", null, 1, false, ControlType.ComboBox, new string[] { "SourceList1", "SourceList2", "SourceList3", "SourceList4" })]
        public int Source
        {
            get => source;
            set
            {
                if (value != source)
                {
                    source = value;

                    UpdateTaskPaneVisibility();

                    OnPropertyChanged("Source");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the source of the Key Data
        /// </summary>
        [TaskPane("E_or_D", "E_or_DTooltip", null, 2, false, ControlType.ComboBox, new string[] { "Enter_e", "Enter_d", })]
        public int E_or_D
        {
            get => e_or_d;
            set
            {
                if (value != e_or_d)
                {
                    e_or_d = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("E_or_D");
                }
            }
        }


        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            switch (source)
            {
                case 0:
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CertificateFile", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CloseFile", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Password", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E_or_D", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E", e_or_d == 0 ? Visibility.Visible : Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("D", e_or_d == 0 ? Visibility.Collapsed : Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("N", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RangeType", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Range", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("GenerateSafePrimes", Visibility.Collapsed)));
                    break;
                case 1:
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CertificateFile", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CloseFile", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Password", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E_or_D", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("D", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("N", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RangeType", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Range", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("GenerateSafePrimes", Visibility.Collapsed)));
                    break;
                case 2:
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CertificateFile", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CloseFile", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Password", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E_or_D", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("D", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("N", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RangeType", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Range", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("GenerateSafePrimes", Visibility.Visible)));
                    break;
                case 3:
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CertificateFile", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CloseFile", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Password", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E_or_D", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("D", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("N", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RangeType", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Range", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("GenerateSafePrimes", Visibility.Collapsed)));
                    break;
            }
        }

        /// <summary>
        /// Getter/Setter for prime P
        /// </summary>
        [TaskPane("PCaption", "PTooltip", null, 3, false, ControlType.TextBox)]
        public string P
        {
            get => p;
            set
            {
                p = value;
                OnPropertyChanged("P");
            }
        }

        /// <summary>
        /// Getter/Setter for the prime Q
        /// </summary>
        [TaskPane("QCaption", "QTooltip", null, 4, false, ControlType.TextBox)]
        public string Q
        {
            get => q;
            set
            {
                q = value;
                OnPropertyChanged("Q");
            }
        }

        /// <summary>
        /// Getter/Setter for the N
        /// </summary>
        [TaskPane("NCaption", "NTooltip", null, 5, false, ControlType.TextBox)]
        public string N
        {
            get => n;
            set
            {
                n = value;
                OnPropertyChanged("N");
            }
        }

        /// <summary>
        /// Getter/Setter for the e
        /// </summary>
        [TaskPane("ECaption", "ETooltip", null, 6, false, ControlType.TextBox)]
        public string E
        {
            get => e;
            set
            {
                e = value;
                OnPropertyChanged("E");
            }
        }

        /// <summary>
        /// Getter/Setter for the D
        /// </summary>
        [TaskPane("DCaption", "DTooltip", null, 7, false, ControlType.TextBox)]
        public string D
        {
            get => d;
            set
            {
                d = value;
                OnPropertyChanged("D");
            }
        }

        /// <summary>
        /// Getter/Setter for the certificate file
        /// </summary>
        [TaskPane("CertificateFileCaption", "CertificateFileTooltip", null, 6, false, ControlType.OpenFileDialog, FileExtension = "X.509 certificates (*.cer)|*.cer")]
        public string CertificateFile
        {
            get => certificateFile;
            set
            {
                if (value != certificateFile)
                {
                    certificateFile = value;
                    OnPropertyChanged("CertificateFile");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the password of the certificate
        /// </summary>
        [DontSave]
        [TaskPane("PasswordCaption", "PasswordTooltip", null, 5, false, ControlType.TextBoxHidden)]
        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged("Password");
            }
        }

        /// <summary>
        /// Button to "close" the certificate file. That means it will not appear any more in the text field
        /// </summary>
        [TaskPane("CloseFileCaption", "CloseFileTooltip", null, 7, false, ControlType.Button)]
        public void CloseFile()
        {
            CertificateFile = null;
        }

        /// <summary>
        /// Getter/Setter for the type of the given range
        /// </summary>
        [TaskPane("RangeTypeCaption", "RangeTypeTooltip", null, 3, false, ControlType.RadioButton, new string[] { "RangeTypeList1", "RangeTypeList2" })]
        public int RangeType
        {
            get => rangeType;
            set
            {
                rangeType = value;
                OnPropertyChanged("RangeType");
            }
        }

        /// <summary>
        /// Getter/Setter for the range of the primes
        /// </summary>
        [TaskPane("RangeCaption", "RangeTooltip", null, 4, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]+$")]
        public string Range
        {
            get => range;
            set
            {
                if (value != range)
                {
                    range = value;
                    OnPropertyChanged("Range");
                }
            }
        }

        /// <summary>
        /// Enable/disable generation of safe primes
        /// </summary>
        [TaskPane("GenerateSafePrimesCaption", "GenerateSafePrimesTooltip", null, 5, false, ControlType.CheckBox)]
        public bool GenerateSafePrimes
        {
            get => generateSafePrimes;
            set
            {
                if (value != generateSafePrimes)
                {
                    generateSafePrimes = value;
                    OnPropertyChanged("GenerateSafePrimes");
                }
            }
        }

        #endregion

        #region private

        /// <summary>
        /// The property p changed
        /// </summary>
        /// <param name="p">p</param>
        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion

    }//end RSAKeyGeneratorSettings

}//end CrypTool.Plugins.RSA
