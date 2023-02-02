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
        private Random Randomizer = new Random();

        public LugType[] Bar = new LugType[27]
        {
            new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), 
            new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), 
            new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0), new LugType(0)
        };

        public int Length { get { return Bar.Length; }}

        /// <summary>
        /// Generate a random setting for the lugs.
        /// </summary>
        public void Randomize()
        {
            for (int i = 0; i < this.Bar.Length; i++)
            {
                this.Bar[i].SetPosition(this.Randomizer.Next(0, 20));
            }
        }

        public string ToStringRepresentation()
        {
            string stringRepresentation = "";
            for (int i = 0; i < Bar.Length; i++)
            {
                stringRepresentation += (Bar[i].Value[0] + 1).ToString() + "-" + (Bar[i].Value[1] + 1).ToString() + ";";
            }
            return stringRepresentation;
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

        public bool IsValid()
        {
            // TODO
            return true;
        }

        /// <summary>
        /// These simple transformations consist of reducing the count of one type of bars, and increasing the count of another type.
        /// On on bar a lug position gets increased on another bar a lug position gets decreased.
        /// </summary>
        public void ApplySimpleTransformation(int bar1 = 0, int bar2 = 0)
        {
            // the changes have to be on different bars
            if (bar1 == bar2)
            {
                return;
            }
            // increase lug type
            this.Bar[bar1].Increase();
            this.Bar[bar2].Decrease();
        }

        /// <summary>
        /// Those consist of reducing the count of two types of bar, and increasing the counts of two other types.
        /// </summary>
        public void ApplyComplexTransformation(int bar1, int bar2, int bar3, int bar4, int nr)
        {
            switch (nr)
            {
                case 0:
                    this.Bar[bar1].Increase();
                    this.Bar[bar2].Increase();
                    this.Bar[bar3].Decrease();
                    this.Bar[bar4].Decrease();
                    break;
                case 1:
                    this.Bar[bar1].Increase();
                    this.Bar[bar2].Decrease();
                    this.Bar[bar3].Increase();
                    this.Bar[bar4].Decrease();
                    break;
                case 2:
                    this.Bar[bar1].Increase();
                    this.Bar[bar2].Decrease();
                    this.Bar[bar3].Decrease();
                    this.Bar[bar4].Increase();
                    break;
                case 3:
                    this.Bar[bar1].Decrease();
                    this.Bar[bar2].Increase();
                    this.Bar[bar3].Increase();
                    this.Bar[bar4].Decrease();
                    break;
                case 4:
                    this.Bar[bar1].Decrease();
                    this.Bar[bar2].Increase();
                    this.Bar[bar3].Decrease();
                    this.Bar[bar4].Increase();
                    break;
                case 5:
                    this.Bar[bar1].Decrease();
                    this.Bar[bar2].Decrease();
                    this.Bar[bar3].Increase();
                    this.Bar[bar4].Increase();
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// There are only 21 possible settings for the lugs on a bar. They are called LugTypes.
    /// </summary>
    internal class LugType
    {
        public LugType(int pos)
        {
            _pos = pos;
        }

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

        /// <summary>
        /// Parse an arbitrary lug setting into an unique lugType
        /// </summary>
        /// <param name="lugSetting">Arbitrary lug setting</param>
        private void parseLugSetting(int[] lugSetting)
        {
            if (lugSetting[0] == lugSetting[1])
            {
                // -1 means ineffective 
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

        public void SetPosition(int pos)
        {
            this._pos = pos;
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
    }
}
