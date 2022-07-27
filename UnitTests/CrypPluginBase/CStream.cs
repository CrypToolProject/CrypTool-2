using CrypTool.PluginBase.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace UnitTests
{
    /// <summary>
    /// Summary description for CStreamTest
    /// </summary>
    [TestClass]
    public class CStreamTest
    {

        private byte[] ShortData
        {
            get;
            set;
        }

        private byte[] LongData
        {
            get;
            set;
        }

        public CStreamTest()
        {
            ShortData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
            LongData = new byte[65535];

            Random rng = new Random();
            rng.NextBytes(LongData);
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get => testContextInstance;
            set => testContextInstance = value;
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void SelfTest()
        {
            // assert 0 < ShortData < LongData
            Assert.IsTrue(0 < ShortData.Length);
            Assert.IsTrue(ShortData.Length < LongData.Length);
        }

        [TestMethod]
        public void TestWriter()
        {
            CStreamWriter writer = new CStreamWriter();

            // put 6 bytes
            writer.Write(ShortData);

            // length == position == 6
            Assert.AreEqual(ShortData.Length, writer.Length);
            Assert.AreEqual(ShortData.Length, writer.Position);

            // not swapped
            Assert.IsFalse(writer.IsSwapped);
        }

        [TestMethod]
        public void TestReader()
        {
            CStreamWriter writer = new CStreamWriter();

            // put 6 bytes
            writer.Write(ShortData);
            CStreamReader reader = writer.CreateReader();

            // length==6, position==0
            Assert.AreEqual(0, reader.Position);
            Assert.AreEqual(ShortData.Length, reader.Length);

            // try to read more bytes than available
            byte[] buf = new byte[ShortData.Length * 1000];
            Assert.AreNotEqual(buf.Length, ShortData.Length);
            Assert.IsFalse(buf.SequenceEqual(ShortData));
            int read = reader.Read(buf);
            Assert.AreEqual(ShortData.Length, read);

            // assert the first few bytes are still correct
            byte[] buf2 = new byte[ShortData.Length];
            Array.Copy(buf, buf2, buf2.Length);
            Assert.IsTrue(buf2.SequenceEqual(ShortData));

            // not swapped
            Assert.IsFalse(reader.IsSwapped);
        }

        [TestMethod]
        public void TestSwap()
        {
            CStreamWriter writer = new CStreamWriter();

            // fill buffer with Length-1 bytes
            writer.Write(LongData);
            Assert.AreEqual(LongData.Length, writer.Position);
            Assert.IsFalse(writer.IsSwapped);

            // fill last byte
            writer.WriteByte(5);
            Assert.AreEqual(LongData.Length + 1, writer.Position);
            Assert.IsFalse(writer.IsSwapped);

            // write one byte more, assert swap
            writer.WriteByte(10);
            Assert.AreEqual(LongData.Length + 2, writer.Position);
            Assert.IsTrue(writer.IsSwapped);
        }

        [TestMethod]
        public void TestSwapWithReader()
        {
            CStreamWriter writer = new CStreamWriter();
            CStreamReader reader = writer.CreateReader();

            // write, not swapped
            writer.Write(LongData);
            Assert.IsFalse(writer.IsSwapped);

            // read a few bytes, but there are still a few bytes left
            byte[] buf = new byte[ShortData.Length];
            reader.Read(buf);
            Assert.IsTrue(reader.Position > 0);
            Assert.IsTrue(reader.Length > reader.Position);

            // fill buffer, assert swap
            writer.Write(LongData);
            writer.Write(LongData);
            Assert.IsTrue(writer.IsSwapped);
            Assert.IsTrue(reader.IsSwapped);

            // try to read more than available, receive less
            buf = new byte[writer.Length * 2];
            int read = reader.Read(buf);
            Assert.IsTrue(read < buf.Length);

            // close writer, assert EOF
            writer.Close();
            int result = reader.ReadByte();
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void TestDestructor()
        {
            string filePath;
            DoTestDestructor();

            // force GC
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // tempfile deleted
            Assert.IsFalse(File.Exists(filePath));

            //by using a method, the writer is garbace collected
            //and so the temp file should also be deleted after that
            void DoTestDestructor()
            {
                CStreamWriter writer = new CStreamWriter();

                Assert.IsFalse(writer.IsSwapped);
                writer.Write(LongData);
                Assert.IsFalse(writer.IsSwapped);
                writer.Write(LongData);

                Assert.IsTrue(writer.IsSwapped);

                filePath = writer.FilePath;
                Assert.IsTrue(File.Exists(filePath));
            }
        }

        [TestMethod]
        public void TestDestructorWithReader()
        {
            string filePath;
            DoTestDestructorWithReader();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            // deleted tempfile
            Assert.IsFalse(File.Exists(filePath));

            //by using a method, the writer is garbace collected
            //and so the temp file should also be deleted after that
            void DoTestDestructorWithReader()
            {
                CStreamWriter writer = new CStreamWriter();

                // write, not swapped
                writer.Write(LongData);

                // read something and assert there's more
                CStreamReader reader = writer.CreateReader();
                byte[] buf = new byte[ShortData.Length];
                reader.Read(buf);
                Assert.IsTrue(reader.Position > 0);
                Assert.IsTrue(reader.Length > reader.Position);
                Assert.IsFalse(reader.IsSwapped);

                // write more, assert swap
                writer.Write(LongData);
                writer.Write(LongData);
                Assert.IsTrue(reader.IsSwapped);

                filePath = writer.FilePath;

                // destroy ref to writer
                writer.Close();
                writer = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Assert.IsNull(writer);

                // assert reading still works
                Assert.IsTrue(File.Exists(filePath));
                Assert.IsNotNull(reader);
                int sum = 0;
                while (sum < LongData.Length * 2)
                {
                    int read = reader.Read(buf);
                    Assert.IsTrue(read > 0);
                    sum += read;
                }

                // destroy reader
                reader = null;
            }
        }

        [TestMethod]
        public void TestExhaustiveRead()
        {
            CStreamWriter writer = new CStreamWriter();

            writer.Write(LongData);
            writer.Write(LongData);
            writer.Write(LongData);
            writer.Close();

            CStreamReader reader = writer.CreateReader();
            byte[] bigbuf = reader.ReadFully();

            Assert.AreEqual(LongData.Length * 3, bigbuf.Length);
        }

        [TestMethod]
        public void TestSeek()
        {
            CStreamWriter writer = new CStreamWriter();
            writer.Write(LongData);
            writer.Close();

            CStreamReader reader = writer.CreateReader();
            byte[] buf = new byte[1024];

            { // seek 5 bytes before EOF, attempt to read much, get 5 bytes
                reader.Seek(LongData.Length - 5, SeekOrigin.Begin);
                int read = reader.Read(buf, 0, int.MaxValue);
                Assert.AreEqual(5, read);
            }

            { // read EOF
                int read = reader.Read(buf, 0, int.MaxValue);
                Assert.AreEqual(0, read);
            }

            { // seek beyond stream length, read EOF
                reader.Seek(LongData.Length * 3, SeekOrigin.Begin);
                int read = reader.Read(buf, 0, int.MaxValue);
                Assert.AreEqual(0, read);
            }

            { // seek back, read again
                reader.Seek(LongData.Length - 5, SeekOrigin.Begin);
                int read = reader.Read(buf, 0, int.MaxValue);
                Assert.AreEqual(5, read);
            }
        }

        [TestMethod]
        public void TestSeekSwap()
        {
            CStreamWriter writer = new CStreamWriter();
            writer.Write(LongData);

            CStreamReader reader = writer.CreateReader();
            byte[] buf = new byte[1024];

            // seek 5 bytes before EOF
            reader.Seek(LongData.Length - 5, SeekOrigin.Begin);

            // write more, ensure swap
            writer.Write(LongData);
            writer.Write(LongData);
            Assert.IsTrue(writer.IsSwapped);

            { // seek somewhere to the middle, read
                reader.Seek(LongData.Length * 2, SeekOrigin.Begin);
                int read = reader.Read(buf);
                Assert.AreEqual(buf.Length, read);
            }

            { // seek beyond length, assert still open, get EOF
                reader.Seek(LongData.Length * 3, SeekOrigin.Current);
                Assert.IsFalse(writer.IsClosed);
                int read = reader.Read(buf);
                Assert.AreEqual(0, read);
            }
        }

        [TestMethod]
        public void TestEmpty()
        {
            ICrypToolStream empty = CStreamWriter.Empty;

            CStreamReader reader = empty.CreateReader();
            byte[] buf = new byte[1024];
            int read = reader.ReadFully(buf);

            Assert.AreEqual(0, read);
        }

        [TestMethod]
        public void TestExistingFileReader()
        {
            string tempFile;
            DoTestExistingFileReader();

            // force gc            
            GC.Collect();
            GC.WaitForPendingFinalizers();

            File.Delete(tempFile);

            //by using a method, the writer is garbace collected
            //and so the temp file should also be deleted after that
            void DoTestExistingFileReader()
            {
                tempFile = DirectoryHelper.GetNewTempFilePath();
                FileStream tempWriter = new FileStream(tempFile, FileMode.CreateNew);
                tempWriter.Write(ShortData, 0, ShortData.Length);
                tempWriter.Write(LongData, 0, LongData.Length);
                tempWriter.Close();

                CStreamWriter cstream = new CStreamWriter(tempFile);
                CStreamReader reader = cstream.CreateReader();
                byte[] buf = new byte[ShortData.Length];
                reader.ReadFully(buf);
                Assert.IsTrue(buf.SequenceEqual(ShortData));

                buf = new byte[LongData.Length];
                reader.ReadFully(buf);
                Assert.IsTrue(buf.SequenceEqual(LongData));

                reader = null;
                cstream = null;
            }
        }
    }
}
