using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;

namespace RAPPOR.Model
{
    /// <summary>
    /// Represents the instantaneous randomized responses which can be created from the permanent
    /// randomized response which in turn has been created from the Bloom filter. The IRR has two
    /// parameters which control the rate in which parts of the filter are beeing falsified.
    /// </summary>
    public class InstantaneousRandomizedResponse : INotifyPropertyChanged
    {
        /// <summary>
        /// The boolean array which is representing the IRR.
        /// </summary>
        private readonly bool[] boolArray;

        /// <summary>
        /// Creates the instance of the IRR. The user controlable parameters p and r control the 
        /// rate in which the permanet randomized response is beeing further falsified.
        /// </summary>
        /// <param name="boolInput">The boolean array which is beeing turned into the instantaneous
        /// randomized response. This is normally the boolean array created in the instantaneous 
        /// randomized response.</param>
        /// <param name="r"> A random object used for falsifying the input parameter</param>
        /// <param name="sizeOfBloomFilter"> The original size of the bloom filter, BF IRR and
        /// PRR are always the same size.</param>
        /// <param name="p">The user tunable parameter p.</param>
        /// <param name="q">The user tunable parameter q.</param>
        public InstantaneousRandomizedResponse(bool[] boolInput, Random r, int sizeOfBloomFilter, int p, int q)
        {
            boolArray = new bool[sizeOfBloomFilter];
            double x;

            //Calculating the entry of the altered bloom filters with the given parameters
            for (int j = 0; j < sizeOfBloomFilter; j++)
            {
                x = r.NextDouble() * 100;
                if (boolInput[j] == true)
                {
                    if (x < q)
                    {
                        boolArray[j] = true;
                    }
                    else
                    {
                        boolArray[j] = false;
                    }
                }
                else
                {
                    if (x < p)
                    {
                        boolArray[j] = true;
                    }
                    else
                    {
                        boolArray[j] = false;
                    }
                }
            }
        }
        /// <summary>
        /// Returns the boolean array created by the alteration of the instantaneous randomized
        /// response.
        /// </summary>
        /// <returns>A boolean array as altered by the instantaneous randomized response.</returns>
        public bool[] GetBoolArray()
        {
            return boolArray;
        }

        /// <summary>
        /// Returns a string representation of the Bloom filter boolean array which has been
        /// altered instantaneous randomized response.
        /// </summary>
        /// <returns>A string of the Bloom filter boolean array which has been
        /// altered instantaneous randomized response.</returns>
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
        /// The canvas of the permantent randmized response array
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
