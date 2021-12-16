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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using VoluntLib2.Tools;

namespace VoluntLib2.ManagementLayer.Messages
{
    /// <summary>
    /// Each message has a unique type number defined by this enum
    /// </summary>
    public enum MessageType
    {
        Undefined = 0,

        RequestJobListMessage = 10,
        ResponseJobListMessage = 11,

        RequestJobMessage = 20,
        ResponseJobMessage = 21,
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
                case MessageType.RequestJobListMessage:
                    message = new RequestJobListMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.ResponseJobListMessage:
                    message = new ResponseJobListMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.RequestJobMessage:
                    message = new RequestJobMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.ResponseJobMessage:
                    message = new ResponseJobMessage();
                    message.Deserialize(data);
                    return message;
                //add new message types here

                default:
                    throw new VoluntLibSerializationException(string.Format("Received a message of an unknown MessageType: {0}", message.MessageHeader.MessageType));
            }
        }
    }

    /// <summary>
    /// The header of all messages of VoluntLib2 JobManagementLayer
    /// </summary>
    public class MessageHeader
    {
        public byte[] MessageId = new byte[16];        // 16 bytes
        public MessageType MessageType;                // 1 byte
        public ushort PayloadLength;                   // 2 bytes
        //public ushort WorldNameLength;               // 2 bytes
        public string WorldName;                       // WorldNameLength bytes
        //public ushort SenderNameLength;              // 2 bytes
        public string SenderName;                      // SenderNameLength bytes
        //public ushort CertificateLength;             // 2 bytes
        public byte[] CertificateData = new byte[0];   // CertificateLength bytes
        //public ushort SignatureLength;               // 2 bytes
        public byte[] SignatureData = new byte[0];     // SignatureLength bytes

        public MessageHeader()
        {
            MessageType = MessageType.Undefined;
            PayloadLength = 0;
            WorldName = string.Empty;
            SenderName = string.Empty;
        }

        /// <summary>
        /// Creates a serialization of this message and returns it in a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            //World Name
            if (WorldName.Length > Constants.MESSAGES_STRING_MAX_LENGTH)
            {
                WorldName = WorldName.Substring(0, Constants.MESSAGES_STRING_MAX_LENGTH);
            }
            //convert World Name to byte array and get its length
            byte[] worldNameBytes = UTF8Encoding.UTF8.GetBytes(WorldName);
            int worldNameLength = worldNameBytes.Length;

            //Sender Name
            if (SenderName.Length > Constants.MESSAGES_STRING_MAX_LENGTH)
            {
                SenderName = SenderName.Substring(0, Constants.MESSAGES_STRING_MAX_LENGTH);
            }
            //convert Sender Name to byte array and get its length
            byte[] senderNameBytes = UTF8Encoding.UTF8.GetBytes(SenderName);
            int senderNameLength = senderNameBytes.Length;

            byte[] data = new byte[16 + 1 + 2 + 2 + worldNameBytes.Length + 2 + senderNameBytes.Length + 2 + CertificateData.Length + 2 + SignatureData.Length];

            Array.Copy(MessageId, 0, data, 0, 16);

            data[16] = (byte)MessageType;

            byte[] payloadLengthBytes = BitConverter.GetBytes(PayloadLength);
            data[17] = payloadLengthBytes[0];
            data[18] = payloadLengthBytes[1];

            byte[] worldNameLengthBytes = BitConverter.GetBytes(worldNameLength);
            data[19] = worldNameLengthBytes[0];
            data[20] = worldNameLengthBytes[1];
            Array.Copy(worldNameBytes, 0, data, 21, worldNameBytes.Length);

            byte[] senderNameLengthBytes = BitConverter.GetBytes(senderNameLength);
            data[21 + worldNameBytes.Length] = senderNameLengthBytes[0];
            data[21 + worldNameBytes.Length + 1] = senderNameLengthBytes[1];
            Array.Copy(senderNameBytes, 0, data, 21 + worldNameBytes.Length + 2, senderNameBytes.Length);

            //Certificate Data
            int certificateDataLength = CertificateData.Length;
            byte[] certificateDataLengthBytes = BitConverter.GetBytes(certificateDataLength);
            data[23 + worldNameBytes.Length + senderNameBytes.Length] = certificateDataLengthBytes[0];
            data[23 + worldNameBytes.Length + senderNameBytes.Length + 1] = certificateDataLengthBytes[1];
            Array.Copy(CertificateData, 0, data, 23 + worldNameBytes.Length + senderNameBytes.Length + 2, CertificateData.Length);

            //Signature Data
            int signatureDataLength = SignatureData.Length;
            byte[] signatureDataLengthBytes = BitConverter.GetBytes(signatureDataLength);
            data[25 + worldNameBytes.Length + senderNameBytes.Length + CertificateData.Length] = signatureDataLengthBytes[0];
            data[25 + worldNameBytes.Length + senderNameBytes.Length + CertificateData.Length + 1] = signatureDataLengthBytes[1];
            Array.Copy(SignatureData, 0, data, 25 + worldNameBytes.Length + senderNameBytes.Length + CertificateData.Length + 2, SignatureData.Length);

            return data;
        }

        /// <summary>
        /// Deserialize a message using the given byte array
        /// </summary>
        /// <param name="data"></param>
        public void Deserialize(byte[] data)
        {
            MessageId = new byte[16];
            Array.Copy(data, 0, MessageId, 0, 16);

            MessageType = (MessageType)data[16];

            PayloadLength = BitConverter.ToUInt16(data, 17);

            int worldNameLength = BitConverter.ToUInt16(data, 19);
            WorldName = UTF8Encoding.UTF8.GetString(data, 21, worldNameLength);

            int senderNameLength = BitConverter.ToUInt16(data, 21 + worldNameLength);
            SenderName = UTF8Encoding.UTF8.GetString(data, 23 + worldNameLength, senderNameLength);

            int certificateDataLength = BitConverter.ToUInt16(data, 23 + worldNameLength + senderNameLength);
            CertificateData = new byte[certificateDataLength];
            Array.Copy(data, 25 + worldNameLength + senderNameLength, CertificateData, 0, certificateDataLength);

            int signatureDataLength = BitConverter.ToUInt16(data, 25 + worldNameLength + senderNameLength + certificateDataLength);
            SignatureData = new byte[signatureDataLength];
            Array.Copy(data, 27 + worldNameLength + senderNameLength + certificateDataLength, SignatureData, 0, signatureDataLength);
        }

        /// <summary>
        /// Returns a string representation of this message
        /// </summary>
        /// <returns></returns>
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
            builder.Append("  WorldName: ");
            builder.AppendLine("" + WorldName + ",");
            builder.Append("  SenderName: ");
            builder.AppendLine("" + SenderName + ",");
            builder.Append("  CertificateData: ");
            builder.AppendLine("" + BitConverter.ToString(CertificateData) + ",");
            builder.Append("  SignatureData: ");
            builder.AppendLine("" + BitConverter.ToString(SignatureData));
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
                return header.CertificateData.SequenceEqual(CertificateData) &&
                       header.MessageId.SequenceEqual(MessageId) &&
                       header.MessageType.Equals(MessageType) &&
                       header.PayloadLength.Equals(PayloadLength) &&
                       header.SenderName.Equals(SenderName) &&
                       header.SignatureData.SequenceEqual(SignatureData) &&
                       header.WorldName.Equals(WorldName);
            }
            return false;
        }
    }

    /// <summary>
    /// Super class of all messages
    /// containing a MessageHeader and the Payload
    /// </summary>
    public class Message
    {
        public byte[] PeerId = new byte[0];             //set by receiving thread; will not be serialized
        public MessageHeader MessageHeader;
        public byte[] Payload = new byte[0];            //length defined by header.PayloadLength
        public byte VoluntLibVersion = Constants.MGM_MESSAGE_VOLUNTLIB2_VERSION;

        public Message()
        {
            MessageHeader = new MessageHeader
            {
                MessageType = MessageType.Undefined,
                MessageId = Guid.NewGuid().ToByteArray()
            };
        }

        /// <summary>
        /// Serializes the message to a byte array.
        /// if signMessage == false, the MessageHeader.SignatureData is byte[0] after calling this method.
        /// if signMessage == true, the MessageHeader.SignatureData is the signature of the method after calling this method.
        /// Uses the cert given to the CertificateService
        /// </summary>
        /// <param name="signMessage"></param>
        /// <returns></returns>
        public virtual byte[] Serialize(bool signMessage = true)
        {
            if (Payload != null && Payload.Length != 0)
            {
                MessageHeader.PayloadLength = (ushort)Payload.Length;
            }
            else
            {
                MessageHeader.PayloadLength = 0;
            }

            MessageHeader.SignatureData = new byte[0];

            byte[] magicNumber = Encoding.ASCII.GetBytes(Constants.MESSAGE_VLIB2MNGMT);       //10 bytes
            // 1 byte protocol version
            byte[] headerbytes = MessageHeader.Serialize();

            ushort payloadLengthBytes = (ushort)(Payload != null ? Payload.Length : 0);
            byte[] messagebytes = new byte[10 + 1 + headerbytes.Length + payloadLengthBytes];

            Array.Copy(magicNumber, 0, messagebytes, 0, 10);
            messagebytes[10] = Constants.MGM_MESSAGE_VOLUNTLIB2_VERSION;
            Array.Copy(headerbytes, 0, messagebytes, 11, headerbytes.Length);
            if (Payload != null && Payload.Length > 0)
            {
                Array.Copy(Payload, 0, messagebytes, 11 + headerbytes.Length, Payload.Length);
            }

            //If we don't sign the message, we are finished here
            if (!signMessage)
            {
                return messagebytes;
            }

            byte[] signature = CertificateService.GetCertificateService().SignData(messagebytes);
            MessageHeader.SignatureData = signature;

            headerbytes = MessageHeader.Serialize();
            messagebytes = new byte[10 + 1 + headerbytes.Length + payloadLengthBytes];
            Array.Copy(magicNumber, 0, messagebytes, 0, 10);
            messagebytes[10] = Constants.MGM_MESSAGE_VOLUNTLIB2_VERSION;
            Array.Copy(headerbytes, 0, messagebytes, 11, headerbytes.Length);
            if (Payload != null && Payload.Length > 0)
            {
                Array.Copy(Payload, 0, messagebytes, 11 + headerbytes.Length, Payload.Length);
            }

            return messagebytes;
        }

        /// <summary>
        /// Deserialize a message using the given byte array
        /// </summary>
        /// <param name="data"></param>
        public virtual void Deserialize(byte[] data)
        {
            if (data.Length < 27)
            {
                throw new VoluntLibSerializationException(string.Format("Invalid message received. Expected minimum 27 bytes. Got {0} bytes!", data.Length));
            }
            string magicnumber = Encoding.ASCII.GetString(data, 0, 10);
            if (!magicnumber.Equals(Constants.MESSAGE_VLIB2MNGMT))
            {
                throw new VoluntLibSerializationException(string.Format("Invalid magic number. Expected '{0}'. Received '{1}'", Constants.MESSAGE_VLIB2MNGMT, magicnumber));
            }
            if (data[10] > Constants.MGM_MESSAGE_VOLUNTLIB2_VERSION)
            {
                throw new VoluntLibSerializationException(string.Format("Expected a VoluntLib2 version <= {0}. Received a version = {1}. Please update your VoluntLib2 or the application using it!", Constants.MGM_MESSAGE_VOLUNTLIB2_VERSION, (int)data[10]));
            }

            MessageHeader = new MessageHeader();
            byte[] messageheaderbytes = new byte[data.Length - 11];
            VoluntLibVersion = data[10];
            Array.Copy(data, 11, messageheaderbytes, 0, messageheaderbytes.Length);
            MessageHeader.Deserialize(messageheaderbytes);
            Payload = new byte[MessageHeader.PayloadLength];
            Array.Copy(data, data.Length - Payload.Length, Payload, 0, Payload.Length);
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
    /// Message to request the job lists of a neighbor
    /// </summary>
    internal class RequestJobListMessage : Message
    {
        public RequestJobListMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.RequestJobListMessage;
        }

        /// <summary>
        /// Compares all fields of given RequestJobListMessage with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            RequestJobListMessage requestJobListMessage = value as RequestJobListMessage;
            if (requestJobListMessage != null)
            {
                return base.Equals(requestJobListMessage);
            }
            return false;
        }
    }

    /// <summary>
    /// Message to answer the job request of a neighbor
    /// </summary>
    internal class ResponseJobListMessage : Message
    {
        public List<Job> Jobs = new List<Job>();
        public ResponseJobListMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.ResponseJobListMessage;
        }

        public override byte[] Serialize(bool signMessage = true)
        {
            Payload = SerializeJobList();
            return base.Serialize(signMessage);
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);

            Jobs = DeserializeJobList(Payload);
        }

        private byte[] SerializeJobList()
        {
            int size = 0;
            ushort numberOfJobs = (ushort)Jobs.Count;
            byte[] numberOfJobsBytes = BitConverter.GetBytes(numberOfJobs);
            size += 2;

            List<byte[]> serializedJobs = new List<byte[]>();

            foreach (Job job in Jobs)
            {
                byte[] serializedJob = job.Serialize();
                serializedJobs.Add(serializedJob);
                size += 2;      // size of job field
                size += serializedJob.Length;
            }

            byte[] data = new byte[size];
            int offset = 0;
            Array.Copy(numberOfJobsBytes, 0, data, offset, 2);
            offset += 2;

            foreach (byte[] jobBytes in serializedJobs)
            {
                byte[] jobSizeBytes = BitConverter.GetBytes((ushort)jobBytes.Length);
                Array.Copy(jobSizeBytes, 0, data, offset, 2);
                offset += 2;
                Array.Copy(jobBytes, 0, data, offset, jobBytes.Length);
                offset += jobBytes.Length;
            }
            return data;
        }

        private List<Job> DeserializeJobList(byte[] Payload)
        {
            List<Job> jobList = new List<Job>();
            int offset = 0;
            ushort numberOfJobs = BitConverter.ToUInt16(Payload, offset);
            offset += 2;
            for (int i = 0; i < numberOfJobs; i++)
            {
                ushort jobSize = BitConverter.ToUInt16(Payload, offset);
                offset += 2;
                byte[] jobBytes = new byte[jobSize];
                Array.Copy(Payload, offset, jobBytes, 0, jobSize);
                Job job = new Job(0);
                job.Deserialize(jobBytes);
                jobList.Add(job);
                offset += jobSize;
            }
            return jobList;
        }

        /// <summary>
        /// Compares all fields of given ResponseJobListMessage with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            ResponseJobListMessage responseJobListMessage = value as ResponseJobListMessage;
            if (responseJobListMessage != null)
            {
                //check, if number of jobs in list are equal
                if (responseJobListMessage.Jobs.Count != Jobs.Count)
                {
                    return false;
                }

                for (int i = 0; i < Jobs.Count; i++)
                {
                    Job joba = responseJobListMessage.Jobs[i];
                    Job jobb = Jobs[i];
                    if (!joba.Equals(jobb))
                    {
                        return false;
                    }
                }

                return base.Equals(responseJobListMessage);
            }

            return false;
        }
    }

    /// <summary>
    /// Message to request details of a job
    /// </summary>
    internal class RequestJobMessage : Message
    {
        public BigInteger JobId;

        public RequestJobMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.RequestJobMessage;
        }

        public override byte[] Serialize(bool signMessage = true)
        {
            Payload = JobId.ToByteArray();
            return base.Serialize(signMessage);
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            JobId = new BigInteger(Payload);
        }

        /// <summary>
        /// Compares all fields of given RequestJobMessage with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            RequestJobMessage requestJobMessage = value as RequestJobMessage;
            if (requestJobMessage != null)
            {
                return base.Equals(requestJobMessage) &&
                    requestJobMessage.JobId.Equals(JobId);
            }
            return false;
        }
    }

    /// <summary>
    /// Message to response the details, i.e. Payload, of a job
    /// </summary>
    internal class ResponseJobMessage : Message
    {
        public Job Job { get; set; }

        public ResponseJobMessage()
            : base()
        {
            Job = new Job(0);
            MessageHeader.MessageType = MessageType.ResponseJobMessage;
        }

        public override byte[] Serialize(bool signMessage = true)
        {
            Payload = Job.Serialize();
            return base.Serialize(signMessage);
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            Job.Deserialize(Payload);
        }

        /// <summary>
        /// Compares all fields of given ResponseJobMessage with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            ResponseJobMessage responseJobMessage = value as ResponseJobMessage;
            if (responseJobMessage != null)
            {
                return base.Equals(responseJobMessage) &&
                       responseJobMessage.Job.Equals(Job);
            }
            return false;
        }
    }
}
