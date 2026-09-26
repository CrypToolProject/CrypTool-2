/*
   Copyright (C) CrypTool 2 Team

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
using CrypTool.Chaocipher.Enums;
using System;
using System.Collections.Generic;

namespace CrypTool.Chaocipher.Models
{
    public class PresentationState
    {
        public char[] CipherWorkingAlphabet { get; set; }
        public char[] PlainWorkingAlphabet { get; set; }
        public Description Description { get; set; }
        public Step Step { get; set; }
        public string InputCharInFocus { get; set; }
        public string OutputCharInFocus { get; set; }

        public PresentationState(List<char> cipherWorking, List<char> plainWorking, Step step,
            object[] descriptionDetails = null)
        {
            CipherWorkingAlphabet = cipherWorking.ToArray();
            PlainWorkingAlphabet = plainWorking.ToArray();
            Step = step;
            Description = new Description
            {
                DescriptionDetails = descriptionDetails
            };
        }
    }

    public class Description
    {
        private string _text;
        public int Index { get; set; }
        public string Text
        {
            get => _text;
            set => _text = FormatDescriptionText(value);
        }
        public object[] DescriptionDetails { private get; set; }

        public override string ToString()
        {
            return $"{Index + 1}. " + Text;
        }

        private string FormatDescriptionText(string text)
        {
            try
            {
                return string.Format(text, DescriptionDetails ?? new object[] { "" });
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
        }


    }
}