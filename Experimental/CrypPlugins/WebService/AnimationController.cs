using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WebService
{
    public class AnimationController
    {
        #region fields

        private readonly WebServicePresentation _presentation;
        private readonly DispatcherTimer _controllerTimer;
        private int _status;
        private int _wsSecurityElementsCounter;
        private int _actualSecurityElementNumber;

        #endregion

        #region Properties

        public DispatcherTimer ControllerTimer => _controllerTimer;

        #endregion

        #region Constructor

        public AnimationController(WebServicePresentation presentation)
        {
            _presentation = presentation;
            _controllerTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 1, 0)
            };
            _controllerTimer.Tick += new EventHandler(ControllerTimerTickEventHandler);

        }

        #endregion

        #region Methods

        public void InitializeAnimation()
        {
            _presentation.DecryptionAnimation.fillEncryptedDataTreeviewElements();
            _presentation.DecryptionAnimation.initializeAnimation();

            if (GetSecurityHeaderElement(0).Equals("ds:Signature"))
            {
                _status = 1;
            }
            else if (GetSecurityHeaderElement(0).Equals("xenc:EncryptedKey"))
            {
                _status = 2;
            }
            else
            {
                _presentation.WebService.ShowWarning("There are no security elements inside the SOAP message so that animation is not possible");

            }

            _actualSecurityElementNumber = 0;
            _presentation.FindSignatureItems((TreeViewItem)_presentation.SoapInputItem.Items[0], "ds:Signature");
            GetTotalSecurityElementsNumber();
            _controllerTimer.Start();

        }
        private int GetStatus(int actualNumber)
        {
            if (GetSecurityHeaderElement(actualNumber).Equals("ds:Signature"))
            {
                return 1;

            }
            else
            {
                return 2;
            }
        }

        public void InitializeAnimations()
        {
            _presentation.DecryptionAnimation.initializeAnimation();
            _presentation.InitializeAnimation();
        }
        public void GetTotalSecurityElementsNumber()
        {
            _wsSecurityElementsCounter = _presentation.WebService.Validator.GetTotalSecurityElementsNumber();
        }
        private string GetSecurityHeaderElement(int elementNumber)
        {
            string securityHeaderName = _presentation.WebService.Validator.GetWSSecurityHeaderElementName(elementNumber);
            return securityHeaderName;
        }

        #endregion

        #region EventHandlers

        private void ControllerTimerTickEventHandler(object sender, EventArgs e)
        {
            switch (_status)
            {
                case 1:

                    _controllerTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    _controllerTimer.Stop();
                    _presentation.SignatureTimer.Start();
                    if (_actualSecurityElementNumber + 1 < _wsSecurityElementsCounter)
                    {
                        _actualSecurityElementNumber++;
                    }
                    _status = GetStatus(_actualSecurityElementNumber);

                    break;
                case 2:

                    _controllerTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    _controllerTimer.Stop();
                    _presentation.DecryptionAnimation.getDecryptiontimer().Start();
                    if (_actualSecurityElementNumber + 1 < _wsSecurityElementsCounter)
                    {
                        _actualSecurityElementNumber++;
                    }
                    _status = GetStatus(_actualSecurityElementNumber);

                    break;
                case 3:
                    _controllerTimer.Stop();
                    break;

            }
        }

        #endregion

    }
}
