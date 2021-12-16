using CrypTool.PluginBase;
using System;

namespace CrypTool.CrypWin.Helper
{
    [Serializable()]
    public abstract class StoredTab
    {
        public TabInfo Info { get; set; }

        protected StoredTab(TabInfo info)
        {
            Info = info;
        }
    }

    [Serializable()]
    internal class EditorTypeStoredTab : StoredTab
    {
        public Type EditorType { get; private set; }


        public EditorTypeStoredTab(TabInfo info, Type editorType)
            : base(info)
        {
            EditorType = editorType;
        }
    }

    [Serializable()]
    internal class CommonTypeStoredTab : StoredTab
    {
        public Type Type { get; private set; }

        public CommonTypeStoredTab(TabInfo info, Type type)
            : base(info)
        {
            Type = type;
        }
    }

    [Serializable()]
    internal class EditorFileStoredTab : StoredTab
    {
        public string Filename { get; private set; }

        public EditorFileStoredTab(TabInfo info, string filename) : base(info)
        {
            Filename = filename;
        }
    }
}
