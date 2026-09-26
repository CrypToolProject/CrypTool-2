/*
   Copyright 2008 - 2022 CrypTool Team

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
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using WorkspaceManager.Model;
using WorkspaceManagerModel.Model.Tools;

namespace UnitTests
{
    [TestClass]
    public class CoreAuditRegressionTests
    {
        [TestMethod]
        public void ReadFullyKeepsPreviouslyReadChunksAtRequestedOffset()
        {
            using (CStreamWriter writer = new CStreamWriter())
            using (CStreamReader reader = writer.CreateReader())
            {
                writer.Write(new byte[] { 1, 2 });
                Thread producer = new Thread(() =>
                {
                    Thread.Sleep(50);
                    writer.Write(new byte[] { 3, 4 });
                    writer.Close();
                });
                producer.Start();

                byte[] result = Enumerable.Repeat((byte)0xff, 8).ToArray();
                Assert.AreEqual(4, reader.ReadFully(result, 2, 4));
                CollectionAssert.AreEqual(new byte[] { 0xff, 0xff, 1, 2, 3, 4, 0xff, 0xff }, result);
                producer.Join();
            }
        }

        [TestMethod]
        public void SeekFromEndUsesStreamLengthInsteadOfBufferCapacity()
        {
            using (CStreamWriter writer = new CStreamWriter(new byte[] { 10, 20, 30 }))
            using (CStreamReader reader = writer.CreateReader())
            {
                Assert.AreEqual(2, reader.Seek(-1, SeekOrigin.End));
                Assert.AreEqual(30, reader.ReadByte());
            }
        }

        [TestMethod]
        public void CfbAndOfbRoundTripPartialBlocks()
        {
            AssertPartialModeRoundTrip(BlockCipherHelper.BlockMode.CFB);
            AssertPartialModeRoundTrip(BlockCipherHelper.BlockMode.OFB);
        }

        [TestMethod]
        public void PaddingRejectsZeroLengthMarkersAndHandlesAllZeroBlocks()
        {
            AssertThrows(() => BlockCipherHelper.StripPadding(new byte[8], 8, BlockCipherHelper.PaddingType.PKCS7, 8));
            AssertThrows(() => BlockCipherHelper.StripPadding(new byte[8], 8, BlockCipherHelper.PaddingType.ANSIX923, 8));
            AssertThrows(() => BlockCipherHelper.StripPadding(new byte[8], 8, BlockCipherHelper.PaddingType.ISO10126, 8));
            Assert.AreEqual(0, BlockCipherHelper.StripPadding(new byte[8], 8, BlockCipherHelper.PaddingType.Zeros, 8));
        }

        [TestMethod]
        public void WorkspaceDeserializerRejectsTypesOutsideTheModelAssembly()
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml("<objects><object><type>System.Version</type><id>1</id><members /></object></objects>");
            AssertThrows(() => XMLSerialization.XMLSerialization.Deserialize(document));
        }

        [TestMethod]
        public void DataPresentationCallsDoNotCancelEachOther()
        {
            string first = null;
            string second = null;
            Thread a = new Thread(() => first = ViewHelper.GetDataPresentationString(new[] { 1, 2, 3 }));
            Thread b = new Thread(() => second = ViewHelper.GetDataPresentationString(new[] { 4, 5, 6 }));
            a.Start();
            b.Start();
            a.Join();
            b.Join();
            Assert.AreEqual("[1,2,3]", first);
            Assert.AreEqual("[4,5,6]", second);
        }

        [TestMethod]
        public void PluginModelsResolveOnlyAlreadyLoadedCrypToolPlugins()
        {
            string pluginPath = Path.Combine(Path.GetDirectoryName(typeof(CoreAuditRegressionTests).Assembly.Location), "CrypPlugins", "ADFGVX.dll");
            Type pluginType = Assembly.LoadFrom(pluginPath).GetType("CrypTool.ADFGVX.ADFGVX", true);
            PluginModel valid = CreateSerializedPluginModel(pluginType.FullName, pluginType.Assembly.GetName().Name);
            Assert.AreEqual(pluginType, valid.PluginType);

            PluginModel invalid = CreateSerializedPluginModel(typeof(System.Diagnostics.Process).FullName, typeof(System.Diagnostics.Process).Assembly.GetName().Name);
            Assert.IsNull(invalid.PluginType);
        }

        private static void AssertPartialModeRoundTrip(BlockCipherHelper.BlockMode mode)
        {
            byte[] input = { 1, 2, 3, 4, 5 };
            byte[] key = { 0 };
            byte[] iv = { 9, 8, 7, 6, 5, 4, 3, 2 };
            bool stop = false;
            byte[] last = null;
            ICrypToolStream source = new CStreamWriter(input);
            ICrypToolStream encrypted = null;
            ICrypToolStream decrypted = null;
            BlockCipherHelper.BlockCipher cipher = (block, ignoredKey) => block.Select(value => (byte)(value ^ 0x5a)).ToArray();

            if (mode == BlockCipherHelper.BlockMode.CFB)
            {
                BlockCipherHelper.ExecuteCFB(cipher, BlockCipherHelper.CipherAction.Encrypt, ref source, ref encrypted, key, iv, BlockCipherHelper.PaddingType.None, ref stop, (v, m) => { }, ref last);
                BlockCipherHelper.ExecuteCFB(cipher, BlockCipherHelper.CipherAction.Decrypt, ref encrypted, ref decrypted, key, iv, BlockCipherHelper.PaddingType.None, ref stop, (v, m) => { }, ref last);
            }
            else
            {
                BlockCipherHelper.ExecuteOFB(cipher, BlockCipherHelper.CipherAction.Encrypt, ref source, ref encrypted, key, iv, BlockCipherHelper.PaddingType.None, ref stop, (v, m) => { }, ref last);
                BlockCipherHelper.ExecuteOFB(cipher, BlockCipherHelper.CipherAction.Decrypt, ref encrypted, ref decrypted, key, iv, BlockCipherHelper.PaddingType.None, ref stop, (v, m) => { }, ref last);
            }

            CollectionAssert.AreEqual(input, BlockCipherHelper.StreamToByteArray(decrypted));
        }

        private static void AssertThrows(Action action)
        {
            try
            {
                action();
                Assert.Fail("Expected an exception.");
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception)
            {
            }
        }

        private static PluginModel CreateSerializedPluginModel(string typeName, string assemblyName)
        {
            PluginModel model = new PluginModel();
            typeof(PluginModel).GetField("PluginTypeName", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(model, typeName);
            typeof(PluginModel).GetField("PluginTypeAssemblyName", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(model, assemblyName);
            return model;
        }
    }
}
