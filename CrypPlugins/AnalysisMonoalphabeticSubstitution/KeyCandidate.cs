/*
   Copyright 2024 CrypTool 2 Team <ct2contact@CrypTool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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
