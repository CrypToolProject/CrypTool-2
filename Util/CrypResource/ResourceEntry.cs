using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.Resource
{
    [Serializable]
    public class ResourceEntry
    {
        private string original;
        private string translation;

        public string Original 
        { 
            get { return this.original;}
            set { this.original = value; }
        }

        public string Translation
        {
            get { return this.translation; }
            set { this.translation = value; }
        }

        public ResourceEntry()
        {

        }

        public ResourceEntry(string translation, string original)
        {
            this.translation = translation;
            this.original = original;
        }
    }
}
