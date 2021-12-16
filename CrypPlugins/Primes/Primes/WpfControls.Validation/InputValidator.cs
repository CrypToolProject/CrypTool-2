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

namespace Primes.WpfControls.Validation
{
    public class InputValidator<T>
    {
        private IValidator<T> m_Validator;

        public IValidator<T> Validator
        {
            get => m_Validator;
            set => m_Validator = value;
        }

        private OnlineHelpActions m_LinkOnlinehelp;

        public OnlineHelpActions LinkOnlinehelp
        {
            get => m_LinkOnlinehelp;
            set => m_LinkOnlinehelp = value;
        }

        private string m_DefaultValue;

        public string DefaultValue
        {
            get => m_DefaultValue;
            set => m_DefaultValue = value;
        }
    }
}
