using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public class KeyProbability
    {
        public int Key;
        public int Counter;
        public int randomNumber;

        public KeyProbability()
        {
            randomNumber = (new Random()).Next(int.MaxValue);
        }
    }
}
