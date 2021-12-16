/*  
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
using System.Windows;
using System.Windows.Media;

namespace CrypTool.FrequencyTest
{
    public class CollectionElement
    {
        private string caption;
        private int absoluteValue;
        private double percentageValue;
        private double height;
        private readonly bool showAbsoluteValues = false;
        private Visibility visibility;
        private Color colorA = Colors.Turquoise;
        private Color colorB = Colors.DarkBlue;

        public CollectionElement(double height, int absoluteValue, double percentageValue, string caption, bool showAbsoluteValues, Visibility visibility = Visibility.Visible)
        {
            this.height = height;
            this.caption = caption;
            this.absoluteValue = absoluteValue;
            this.percentageValue = percentageValue;
            this.showAbsoluteValues = showAbsoluteValues;
            this.visibility = visibility;
        }

        /// <summary>
        /// The caption to appear under the bar
        /// </summary>
        public string Caption
        {
            get => caption;
            set => caption = value;
        }

        /// <summary>
        /// The value to be written on top of the bar, usually the percentage value
        /// </summary>
        public string BarHeadValue
        {
            get
            {
                if (showAbsoluteValues)
                {
                    if (AbsoluteValue == 0)
                    {
                        return string.Empty;
                    }
                    return AbsoluteValue.ToString();
                }
                else
                {
                    if (PercentageValue == 0)
                    {
                        return string.Empty;
                    }
                    return PercentageValue.ToString();
                }
            }
        }

        /// <summary>
        /// The absolute value
        /// </summary>
        public int AbsoluteValue
        {
            get => absoluteValue;
            set => absoluteValue = value;
        }

        /// <summary>
        /// The percentage value
        /// </summary>
        public double PercentageValue
        {
            get => percentageValue;
            set => percentageValue = value;
        }

        /// <summary>
        /// Height of the bar
        /// </summary>
        public double Height
        {
            get => height;
            set => height = value;
        }

        public Visibility Visibility
        {
            get => visibility;
            set => visibility = value;
        }

        public Color ColorA
        {
            get => colorA;
            set => colorA = value;
        }

        public Color ColorB
        {
            get => colorB;
            set => colorB = value;
        }
    }
}
