using System;

namespace KeyTextBox
{
    public class InvalidKeyException : Exception
    {
        public string Key { get; set; }

        public InvalidKeyException(string key)
        {
            Key = key;
        }
    }
}