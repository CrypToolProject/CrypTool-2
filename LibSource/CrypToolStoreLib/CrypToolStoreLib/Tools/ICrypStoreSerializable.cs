namespace CrypToolStoreLib.Tools
{
    /// <summary>
    /// Interface for serializable CrypToolStoreObjects
    /// </summary>
    public interface ICrypToolStoreSerializable
    {
        byte[] Serialize();
        void Deserialize(byte[] bytes);
    }
}
