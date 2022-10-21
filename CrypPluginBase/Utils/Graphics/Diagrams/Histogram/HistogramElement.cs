/*
   Copyright 2008 - 2022 CrypTool Team

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

namespace CrypTool.PluginBase.Utils.Graphics.Diagrams.Histogram
{
    public class HistogramElement
    {
        private string caption;
        private double normalizedValue;
        private double absoluteValue;

        public HistogramElement(double absoluteVal, double normalizedVal, string caption)
        {
            absoluteValue = absoluteVal;
            this.caption = caption;
            normalizedValue = normalizedVal;
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
        public double Percent
        {
            get => normalizedValue;
            set => normalizedValue = value;
        }

        /// <summary>
        /// The absolute value, used for the absolute heigth of the bar
        /// </summary>
        public double Amount
        {
            get => absoluteValue;
            set => absoluteValue = value;
        }
    }
}
