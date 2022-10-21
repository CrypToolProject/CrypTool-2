using CrypTool.PluginBase.Utils.Datatypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M209Analyzer
{
    internal class LugSetting
    {
        public string[] Bar = new string[27] {
            "36","06","16","15","45","04","04","04","04",
            "20","20","20","20","20","20","20","20","20",
            "20","25","25","05","05","05","05","05","05"
        };

        private Random Randomizer = new Random();

        /// <summary>
        /// Generate a random setting for the lugs.
        /// </summary>
        public void Randomize()
        {
            for(int i = 0; i < this.Bar.Length; i++)
            {
                this.Bar[i] = $"{this.Randomizer.Next(0, 9)}{this.Randomizer.Next(0, 9)}";
            }
        }

        /// <summary>
        /// Get neighbor lug settings.
        /// </summary>
        /// <param name="V">Operation instruction version</param>
        /// <returns></returns>
        public LugSetting[] GetNeighborLugs(string V)
        {
            // variable neighborhood approach
            return new LugSetting[] {new LugSetting() };
        }

        /// <summary>
        /// These simple transformations consist of reducing the count of one type of bars, and increasing the count of another type.
        /// On on bar a lug position gets increased on another bar a lug position gets decreased.
        /// </summary>
        public void ApplyTransformationSimple()
        {
            // Choose random bar and lug
            int ChoosenBar = this.Randomizer.Next(0, 26);
            int ChoosenLug = this.Randomizer.Next(0, 1);

            StringBuilder LugString = new StringBuilder(this.Bar[ChoosenBar]);
            
            int LugPosition = (int)Char.GetNumericValue(this.Bar[ChoosenBar][ChoosenLug]);
            if (LugPosition < 9)
            {
                LugString[ChoosenLug] = (char)(LugPosition + 1);
                this.Bar[ChoosenBar] = LugString.ToString();
            }

            // Choose another random bar and lug
            ChoosenBar = this.Randomizer.Next(0, 26);
            ChoosenLug = this.Randomizer.Next(0, 1);

            LugString = new StringBuilder(this.Bar[ChoosenBar]);

            LugPosition = (int)Char.GetNumericValue(this.Bar[ChoosenBar][ChoosenLug]);
            if (LugPosition > 0){
                LugString[ChoosenLug] = (char)(LugPosition - 1);
                this.Bar[ChoosenBar] = LugString.ToString();
            }
        }

        /// <summary>
        /// Those consist of reducing the count of two types of bar, and increasing the counts of two other types.
        /// </summary>
        public void ApplyTransformationComplex()
        {
            
        }
    }
}
