using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VoluntLib2.Tools;

namespace VoluntLib2.ConnectionLayer.Messages
{
    /*
    Hello Protocol:
    A       --> HelloMessage -->             B
    A       <-- HelloResponseMessage <--     B
    The hello protocol is used every 30 seconds to "refresh" connections between A and B
     
    Neighbor lists exchange protocol:
    A       --> RequestNeighborListMessage -->  B
    A       <-- ResponseNeighborListMessage <-- B
     
    Connection Protocol:
    A       --> HelpMeConnectMessage    -->  S
    B       <-- WantsConnectionMessage  <--  S
    now A and B perform HelloMessages n times, until both received a HelloResponse
    if A received HelloResponse from B, connection from A->B works for A
    if B received HelloResponse from A, connection from B->A works for B
    
    Data Protocol:
    A         --> DataMessage -->  B    
    
    Offline Protocol
    A         --> GoingOfflineMessage --> B
     
    */

    /// <summary>
    /// Each message has a unique type number defined by this enum
    /// </summary>
    internal enum MessageType
    {
        Undefined = 0,
        HelloMessage = 10,
        HelloResponseMessage = 11,
        RequestNeighborListMessage = 20,
        ResponseNeighborListMessage = 21,
        HelpMeConnectMessage = 30,
        WantsConnectionMessage = 31,
        DataMessage = 40,
        GoingOfflineMessage = 50
    }

    /// <summary>
    /// Helper class with a static method for Deserialization
    /// </summary>
    internal class MessageHelper
    {
        public static Message Deserialize(byte[] data)
        {
            //Deserialize to general message object; if it fails we did not get a valid message
            Message message = new Message();
            message.Deserialize(data);

            switch (message.MessageHeader.MessageType)
            {
                case MessageType.Undefined:
                    throw new VoluntLibSerializationException(string.Format("Received a message of MessageType {0} - cannot do anything with that!", message.MessageHeader.MessageType));
                case MessageType.HelloMessage:
                    message = new HelloMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.HelloResponseMessage:
                    message = new HelloResponseMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.RequestNeighborListMessage:
                    message = new RequestNeighborListMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.ResponseNeighborListMessage:
                    message = new ResponseNeighborListMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.HelpMeConnectMessage:
                    message = new HelpMeConnectMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.WantsConnectionMessage:
                    message = new WantsConnectionMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.DataMessage:
                    message = new DataMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.GoingOfflineMessage:
                    message = new GoingOfflineMessage();
                    message.Deserialize(data);
                    return message;
                //add new message types here

                default:
                    throw new VoluntLibSerializationException(string.Format("Received a message of an unknown MessageType: {0}", message.MessageHeader.MessageType));
            }
        }
    }

    /// <summary>
    /// The header of all messages of VoluntLib2 Connection Layer
    /// </summary>
    internal class MessageHeader
    {
        //we have a header size of total 63 bytes
        public byte[] MessageId = new byte[16];        // 16 bytes
        public MessageType MessageType;                // 1 byte
        public ushort PayloadLength;                   // 2 bytes

        public byte[] SenderPeerId = new byte[16];     // 16 bytes
        public byte[] ReceiverPeerId = new byte[16];   // 16 bytes

        public byte[] SenderIPAddress = new byte[4];   // 4 bytes
        public byte[] ReceiverIPAddress = new byte[4]; // 4 bytes

        public ushort SenderExternalPort = 0;           // 2 bytes
        public ushort ReceiverExternalPort = 0;         // 2 bytes

