using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public class SBoxCharacteristic : ICloneable
    {
        public int InputDifferential;
        public int OutputDifferential;
        public List<Pair> InputPairList;
        public List<Pair> OutputPairList;
        public int Count = 0;
        public double Probability;

        public SBoxCharacteristic()
        {
            InputDifferential = 0;
            OutputDifferential = 0;
            Count = 0;

            InputPairList = new List<Pair>();
            OutputPairList = new List<Pair>();
        }

        public object Clone()
        {
            List<Pair> newListOfInputPairs = new List<Pair>(this.InputPairList.Count);
            this.InputPairList.ForEach((item) =>
            {
                newListOfInputPairs.Add((Pair)item.Clone());
            });

            List<Pair> newListOfOutputPairs = new List<Pair>(this.OutputPairList.Count);
            this.OutputPairList.ForEach((item) =>
            {
                newListOfOutputPairs.Add((Pair)item.Clone());
            });

            var clone = new SBoxCharacteristic
            {
                InputDifferential = this.InputDifferential,
                OutputDifferential = this.OutputDifferential,
                InputPairList = newListOfInputPairs,
                OutputPairList = newListOfOutputPairs,
                Count = this.Count,
                Probability = this.Probability
            };

            return clone;
        }
    }
}
