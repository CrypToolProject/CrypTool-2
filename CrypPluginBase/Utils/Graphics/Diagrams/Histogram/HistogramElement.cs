

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