        public byte[] Serialize()
        {
            byte[] data = new byte[16 + 1 + 2 + 16 + 16 + 4 + 4 + 2 + 2];
            Array.Copy(MessageId, 0, data, 0, 16);
            Array.Copy(BitConverter.GetBytes((byte)MessageType), 0, data, 16, 1);
            Array.Copy(BitConverter.GetBytes(PayloadLength), 0, data, 17, 2);
            Array.Copy(SenderPeerId, 0, data, 19, 16);
            Array.Copy(ReceiverPeerId, 0, data, 35, 16);
            Array.Copy(SenderIPAddress, 0, data, 51, 4);
            Array.Copy(ReceiverIPAddress, 0, data, 55, 4);
            Array.Copy(BitConverter.GetBytes(SenderExternalPort), 0, data, 59, 2);
            Array.Copy(BitConverter.GetBytes(ReceiverExternalPort), 0, data, 61, 2);
            return data;
        }
        public void Deserialize(byte[] data)
        {
            Array.Copy(data, 0, MessageId, 0, 16);
            MessageType = (MessageType)data[16];
            PayloadLength = BitConverter.ToUInt16(data, 17);
            Array.Copy(data, 19, SenderPeerId, 0, 16);
            Array.Copy(data, 35, ReceiverPeerId, 0, 16);
            Array.Copy(data, 51, SenderIPAddress, 0, 4);
            Array.Copy(data, 55, ReceiverIPAddress, 0, 4);
            SenderExternalPort = BitConverter.ToUInt16(data, 59);
            ReceiverExternalPort = BitConverter.ToUInt16(data, 61);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MessageHeader");
            builder.AppendLine("{");
            builder.Append("  MessageID: ");
            builder.AppendLine(BitConverter.ToString(MessageId) + ",");
            builder.Append("  MessageType: ");
            builder.AppendLine(MessageType.ToString() + ",");
            builder.Append("  PayloadLength: ");
            builder.AppendLine("" + PayloadLength + ",");
            builder.Append("  SenderPeerId: ");
            builder.AppendLine("" + BitConverter.ToString(SenderPeerId) + ",");
            builder.Append("  ReceiverPeerId: ");
            builder.AppendLine("" + BitConverter.ToString(ReceiverPeerId) + ",");
            builder.Append("  SenderAddress: ");
            builder.AppendLine("" + new IPAddress(SenderIPAddress) + ",");
            builder.Append("  ReceiverAddress: ");
            builder.AppendLine("" + new IPAddress(ReceiverIPAddress) + ",");
            builder.Append("  SenderPort: ");
            builder.AppendLine("" + SenderExternalPort + ",");
            builder.Append("  ReceiverPort: ");
            builder.AppendLine("" + ReceiverExternalPort + ",");
            builder.AppendLine("}");

            return builder.ToString();
        }

