using System;
using System.Collections.Generic;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    public class KeyCandidate : IEquatable<KeyCandidate>
    {
        private int[] key;
        private double fitness;
        private String plaintext;
        private String key_string;
        private bool genAttack;
        private bool dicAttack;
        private bool hillAttack;

        public String Key_string
        {
            get { return this.key_string; }
            set { this.key_string = value; }
        }

        public int[] Key
        {
            get { return this.key; }
            set { this.key = value; }
        }

        public double Fitness
        {
            get { return this.fitness; }
            set { this.fitness = value; }
        }

        public String Plaintext
        {
            get { return this.plaintext; }
            set { ; }
        }

        public Boolean GenAttack
        {
            get { return this.genAttack; }
            set { this.genAttack = value; }
        }

        public Boolean DicAttack
        {
            get { return this.dicAttack; }
            set { this.dicAttack = value; }
        }

        public Boolean HillAttack
        {
            get { return this.hillAttack; }
            set { this.hillAttack = value; }
        }

        public KeyCandidate(int[] key, double fitness, String plaintext, String key_string)
        {
            this.key = key;
            this.fitness = fitness;
            this.plaintext = plaintext;
            this.key_string = key_string;
        }

        public bool Equals(KeyCandidate keyCandidate)
        {
            if (this.plaintext.Equals(keyCandidate.plaintext))
            {
                return true;
            }

            return false;
        }
    }

    class KeyCandidateComparer : IComparer<KeyCandidate>
    {
        public int Compare(KeyCandidate a, KeyCandidate b)
        {
            return -a.Fitness.CompareTo(b.Fitness);
        }
    }
}
