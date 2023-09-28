using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.Plugins.HomophonicAnalyzer
{
    public class KeyCandidate : IEquatable<KeyCandidate>
    {
        private int[] key;
        private double fitness;
        private String plaintext;
        private String key_string;
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
        
        public Boolean HillAttack
        {
            get { return this.hillAttack; }
            set { this.hillAttack = value; }
        }

        public KeyCandidate(int[] key, double fitness, String plaintext, String key_string)
        {
            this.key = (int[])key.Clone();
            this.fitness = fitness;
            this.plaintext = plaintext;
            this.key_string = key_string;
        }

        public bool Equals(KeyCandidate keyCandidate)
        {
            return this.plaintext.Equals(keyCandidate.plaintext);

           /* if (keyCandidate == null)
            {
                return false;
            }

            if (this.key.Length != keyCandidate.key.Length)
            {
                return false;
            }

            for (int i = 0; i < this.key.Length; i++)
            {
                if (this.key[i] != keyCandidate.key[i])
                {
                    return false;
                }
            }

            return true;*/
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
