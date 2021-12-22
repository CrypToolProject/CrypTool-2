using System;

namespace CrypTool.PluginBase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoAssumeZeroBeginProgressAttribute : Attribute
    {
        public bool AutoProgressChanged { get; private set; }

        public AutoAssumeZeroBeginProgressAttribute(bool autoProgressChanged)
        {
            AutoProgressChanged = autoProgressChanged;
        }
    }
}
