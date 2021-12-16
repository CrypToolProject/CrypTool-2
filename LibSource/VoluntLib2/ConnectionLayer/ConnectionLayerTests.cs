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
using System.Net;
using VoluntLib2.ConnectionLayer.Messages;

namespace VoluntLib2.ConnectionLayer
{
    [TestClass()]
    public class ConnectionLayerTests
    {
        /// <summary>
        /// This test method tests the serialization and deserialization of all messages of the ConnectionLayer
        /// </summary>
        [TestMethod()]
        public void ConLayer_TestMessagesSerializationDeserialization()
        {
            byte[] bytes; // helper variable for serialized data

            //Test #1: Test Message class serialization/deserialization
            Message message = new Message
            {
                Payload = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
            };
            message.MessageHeader.ReceiverIPAddress = new byte[] { 1, 2, 3, 4 };
            message.MessageHeader.SenderIPAddress = new byte[] { 5, 6, 7, 8 };
            message.MessageHeader.SenderPeerId = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            message.MessageHeader.ReceiverPeerId = new byte[] { 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };
            message.MessageHeader.ReceiverExternalPort = 1122;
            message.MessageHeader.SenderExternalPort = 3344;
            bytes = message.Serialize();
            Message message_deserialized = new Message();
            message_deserialized.Deserialize(bytes);
            Assert.AreEqual(message, message_deserialized, "Test #1: Deserialized Message not equals original Message");

            //Test #2: Test HelloMessage
            HelloMessage helloMessage = new HelloMessage();
            helloMessage.MessageHeader.ReceiverIPAddress = new byte[] { 11, 12, 13, 14 };
            helloMessage.MessageHeader.SenderIPAddress = new byte[] { 15, 16, 17, 18 };
            helloMessage.MessageHeader.SenderPeerId = new byte[] { 21, 22, 23, 24, 25, 26, 27, 28, 29, 210, 211, 212, 213, 214, 215, 216 };
            helloMessage.MessageHeader.ReceiverPeerId = new byte[] { 217, 218, 219, 200, 210, 220, 230, 240, 125, 126, 127, 128, 129, 130, 131, 132 };
            helloMessage.MessageHeader.ReceiverExternalPort = 55666;
            helloMessage.MessageHeader.SenderExternalPort = 7788;
            bytes = helloMessage.Serialize();
            HelloMessage helloMessage_deserialized = new HelloMessage();
            helloMessage_deserialized.Deserialize(bytes);
            Assert.AreEqual(helloMessage, helloMessage_deserialized, "Test #2: Deserialized HelloMessage not equals original HelloMessage");

            //Test #3: Test HelloResponseMessage
            HelloResponseMessage helloResponseMessage = new HelloResponseMessage();
            helloResponseMessage.MessageHeader.ReceiverIPAddress = new byte[] { 111, 112, 113, 114 };
            helloResponseMessage.MessageHeader.SenderIPAddress = new byte[] { 115, 116, 117, 118 };
            helloResponseMessage.MessageHeader.SenderPeerId = new byte[] { 31, 32, 33, 34, 35, 36, 37, 38, 39, 30, 31, 32, 33, 34, 35, 36 };
            helloResponseMessage.MessageHeader.ReceiverPeerId = new byte[] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            helloResponseMessage.MessageHeader.ReceiverExternalPort = ushort.MaxValue;
            helloResponseMessage.MessageHeader.SenderExternalPort = ushort.MaxValue;
            bytes = helloResponseMessage.Serialize();
            HelloResponseMessage helloResponseMessage_deserialized = new HelloResponseMessage();
            helloResponseMessage_deserialized.Deserialize(bytes);
            Assert.AreEqual(helloResponseMessage, helloResponseMessage_deserialized, "Test #3: Deserialized HelloResponseMessage not equals original HelloResponseMessage");

            //Test #4: Test RequestNeighborListMessage
            RequestNeighborListMessage requestNeighborListMessage = new RequestNeighborListMessage();
            requestNeighborListMessage.MessageHeader.ReceiverIPAddress = new byte[] { 100, 200, 50, 0 };
            requestNeighborListMessage.MessageHeader.SenderIPAddress = new byte[] { 0, 50, 100, 200 };
            requestNeighborListMessage.MessageHeader.SenderPeerId = new byte[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7 };
            requestNeighborListMessage.MessageHeader.ReceiverPeerId = new byte[] { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 0, 0 };
            requestNeighborListMessage.MessageHeader.ReceiverExternalPort = ushort.MinValue;
            requestNeighborListMessage.MessageHeader.SenderExternalPort = ushort.MinValue;
            bytes = requestNeighborListMessage.Serialize();
            RequestNeighborListMessage requestNeighborListMessage_deserialized = new RequestNeighborListMessage();
            requestNeighborListMessage_deserialized.Deserialize(bytes);
            Assert.AreEqual(requestNeighborListMessage, requestNeighborListMessage_deserialized, "Test #4: Deserialized RequestNeighborListMessage not equals original RequestNeighborListMessage");

            //Test #5: Test ResponseNeighborListMessage (with 0 to 10 contacts in list)
            for (int i = 0; i < 11; i++)
            {
                ResponseNeighborListMessage responseNeighborListMessage = new ResponseNeighborListMessage();
                responseNeighborListMessage.MessageHeader.ReceiverIPAddress = new byte[] { 100, 200, 50, 0 };
                responseNeighborListMessage.MessageHeader.SenderIPAddress = new byte[] { 0, 50, 100, 200 };
                responseNeighborListMessage.MessageHeader.SenderPeerId = new byte[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7 };
                responseNeighborListMessage.MessageHeader.ReceiverPeerId = new byte[] { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 0, 0 };
                responseNeighborListMessage.MessageHeader.ReceiverExternalPort = ushort.MinValue;
                responseNeighborListMessage.MessageHeader.SenderExternalPort = ushort.MinValue;
                //here, add Contacts to list of neighbors
                for (int c = 0; c < i; c++)
                {
                    Contact contact = new Contact
                    {
                        IPAddress = new IPAddress(new byte[] { (byte)((c + 13) % 256), (byte)((c + 14) % 256), (byte)((c + 15) % 256), (byte)((c + 16) % 256) }),
                        PeerId = new byte[]
                    {
                        (byte)((c + 1) % 256), (byte)((c + 2) % 256), (byte)((c + 3) % 256), (byte)((c + 4) % 256), (byte)((c + 5) % 256), (byte)((c + 6) % 256),
                        (byte)((c + 7) % 256), (byte)((c + 8) % 256), (byte)((c + 9) % 256), (byte)((c + 10) % 256), (byte)((c + 11) % 256), (byte)((c + 12) % 256),
                        (byte)((c + 13) % 256), (byte)((c + 14) % 256), (byte)((c + 15) % 256), (byte)((c + 16) % 256)
                    },
                        Port = (ushort)((c + 100) % ushort.MaxValue)
                    };
                    responseNeighborListMessage.Neighbors.Add(contact);
                }
                bytes = responseNeighborListMessage.Serialize();
                ResponseNeighborListMessage responseNeighborListMessage_deserialized = new ResponseNeighborListMessage();
                responseNeighborListMessage_deserialized.Deserialize(bytes);
                Assert.AreEqual(responseNeighborListMessage, responseNeighborListMessage_deserialized, "Test #5: Deserialized ResponseNeighborListMessage not equals original ResponseNeighborListMessage");
            }

            //Test #6: Test HelpMeConnectMessage
            HelpMeConnectMessage helpMeConnectMessage = new HelpMeConnectMessage();
            helpMeConnectMessage.MessageHeader.ReceiverIPAddress = new byte[] { 100, 200, 50, 0 };
            helpMeConnectMessage.MessageHeader.SenderIPAddress = new byte[] { 0, 50, 100, 200 };
            helpMeConnectMessage.MessageHeader.SenderPeerId = new byte[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7 };
            helpMeConnectMessage.MessageHeader.ReceiverPeerId = new byte[] { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 0, 0 };
            helpMeConnectMessage.MessageHeader.ReceiverExternalPort = ushort.MinValue;
            helpMeConnectMessage.MessageHeader.SenderExternalPort = ushort.MinValue;
            helpMeConnectMessage.Port = 1122;
            helpMeConnectMessage.IPAddress = new IPAddress(new byte[] { 1, 2, 3, 4 });
            bytes = helpMeConnectMessage.Serialize();
            HelpMeConnectMessage helpMeConnectMessage_deserialized = new HelpMeConnectMessage();
            helpMeConnectMessage_deserialized.Deserialize(bytes);
            Assert.AreEqual(helpMeConnectMessage, helpMeConnectMessage_deserialized, "Test #6: Deserialized HelpMeConnectMessage not equals original HelpMeConnectMessage");

            //Test #7: Test WantsConnectionMessage
            WantsConnectionMessage wantsConnectionMessage = new WantsConnectionMessage();
            wantsConnectionMessage.MessageHeader.ReceiverIPAddress = new byte[] { 100, 200, 50, 0 };
            wantsConnectionMessage.MessageHeader.SenderIPAddress = new byte[] { 0, 50, 100, 200 };
            wantsConnectionMessage.MessageHeader.SenderPeerId = new byte[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7 };
            wantsConnectionMessage.MessageHeader.ReceiverPeerId = new byte[] { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 0, 0 };
            wantsConnectionMessage.MessageHeader.ReceiverExternalPort = ushort.MinValue;
            wantsConnectionMessage.MessageHeader.SenderExternalPort = ushort.MinValue;
            wantsConnectionMessage.Port = 1122;
            wantsConnectionMessage.IPAddress = new IPAddress(new byte[] { 1, 2, 3, 4 });
            bytes = wantsConnectionMessage.Serialize();
            WantsConnectionMessage wantsConnectionMessage_deserialized = new WantsConnectionMessage();
            wantsConnectionMessage_deserialized.Deserialize(bytes);
            Assert.AreEqual(wantsConnectionMessage, wantsConnectionMessage_deserialized, "Test #7: Deserialized WantsConnectionMessage not equals original WantsConnectionMessage");

            //Test #8: Test DataMessage
            DataMessage dataMessage = new DataMessage();
            dataMessage.MessageHeader.ReceiverIPAddress = new byte[] { 100, 200, 50, 0 };
            dataMessage.MessageHeader.SenderIPAddress = new byte[] { 0, 50, 100, 200 };
            dataMessage.MessageHeader.SenderPeerId = new byte[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7 };
            dataMessage.MessageHeader.ReceiverPeerId = new byte[] { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 0, 0 };
            dataMessage.MessageHeader.ReceiverExternalPort = ushort.MinValue;
            dataMessage.MessageHeader.SenderExternalPort = ushort.MinValue;
            bytes = dataMessage.Serialize();
            DataMessage dataMessage_deserialized = new DataMessage();
            dataMessage_deserialized.Deserialize(bytes);
            Assert.AreEqual(dataMessage, dataMessage_deserialized, "Test #8: Deserialized DataMessage not equals original DataMessage");

            //Test #9: Test GoingOfflineMessage
            GoingOfflineMessage goingOfflineMessage = new GoingOfflineMessage();
            goingOfflineMessage.MessageHeader.ReceiverIPAddress = new byte[] { 100, 200, 50, 0 };
            goingOfflineMessage.MessageHeader.SenderIPAddress = new byte[] { 0, 50, 100, 200 };
            goingOfflineMessage.MessageHeader.SenderPeerId = new byte[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7 };
            goingOfflineMessage.MessageHeader.ReceiverPeerId = new byte[] { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 0, 0 };
            goingOfflineMessage.MessageHeader.ReceiverExternalPort = ushort.MinValue;
            goingOfflineMessage.MessageHeader.SenderExternalPort = ushort.MinValue;
            bytes = goingOfflineMessage.Serialize();
            GoingOfflineMessage goingOfflineMessage_deserialized = new GoingOfflineMessage();
            goingOfflineMessage_deserialized.Deserialize(bytes);
            Assert.AreEqual(goingOfflineMessage, goingOfflineMessage_deserialized, "Test #9: Deserialized GoingOfflineMessage not equals original GoingOfflineMessage");
        }
    }
}
