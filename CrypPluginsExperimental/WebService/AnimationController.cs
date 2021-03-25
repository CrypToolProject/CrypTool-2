using System;
using System.Windows.Threading;
using System.Windows.Controls;

namespace WebService
{
    public class AnimationController
    {
        #region fields

        private WebServicePresentation _presentation;
        private DispatcherTimer _controllerTimer;
        private int _status;
        private int _wsSecurityElementsCounter;
        private int _actualSecurityElementNumber;
       
        #endregion

        #region Properties

        public DispatcherTimer ControllerTimer
        {
            get
            {
                return this._controllerTimer;
            }
        }

        #endregion

        #region Constructor

        public AnimationController(WebServicePresentation presentation)
        {
            this._presentation = presentation;
            this._controllerTimer = new DispatcherTimer();
            this._controllerTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            this._controllerTimer.Tick += new EventHandler(ControllerTimerTickEventHandler);

        }

        #endregion

        #region Methods

        public void InitializeAnimation()
        {
            this._presentation.DecryptionAnimation.fillEncryptedDataTreeviewElements();
            this._presentation.DecryptionAnimation.initializeAnimation();

            if (this.GetSecurityHeaderElement(0).Equals("ds:Signature"))
            {
                this._status = 1;
            }
            else if (this.GetSecurityHeaderElement(0).Equals("xenc:EncryptedKey"))
            {
                this._status = 2;
            }
            else
            {
                this._presentation.WebService.ShowWarning("There are no security elements inside the SOAP message so that animation is not possible");

            }

            this._actualSecurityElementNumber = 0;
            this._presentation.FindSignatureItems((TreeViewItem)this._presentation.SoapInputItem.Items[0], "ds:Signature");
            this.GetTotalSecurityElementsNumber();
            this._controllerTimer.Start();

        }
        private int GetStatus(int actualNumber)
        {
            if (this.GetSecurityHeaderElement(actualNumber).Equals("ds:Signature"))
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
            this._presentation.DecryptionAnimation.initializeAnimation();
            this._presentation.InitializeAnimation();
        }
        public void GetTotalSecurityElementsNumber()
        {
            this._wsSecurityElementsCounter = this._presentation.WebService.Validator.GetTotalSecurityElementsNumber();
        }
        private string GetSecurityHeaderElement(int elementNumber)
        {
            string securityHeaderName = this._presentation.WebService.Validator.GetWSSecurityHeaderElementName(elementNumber);
            return securityHeaderName;
        }

        #endregion

        #region EventHandlers

        private void ControllerTimerTickEventHandler(object sender, EventArgs e)
        {
            switch (this._status)
            {
                case 1:

                    this._controllerTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    this._controllerTimer.Stop();
                    this._presentation.SignatureTimer.Start();
                    if (this._actualSecurityElementNumber + 1 < this._wsSecurityElementsCounter)
                    {
                        this._actualSecurityElementNumber++;
                    }
                    this._status = this.GetStatus(this._actualSecurityElementNumber);

                    break;
                case 2:

                    this._controllerTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    this._controllerTimer.Stop();
                    this._presentation.DecryptionAnimation.getDecryptiontimer().Start();
                    if (this._actualSecurityElementNumber + 1 < this._wsSecurityElementsCounter)
                    {
                        this._actualSecurityElementNumber++;
                    }
                    this._status = this.GetStatus(this._actualSecurityElementNumber);

                    break;
                case 3:
                    this._controllerTimer.Stop();
                    break;

            }
        }

        #endregion

    }
}
