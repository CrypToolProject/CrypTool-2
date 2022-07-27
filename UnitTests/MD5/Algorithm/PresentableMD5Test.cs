using CrypTool.MD5.Algorithm;
using CrypTool.PluginBase.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    public class PresentableMD5Test
    {
        private Random RNG;

        [TestInitialize]
        public void Initialize()
        {
            RNG = new Random();
        }

        [TestMethod]
        public void Construction()
        {
            new PresentableMD5();
        }

        [TestMethod]
        public void UninitializedStateAfterConstruction()
        {
            PresentableMD5 md5 = new PresentableMD5();

            Assert.AreEqual(md5.CurrentState.State, MD5StateDescription.UNINITIALIZED);
        }

        [TestMethod]
        public void InitializedStateAfterInitialization()
        {
            PresentableMD5 md5 = new PresentableMD5();
            md5.Initialize(CStreamWriter.Empty);

            Assert.AreEqual(md5.CurrentState.State, MD5StateDescription.INITIALIZED);
        }

        [TestMethod]
        public void VerifyResultsForRandomData()
        {
            const int TEST_RUNS = 10;
            const int MIN_TEST_DATA_LENGTH = 1;
            const int MAX_TEST_DATA_LENGTH = 1000;

            for (int i = 0; i < TEST_RUNS; i++)
            {
                byte[] testData = GenerateTestData(MIN_TEST_DATA_LENGTH, MAX_TEST_DATA_LENGTH);
                VerifyResult(testData);
            }
        }

        [TestMethod]
        public void VerifyResultForEmptyData()
        {
            VerifyResult(new byte[0]);
        }

        [TestMethod]
        public void VerifyResultsForRandomDataWithInterestingLength()
        {
            VerifyResult(GenerateTestData(54));
            VerifyResult(GenerateTestData(55));
            VerifyResult(GenerateTestData(56));
        }

        private void VerifyResult(byte[] data)
        {
            System.Security.Cryptography.MD5 builtinMD5 = System.Security.Cryptography.MD5.Create();
            byte[] builtinResult = builtinMD5.ComputeHash(data);

            PresentableMD5 presentableMD5 = new PresentableMD5();
            presentableMD5.Initialize(new CStreamWriter(data));
            presentableMD5.NextStepUntilFinished();
            byte[] presentableMD5Result = presentableMD5.HashValueBytes;

            CollectionAssert.AreEqual(builtinResult, presentableMD5Result);
        }

        private byte[] GenerateTestData(int minLength, int maxLength)
        {
            int resultLength = minLength + RNG.Next(maxLength - minLength) + 1;
            return GenerateTestData(resultLength);
        }

        private byte[] GenerateTestData(int length)
        {
            byte[] result = new byte[length];

            RNG.NextBytes(result);

            return result;
        }
    }
}

