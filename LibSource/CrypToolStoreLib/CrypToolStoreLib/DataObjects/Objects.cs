/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypToolStoreLib.Tools;
using System;
using System.IO;

namespace CrypToolStoreLib.DataObjects
{
    /// <summary>
    /// Buildstate for build process of sources
    /// </summary>
    public enum BuildState
    {
        CREATED = 0,
        UPLOADED = 1,
        BUILDING = 2,
        SUCCESS = 3,
        ERROR = 4
    }

    /// <summary>
    /// Publishstate for builds of sources
    /// </summary>
    public enum PublishState
    {
        NOTPUBLISHED = 0,
        DEVELOPER = 1,
        NIGHTLY = 2,
        BETA = 3,
        RELEASE = 4
    }

    /// <summary>
    /// Simple object to store developer data
    /// </summary>
    public class Developer : ICrypToolStoreSerializable
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Developer()
        {
            Username = string.Empty;
            Password = string.Empty;
            Firstname = string.Empty;
            Lastname = string.Empty;
            Email = string.Empty;
            IsAdmin = false;
        }

        public override string ToString()
        {
            return string.Format("Developer{{username={0}, firstname={1}, lastname={2}, email={3}, isadmin={4}}}", Username, Firstname, Lastname, Email, (IsAdmin ? "true" : "false"));
        }

