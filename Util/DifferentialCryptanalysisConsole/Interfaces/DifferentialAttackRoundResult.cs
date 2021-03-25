using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public class DifferentialAttackRoundResult
    {
        public int Round;
        public int PossibleKey;
        public double Probability;
        public double ExpectedProbability;
        public List<KeyProbability> KeyCandidateProbabilities;

        public DifferentialAttackRoundResult()
        {
            Round = -1;
            PossibleKey = -1;
            Probability = -1;
            ExpectedProbability = -1;
            KeyCandidateProbabilities = new List<KeyProbability>();
        }
    }
}
