using CrypTool.PluginBase;
using System.Collections;
using System.ComponentModel;

namespace Soap
{
    public class SoapSettings : ISettings
    {
        #region ISettings Member

        public string ResetSoap => null;

        [TaskPane("sendSoapCaption", "sendSoapTooltip", null, 1, true, ControlType.Button)]
        public void sendSoap()
        {
            OnPropertyChanged("sendSoap");
        }

        [TaskPane("resetSoapCaption", "resetSoapTooltip", null, 1, true, ControlType.Button)]
        public void resetSoap()
        {
            OnPropertyChanged("ResetSoap");
        }

        private string signatureAlg = "1";
        [TaskPane("SignatureAlgCaption", "SignatureAlgTooltip", "Signature", 3, true, ControlType.ComboBox, new string[] { "DSA-SHA1", "RSA-SHA1" })]
        public string SignatureAlg
        {
            get => signatureAlg;
            set
            {
                signatureAlg = value;
                OnPropertyChanged("SignatureAlg");
            }
        }

        private bool sigXPathRef;
        [TaskPane("SigXPathRefCaption", "SigXPathRefTooltip", "Signature", 4, true, ControlType.CheckBox)]
        public bool SigXPathRef
        {
            get => sigXPathRef;
            set
            {
                sigXPathRef = value;
                OnPropertyChanged("SigXPathRef");
            }
        }

        private bool sigShowSteps;
        [TaskPane("SigShowStepsCaption", "SigShowStepsTooltip", "Signature", 5, true, ControlType.CheckBox)]
        public bool SigShowSteps
        {
            get => sigShowSteps;
            set
            {
                sigShowSteps = value;
                OnPropertyChanged("SigShowSteps");
            }
        }


        private int animationSpeed = 3;
        [TaskPane("AnimationSpeedCaption", "AnimationSpeedTooltip", "Animation", 9, true, ControlType.NumericUpDown, CrypTool.PluginBase.ValidationType.RangeInteger, 1, 5)]
        public int AnimationSpeed
        {
            get => animationSpeed;
            set
            {
                animationSpeed = value;
                OnPropertyChanged("AnimationSpeed");
            }
        }

        [TaskPane("playPauseCaption", "playPauseTooltip", "Animation", 7, true, ControlType.Button)]
        public void playPause()
        {
            OnPropertyChanged("playPause");
            {
            }
        }

        [TaskPane("endAnimationCaption", "endAnimationTooltip", "Animation", 8, true, ControlType.Button)]
        public void endAnimation()
        {
            OnPropertyChanged("endAnimation");
            {
            }
        }



        public enum encryptionType { Element = 0, Content = 1 };
        private encryptionType encContentRadio;

        //    [ContextMenu("Encryption Mode", "Choose wether to encrypt the XML-Element or the content of the XML-Element", 6, ContextMenuControlType.ComboBox, null, new string[] { "XML-Element", "Content of XML-Element" })]
        [TaskPane("EncContentRadioCaption", "EncContentRadioTooltip", "Encryption", 6, true, ControlType.RadioButton, new string[] { "XML-Element", "Content of XML-Element" })]
        public int EncContentRadio
        {
            get => (int)encContentRadio;
            set
            {
                if (encContentRadio != (encryptionType)value)
                {
                    encContentRadio = (encryptionType)value;
                    OnPropertyChanged("EncContentRadio");
                }
            }
        }



        private bool encShowSteps;
        [TaskPane("EncShowStepsCaption", "EncShowStepsTooltip", "Encryption", 12, true, ControlType.CheckBox)]
        public bool EncShowSteps
        {
            get => encShowSteps;
            set
            {
                encShowSteps = value;
                OnPropertyChanged("EncShowSteps");
            }
        }

        #endregion

        private string soapElement;
        public string soapelement
        {
            get => soapElement;
            set
            {
                soapElement = value;
                OnPropertyChanged("soapelement");
            }
        }

        private string securedSoap;
        public string securedsoap
        {
            get => securedSoap;
            set
            {

                securedSoap = value;
                OnPropertyChanged("securedsoap");
            }
        }

        private Hashtable idTable;
        public Hashtable idtable
        {
            get => idTable;
            set
            {
                idTable = value;
                OnPropertyChanged("idtable");
            }
        }


        private bool bodySigned, methodNameSigned, bodyEncrypted, methodNameEncrypted, secHeaderEnc, secHeaderSigned;
        public bool bodysigned
        {
            get => bodySigned;
            set
            {
                bodySigned = value;
                OnPropertyChanged("bodysigned");
            }
        }

        public bool methodnameSigned
        {
            get => methodNameSigned;
            set
            {
                methodNameSigned = value;
                OnPropertyChanged("methodnameSigned");
            }
        }



        public bool bodyencrypted
        {
            get => bodyEncrypted;
            set
            {
                bodyEncrypted = value;
                OnPropertyChanged("bodyencrypted");
            }
        }

        public bool methodnameencrypted
        {
            get => methodNameEncrypted;
            set
            {
                methodNameEncrypted = value;
                OnPropertyChanged("methodnameencrypted");
            }
        }
        public bool secheaderEnc
        {
            get => secHeaderEnc;
            set
            {
                secHeaderEnc = value;
                OnPropertyChanged("secheaderEnc");
            }
        }

        public bool secheaderSigned
        {
            get => secHeaderSigned;
            set
            {
                secHeaderSigned = value;
                OnPropertyChanged("secheaderSigned");
            }
        }

        private int contentCounter;

        public int contentcounter
        {
            get => contentCounter;
            set
            {
                contentCounter = value;
                OnPropertyChanged("contentcounter");
            }
        }

        private string wsRSACryptoProv, rsaCryptoProv;

        public string wsRSAcryptoProv
        {
            get => wsRSACryptoProv;
            set
            {
                wsRSACryptoProv = value;
                OnPropertyChanged("wsRSAcryptoProv");
            }
        }

        public string rsacryptoProv
        {
            get => rsaCryptoProv;
            set
            {
                rsaCryptoProv = value;
                OnPropertyChanged("rsacryptoProv");
            }
        }



        private string dsaCryptoProv;
        public string dsacryptoProv
        {
            get => dsaCryptoProv;
            set
            {
                dsaCryptoProv = value;
                OnPropertyChanged("dsacryptoProv");
            }
        }

        private string wsPublicKey;
        public string wspublicKey
        {
            get => wsPublicKey;
            set
            {
                wsPublicKey = value;
                OnPropertyChanged("wspublicKey");
            }
        }

        private bool gotKey;
        public bool gotkey
        {
            get => gotKey;
            set
            {
                gotKey = value;
                OnPropertyChanged("gotkey");
            }
        }
        private bool wsdlLoaded;
        public bool wsdlloaded
        {
            get => wsdlLoaded;
            set
            {
                wsdlLoaded = value;
                OnPropertyChanged("wsdlloaded");
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
