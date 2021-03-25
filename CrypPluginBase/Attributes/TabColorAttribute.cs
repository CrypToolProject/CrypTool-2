using System;

namespace CrypTool.PluginBase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TabColorAttribute : Attribute
    {
        public string Brush { get; private set; }

        public TabColorAttribute(string brush)
        {
            Brush = brush;
        }
    }
}
