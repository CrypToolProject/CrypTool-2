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