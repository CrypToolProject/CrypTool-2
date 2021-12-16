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
using Primes.WpfControls.Validation.ControlValidator.Exceptions;
using System.Windows.Controls;

namespace Primes.WpfControls.Validation.ControlValidator
{
    public class TextBoxValidator<T>
    {
        private IValidator<T> m_Validator;

        public IValidator<T> Validator
        {
            get => m_Validator;
            set => m_Validator = value;
        }

        private TextBox m_TextBox = null;

        public TextBox TextBox
        {
            get => m_TextBox;
            set => m_TextBox = value;
        }

        private string m_DefaultValue;

        public string DefaultValue
        {
            get => m_DefaultValue;
            set => m_DefaultValue = value;
        }

        public TextBoxValidator()
        {
        }

        public TextBoxValidator(IValidator<T> validator, TextBox tbSource)
        {
            Validator = validator;
            TextBox = tbSource;
            Validator.Value = tbSource.Text;
        }

        public TextBoxValidator(IValidator<T> validator, TextBox tbSource, string defaultvalue)
            : this(validator, tbSource)
        {
            if (string.IsNullOrEmpty(tbSource.Text))
            {
                Validator.Value = defaultvalue;
                ControlHandler.SetPropertyValue(tbSource, "Text", defaultvalue);
                ControlHandler.ExecuteMethod(tbSource, "SelectAll");
            }
        }

        public bool Validate(ref T t)
        {
            bool result = true;
            ValidationResult validationResult = m_Validator.Validate(ref t);

            if (validationResult != Primes.WpfControls.Validation.ValidationResult.OK)
            {
                result = false;
                t = default(T);

                switch (validationResult)
                {
                    case Primes.WpfControls.Validation.ValidationResult.ERROR:
                        Error(m_Validator.Message);
                        break;
                    case Primes.WpfControls.Validation.ValidationResult.WARNING:
                        Warning(m_Validator.Message);
                        break;
                }
            }

            return result;
        }

        private void Warning(string message)
        {
            throw new ControlValidationException(message, TextBox, ValidationResult.WARNING);
        }

        private void Error(string message)
        {
            throw new ControlValidationException(message, TextBox, ValidationResult.ERROR);
        }
    }
}