        /// <summary>
        /// Serializes this developer into a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(Username);
                        writer.Write(Password);
                        writer.Write(Firstname);
                        writer.Write(Lastname);
                        writer.Write(Email);
                        writer.Write(IsAdmin);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Exception during serialization of developer: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Deserializes a developer from the byte array
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        Username = reader.ReadString();
                        Password = reader.ReadString();
                        Firstname = reader.ReadString();
                        Lastname = reader.ReadString();
                        Email = reader.ReadString();
                        IsAdmin = reader.ReadBoolean();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DeserializationException(string.Format("Exception during deserialization of developer: {0}", ex.Message));
            }
        }
    }

    /// <summary>
    /// Simple object to store plugin data
    /// </summary>
    public class Plugin : ICrypToolStoreSerializable
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string Authornames { get; set; }
        public string Authorinstitutes { get; set; }
        public string Authoremails { get; set; }
        public byte[] Icon { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Plugin()
        {
            Id = 0;
            Username = string.Empty;
            Name = string.Empty;
            ShortDescription = string.Empty;
            LongDescription = string.Empty;
            Authornames = string.Empty;
            Authorinstitutes = string.Empty;
            Authoremails = string.Empty;
            Icon = new byte[0];
        }

        /// <summary>
        /// Serializes this plugin into a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(Id);
                        writer.Write(Username);
                        writer.Write(Name);
                        writer.Write(ShortDescription);
                        writer.Write(LongDescription);
                        writer.Write(Authornames);
                        writer.Write(Authorinstitutes);
                        writer.Write(Authoremails);
                        writer.Write(Icon.Length); //first write length of byte array
                        writer.Write(Icon);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Exception during serialization of plugin: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Deserializes a plugin from the byte array
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        Id = reader.ReadInt32();
                        Username = reader.ReadString();
                        Name = reader.ReadString();
                        ShortDescription = reader.ReadString();
                        LongDescription = reader.ReadString();
                        Authornames = reader.ReadString();
                        Authorinstitutes = reader.ReadString();
                        Authoremails = reader.ReadString();
                        int length = reader.ReadInt32(); //first read length of byte array
                        Icon = reader.ReadBytes(length);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DeserializationException(string.Format("Exception during deserialization of plugin: {0}", ex.Message));
            }
        }

        public override string ToString()
        {
            return string.Format("Plugin{{id={0}, username={1}, name={2}, shortdescription={3}, longdescription={4}, authornames={5}, authoremails={6}, authorinstitutes={7}, icon={8}}}",
                Id, Username, Name, ShortDescription, LongDescription, Authornames, Authoremails, Authorinstitutes, Icon != null ? Icon.Length.ToString() : "null");
        }
    }

    /// <summary>
    /// Simple object to store source data
    /// </summary>
    public class Source : ICrypToolStoreSerializable
    {
        public int PluginId { get; set; }
        public int PluginVersion { get; set; }
        public int BuildVersion { get; set; }
        public string ZipFileName { get; set; }
        public string BuildState { get; set; }
        public string BuildLog { get; set; }
        public string AssemblyFileName { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime BuildDate { get; set; }
        public string PublishState { get; set; }

        public bool HasZipFile => !ZipFileName.Equals(string.Empty);

        public bool HasAssemblyFile => !AssemblyFileName.Equals(string.Empty);

        /// <summary>
        /// Default constructor
        /// </summary>
        public Source()
        {
            PluginId = -1;
            PluginVersion = -1;
            BuildVersion = -1;
            ZipFileName = string.Empty;
            BuildState = string.Empty;
            BuildLog = string.Empty;
            AssemblyFileName = string.Empty;
            UploadDate = DateTime.MinValue;
            BuildDate = DateTime.MinValue;
            PublishState = CrypToolStoreLib.DataObjects.PublishState.NOTPUBLISHED.ToString();
        }

        /// <summary>
        /// Serializes this source into a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(PluginId);
                        writer.Write(PluginVersion);
                        writer.Write(BuildVersion);
                        writer.Write(ZipFileName);
                        writer.Write(BuildState);
                        writer.Write(BuildLog);
                        writer.Write(AssemblyFileName);
                        writer.Write(UploadDate.ToBinary());
                        writer.Write(BuildDate.ToBinary());
                        writer.Write(PublishState);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Exception during serialization of source: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Deserializes a source from the byte array
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        PluginId = reader.ReadInt32();
                        PluginVersion = reader.ReadInt32();
                        BuildVersion = reader.ReadInt32();
                        ZipFileName = reader.ReadString();
                        BuildState = reader.ReadString();
                        BuildLog = reader.ReadString();
                        AssemblyFileName = reader.ReadString();
                        UploadDate = DateTime.FromBinary(reader.ReadInt64());
                        BuildDate = DateTime.FromBinary(reader.ReadInt64());
                        PublishState = reader.ReadString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DeserializationException(string.Format("Exception during deserialization of source: {0}", ex.Message));
            }
        }

        public override string ToString()
        {
            return string.Format("Source{{pluginid={0}, pluginversion={1}, buildversion={2}, zipfile={3},buildstate={4}, buildlog={5}, assembly={6}, uploaddate={7}, builddate={8}, publishstate={9}}}",
                PluginId, PluginVersion, BuildVersion, ZipFileName != null ? ZipFileName.Length.ToString() : "null", BuildState, BuildLog, AssemblyFileName != null ? AssemblyFileName.Length.ToString() : "null", UploadDate, BuildDate, PublishState);
        }
    }

    /// <summary>
    /// Simple object to store one plugin and one source
    /// </summary>
    public class PluginAndSource : ICrypToolStoreSerializable
    {

        public Plugin Plugin { get; set; }
        public Source Source { get; set; }
        public long FileSize { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PluginAndSource()
        {
            Plugin = new Plugin();
            Source = new Source();
            FileSize = 0;
        }

        /// <summary>
        /// Serializes this pluginsource into a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        //1. serialize plugin
                        byte[] pluginbytes = Plugin.Serialize();
                        writer.Write(pluginbytes.Length);
                        writer.Write(pluginbytes);

                        //2. serialize source
                        byte[] sourcebytes = Source.Serialize();
                        writer.Write(sourcebytes.Length);
                        writer.Write(sourcebytes);

                        //3. serialize file size of assembly zip
                        writer.Write(FileSize);

                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Exception during serialization of pluginsource: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Deserializes a pluginsource from the byte array
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        //1. deserialize plugin
                        int pluginbyteslength = reader.ReadInt32();
                        byte[] pluginbytes = new byte[pluginbyteslength];
                        reader.Read(pluginbytes, 0, pluginbyteslength);
                        Plugin.Deserialize(pluginbytes);

                        //2. deserialize source
                        int sourcebyteslength = reader.ReadInt32();
                        byte[] sourcebytes = new byte[sourcebyteslength];
                        reader.Read(sourcebytes, 0, sourcebyteslength);
                        Source.Deserialize(sourcebytes);

                        //3. deserialize files ize of assembly zip
                        FileSize = reader.ReadInt64();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DeserializationException(string.Format("Exception during deserialization of pluginsource: {0}", ex.Message));
            }
        }

        public override string ToString()
        {
            return string.Format("PluginAndSource{{Plugin{{{0}}}, Source{{{1}}}}}", Plugin.ToString(), Source.ToString());
        }
    }


    /// <summary>
    /// Simple object to store resource data
    /// </summary>
    public class Resource : ICrypToolStoreSerializable
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Resource()
        {
            Id = -1;
            Username = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
        }

        /// <summary>
        /// Serializes this resource into a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(Id);
                        writer.Write(Username);
                        writer.Write(Name);
                        writer.Write(Description);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Exception during serialization of resource: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Deserializes a resource from the byte array
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        Id = reader.ReadInt32();
                        Username = reader.ReadString();
                        Name = reader.ReadString();
                        Description = reader.ReadString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DeserializationException(string.Format("Exception during deserialization of resource: {0}", ex.Message));
            }
        }

        public override string ToString()
        {
            return string.Format("Resource{{id={0}, username={1}, name={2}, description={3}}}", Id, Username, Name, Description);
        }
    }

    /// <summary>
    /// Simple object to store resourceData data
    /// </summary>
    public class ResourceData : ICrypToolStoreSerializable
    {
        public int ResourceId { get; set; }
        public int ResourceVersion { get; set; }
        public string DataFilename { get; set; }
        public DateTime UploadDate { get; set; }
        public string PublishState { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ResourceData()
        {
            ResourceId = -1;
            ResourceVersion = -1;
            DataFilename = string.Empty;
            UploadDate = DateTime.MinValue;
            PublishState = CrypToolStoreLib.DataObjects.PublishState.NOTPUBLISHED.ToString();
        }

        /// <summary>
        /// Returns true if this ResourceData contains any Data
        /// </summary>
        /// <returns></returns>
        public bool HasData => !string.IsNullOrEmpty(DataFilename);

        /// <summary>
        /// Serializes this resource data into a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(ResourceId);
                        writer.Write(ResourceVersion);
                        writer.Write(DataFilename);
                        writer.Write(UploadDate.ToBinary());
                        writer.Write(PublishState);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Exception during serialization of resource data: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Deserializes a resource data from the byte array
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        ResourceId = reader.ReadInt32();
                        ResourceVersion = reader.ReadInt32();
                        DataFilename = reader.ReadString();
                        UploadDate = DateTime.FromBinary(reader.ReadInt64());
                        PublishState = reader.ReadString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DeserializationException(string.Format("Exception during deserialization of resource data: {0}", ex.Message));
            }
        }

        public override string ToString()
        {
            return string.Format("ResourceData{{resourceid={0}, version={1}, datafilename={2}, uploadtime={3}, publishstate={4}}}",
                ResourceId, ResourceVersion, DataFilename, UploadDate, PublishState);
        }
    }

    /// <summary>
    /// Simple object to store one resource and resourcedata
    /// </summary>
    public class ResourceAndResourceData : ICrypToolStoreSerializable
    {
        public Resource Resource { get; set; }
        public ResourceData ResourceData { get; set; }
        public long FileSize { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ResourceAndResourceData()
        {
            Resource = new Resource();
            ResourceData = new ResourceData();
        }

        /// <summary>
        /// Serializes resourceandresourcedata into a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        //1. serialize resource
                        byte[] resourcebytes = Resource.Serialize();
                        writer.Write(resourcebytes.Length);
                        writer.Write(resourcebytes);

                        //2. serialize resourcedata
                        byte[] resourcedatabytes = ResourceData.Serialize();
                        writer.Write(resourcedatabytes.Length);
                        writer.Write(resourcedatabytes);

                        //3. serialize file size
                        writer.Write(FileSize);

                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Exception during serialization of resourceandrosourcedata: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Deserializes a resourceandresourcedata from the byte array
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        //1. deserialize resource
                        int resourcebyteslength = reader.ReadInt32();
                        byte[] resoucebytes = new byte[resourcebyteslength];
                        reader.Read(resoucebytes, 0, resourcebyteslength);
                        Resource.Deserialize(resoucebytes);

                        //2. deserialize resourcedata
                        int resourcedatabyteslength = reader.ReadInt32();
                        byte[] resourcedatabytes = new byte[resourcedatabyteslength];
                        reader.Read(resourcedatabytes, 0, resourcedatabyteslength);
                        ResourceData.Deserialize(resourcedatabytes);

                        //3. deserialize filesize
                        try
                        {
                            if (reader.BaseStream.Position < reader.BaseStream.Length)
                            {
                                FileSize = reader.ReadInt64();
                            }
                        }
                        catch (Exception)
                        {
                            //old versions do not have a file size, thus, it could crash here
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DeserializationException(string.Format("Exception during deserialization of resourceandrosourcedata: {0}", ex.Message));
            }
        }

        public override string ToString()
        {
            return string.Format("ResourceAndResourceData{{Resource{{{0}}}, ResourceData{{{1}}}}}", Resource.ToString(), ResourceData.ToString());
        }
    }

    /// <summary>
    /// A PasswordTry memorizes the number of username/password tries and the last time of the last try
    /// </summary>
    public class PasswordTry
    {
        public int Number { get; set; }
        public DateTime LastTryDateTime { get; set; }
    }

    /// <summary>
    /// A ModificationResult is returned by each method for modifying or requesting data 
    /// </summary>
    public class DataModificationOrRequestResult
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public object DataObject { get; set; }
    }
}
