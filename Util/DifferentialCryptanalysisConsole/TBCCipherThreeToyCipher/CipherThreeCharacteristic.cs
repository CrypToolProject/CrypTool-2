using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace TBCCipherThreeToyCipher
{
    class CipherThreeCharacteristic : Characteristic
    {
        public CipherThreeCharacteristic()
        {
            InputDifferentials = new int[3];
            OutputDifferentials = new int[2];

            for (int i = 0; i < InputDifferentials.Length; i++)
            {
                InputDifferentials[i] = 0;
            }

            for (int i = 0; i < OutputDifferentials.Length; i++)
            {
                OutputDifferentials[i] = 0;
            }
        }

        public override object Clone()
        {
            Characteristic obj = new CipherThreeCharacteristic
            {
                InputDifferentials = (int[])this.InputDifferentials.Clone(),
                OutputDifferentials = (int[])this.OutputDifferentials.Clone(),
                Probability = this.Probability
            };

            return obj;
        }

        public override string ToString()
        {
            return "Prob: " + Probability + " InputDiffRound3: " + InputDifferentials[2] + " OutputDiffRound2: " +
                   OutputDifferentials[1] + " InputDiffRound2: " + InputDifferentials[1] + " OutputDiffRound1: " +
                   OutputDifferentials[0] + " InputDiffRound1: " + InputDifferentials[0];
        }
    }
}
