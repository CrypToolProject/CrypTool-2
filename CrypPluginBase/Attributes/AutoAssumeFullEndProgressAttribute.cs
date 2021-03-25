using System;

namespace CrypTool.PluginBase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoAssumeFullEndProgressAttribute : Attribute
    {
        public bool AutoProgressChanged { get; private set; }

        public AutoAssumeFullEndProgressAttribute(bool autoProgressChanged)
        {
            AutoProgressChanged = autoProgressChanged;
        }
    }
}
