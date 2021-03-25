using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public abstract class Characteristic : ICloneable
    {
        /// <summary>
        /// Attributes
        /// </summary>

        //Heys Cipher
        /*
        public int[] InputDifferentials = new int[4];
        public int[] OutputDifferentials = new int[3];
        */

        //TBC CipherFour
        /* */
        public int[] InputDifferentials;
        public int[] OutputDifferentials;
        

        public double Probability = -1;

        /// <summary>
        /// Constructor with init of arrays to 0
        /// </summary>
        /*
        public Characteristic()
        {
            for (int i = 0; i < 5; i++)
            {
                InputDifferentials[i] = 0;
            }

            for (int i = 0; i < 4; i++)
            {
                OutputDifferentials[i] = 0;
            }
        }
        */

        public abstract object Clone();

        public abstract string ToString();

        /// <summary>
        /// Copyconstructor
        /// </summary>
        /// <returns></returns>
        /*
        public object Clone()
        {
            Characteristic obj = new Characteristic
            {
                InputDifferentials = (int[])this.InputDifferentials.Clone(),
                OutputDifferentials = (int[])this.OutputDifferentials.Clone(),
                Probability = this.Probability
            };

            return obj;
        }
        */

        /// <summary>
        /// ToString() Method for output
        /// </summary>
        /// <returns></returns>
        /*
        public override string ToString()
        {
            //Heys Cipher
            
            return "Prob: " + Probability + " InputDiffRound4: " + InputDifferentials[3] + " OutputDiffRound3: " +
                   OutputDifferentials[2] + " InputDiffRound3: " + InputDifferentials[2] + " OutputDiffRound2: " +
                   OutputDifferentials[1] + " InputDiffRound2: " + InputDifferentials[1] + " OutputDiffRound1: " +
                   OutputDifferentials[0] + " InputDiffRound1: " + InputDifferentials[0];
            

            //TBC CipherFour
            return "Prob: " + Probability + " InputDiffRound5: " + InputDifferentials[4] + " OutputDiffRound4: " +
                   OutputDifferentials[3] + " InputDiffRound4: " + InputDifferentials[3] + " OutputDiffRound3: " +
                   OutputDifferentials[2] + " InputDiffRound3: " + InputDifferentials[2] + " OutputDiffRound2: " +
                   OutputDifferentials[1] + " InputDiffRound2: " + InputDifferentials[1] + " OutputDiffRound1: " +
                   OutputDifferentials[0] + " InputDiffRound1: " + InputDifferentials[0];
            
        }
        */
    }
}
