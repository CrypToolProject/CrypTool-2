/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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

using Primes.Library;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.Start
{
    /// <summary>
    /// Interaction logic for StartControl.xaml
    /// </summary>
    public partial class StartControl : UserControl, IPrimeMethodDivision
    {
        private readonly System.Windows.Forms.WebBrowser b;
        public event Navigate OnStartpageLinkClick;
        public StartControl()
        {
            InitializeComponent();
            b = new System.Windows.Forms.WebBrowser
            {
                Dock = System.Windows.Forms.DockStyle.Fill
            };
            b.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(b_Navigating);
            windowsFormsHost1.Child = b;
            b.DocumentText = Properties.Resources.Start;
            //b.Document.ContextMenuShowing += new System.Windows.Forms.HtmlElementEventHandler(Document_ContextMenuShowing);
        }

        private void Document_ContextMenuShowing(object sender, System.Windows.Forms.HtmlElementEventArgs e)
        {
            e.ReturnValue = false;
        }

        private void b_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e)
        {
            string target = e.Url.AbsoluteUri;
            if (target.IndexOf("exec://") >= 0)
            {
                target = target.Replace("exec://", "");
                target = target.Replace("/", "");
            }
            if (!string.IsNullOrEmpty(target) && OnStartpageLinkClick != null)
            {
                NavigationCommandType action = NavigationCommandType.None;
                switch (target.ToLower())
                {
                    case "factor_bf":
                        action = NavigationCommandType.Factor_Bf;
                        break;
                    case "factor_qs":
                        action = NavigationCommandType.Factor_QS;
                        break;
                    case "primetest_sieve":
                        action = NavigationCommandType.Primetest_Sieve;
                        break;
                    case "primetest_miller":
                        action = NavigationCommandType.Primetest_Miller;
                        break;
                    case "primesgeneration":
                        action = NavigationCommandType.Primesgeneration;
                        break;
                    case "graph":
                        action = NavigationCommandType.Graph;
                        break;
                    case "primedistrib_numberline":
                        action = NavigationCommandType.PrimeDistrib_Numberline;
                        break;
                    case "primedistrib_numberrec":
                        action = NavigationCommandType.PrimeDistrib_Numberrec;
                        break;
                    case "primedistrib_ulam":
                        action = NavigationCommandType.PrimeDistrib_Ulam;
                        break;
                    case "primitivroot":
                        action = NavigationCommandType.PrimitivRoot;
                        break;
                    case "powermod":
                        action = NavigationCommandType.PowerMod;
                        break;
                    case "numbertheoryfunctions":
                        action = NavigationCommandType.NumberTheoryFunctions;
                        break;
                    case "primedistrib_goldbach":
                        action = NavigationCommandType.PrimeDistrib_Goldbach;
                        break;
                }
                if (action != NavigationCommandType.None)
                {
                    OnStartpageLinkClick(action);
                    e.Cancel = true;
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            //this.windowsFormsHost1.Width = sizeInfo.NewSize.Width;
        }

        #region IPrimeUserControl Members

        public void Dispose()
        {
            //
        }

        #endregion

        #region IPrimeUserControl Members

        public void SetTab(int i)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IPrimeUserControl Members

        public event VoidDelegate Execute;

        public void FireExecuteEvent()
        {
            if (Execute != null)
            {
                Execute();
            }
        }

        public event VoidDelegate Stop;

        public void FireStopEvent()
        {
            if (Stop != null)
            {
                Stop();
            }
        }

        #endregion

        #region IPrimeUserControl Members

        public void Init()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
