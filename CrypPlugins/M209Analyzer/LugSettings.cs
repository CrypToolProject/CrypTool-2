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

                this.Bar[i].Value = new int[] { lug1Position, lug2Position };
            }

        }

        public LugType[] Bar = new LugType[27]
        {
            new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), 
            new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), 
            new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0)
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
        /// LugCount of w is the number of lugs set in front of wheel w. Wheels are numbered 1 to 6
        /// </summary>
        public int GetLugCount(int w)
        {
            int lugCount = 0;
            
            // Because we want to address the wheel on position 0 with 1.
            w = w - 1;

            for (int i = 0; i < this.Bar.Length; i++)
            {
                if (this.Bar[i].Value[0] == w | this.Bar[i].Value[1] == w)
                {
                    lugCount++;
                }                                   
            }
            return lugCount;
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

    internal class LugType
    {
        public LugType(int pos)
        {
            _pos = pos;
        }
        private Random Randomizer = new Random();

        private int _pos = 0;

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

            for (int i = 0; i < this._possibleLugSettings.Length; i++)
            {
                if (lugSetting[0] == this._possibleLugSettings[i][0] && lugSetting[1] == this._possibleLugSettings[i][1])
                {
                    this._pos = i;
                }
            }
        }

        public int[] Value
        {
            get
            {
                return this._possibleLugSettings[_pos];
            }
            set 
            { 
                parseLugSetting(value); 
            }
        }

        public void Increase()
        {
            if(this._pos < this._possibleLugSettings.Length - 1)
            {
                this._pos++;
            } else
            {
                this._pos = 0;
            }
        }

        public void Decrease()
        {
            if (this._pos > 0)
            {
                this._pos--;
            } else
            {
                this._pos = 20;
            }
        }

        public void Randomize()
        {
            this._pos = this.Randomizer.Next(0, 20);
        }
    }
}
