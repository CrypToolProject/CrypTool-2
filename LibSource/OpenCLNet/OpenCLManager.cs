/*
 * Copyright (c) 2009 Olav Kalgraf(olav.kalgraf@gmail.com)
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace OpenCLNet
{
    /// <summary>
    /// OpenCLManager is a class that provides generic setup, compilation and caching services.
    /// </summary>
    public partial class OpenCLManager : IDisposable
    {
        #region Properties

        private bool disposed = false;

        private int _MaxCachedBinaries;

        /// <summary>
        /// FileSystem is an instance of the OCLManFileSystem class containing accessor
        /// methods to a file system.
        /// This property has a default implementation that uses normal .Net file access.
        /// However, if one requires OpenCLManager to access a virtual file system,
        /// like a .zip file, or similar. It is possible to subclass OCLManFileSystem
        /// and provide an instance of such an alternate file system implementation through
        /// this property.
        /// </summary>
        public OCLManFileSystem FileSystem { get; set; }

        /// <summary>
        /// True if OpenCL is available on this machine
        /// </summary>
        public bool OpenCLIsAvailable => OpenCL.NumberOfPlatforms > 0;

        /// <summary>
        /// Each element in this list is interpreted as the name of an extension.
        /// Any device that does not present this extension in its Extensions
        /// property will be filtered out during context creation.
        /// </summary>
        public List<string> RequiredExtensions = new List<string>();

        /// <summary>
        /// If true, OpenCLManager will filter out any devices that don't signal image support through the HasImageSupport property
        /// </summary>
        public bool RequireImageSupport { get; set; }

        /// <summary>
        /// If true, OpenCLManager will attempt to use stored binaries(Stored at 'BinaryPath') to avoid recompilation
        /// </summary>
        public bool AttemptUseBinaries { get; set; }

        /// <summary>
        /// The location to store and look for compiled binaries
        /// </summary>
        public string BinaryPath { get; set; }

        /// <summary>
        /// If true, OpenCLManager will attempt to compile sources(Stored at 'SourcePath') to compile programs, and possibly
        /// to store binaries(If 'AttemptUseBinaries' is true)
        /// </summary>
        public bool AttemptUseSource { get; set; }

        /// <summary>
        /// The location where sources are stored
        /// </summary>
        public string SourcePath { get; set; }

        public List<DeviceType> DeviceTypes = new List<DeviceType>();

        /// <summary>
        /// BuildOptions is passed to the OpenCL build functions that take compiler options
        /// </summary>
        public string BuildOptions { get; set; }

        /// <summary>
        /// This string is prepended verbatim to any and all sources that are compiled.
        /// It can contain any kind of useful global definitions.
        /// </summary>
        public string Defines { get; set; }
        public Platform Platform;
        public Context Context;

        /// <summary>
        /// Array of CommandQueues. Indices correspond to the devices in Context.Devices.
        /// Simple OpenCL programs will typically just enqueue operations on CQ[0] and ignore any additional devices.
        /// </summary>
        public CommandQueue[] CQ;

        /// <summary>
        /// The maximum number of entries in the binary cache. Default value = 50.
        /// Setting MaxCachedBinaries to a negative number disables cache trimming.
        /// 
        /// In general, it's ok to disable cache trimming if your OpenCL code is believed to be fairly static.
        /// For example if the sources are "user plugins" or in file form.
        /// However, if you do on-the-fly code generation, it should be set to a reasonable value to avoid excessive disk space consumption.
        /// </summary>
        public int MaxCachedBinaries
        {
            get => _MaxCachedBinaries;
            set => _MaxCachedBinaries = value;
        }

        #endregion

        #region Construction/Destruction

        public OpenCLManager()
        {
            DefaultProperties();
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~OpenCLManager()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

        #region IDisposable Members

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                if (CQ != null)
                {
                    foreach (CommandQueue cq in CQ)
                    {
                        //we had an error in CT2 concerning a null reference in this method
                        //this can be the only reference which could be null
                        //so we add the null check...
                        //Nils Kopal, 20.05.2014
                        if (cq != null)
                        {
                            cq.Dispose();
                        }
                    }
                    CQ = null;
                }

                if (Context != null)
                {
                    Context.Dispose();
                    Context = null;
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

        #endregion

        private void DefaultProperties()
        {
            MaxCachedBinaries = 50;
            FileSystem = new OCLManFileSystem();
            RequireImageSupport = false;
            BuildOptions = "";
            Defines = "";
            SourcePath = "OpenCL" + FileSystem.GetDirectorySeparator() + "src";
            BinaryPath = "OpenCL" + FileSystem.GetDirectorySeparator() + "bin";
            AttemptUseBinaries = true;
            AttemptUseSource = true;
        }

        /// <summary>
        /// Create a context containing all devices in the platform returned by OpenCL.GetPlatform(0) that satisfy the current RequireImageSupport and RequireExtensions property settings.
        /// Default command queues are made available through the CQ property
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="devices"></param>
        public void CreateDefaultContext()
        {
            CreateDefaultContext(0, DeviceType.ALL);
        }

        /// <summary>
        /// Create a context containing all devices of a given type that satisfy the
        /// current RequireImageSupport and RequireExtensions property settings.
        /// Default command queues are made available through the CQ property
        /// </summary>
        /// <param name="deviceType"></param>
        public void CreateDefaultContext(int platformNumber, DeviceType deviceType)
        {
            if (!OpenCLIsAvailable)
            {
                throw new OpenCLNotAvailableException();
            }

            Platform = OpenCL.GetPlatform(platformNumber);
            IEnumerable<Device> devices = from d in Platform.QueryDevices(deviceType)
                                          where ((RequireImageSupport && d.ImageSupport == true) || !RequireImageSupport) && d.HasExtensions(RequiredExtensions.ToArray<string>())
                                          select d;
            IntPtr[] properties = new IntPtr[]
            {
                (IntPtr)ContextProperties.PLATFORM, Platform,
                IntPtr.Zero
            };

            if (devices.Count() == 0)
            {
                throw new OpenCLException("CreateDefaultContext: No OpenCL devices found that matched filter criteria.");
            }

            CreateContext(Platform, properties, devices);
        }


        /// <summary>
        /// Create a context and initialize default command queues in the CQ property
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="devices"></param>
        public void CreateContext(Platform platform, IntPtr[] contextProperties, IEnumerable<Device> devices)
        {
            CreateContext(platform, contextProperties, devices, null, IntPtr.Zero);
        }

        /// <summary>
        /// Create a context and initialize default command queues in the CQ property
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="devices"></param>
        public void CreateContext(Platform platform, IntPtr[] contextProperties, IEnumerable<Device> devices, ContextNotify notify, IntPtr userData)
        {
            if (!OpenCLIsAvailable)
            {
                throw new OpenCLNotAvailableException();
            }

            Platform = platform;
            Context = platform.CreateContext(contextProperties, devices.ToArray<Device>(), notify, userData);
            CQ = new CommandQueue[Context.Devices.Length];
            for (int i = 0; i < Context.Devices.Length; i++)
            {
                CQ[i] = Context.CreateCommandQueue(Context.Devices[0]);
            }
        }

        /// <summary>
        /// CompileSource
        /// 
        /// Attempt to create a program from a source string and build it.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Program CompileSource(string source)
        {
            Program p;

            try
            {
                byte[][] binaries = LoadAllBinaries(Context, source, "");
                ErrorCode[] status = new ErrorCode[Context.Devices.Length];
                p = Context.CreateProgramWithBinary(Context.Devices, binaries, status);
                p.Build();
            }
            catch (Exception)
            {
                p = Context.CreateProgramWithSource(Defines + System.Environment.NewLine + source);
                p.Build(Context.Devices, BuildOptions, null, IntPtr.Zero);
                SaveAllBinaries(Context, source, "", p.Binaries);
            }
            return p;
        }

        /// <summary>
        /// CompileFile
        /// 
        /// Attempt to compile the file identified by fileName.
        /// If the AttemptUseBinaries property is true, the method will first check if an up-to-date precompiled binary exists.
        /// If it does, it will load the binary instead, if no binary exists, compilation will be performed and the resulting binaries saved.
        /// 
        /// If the AttemptUseBinaries property is false, only compilation will be attempted.
        /// 
        /// Caveat: If AttemptUseSource is false, no compilation will be attempted - ever.
        /// If both AttemptUseSource and AttemptUseBinaries are false this function will throw an exception.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Program CompileFile(string fileName)
        {
            string sourcePath = SourcePath + FileSystem.GetDirectorySeparator() + fileName;
            string binaryPath = BinaryPath + FileSystem.GetDirectorySeparator() + fileName;
            Program p;

            if (!FileSystem.Exists(sourcePath))
            {
                throw new FileNotFoundException(sourcePath);
            }

            if (AttemptUseBinaries && !AttemptUseSource)
            {
                byte[][] binaries = LoadAllBinaries(Context, "", fileName);
                ErrorCode[] status = new ErrorCode[Context.Devices.Length];
                p = Context.CreateProgramWithBinary(Context.Devices, binaries, status);
                p.Build();
                return p;
            }
            else if (!AttemptUseBinaries && AttemptUseSource)
            {
                string source = Defines + System.Environment.NewLine + File.ReadAllText(sourcePath);
                p = Context.CreateProgramWithSource(source);
                p.Build(Context.Devices, BuildOptions, null, IntPtr.Zero);
                SaveAllBinaries(Context, "", fileName, p.Binaries);
                return p;
            }
            else if (AttemptUseBinaries && AttemptUseSource)
            {
                try
                {
                    byte[][] binaries = LoadAllBinaries(Context, "", fileName);
                    ErrorCode[] status = new ErrorCode[Context.Devices.Length];
                    p = Context.CreateProgramWithBinary(Context.Devices, binaries, status);
                    p.Build();
                    return p;
                }
                catch (Exception)
                {
                    // Loading binaries failed for some reason. Attempt to compile instead.
                    string source = Defines + System.Environment.NewLine + File.ReadAllText(sourcePath);
                    p = Context.CreateProgramWithSource(source);
                    p.Build(Context.Devices, BuildOptions, null, IntPtr.Zero);
                    SaveAllBinaries(Context, "", fileName, p.Binaries);
                    return p;
                }
            }
            else
            {
                throw new OpenCLException("OpenCLManager has both AttemptUseBinaries and AttemptUseSource set to false, and therefore can't build Programs from files");
            }
        }

        protected void SaveDeviceBinary(Context context, string fileName, byte[][] binaries, string platformDirectoryName, Device device)
        {
            throw new NotImplementedException("SaveDeviceBinary not implemented");
        }

        protected void SaveAllBinaries(Context context, string source, string fileName, byte[][] binaries)
        {
            XmlSerializer xml = new XmlSerializer(typeof(BinaryMetaInfo));
            TestAndCreateDirectory(BinaryPath);
            using (BinaryMetaInfo bmi = BinaryMetaInfo.FromPath(BinaryPath, FileAccess.ReadWrite, FileShare.None))
            {
                for (int i = 0; i < context.Devices.Length; i++)
                {
                    Device device = context.Devices[i];
                    string binaryFileName;

                    MetaFile mf = bmi.FindMetaFile(source, fileName, context.Platform.Name, device.Name, device.DriverVersion, Defines, BuildOptions);
                    if (mf == null)
                    {
                        mf = bmi.CreateMetaFile(source, fileName, context.Platform.Name, device.Name, device.DriverVersion, Defines, BuildOptions);
                    }

                    binaryFileName = BinaryPath + FileSystem.GetDirectorySeparator() + mf.BinaryName;
                    FileSystem.WriteAllBytes(binaryFileName, binaries[i]);
                }
                bmi.TrimBinaryCache(FileSystem, MaxCachedBinaries);
                bmi.Save();
            }
        }

        protected byte[][] LoadAllBinaries(Context context, string source, string fileName)
        {
            string sourcePath = SourcePath + FileSystem.GetDirectorySeparator() + fileName;
            DateTime sourceDateTime = FileSystem.GetLastWriteTime(sourcePath);
            byte[][] binaries = new byte[context.Devices.Length][];

            if (!Directory.Exists(BinaryPath))
            {
                throw new DirectoryNotFoundException(BinaryPath);
            }

            using (BinaryMetaInfo bmi = BinaryMetaInfo.FromPath(BinaryPath, FileAccess.Read, FileShare.Read))
            {
                Device[] devices;

                devices = context.Devices;
                for (int i = 0; i < devices.Length; i++)
                {
                    if (binaries[i] != null)
                    {
                        continue;
                    }

                    Device device = devices[i];
                    string binaryFilePath;
                    MetaFile mf = bmi.FindMetaFile("", fileName, Context.Platform.Name, device.Name, device.DriverVersion, Defines, BuildOptions);
                    if (mf == null)
                    {
                        throw new FileNotFoundException("No compiled binary file present in MetaFile");
                    }

                    binaryFilePath = BinaryPath + FileSystem.GetDirectorySeparator() + mf.BinaryName;
                    if (AttemptUseSource)
                    {
                        // This exception will be caught inside the manager and cause recompilation
                        if (FileSystem.GetLastWriteTime(binaryFilePath) < sourceDateTime)
                        {
                            throw new Exception("Binary older than source");
                        }
                    }
                    binaries[i] = FileSystem.ReadAllBytes(binaryFilePath);

                    // Check of there are other identical devices that can use the binary we just loaded
                    // If there are, patch it in in the proper slots in the list of binaries
                    for (int j = i + 1; j < devices.Length; j++)
                    {
                        if (devices[i].Name == devices[j].Name &&
                            devices[i].Vendor == devices[j].Vendor &&
                            devices[i].Version == devices[j].Version &&
                            devices[i].AddressBits == devices[j].AddressBits &&
                            devices[i].DriverVersion == devices[j].DriverVersion &&
                            devices[i].EndianLittle == devices[j].EndianLittle)
                        {
                            binaries[j] = binaries[i];
                        }
                    }
                }
            }
            return binaries;
        }

        private void TestAndCreateDirectory(string path)
        {
            if (!FileSystem.DirectoryExists(path))
            {
                FileSystem.CreateDirectory(path);
            }
        }

        private void TestAndCreateFile(string path)
        {
            try
            {
                if (!FileSystem.Exists(path))
                {
                    FileStream fs = FileSystem.Open(path, FileMode.Create, FileAccess.ReadWrite);
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                if (!FileSystem.Exists(path))
                {
                    throw e;
                }
            }
        }

        private string CreateRandomDirectory(string path)
        {
            int tries = 0;

            while (true)
            {
                string randomFileName = path + FileSystem.GetDirectorySeparator() + FileSystem.GetRandomFileName();
                try
                {
                    if (!FileSystem.DirectoryExists(randomFileName))
                    {
                        FileSystem.CreateDirectory(randomFileName);
                    }

                    return randomFileName;
                }
                catch (IOException e)
                {
                    if (tries++ > 50)
                    {
                        throw e;
                    }
                    if (!FileSystem.DirectoryExists(randomFileName))
                    {
                        throw e;
                    }
                }
            }
        }

        private string CreateRandomFile(string path)
        {
            int tries = 0;

            while (true)
            {
                string randomFileName = path + FileSystem.GetDirectorySeparator() + FileSystem.GetRandomFileName();
                try
                {
                    if (!FileSystem.Exists(randomFileName))
                    {
                        FileStream fs = FileSystem.Open(randomFileName, FileMode.CreateNew, FileAccess.ReadWrite);
                        fs.Close();
                        return randomFileName;
                    }
                }
                catch (IOException e)
                {
                    if (tries++ > 50)
                    {
                        throw e;
                    }

                    if (!FileSystem.Exists(randomFileName))
                    {
                        throw e;
                    }
                }
            }
        }
    }

    #region OCLManFileSystem

    public class OCLManFileSystem
    {
        /// <summary>
        /// Return true if this file system is read only.
        /// The implied consequences of this function returning true are that CreateDirectory
        /// and WriteAll functions won't work
        /// </summary>
        /// <returns></returns>
        public virtual bool IsReadOnly()
        {
            return false;
        }

        /// <summary>
        /// Returns the root of the filesystem, say "/", "c:\" or something.
        /// "" means the root is the current directory in the default implementation.
        /// </summary>
        public virtual string GetRoot()
        {
            return "";
        }

        /// <summary>
        /// Returns the directory separator character 
        /// </summary>
        /// <returns></returns>
        public virtual char GetDirectorySeparator()
        {
            return Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Return the names of the files in path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public virtual bool Exists(string path)
        {
            return File.Exists(path);
        }

        public virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public virtual void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public virtual void Delete(string path)
        {
            File.Delete(path);
        }

        public virtual string GetRandomFileName()
        {
            return Path.GetRandomFileName();
        }

        /// <summary>
        /// Return the names of the directories in path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        /// <summary>
        /// Returns a DateTime object referring to the time of the last write.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        /// <summary>
        /// Returns a DateTime object referring to the time of creation
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual DateTime GetCreationTime(string path)
        {
            return File.GetCreationTime(path);
        }

        /// <summary>
        /// Return the entire contents of the file as a text string using default encoding
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        /// <summary>
        /// Returns the entire contents of the file as a text string using te specified encoding
        /// </summary>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public virtual string ReadAllText(string path, Encoding encoding)
        {
            return File.ReadAllText(path, encoding);
        }

        /// <summary>
        /// Returns the entire contents of the file as a byte array
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public virtual void WriteAllText(string path, string text)
        {
            File.WriteAllText(path, text);
        }

        public virtual void WriteAllText(string path, string text, Encoding encoding)
        {
            File.WriteAllText(path, text, encoding);
        }

        public virtual void WriteAllBytes(string path, byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }

        public FileStream Open(string path, FileMode mode, FileAccess access)
        {
            return Open(path, mode, access, FileShare.None);
        }

        public virtual FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return File.Open(path, mode, access, share);
        }
    }

    #endregion

    [Serializable]
    public class BinaryMetaInfo : IDisposable
    {
        [XmlIgnore]
        [DefaultValue("")]
        public string Root { get; set; }
        [DefaultValue("")]
        public string MetaFileName => Root + Path.DirectorySeparatorChar + "metainfo.xml";
        public List<MetaFile> MetaFiles = new List<MetaFile>();
        private readonly Random Random = new Random();
        internal FileStream FileStream;

        // Track whether Dispose has been called.
        private bool disposed = false;

        #region Construction / Destruction

        public BinaryMetaInfo()
        {
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~BinaryMetaInfo()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

        public static BinaryMetaInfo FromPath(string path, FileAccess fileAccess, FileShare fileShare)
        {
            Random rnd = new Random();
            BinaryMetaInfo bmi;
            XmlSerializer xml = new XmlSerializer(typeof(BinaryMetaInfo));
            string metaFileName = path + Path.DirectorySeparatorChar + "metainfo.xml";

            if (File.Exists(metaFileName))
            {
                DateTime obtainLockStart = DateTime.Now;
                FileStream fs = null;
                while (true)
                {
                    DateTime obtainLockNow = DateTime.Now;
                    TimeSpan dt = obtainLockNow - obtainLockStart;
                    if (dt.TotalSeconds > 30)
                    {
                        break;
                    }

                    try
                    {
                        fs = File.Open(metaFileName, FileMode.Open, fileAccess, fileShare);
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.CurrentThread.Join(50 + rnd.Next(50));
                    }
                }
                XmlReader xmlReader = XmlReader.Create(fs);
                try
                {
                    bmi = (BinaryMetaInfo)xml.Deserialize(xmlReader);
                    bmi.FileStream = fs;
                    bmi.Root = path;
                    xmlReader.Close();
                }
                catch (Exception)
                {
                    xmlReader.Close();
                    bmi = new BinaryMetaInfo
                    {
                        Root = path,
                        FileStream = fs
                    };
                }
            }
            else
            {
                FileStream fs = null;
                try
                {
                    fs = File.Open(metaFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                }
                catch (Exception)
                {
                    if (File.Exists(metaFileName))
                    {
                        // Another process created the file just before us. Just call ourselves recursively,
                        // which should land us in the other branch of the if-statement now that the metaFile exists
                        return FromPath(path, fileAccess, fileShare);
                    }
                }

                bmi = new BinaryMetaInfo
                {
                    Root = path,
                    FileStream = fs
                };
            }
            return bmi;
        }

        public void Exists(string sourceName, string defines, string buildOptions)
        {
            MetaFiles.Exists(file => file.SourceName == sourceName && file.Defines == defines && file.BuildOptions == buildOptions);
        }

        public MetaFile FindMetaFile(string source, string sourceName, string platform, string device, string driverVersion, string defines, string buildOptions)
        {
            return MetaFiles.Find(file => file.Source == source && file.SourceName == sourceName && file.Platform == platform && file.Device == device && file.DriverVersion == driverVersion && file.Defines == defines && file.BuildOptions == buildOptions);
        }

        public MetaFile CreateMetaFile(string source, string sourceName, string platform, string device, string driverVersion, string defines, string buildOptions)
        {
            MetaFile mf = null;
            while (true)
            {
                string randomFileName = Path.GetRandomFileName();
                try
                {
                    FileStream fs = File.Open(Root + Path.DirectorySeparatorChar + randomFileName, FileMode.CreateNew, FileAccess.ReadWrite);
                    fs.Close();
                    mf = new MetaFile(source, sourceName, platform, device, driverVersion, defines, buildOptions, randomFileName);
                    MetaFiles.Add(mf);
                    break;
                }
                catch (Exception)
                {
                    Thread.CurrentThread.Join(50 + Random.Next(50));
                }
            }
            return mf;
        }

        /// <summary>
        /// Delete excess items in MetaFiles
        /// </summary>
        public void TrimBinaryCache(OCLManFileSystem fileSystem, int size)
        {
            if (size < 0)
            {
                return;
            }

            while (MetaFiles.Count > size && MetaFiles.Count > 0)
            {
                MetaFile mf = MetaFiles[0];
                fileSystem.Delete(Root + Path.DirectorySeparatorChar + mf.BinaryName);
                MetaFiles.RemoveAt(0);
            }
        }

        public void Save()
        {
            XmlSerializer xml = new XmlSerializer(typeof(BinaryMetaInfo));
            FileStream.SetLength(0L);
            XmlWriter xmlWriter = XmlWriter.Create(FileStream);
            xml.Serialize(xmlWriter, this);
            xmlWriter.Close();
        }

        #region IDisposable Members

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                FileStream.Dispose();
                FileStream = null;

                // Note disposing has been done.
                disposed = true;
            }
        }


        #endregion
    }

    [Serializable]
    public class MetaFile
    {
        [DefaultValue("")]
        public string Source { get; set; }
        [DefaultValue("")]
        public string SourceName { get; set; }
        [DefaultValue("")]
        public string Platform { get; set; }
        [DefaultValue("")]
        public string Device { get; set; }
        [DefaultValue("")]
        public string DriverVersion { get; set; }
        [DefaultValue("")]
        public string Defines { get; set; }
        [DefaultValue("")]
        public string BuildOptions { get; set; }
        [DefaultValue("")]
        public string BinaryName { get; set; }

        public MetaFile()
        {
            Source = "";
            SourceName = "";
            Platform = "";
            Device = "";
            DriverVersion = "";
            Defines = "";
            BuildOptions = "";
            BinaryName = "";
        }

        public MetaFile(string source, string sourceName, string platform, string device, string driverVersion, string defines, string buildOptions, string binaryName)
        {
            if (source != null)
            {
                Source = source;
            }
            else
            {
                Source = "";
            }

            if (sourceName != null)
            {
                SourceName = sourceName;
            }
            else
            {
                SourceName = "";
            }

            if (platform != null)
            {
                Platform = platform;
            }
            else
            {
                Platform = "";
            }

            if (device != null)
            {
                Device = device;
            }
            else
            {
                Device = "";
            }

            if (driverVersion != null)
            {
                DriverVersion = driverVersion;
            }
            else
            {
                DriverVersion = "";
            }

            if (defines != null)
            {
                Defines = defines;
            }
            else
            {
                Defines = "";
            }

            if (buildOptions != null)
            {
                BuildOptions = buildOptions;
            }
            else
            {
                BuildOptions = "";
            }

            if (binaryName != null)
            {
                BinaryName = binaryName;
            }
            else
            {
                BinaryName = "";
            }
        }
    }

}
