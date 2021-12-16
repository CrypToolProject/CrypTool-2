using System;
using System.Collections.Generic;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    public class KeyCandidate : IEquatable<KeyCandidate>
    {
        private int[] key;
        private double fitness;
        private readonly string plaintext;
        private string key_string;
        private bool genAttack;
        private bool dicAttack;
        private bool hillAttack;

        public string Key_string
        {
            get => key_string;
            set => key_string = value;
        }

        public int[] Key
        {
            get => key;
            set => key = value;
        }

        public double Fitness
        {
            get => fitness;
            set => fitness = value;
        }

        public string Plaintext
        {
            get => plaintext;
            set {; }
        }

        public bool GenAttack
        {
            get => genAttack;
            set => genAttack = value;
        }

        public bool DicAttack
        {
            get => dicAttack;
            set => dicAttack = value;
        }

        public bool HillAttack
        {
            get => hillAttack;
            set => hillAttack = value;
        }

        public KeyCandidate(int[] key, double fitness, string plaintext, string key_string)
        {
            this.key = key;
            this.fitness = fitness;
            this.plaintext = plaintext;
            this.key_string = key_string;
        }

        public bool Equals(KeyCandidate keyCandidate)
        {
            if (plaintext.Equals(keyCandidate.plaintext))
            {
                return true;
            }

            return false;
        }
    }

    internal class KeyCandidateComparer : IComparer<KeyCandidate>
    {
        public int Compare(KeyCandidate a, KeyCandidate b)
        {
            return -a.Fitness.CompareTo(b.Fitness);
        }
    }
}
