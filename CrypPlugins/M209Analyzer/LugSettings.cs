using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M209Analyzer
{
    internal class LugSettings
    {
        public LugSettings(string[] lugSettingString)
        {
            for (int i = 0; i < lugSettingString.Length; i++)
            {
                int lug1Position = int.Parse(lugSettingString[i][0].ToString());
                int lug2Position = int.Parse(lugSettingString[i][1].ToString());

                // -1 means ineffective 
                lug1Position -= 1;
                lug2Position -= 1;

                // some normalization for LugCount
                if(lug1Position > lug2Position)
                {
                    int tmpLug = lug1Position;
                    lug1Position = lug2Position;
                    lug2Position = tmpLug;
                }

                Bar[i].Value = new int[] { lug1Position, lug2Position };
                Console.WriteLine($"{lugSettingString[i][0]}-{lugSettingString[i][1]} => {Bar[i].Value[0]}-{Bar[i].Value[1]}");
            }

        }

        public LugCount[] Bar = new LugCount[27]
        {
            new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), 
            new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), 
            new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0), new LugCount(0)
        };

        public int Length { get { return Bar.Length; }}

        private Random Randomizer = new Random();

        /// <summary>
        /// Generate a random setting for the lugs.
        /// </summary>
        public void Randomize()
        {
            for (int i = 0; i < this.Bar.Length; i++)
            {
                this.Bar[i].Randomize();
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
            return new LugSetting[] { new LugSetting() };
        }

        /// <summary>
        /// These simple transformations consist of reducing the count of one type of bars, and increasing the count of another type.
        /// On on bar a lug position gets increased on another bar a lug position gets decreased.
        /// </summary>
        public void ApplyTransformationSimple()
        {
            // Choose random bar and increase lug count
            int ChoosenBar1 = this.Randomizer.Next(0, 26);
            this.Bar[ChoosenBar1].Increase();

            // Choose random but different bar and increase lug count
            int ChoosenBar2 = this.Randomizer.Next(0, 26);
            while (ChoosenBar1 == ChoosenBar2)
            {
                ChoosenBar2 = this.Randomizer.Next(0, 26);
            }
            this.Bar[ChoosenBar2].Decrease();
        }

        /// <summary>
        /// Those consist of reducing the count of two types of bar, and increasing the counts of two other types.
        /// </summary>
        public void ApplyTransformationComplex()
        {
            // Choose random bar and increase lug count
            int ChoosenBar1 = this.Randomizer.Next(0, 26);
            this.Bar[ChoosenBar1].Increase();

            // Choose random but different bar and increase lug count
            int ChoosenBar2 = this.Randomizer.Next(0, 26);
            while (ChoosenBar1 == ChoosenBar2)
            {
                ChoosenBar2 = this.Randomizer.Next(0, 26);
            }
            this.Bar[ChoosenBar2].Increase();

            // Choose random but different bar and decrease lug count
            int ChoosenBar3 = this.Randomizer.Next(0, 26);
            while (ChoosenBar1 == ChoosenBar3 || ChoosenBar2 == ChoosenBar3)
            {
                ChoosenBar3 = this.Randomizer.Next(0, 26);
            }
            this.Bar[ChoosenBar3].Decrease();

            // Choose random but different bar and decrease lug count
            int ChoosenBar4 = this.Randomizer.Next(0, 26);
            while (ChoosenBar1 == ChoosenBar4 || ChoosenBar2 == ChoosenBar4 || ChoosenBar3 == ChoosenBar4)
            {
                ChoosenBar4 = this.Randomizer.Next(0, 26);
            }
            this.Bar[ChoosenBar4].Decrease();
        }
    }

    internal class LugCount
    {
        public LugCount(int count)
        {
            _count = count;
        }
        private Random Randomizer = new Random();

        private int _count = 0;

        private int[][] _possibleLugSettings = new int[21][]
        {
            new int[]{-1, 0},
            new int[]{-1, 1},
            new int[]{-1, 2},
            new int[]{-1, 3},
            new int[]{-1, 4},
            new int[]{-1, 5},
            new int[]{0, 1},
            new int[]{0, 2},
            new int[]{0, 3},
            new int[]{0, 4},
            new int[]{0, 5},
            new int[]{1, 2},
            new int[]{1, 3},
            new int[]{1, 4},
            new int[]{1, 5},
            new int[]{2, 3},
            new int[]{2, 4},
            new int[]{2, 5},
            new int[]{3, 4},
            new int[]{3, 5},
            new int[]{4, 5}
        };

        private void parseLugSetting(int[] lugSetting)
        {
            if (lugSetting[0] == lugSetting[1])
            {
                lugSetting[0] = -1;
            }

            for (int i = 0; i < _possibleLugSettings.Length; i++)
            {
                if (lugSetting[0] == _possibleLugSettings[i][0] && lugSetting[1] == _possibleLugSettings[i][1])
                {
                    _count = i;
                }
            }
        }

        public int[] Value
        {
            get
            {
                return _possibleLugSettings[_count];
            }
            set 
            { 
                parseLugSetting(value); 
            }
        }

        public void Increase()
        {
            if(_count < _possibleLugSettings.Length - 1)
            {
                _count++;
            } else
            {
                _count = 0;
            }
        }

        public void Decrease()
        {
            if (_count > 0)
            {
                _count--;
            } else
            {
                _count = 20;
            }
        }

        public void Randomize()
        {
            _count = this.Randomizer.Next(0, 20);
        }
    }
}
