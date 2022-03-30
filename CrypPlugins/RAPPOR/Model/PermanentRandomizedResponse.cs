using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;

namespace RAPPOR.Model
{
    public class PermanentRandomizedResponse : INotifyPropertyChanged
    {
        /// <summary>Thee boolean array representing the permanent randomized response</summary>
        private readonly bool[] boolArray;
        /// <summary>
        /// Creates an instance of the permanet randomized response. The PRR is created by
        /// falsifying the Bloom filter through randomization, the scale of randomization
        /// and falsification is controlled by the user tunable parameter f. The PRR
        /// is later used to create the IRR instances.
        /// </summary>
        /// <param name="boolInput">The boolean array of the bloom filter which is beeing
        /// falsified through the randomized response mechanism.</param>
        /// <param name="r">tThe random instance which is beeing used to falsify the boolean 
        /// array.</param>
        /// <param name="sizeOfBloomFilter">The size of the orginal Bloom filter.</param>
        /// <param name="f">The user tunable parameter f.</param>
        public PermanentRandomizedResponse(bool[] boolInput, Random r, int sizeOfBloomFilter, int f)
        {
            boolArray = new bool[sizeOfBloomFilter];
            double x;

            //Calculating the entry of the altered bloom filters with the given parameters
            for (int i = 0; i < boolInput.Length; i++)
            {
                x = r.NextDouble() * 100;
                if (x < f / 2)
                {
                    boolArray[i] = false;
                }
                else if (x < f)
                {
                    boolArray[i] = true;
                }
                else
                {
                    boolArray[i] = boolInput[i];
                }
            }
        }
        /// <summary>
        /// Returns the boolean Array which has been created by the Bloom filter and altered by the permanent randomized response
        /// </summary>
        /// <returns>The boolean array which has been created ad is now altered by ther permanent randomized response.</returns>
        public bool[] GetBoolArray()
        {
            return boolArray;
        }
        /// <summary>
        /// Returns a string representaion of the Boolean array S, which is the processed Boolean
        /// array B from the Bloom filter.
        /// </summary>
        /// <returns>A string representaion of the Boolean array S</returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(boolArray.Length);
            for (int i = 0; i < boolArray.Length; i++)
            {
                if (boolArray[i].ToString() == "True")
                {
                    stringBuilder.Append("1");
                }
                else
                {
                    stringBuilder.Append("0");
                }
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        ///  Canvas of the PRR array
        /// </summary>
        private Canvas _PRRArray;

        /// <summary>
        /// The event handle of the class
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Getter and setter for the PRRArray canvas.
        /// </summary>
        public Canvas PRRArray
        {
            get
            {
                if (_PRRArray == null)
                {
                    return null;
                }

                return _PRRArray;
            }
            set
            {
                _PRRArray = value;
                OnPropertyChanged("PRRArray");
            }
        }

        /// <summary>
        /// This method changes the property if an event has occured.
        /// </summary>
        /// <param name="propertyName">The name of the parameter which is to be changed</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
