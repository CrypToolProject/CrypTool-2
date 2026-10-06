/*
   Copyright (C) CrypTool 2 Team

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
using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UnitTests
{
    public static class TestHelpers
    {
        private static bool _initialized = false;

        private static readonly string[] Subfolders =
        {
            "",
            "CrypPlugins",
            "Lib",
        };

        static TestHelpers()
        {
            SetAssemblyPaths();
        }

        public static void SetAssemblyPaths()
        {
            if (!_initialized)
            {
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyResolve += LoadAssembly;
                _initialized = true;
            }
        }

        public static void LoadPluginAssemblies()
        {
            SetAssemblyPaths();
            string buildDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginDirectory = Path.Combine(buildDirectory, "CrypPlugins");
            if (!Directory.Exists(pluginDirectory))
            {
                throw new DirectoryNotFoundException("Plugin build directory not found: " + pluginDirectory);
            }

            foreach (string assemblyPath in Directory.GetFiles(pluginDirectory, "*.dll"))
            {
                try
                {
                    Assembly.Load(Path.GetFileNameWithoutExtension(assemblyPath));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not load plugin assembly {0}: {1}", assemblyPath, ex.Message);
                }
            }
        }

        public static void TestFail(int number)
        {
            Assert.Fail(string.Format("Test {0} failed!", number));
        }

        public static ICrypComponent GetPluginInstance(string pluginName)
        {
            return GetPluginInstance(pluginName, pluginName);
        }

        public static ICrypComponent GetPluginInstance(string pluginName, string assemblyName)
        {
            Assembly a = null;

            try
            {
                a = Assembly.Load(assemblyName);
            }
            catch (Exception ex)
            {
                Assert.Fail(string.Format("Can't load assembly {0}: {1}.", assemblyName, ex));
            }

            Type pluginType = a.GetTypes().FirstOrDefault(x => x.Name == pluginName);
            if (pluginType == null)
            {
                Assert.Fail(string.Format("Can't load plugin {0} from assembly {1}.", pluginName, assemblyName));
            }

            return pluginType.CreateComponentInstance();
        }

        public static ICrypToolStream HexToStream(this string hexString)
        {
            return hexString.HexToByteArray().ToStream();
        }

        public static byte[] HexToByteArray(this string hexString)
        {
            if (hexString == null)
            {
                throw new ArgumentNullException(nameof(hexString));
            }
            if (hexString.Length % 2 != 0)
            {
                throw new FormatException("Hexadecimal input must contain an even number of characters.");
            }

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return bytes;
        }

        public static string ToString2(this byte[] buf)
        {
            return Encoding.GetEncoding("iso-8859-15").GetString(buf);
        }

        public static byte[] ToByteArray(this string s)
        {
            return Encoding.GetEncoding("iso-8859-15").GetBytes(s);
        }

        public static byte[] ToByteArray(this CStreamReader stream)
        {
            if (stream == null)
            {
                return new byte[0];
            }

            stream.WaitEof();
            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.ReadFully(buffer);
            return buffer;
        }

        public static byte[] ToByteArray(this ICrypToolStream stream)
        {
            if (stream == null)
            {
                return new byte[0];
            }

            return stream.CreateReader().ToByteArray();
        }

        public static ICrypToolStream ToStream(this byte[] buf)
        {
            return new CStreamWriter(buf);
        }

        public static ICrypToolStream ToStream(this string s)
        {
            return s.ToByteArray().ToStream();
        }

        public static string ToHex(this byte[] buf)
        {
            return BitConverter.ToString(buf).Replace("-", string.Empty).ToUpper();
        }

        public static string ToHex(this ICrypToolStream stream)
        {
            if (stream == null)
            {
                return "";
            }

            return stream.ToByteArray().ToHex();
        }

        public static string ToHex(this string s)
        {
            return s.ToByteArray().ToHex();
        }

        public static string ToHex(this object obj)
        {
            if (obj is ICrypToolStream)
            {
                return ((ICrypToolStream)obj).ToHex();
            }

            if (obj is byte[])
            {
                return ((byte[])obj).ToHex();
            }

            if (obj is string)
            {
                return ((string)obj).ToHex();
            }

            return "";
        }

        /// <summary>
        /// Loads assemblies defined by subfolders definition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly LoadAssembly(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (string subfolder in Subfolders)
            {
                string assemblyPath = Path.Combine(folderPath, Path.Combine(subfolder, new AssemblyName(args.Name).Name + ".dll"));
                if (File.Exists(assemblyPath))
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    return assembly;
                }
                assemblyPath = Path.Combine(folderPath, (Path.Combine(subfolder, new AssemblyName(args.Name).Name + ".exe")));
                if (File.Exists(assemblyPath))
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    return assembly;
                }
            }
            return null;
        }
    }
}
