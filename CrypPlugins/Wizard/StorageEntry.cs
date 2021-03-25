using System;

namespace Wizard
{
    [Serializable]
    public class StorageEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }

        public DateTime Created { get; set; }

        public StorageEntry(string key, string value, string description)
        {
            Key = key;
            Value = value;
            Description = description;
            Created = DateTime.Now;
        }
    }
}
