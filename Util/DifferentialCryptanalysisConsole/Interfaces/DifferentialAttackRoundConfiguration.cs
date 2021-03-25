using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public class DifferentialAttackRoundConfiguration
    {
        public int Round;
        public bool[] ActiveSBoxes;
        public bool IsLast;
        public bool IsBeforeLast;
        public double Probability;
        public int InputDifference;
        public int ExpectedDifference;
        public List<Characteristic> Characteristics;
        public List<Pair> UnfilteredPairList;
        public List<Pair> FilteredPairList;
        public SearchPolicy SearchPolicy;
        public AbortingPolicy AbortingPolicy;

        public DifferentialAttackRoundConfiguration()
        {
            Round = -1;
            ActiveSBoxes = null;
            InputDifference = -1;
            ExpectedDifference = -1;
            IsLast = false;
            IsBeforeLast = false;
            Characteristics = new List<Characteristic>();
            UnfilteredPairList = new List<Pair>();
            FilteredPairList = new List<Pair>();
        }


        public string GetActiveSBoxes()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = (ActiveSBoxes.Length -1); i >= 0; i--)
            {
                if (ActiveSBoxes[i] == true)
                {
                    sb.Append("1");
                }
                else
                {
                    sb.Append("0");
                }
            }

            return sb.ToString();
        }
    }
}
