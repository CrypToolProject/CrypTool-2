using System;

namespace CrypTool.Plugins.SZ42
{
    /// <summary>
    /// Class that represents a wheel
    /// </summary>
    [Serializable]
    public class Wheel
    {
        private string name;          //name of the wheel        
        private int currentPosition;  //current position of the wheel (position of the active state)
        private char[] pattern;       //the pattern of crosses and dots of the wheel
        private readonly int period;           //length of the tab on circunference

        /// <summary>
        /// Constructor that initialize the name 
        /// and length for the wheel
        /// </summary>
        public Wheel(string nombre, int period)
        {
            name = nombre;
            this.period = period;
            currentPosition = 0;
            pattern = new char[period];
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public int CurrentPosition
        {
            get => currentPosition + 1;
            set => currentPosition = value - 1;
        }

        /// <summary>
        /// Public property of the pattern
        /// of the wheel
        /// </summary>
        public char[] Pattern
        {
            get => pattern;
            set => pattern = value;
        }

        /// <summary>
        /// public property of 
        /// the current active state
        /// of the wheel
        /// </summary>
        public char ActiveState => pattern[currentPosition];

        /// <summary>
        /// Public property of the 
        /// length of wheel (wheel period)
        /// </summary>
        public int Period => period;

        public string PatternSerialized
        {
            get
            {
                string p = "";

                foreach (char c in pattern)
                {
                    p += c;
                }

                return p;
            }
        }

        /// <summary>
        /// Represents the movement
        /// of the wheel 
        /// </summary>
        public void MoveOnce()
        {
            if (currentPosition == pattern.Length - 1)
            {
                currentPosition = 0;
            }
            else
            {
                currentPosition++;
            }
        }
    }
}
