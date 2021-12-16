/*
   Copyright 2019 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VoluntLib2.ComputationLayer;
using VoluntLib2.ManagementLayer.Messages;

namespace VoluntLib2.ManagementLayer
{
    [TestClass()]
    public class ManagementLayerTests
    {
        /// <summary>
        /// This test method tests the serialization and deserialization of all messages of the ManagementLayer
        /// </summary>
        [TestMethod()]
        public void MgmtLayer_TestMessagesSerializationDeserialization()
        {
            byte[] bytes; // helper variable for serialized data

            //Here, we perform all tests with non-signed messages:

            //Test #1: Test Message class serialization/deserialization
            Message message = new Message
            {
                Payload = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }
            };
            bytes = message.Serialize(false);
            Message message_deserialized = new Message();
            message_deserialized.Deserialize(bytes);
            Assert.AreEqual(message, message_deserialized, "Test #1: Deserialized Message not equals original Message");

            //Test #2: Test RequestJobListMessage class serialization/deserialization
            RequestJobListMessage requestJobListMessage = new RequestJobListMessage();
            bytes = requestJobListMessage.Serialize(false);
            RequestJobListMessage requestJobListMessage_deserialized = new RequestJobListMessage();
            requestJobListMessage_deserialized.Deserialize(bytes);
            Assert.AreEqual(requestJobListMessage, requestJobListMessage_deserialized, "Test #2: Deserialized RequestJobListMessage not equals original RequestJobListMessage");

            //Test #3: Test ResponseJobListMessage class serialization/deserialization
            for (int i = 0; i < 11; i++)
            {
                ResponseJobListMessage responseJobListMessage = new ResponseJobListMessage();
                for (int j = 0; j <= i; j++)
                {
                    Job job = new Job(j);
                    job.JobEpochState.Bitmask = new Bitmask(0); // we dont send bitmsaks in ResponseJobListMessages
                    job.CreationDate = DateTime.Now;
                    job.CreatorCertificateData = new byte[] { 1, 2, 3, 4, 5 };
                    job.CreatorName = "Name-" + j;
                    job.JobCreatorSignatureData = new byte[] { 1, 2, 3, 4, 5 };
                    job.JobDeletionSignatureData = new byte[] { 1, 2, 3, 4, 5 };
                    responseJobListMessage.Jobs.Add(job);
                }
                bytes = responseJobListMessage.Serialize(false);
                ResponseJobListMessage responseJobListMessage_deserialized = new ResponseJobListMessage();
                responseJobListMessage_deserialized.Deserialize(bytes);
                Assert.AreEqual(responseJobListMessage, responseJobListMessage_deserialized, "Test #2: Deserialized ResponseJobListMessage not equals original ResponseJobListMessage");
            }

            //Test #4: Test RequestJobListMessage class serialization/deserialization
            RequestJobMessage requestJobMessage = new RequestJobMessage();
            bytes = requestJobMessage.Serialize(false);
            RequestJobMessage requestJobMessage_deserialized = new RequestJobMessage();
            requestJobMessage_deserialized.Deserialize(bytes);
            Assert.AreEqual(requestJobMessage, requestJobMessage_deserialized, "Test #4: Deserialized RequestJobMessage not equals original RequestJobMessage");

            //Test #5: Test ResponseJobListMessage class serialization/deserialization
            ResponseJobMessage responseJobMessage = new ResponseJobMessage();
            bytes = responseJobMessage.Serialize(false);
            ResponseJobMessage responseJobMessage_deserialized = new ResponseJobMessage();
            responseJobMessage_deserialized.Deserialize(bytes);
            Assert.AreEqual(responseJobMessage, responseJobMessage_deserialized, "Test #5: Deserialized ResponseJobMessage not equals original ResponseJobMessage");

        }

        /// <summary>
        /// This test method tests the job class
        /// </summary>
        [TestMethod()]
        public void TestJobClass()
        {
            byte[] bytes; // helper variable for serialized data

            //Test #1: Test Message class serialization/deserialization
            Job job = new Job(1000)
            {
                JobCreatorSignatureData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                JobDeletionSignatureData = new byte[] { 11, 12, 13, 14, 15, 16, 17, 18, 19 },
                JobDescription = "this is a test description",
                JobName = "Jobtestname",
                JobPayload = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                JobPayloadHash = new byte[] { 1, 2, 3 },
                JobType = "Testtype",
                WorldName = "Testworld",
                CreationDate = DateTime.MinValue,
                CreatorCertificateData = new byte[] { 13, 14, 15, 16, 17, 18, 19, 20 },
                CreatorName = "Job Creator"
            };
            bytes = job.Serialize();
            Job job_deserialized = new Job(0);
            job_deserialized.Deserialize(bytes);
            Assert.IsTrue(job.Equals_Test(job_deserialized), "Test #1: Deserialized Job not equals original Job");
        }
    }
}
