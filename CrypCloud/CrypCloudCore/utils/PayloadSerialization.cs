using System;
using System.IO;
using System.IO.Compression;
using WorkspaceManager.Model;

namespace CrypCloud.Core.utils
{
    public static class PayloadSerialization
    {
        public static byte[] Serialize(WorkspaceModel workspaceModel)
        {
            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream))
            {
                var persistantModel = new ModelPersistance().GetPersistantModel(workspaceModel);
                XMLSerialization.XMLSerialization.Serialize(persistantModel, streamWriter, true);
                var serialize = Compress(stream.ToArray());
                return serialize;
            }
        } 

        public static WorkspaceModel Deserialize(byte[] data)
        {
            var decompress = Decompress(data);
            using (var stream = new MemoryStream())
            {
                stream.Write(decompress, 0, decompress.Length);
                using (var streamWriter = new StreamWriter(stream))
                {
                    return new ModelPersistance().loadModel(streamWriter);
                }
            }
        }

        private static int BUFFER_SIZE = 64 * 1024; //64kB
        private static byte[] Compress(byte[] inputData)
        {
            if (inputData == null)
                throw new ArgumentNullException("inputData must be non-null");

            using (var compressIntoMs = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(compressIntoMs,
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
                throw new ArgumentNullException("inputData must be non-null");

            using (var compressedMs = new MemoryStream(inputData))
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(compressedMs,
                     CompressionMode.Decompress), BUFFER_SIZE))
                    {
                        gzs.CopyTo(decompressedMs);
                    }
                    return decompressedMs.ToArray();
                }
            }
        }

    }
}