        /// <summary>
        /// Compares all fields of given message header with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            MessageHeader header = value as MessageHeader;
            if (header != null)
            {
                return header.MessageId.SequenceEqual(MessageId) &&
                       header.MessageType.Equals(MessageType) &&
                       header.PayloadLength.Equals(PayloadLength) &&
                       header.ReceiverExternalPort.Equals(ReceiverExternalPort) &&
                       header.ReceiverIPAddress.SequenceEqual(ReceiverIPAddress) &&
                       header.ReceiverPeerId.SequenceEqual(ReceiverPeerId) &&
                       header.SenderExternalPort.Equals(SenderExternalPort) &&
                       header.SenderIPAddress.SequenceEqual(SenderIPAddress) &&
                       header.SenderPeerId.SequenceEqual(SenderPeerId);
            }
            return false;
        }
    }

    /// <summary>
    /// Super class of all messages
    /// containing a MessageHeader and the Payload
    /// </summary>
    internal class Message
    {
        public MessageHeader MessageHeader;
        public byte[] Payload = new byte[0];                 //length defined by header.PayloadLength
        public byte VoluntLibVersion = Constants.MESSAGE_VOLUNTLIB2_VERSION;

        public Message()
        {
            MessageHeader = new MessageHeader
            {
                MessageType = MessageType.Undefined,
                MessageId = Guid.NewGuid().ToByteArray()
            };
            Payload = new byte[0];
        }

        public virtual byte[] Serialize()
        {
            if (Payload != null && Payload.Length != 0)
            {
                MessageHeader.PayloadLength = (ushort)Payload.Length;
            }
            else
            {
                MessageHeader.PayloadLength = 0;
            }

            byte[] magicNumber = Encoding.ASCII.GetBytes(Constants.MESSAGE_VOLUNTLIB2);       //10 bytes
                                                                                              // 1 byte protocol versin
            byte[] headerbytes = MessageHeader.Serialize();                 //63 bytes

            ushort payloadLengthBytes = (ushort)(Payload != null ? Payload.Length : 0);
            byte[] messagebytes = new byte[10 + 1 + 63 + payloadLengthBytes];

            Array.Copy(magicNumber, 0, messagebytes, 0, 10);
            messagebytes[10] = Constants.MESSAGE_VOLUNTLIB2_VERSION;
            Array.Copy(headerbytes, 0, messagebytes, 11, 63);
            if (Payload != null && Payload.Length > 0)
            {
                Array.Copy(Payload, 0, messagebytes, 74, Payload.Length);
            }

            return messagebytes;
        }
        public virtual void Deserialize(byte[] data)
        {
            if (data.Length < 74)
            {
                throw new VoluntLibSerializationException(string.Format("Invalid message received. Expected minimum 74 bytes. Got {0} bytes!", data.Length));
            }
            string magicnumber = Encoding.ASCII.GetString(data, 0, 10);
            if (!magicnumber.Equals(Constants.MESSAGE_VOLUNTLIB2))
            {
                throw new VoluntLibSerializationException(string.Format("Invalid magic number. Expected '{0}'. Received '{1}'", Constants.MESSAGE_VOLUNTLIB2, magicnumber));
            }
            if (data[10] > Constants.MESSAGE_VOLUNTLIB2_VERSION)
            {
                throw new VoluntLibSerializationException(string.Format("Expected a VoluntLib2 version <= {0}. Received version = {1}. Please update your VoluntLib2 or the application using it!", Constants.MESSAGE_VOLUNTLIB2_VERSION, (int)data[10]));
            }

            MessageHeader = new MessageHeader();
            byte[] messageheaderbytes = new byte[63];
            VoluntLibVersion = data[10];
            Array.Copy(data, 11, messageheaderbytes, 0, 63);
            MessageHeader.Deserialize(messageheaderbytes);
            Payload = new byte[MessageHeader.PayloadLength];
            Array.Copy(data, 74, Payload, 0, Payload.Length);
        }

        /// <summary>
        /// Compares all fields of given Message with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            Message message = value as Message;
            if (message != null)
            {
                return message.MessageHeader.Equals(MessageHeader) &&
                       message.Payload.SequenceEqual(Payload) &&
                       message.VoluntLibVersion.Equals(VoluntLibVersion);
            }
            return false;
        }
    }

    /// <summary>
    /// A HelloMessage which is answered by a HelloResponseMessage
    /// The message contains a nonce for identifcation of the HelloResponse
    /// </summary>
    internal class HelloMessage : Message
    {
        public byte[] HelloNonce = Guid.NewGuid().ToByteArray();

        public HelloMessage() : base()
        {
            MessageHeader.MessageType = MessageType.HelloMessage;
        }

        public override byte[] Serialize()
        {
            Payload = HelloNonce;
            return base.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            HelloNonce = Payload;
        }

        /// <summary>
        /// Compares all fields of given HelloMessage with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            HelloMessage helloMessage = value as HelloMessage;
            if (helloMessage != null)
            {
                return base.Equals(helloMessage) &&
                       helloMessage.HelloNonce.SequenceEqual(HelloNonce);
            }
            return false;
        }
    }

    /// <summary>
    /// A HelloResponseMessage is the answer to a HelloMessage
    /// Must contain the nonce of the HelloMessage which is answered
    /// </summary>
    internal class HelloResponseMessage : Message
    {
        public byte[] HelloResponseNonce = new byte[16];

        public HelloResponseMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.HelloResponseMessage;
        }

        public override byte[] Serialize()
        {
            Payload = HelloResponseNonce;
            return base.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            HelloResponseNonce = Payload;
        }

        /// <summary>
        /// Compares all fields of given HelloResponseMessage with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            HelloResponseMessage helloResponseMessage = value as HelloResponseMessage;
            if (helloResponseMessage != null)
            {
                return base.Equals(helloResponseMessage) &&
                       helloResponseMessage.HelloResponseNonce.SequenceEqual(HelloResponseNonce);
            }
            return false;
        }
    }

    /// <summary>
    /// A RequestNeighborListMessage asks a neighbor to send all of his neighbors
    /// </summary>
    internal class RequestNeighborListMessage : Message
    {
        public byte[] RequestNeighborListNonce = Guid.NewGuid().ToByteArray();
        public RequestNeighborListMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.RequestNeighborListMessage;
        }

        public override byte[] Serialize()
        {
            Payload = RequestNeighborListNonce;
            return base.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            RequestNeighborListNonce = Payload;
        }

        /// <summary>
        /// Compares all fields of given RequestNeighborListMessage with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            RequestNeighborListMessage requestNeighborListMessage = value as RequestNeighborListMessage;
            if (requestNeighborListMessage != null)
            {
                return base.Equals(requestNeighborListMessage) &&
                       requestNeighborListMessage.RequestNeighborListNonce.SequenceEqual(RequestNeighborListNonce);
            }
            return false;
        }
    }

    /// <summary>
    /// A ResponseNeighborListMessage contains a list of neighbors send from one peer to another
    /// </summary>
    internal class ResponseNeighborListMessage : Message
    {
        public byte[] ResponseNeighborListNonce = new byte[16];
        public List<Contact> Neighbors = new List<Contact>();

        public ResponseNeighborListMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.ResponseNeighborListMessage;
        }

        public override byte[] Serialize()
        {
            byte[] data = new byte[16 + 2 + Neighbors.Count * 22];
            Array.Copy(ResponseNeighborListNonce, data, 16);
            Array.Copy(BitConverter.GetBytes((ushort)Neighbors.Count), 0, data, 16, 2);
            uint offset = 18;
            foreach (Contact contact in Neighbors)
            {
                Array.Copy(contact.Serialize(), 0, data, offset, 22);
                offset += 22;
            }
            Payload = data;
            return base.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            ResponseNeighborListNonce = new byte[16];
            Array.Copy(Payload, ResponseNeighborListNonce, 16);
            ushort count = BitConverter.ToUInt16(Payload, 16);

            for (uint offset = 18; offset < 18 + count * 22; offset += 22)
            {
                Contact contact = new Contact();
                byte[] array = new byte[22];
                Array.Copy(Payload, offset, array, 0, 22);
                contact.Deserialize(array);
                Neighbors.Add(contact);
            }
        }

        /// <summary>
        /// Compares all fields of given ResponseNeighborListMessage with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            ResponseNeighborListMessage responseNeighborListMessage = value as ResponseNeighborListMessage;
            if (responseNeighborListMessage != null)
            {
                //check, if number of contacts is equal
                if (Neighbors.Count != responseNeighborListMessage.Neighbors.Count)
                {
                    return false;
                }
                //check if all contacts are equal
                for (int i = 0; i < Neighbors.Count; i++)
                {
                    Contact a = Neighbors[i];
                    Contact b = responseNeighborListMessage.Neighbors[i];
                    if (!a.Equals(b))
                    {
                        return false;
                    }
                }
                return base.Equals(responseNeighborListMessage);
            }
            return false;
        }
    }

    /// <summary>
    /// Send from a peer to another to request help for connection to another peer
    /// </summary>
    internal class HelpMeConnectMessage : Message
    {
        public ushort Port = 0;
        public IPAddress IPAddress = new IPAddress(new byte[] { 0, 0, 0, 0 });

        /// <summary>
        /// Creates a HelpMeConnectMessage
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public HelpMeConnectMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.HelpMeConnectMessage;
        }

        public override byte[] Serialize()
        {
            Payload = new byte[6];
            byte[] ipbytes = IPAddress.GetAddressBytes();
            byte[] portbytes = BitConverter.GetBytes(Port);
            Payload[0] = ipbytes[0];
            Payload[1] = ipbytes[1];
            Payload[2] = ipbytes[2];
            Payload[3] = ipbytes[3];
            Payload[4] = portbytes[0];
            Payload[5] = portbytes[1];
            return base.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            IPAddress = new IPAddress(new byte[] { Payload[0], Payload[1], Payload[2], Payload[3] });
            Port = BitConverter.ToUInt16(Payload, 4);
        }

        /// <summary>
        /// Compares all fields of given HelpMeConnectMessage with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            HelpMeConnectMessage helpMeConnectMessage = value as HelpMeConnectMessage;
            if (helpMeConnectMessage != null)
            {
                return base.Equals(helpMeConnectMessage) &&
                       helpMeConnectMessage.IPAddress.Equals(IPAddress) &&
                       helpMeConnectMessage.Port.Equals(Port);
            }
            return false;
        }
    }

    /// <summary>
    /// Send from a peer to another to tell him, which peers wants to connect to him
    /// </summary>
    internal class WantsConnectionMessage : Message
    {
        public ushort Port = 0;
        public IPAddress IPAddress = new IPAddress(new byte[] { 0, 0, 0, 0 });

        /// <summary>
        /// Creates a WantsConnectionMessage
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public WantsConnectionMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.WantsConnectionMessage;
        }

        public override byte[] Serialize()
        {
            Payload = new byte[6];
            byte[] ipbytes = IPAddress.GetAddressBytes();
            byte[] portbytes = BitConverter.GetBytes(Port);
            Payload[0] = ipbytes[0];
            Payload[1] = ipbytes[1];
            Payload[2] = ipbytes[2];
            Payload[3] = ipbytes[3];
            Payload[4] = portbytes[0];
            Payload[5] = portbytes[1];
            return base.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            IPAddress = new IPAddress(new byte[] { Payload[0], Payload[1], Payload[2], Payload[3] });
            Port = BitConverter.ToUInt16(Payload, 4);
        }

        /// <summary>
        /// Compares all fields of given WantsConnectionMessage with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            WantsConnectionMessage wantsConnectionMessage = value as WantsConnectionMessage;
            if (wantsConnectionMessage != null)
            {
                return base.Equals(wantsConnectionMessage) &&
                       wantsConnectionMessage.IPAddress.Equals(IPAddress) &&
                       wantsConnectionMessage.Port.Equals(Port);
            }
            return false;
        }
    }

    /// <summary>
    /// A message for sending data to a peer
    /// </summary>
    internal class DataMessage : Message
    {
        public DataMessage()
        {
            MessageHeader.MessageType = MessageType.DataMessage;
        }

        /// <summary>
        /// Compares all fields of given DataMessage with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            DataMessage dataMessage = value as DataMessage;
            if (dataMessage != null)
            {
                return base.Equals(dataMessage);
            }
            return false;
        }
    }

    /// <summary>
    /// A message for informing every other peer that this peer goes offline now
    /// </summary>
    internal class GoingOfflineMessage : Message
    {
        public GoingOfflineMessage()
        {
            MessageHeader.MessageType = MessageType.GoingOfflineMessage;
        }

        /// <summary>
        /// Compares all fields of given GoingOfflineMessage with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            GoingOfflineMessage goingOfflineMessage = value as GoingOfflineMessage;
            if (goingOfflineMessage != null)
            {
                return base.Equals(goingOfflineMessage);
            }
            return false;
        }
    }
}
