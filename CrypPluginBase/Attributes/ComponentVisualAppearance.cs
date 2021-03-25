using System;

namespace CrypTool.PluginBase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentVisualAppearance : Attribute
    {
        public enum VisualAppearanceEnum { Closed, Opened };

        public VisualAppearanceEnum DefaultVisualAppearance;

        public ComponentVisualAppearance(VisualAppearanceEnum defaultVisualAppearance)
        {
            DefaultVisualAppearance = defaultVisualAppearance;
        }
    }
}
