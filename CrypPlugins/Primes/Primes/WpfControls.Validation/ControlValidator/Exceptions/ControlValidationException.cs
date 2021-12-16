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

using Primes.OnlineHelp;
using System;
using System.Windows.Controls;

namespace Primes.WpfControls.Validation.ControlValidator.Exceptions
{
    public class ControlValidationException : Exception
    {
        public ControlValidationException(string message, Control control, ValidationResult vr)
            : base(message)
        {
            m_ValidationResult = vr;
            m_Control = control;
        }

        #region Properties

        private ValidationResult m_ValidationResult;

        public ValidationResult ValidationResult
        {
            get => m_ValidationResult;
            set => m_ValidationResult = value;
        }

        private Control m_Control;

        public Control Control
        {
            get => m_Control;
            set => m_Control = value;
        }

        private OnlineHelpActions m_HelpAction;

        public OnlineHelpActions HelpAction
        {
            get => m_HelpAction;
            set => m_HelpAction = value;
        }

        #endregion
    }
}
