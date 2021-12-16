using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using WorkspaceManager.Model;

namespace CrypCloud.Core.utils
{
    public class JobPayload
    {
        private const int BUFFER_SIZE = 64 * 1024; //64kB

        public WorkspaceModel WorkspaceModel;
        public DateTime CreationTime;

        public byte[] Serialize()
        {
            byte[] timestamp = BitConverter.GetBytes(CreationTime.Ticks);
            byte[] serializeWorkspace = SerializeWorkspace();
            byte[] serialize = timestamp.Concat(serializeWorkspace).ToArray();
            return Compress(serialize);
        }

        public JobPayload Deserialize(byte[] data)
        {
            byte[] decompress = Decompress(data);
            byte[] bytesOfTimestamp = decompress.Take(sizeof(long)).ToArray();
            byte[] bytesOfWorkspace = decompress.Skip(sizeof(long)).ToArray();

            WorkspaceModel = DeserializeWorkspace(bytesOfWorkspace);
            CreationTime = new DateTime(BitConverter.ToInt64(bytesOfTimestamp, 0));

            return this;
        }

        #region workspace

        private byte[] SerializeWorkspace()
        {
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter streamWriter = new StreamWriter(stream))
            {
                PersistantModel persistantModel = new ModelPersistance().GetPersistantModel(WorkspaceModel);
                XMLSerialization.XMLSerialization.Serialize(persistantModel, streamWriter, true);
                return stream.ToArray();
            }
        }

        private static WorkspaceModel DeserializeWorkspace(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(data, 0, data.Length);
                using (StreamWriter streamWriter = new StreamWriter(stream))
                {
                    return new ModelPersistance().loadModel(streamWriter);
                }
            }
        }

        #endregion

        #region zip

        private static byte[] Compress(byte[] inputData)
        {
            if (inputData == null)
            {
                throw new ArgumentNullException("inputData must be non-null");
            }

            using (MemoryStream compressIntoMs = new MemoryStream())
            {
                using (BufferedStream gzs = new BufferedStream(new GZipStream(compressIntoMs,
                    CompressionMode.Compress), BUFFER_SIZE))
                {
                    gzs.Write(inputData, 0, inputData.Length);
                }
                return compressIntoMs.ToArray();
            }
        }

        private static byte[] Decompress(byte[] inputData)
        {
            if (inputData == null)
            {
                throw new ArgumentNullException("inputData must be non-null");
            }

            using (MemoryStream compressedMs = new MemoryStream(inputData))
            {
                using (MemoryStream decompressedMs = new MemoryStream())
                {
                    using (BufferedStream gzs = new BufferedStream(new GZipStream(compressedMs,
                        CompressionMode.Decompress), BUFFER_SIZE))
                    {
                        gzs.CopyTo(decompressedMs);
                    }
                    return decompressedMs.ToArray();
                }
            }
        }

        #endregion
    }
}
