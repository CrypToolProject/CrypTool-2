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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using VoluntLib2.ComputationLayer;
using VoluntLib2.Tools;

namespace VoluntLib2.ManagementLayer
{
    public class Job : IComparable<Job>, INotifyPropertyChanged, IVoluntLibSerializable
    {
        private readonly Logger Logger = Logger.GetLogger();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="jobID"></param>
        public Job(BigInteger jobID)
        {
            JobId = jobID;
            JobName = string.Empty;
            JobType = string.Empty;
            JobDescription = string.Empty;
            WorldName = string.Empty;
            CreatorName = string.Empty;
            NumberOfBlocks = BigInteger.Zero;
            CreatorCertificateData = new byte[0];
            JobPayloadHash = new byte[0];
            JobCreatorSignatureData = new byte[0];
            JobDeletionSignatureData = new byte[0];
            JobPayload = new byte[0];
            LastPayloadRequestTime = DateTime.MinValue;
            IsDeleted = false;
            JobEpochState = new EpochState();
            //we set progress and old progress to -1; thus it will be updated
            //by UpdateJobsProgressOperation one time and the progress will
            //be displayed in ui of crypcloud
            Progress = -1;
            EpochProgress = -1;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public BigInteger JobId { get; set; }
        public string JobIdAsString
        {
            get
            {
                if (JobId != null)
                {
                    return ConvertJobId(JobId);
                }
                return string.Empty;
            }
        }
        public string JobName { get; set; }
        public string JobType { get; set; }
        public string JobDescription { get; set; }
        public string WorldName { get; set; }
        public string CreatorName { get; set; }
        public BigInteger NumberOfBlocks { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastPayloadRequestTime { get; set; }
        public byte[] CreatorCertificateData { get; set; }
        public byte[] JobPayloadHash { get; set; }
        public byte[] JobCreatorSignatureData { get; set; }
        public byte[] JobDeletionSignatureData { get; set; }
        public byte[] JobPayload { get; set; }
        public EpochState JobEpochState { get; set; }
        public double Progress { get; set; }
        public string ProgressText { get; set; }
        public double EpochProgress { get; set; }
        public string EpochProgressText { get; set; }

        internal bool NotifiedJobCompletion = false;

        /// <summary>
        /// User can delete job, if (A) its his job or (B) he is an admin
        /// </summary>
        public bool UserCanDeleteJob => CertificateService.GetCertificateService().OwnName.Equals(CreatorName) ||
                    CertificateService.GetCertificateService().IsAdminCertificate(CertificateService.GetCertificateService().OwnCertificate);

        /// <summary>
        /// Returns the size of this job in bytes
        /// </summary>
        public long JobSize
        {
            get
            {
                long size = 0;
                size += JobId.ToByteArray().Length;
                size += UTF8Encoding.UTF8.GetBytes(JobName).Length;
                size += UTF8Encoding.UTF8.GetBytes(JobType).Length;
                size += UTF8Encoding.UTF8.GetBytes(JobDescription).Length;
                size += UTF8Encoding.UTF8.GetBytes(WorldName).Length;
                size += UTF8Encoding.UTF8.GetBytes(CreatorName).Length;
                size += NumberOfBlocks.ToByteArray().Length;
                size += 8; //CreationDate
                size += CreatorCertificateData != null ? CreatorCertificateData.Length : 0;
                size += JobPayloadHash != null ? JobPayloadHash.Length : 0;
                size += JobCreatorSignatureData != null ? JobCreatorSignatureData.Length : 0;
                size += JobDeletionSignatureData != null ? JobDeletionSignatureData.Length : 0;
                size += JobPayload != null ? JobPayload.Length : 0;
                size += JobEpochState != null ? JobEpochState.GetSize() : 0;
                return size;
            }
        }
        public bool IsDeleted { get; set; }

        public DateTime DeletionTime
        {
            get
            {
                if (JobDeletionSignatureData == null || JobDeletionSignatureData.Length == 0)
                {
                    return DateTime.MinValue;
                }
                string magicNumber = UTF8Encoding.UTF8.GetString(JobDeletionSignatureData, 0, 4);
                if (magicNumber.Equals("USER") || magicNumber.Equals("ADMN"))
                {
                    byte[] time = new byte[8];
                    Array.Copy(JobDeletionSignatureData, 4 + 4 + JobId.ToByteArray().Length, time, 0, 8);
                    return DateTime.FromBinary(BitConverter.ToInt64(time, 0));
                }                
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Compares all fields of given Job with this one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            Job job = value as Job;
            if (job != null)
            {
                return job.JobId.Equals(JobId);
            }
            return false;
        }

        /// <summary>
        /// Only used for testing serialization/deserialization of jobs
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        internal bool Equals_Test(Job job)
        {
            return job.CreationDate.Equals(CreationDate) &&
                   job.CreatorCertificateData.SequenceEqual(CreatorCertificateData) &&
                   job.CreatorName.Equals(CreatorName) &&
                   job.JobCreatorSignatureData.SequenceEqual(JobCreatorSignatureData) &&
                   job.JobDeletionSignatureData.SequenceEqual(JobDeletionSignatureData) &&
                   job.JobDescription.Equals(JobDescription) &&
                   job.JobEpochState.Equals(JobEpochState) &&
                   job.JobId.Equals(JobId) &&
                   job.JobName.Equals(JobName) &&
                   job.JobPayload.SequenceEqual(JobPayload) &&
                   job.JobPayloadHash.SequenceEqual(JobPayloadHash) &&
                   job.JobType.Equals(JobType) &&
                   job.WorldName.Equals(WorldName);
        }

        public bool HasPayload => JobPayload != null && JobPayload.Length > 0;
        public override int GetHashCode()
        {
            return JobId.GetHashCode();
        }

        /// <summary>
        /// Serializes the job to a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            //0. Trim string length to max lengths
            if (JobName.Length > Constants.JOB_STRING_MAX_LENGTH)
            {
                JobName = JobName.Substring(0, Constants.JOB_STRING_MAX_LENGTH);
            }
            if (JobType.Length > Constants.JOB_STRING_MAX_LENGTH)
            {
                JobType = JobType.Substring(0, Constants.JOB_STRING_MAX_LENGTH);
            }
            if (JobDescription.Length > Constants.JOB_STRING_MAX_JOB_DESCRIPTION_LENGTH)
            {
                JobDescription = JobDescription.Substring(0, Constants.JOB_STRING_MAX_LENGTH);
            }
            if (WorldName.Length > Constants.JOB_STRING_MAX_LENGTH)
            {
                WorldName = WorldName.Substring(0, Constants.JOB_STRING_MAX_LENGTH);
            }
            if (CreatorName.Length > Constants.JOB_STRING_MAX_LENGTH)
            {
                CreatorName = CreatorName.Substring(0, Constants.JOB_STRING_MAX_LENGTH);
            }

            //1. Convert all to byte arrays + "length of fields"-byte arrays; also calculate total size of data array
            int length = 0;
            byte[] jobIdBytes = JobId.ToByteArray();
            byte[] jobIdLength = BitConverter.GetBytes((ushort)jobIdBytes.Length);
            length += (jobIdBytes.Length + jobIdLength.Length);

            byte[] jobNameBytes = UTF8Encoding.UTF8.GetBytes(JobName);
            byte[] jobNameLength = BitConverter.GetBytes((ushort)jobNameBytes.Length);
            length += (jobNameBytes.Length + jobNameLength.Length);

            byte[] jobTypeBytes = UTF8Encoding.UTF8.GetBytes(JobType);
            byte[] jobTypeLength = BitConverter.GetBytes((ushort)jobTypeBytes.Length);
            length += (jobTypeBytes.Length + jobTypeLength.Length);

            byte[] jobDescriptionBytes = UTF8Encoding.UTF8.GetBytes(JobDescription);
            byte[] jobDescriptionLength = BitConverter.GetBytes((ushort)jobDescriptionBytes.Length);
            length += (jobDescriptionBytes.Length + jobDescriptionLength.Length);

            byte[] worldNameBytes = UTF8Encoding.UTF8.GetBytes(WorldName);
            byte[] worldNameLength = BitConverter.GetBytes((ushort)worldNameBytes.Length);
            length += (worldNameBytes.Length + worldNameLength.Length);

            byte[] creatorBytes = UTF8Encoding.UTF8.GetBytes(CreatorName);
            byte[] creatorLength = BitConverter.GetBytes((ushort)creatorBytes.Length);
            length += (creatorBytes.Length + creatorLength.Length);

            byte[] numberOfBlocksBytes = NumberOfBlocks.ToByteArray();
            byte[] numberOfBlocksLength = BitConverter.GetBytes((ushort)numberOfBlocksBytes.Length);
            length += (numberOfBlocksBytes.Length + numberOfBlocksLength.Length);

            byte[] creationDateBytes = BitConverter.GetBytes(CreationDate.ToBinary());
            length += 8;

            byte[] creatorCertificateDataLength = BitConverter.GetBytes((ushort)CreatorCertificateData.Length);
            length += (creatorCertificateDataLength.Length + CreatorCertificateData.Length);

            byte[] jobPayloadHashLength = BitConverter.GetBytes((ushort)JobPayloadHash.Length);
            length += (jobPayloadHashLength.Length + JobPayloadHash.Length);

            byte[] jobCreatorSignatureDataLength = BitConverter.GetBytes((ushort)JobCreatorSignatureData.Length);
            length += (jobCreatorSignatureDataLength.Length + JobCreatorSignatureData.Length);

            byte[] jobDeletionSignatureDataLength = BitConverter.GetBytes((ushort)JobDeletionSignatureData.Length);
            length += (jobDeletionSignatureDataLength.Length + JobDeletionSignatureData.Length);

            byte[] jobPayloadLength = BitConverter.GetBytes((ushort)JobPayload.Length);
            length += (jobPayloadLength.Length + JobPayload.Length);

            byte[] jobEpochState = JobEpochState == null ? new byte[0] : JobEpochState.Serialize();
            byte[] jobEpochStateLength = BitConverter.GetBytes((ushort)jobEpochState.Length);
            length += (jobEpochStateLength.Length + jobEpochState.Length);

            //2. Generate final array using length; copy everyhting into array
            byte[] data = new byte[length];

            int offset = 0;
            Array.Copy(jobIdLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(jobIdBytes, 0, data, offset, jobIdBytes.Length);
            offset += jobIdBytes.Length;

            Array.Copy(jobNameLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(jobNameBytes, 0, data, offset, jobNameBytes.Length);
            offset += jobNameBytes.Length;

            Array.Copy(jobTypeLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(jobTypeBytes, 0, data, offset, jobTypeBytes.Length);
            offset += jobTypeBytes.Length;

            Array.Copy(jobDescriptionLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(jobDescriptionBytes, 0, data, offset, jobDescriptionBytes.Length);
            offset += jobDescriptionBytes.Length;

            Array.Copy(worldNameLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(worldNameBytes, 0, data, offset, worldNameBytes.Length);
            offset += worldNameBytes.Length;

            Array.Copy(creatorLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(creatorBytes, 0, data, offset, creatorBytes.Length);
            offset += creatorBytes.Length;

            Array.Copy(numberOfBlocksLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(numberOfBlocksBytes, 0, data, offset, numberOfBlocksBytes.Length);
            offset += numberOfBlocksBytes.Length;

            Array.Copy(creationDateBytes, 0, data, offset, creationDateBytes.Length);
            offset += 8;

            Array.Copy(creatorCertificateDataLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(CreatorCertificateData, 0, data, offset, CreatorCertificateData.Length);
            offset += CreatorCertificateData.Length;

            Array.Copy(jobPayloadHashLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(JobPayloadHash, 0, data, offset, JobPayloadHash.Length);
            offset += JobPayloadHash.Length;

            Array.Copy(jobCreatorSignatureDataLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(JobCreatorSignatureData, 0, data, offset, JobCreatorSignatureData.Length);
            offset += JobCreatorSignatureData.Length;

            Array.Copy(jobDeletionSignatureDataLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(JobDeletionSignatureData, 0, data, offset, JobDeletionSignatureData.Length);
            offset += JobDeletionSignatureData.Length;

            Array.Copy(jobPayloadLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(JobPayload, 0, data, offset, JobPayload.Length);
            offset += JobPayload.Length;

            Array.Copy(jobEpochStateLength, 0, data, offset, 2);
            offset += 2;
            Array.Copy(jobEpochState, 0, data, offset, jobEpochState.Length);

            return data;
        }

        /// <summary>
        /// Deserializes the job from a byte array
        /// </summary>
        /// <param name="data"></param>
        public void Deserialize(byte[] data)
        {
            int offset = 0;
            ushort jobIdLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            byte[] jobId = new byte[jobIdLength];
            Array.Copy(data, offset, jobId, 0, jobIdLength);
            JobId = new BigInteger(jobId);
            offset += jobIdLength;

            ushort jobNameLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobName = UTF8Encoding.UTF8.GetString(data, offset, jobNameLength);
            offset += jobNameLength;

            ushort jobTypeLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobType = UTF8Encoding.UTF8.GetString(data, offset, jobTypeLength);
            offset += jobTypeLength;

            ushort jobDescriptionLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobDescription = UTF8Encoding.UTF8.GetString(data, offset, jobDescriptionLength);
            offset += jobDescriptionLength;

            ushort worldNameLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            WorldName = UTF8Encoding.UTF8.GetString(data, offset, worldNameLength);
            offset += worldNameLength;

            ushort creatorLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            CreatorName = UTF8Encoding.UTF8.GetString(data, offset, creatorLength);
            offset += creatorLength;

            ushort numberOfBlocksLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            byte[] numberOfBlocks = new byte[numberOfBlocksLength];
            Array.Copy(data, offset, numberOfBlocks, 0, numberOfBlocksLength);
            NumberOfBlocks = new BigInteger(numberOfBlocks);
            offset += numberOfBlocksLength;

            CreationDate = DateTime.FromBinary(BitConverter.ToInt64(data, offset));
            offset += 8;

            ushort creatorCertificateDataLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            CreatorCertificateData = new byte[creatorCertificateDataLength];
            Array.Copy(data, offset, CreatorCertificateData, 0, creatorCertificateDataLength);
            offset += creatorCertificateDataLength;

            ushort jobPayloadHashLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobPayloadHash = new byte[jobPayloadHashLength];
            Array.Copy(data, offset, JobPayloadHash, 0, jobPayloadHashLength);
            offset += jobPayloadHashLength;

            ushort jobCreatorSignatureDataLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobCreatorSignatureData = new byte[jobCreatorSignatureDataLength];
            Array.Copy(data, offset, JobCreatorSignatureData, 0, jobCreatorSignatureDataLength);
            offset += jobCreatorSignatureDataLength;

            ushort jobDeletionSignatureDataLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobDeletionSignatureData = new byte[jobDeletionSignatureDataLength];
            Array.Copy(data, offset, JobDeletionSignatureData, 0, jobDeletionSignatureDataLength);
            offset += jobDeletionSignatureDataLength;

            ushort jobPayloadLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            JobPayload = new byte[jobPayloadLength];
            Array.Copy(data, offset, JobPayload, 0, jobPayloadLength);
            offset += jobPayloadLength;

            ushort jobEpochStateLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            if (jobEpochStateLength != 0)
            {
                byte[] jobEpochState = new byte[jobEpochStateLength];
                Array.Copy(data, offset, jobEpochState, 0, jobEpochStateLength);
                JobEpochState = new EpochState();
                JobEpochState.Deserialize(jobEpochState);
            }
            else
            {
                JobEpochState = null;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Job");
            builder.AppendLine("{");
            builder.Append("  JobId: ");
            builder.AppendLine(JobId + ",");
            builder.Append("  JobName: ");
            builder.AppendLine(JobName + ",");
            builder.Append("  JobType: ");
            builder.AppendLine("" + JobType + ",");
            builder.Append("  JobDescription: ");
            builder.AppendLine("" + JobDescription + ",");
            builder.Append("  WorldName: ");
            builder.AppendLine("" + WorldName + ",");
            builder.Append("  Creator: ");
            builder.AppendLine("" + CreatorName + ",");
            builder.Append("  NumberOfBlocks: ");
            builder.AppendLine("" + NumberOfBlocks + ",");
            builder.Append("  IsDeleted: ");
            builder.AppendLine("" + IsDeleted + ",");
            builder.Append("  HasPayload: ");
            builder.AppendLine("" + HasPayload + ",");
            builder.Append("  JobPayload: ");
            builder.AppendLine("" + BitConverter.ToString(JobPayload));
            builder.AppendLine("}");

            return builder.ToString();
        }

        /// <summary>
        /// Checks the creator signature
        /// </summary>
        /// <returns></returns>
        public bool HasValidCreatorSignature
        {
            get
            {
                try
                {
                    X509Certificate2 creatorCertificate = new X509Certificate2(CreatorCertificateData);

                    //some checks on the certificate                
                    if (creatorCertificate.SubjectName.Name.ToLower().Equals(Constants.JOB_ANONYMOUS_USER))
                    {
                        //it is not allowed to create a job using the anonymous user
                        return false;
                    }
                    if (!CertificateService.GetCertificateService().IsValidCertificate(creatorCertificate))
                    {
                        return false;
                    }
                    if (CertificateService.GetCertificateService().IsBannedCertificate(creatorCertificate))
                    {
                        return false;
                    }

                    //1. Backup some fields and remove them, since they are not used in signature
                    byte[] jobCreatorSignatureDataBackup = JobCreatorSignatureData;
                    JobCreatorSignatureData = new byte[0];
                    byte[] jobDeletionSignatureDataBackup = JobDeletionSignatureData;
                    JobDeletionSignatureData = new byte[0];
                    byte[] payloadBackup = JobPayload;
                    JobPayload = new byte[0];
                    EpochState epochState = JobEpochState;
                    JobEpochState = null;

                    //2. Serialize for signature check
                    byte[] data = Serialize();

                    //3. Copy backups back
                    JobCreatorSignatureData = jobCreatorSignatureDataBackup;
                    JobDeletionSignatureData = jobDeletionSignatureDataBackup;
                    JobPayload = payloadBackup;
                    JobEpochState = epochState;

                    //5. Check signature; return false if not valid
                    if (!CertificateService.GetCertificateService().VerifySignature(data, JobCreatorSignatureData, creatorCertificate).Equals(CertificateValidationState.Valid))
                    {
                        return false;
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public int CompareTo(Job other)
        {
            return -1 * CreationDate.CompareTo(other.CreationDate);
        }

        /// <summary>
        /// Adds a user deletion signature or an admin deletion singature to the job
        /// if the user created the job, it will be the user signature
        /// if the user did not AND is admint, it will be the admin signature
        /// </summary>
        /// <returns></returns>
        internal bool GenerateDeletionSignature()
        {
            bool userCreatedJob = CreatorName.Equals(CertificateService.GetCertificateService().OwnName);
            bool userIsAdmin = CertificateService.GetCertificateService().IsAdminCertificate(CertificateService.GetCertificateService().OwnCertificate);
            //case 1) When the user created the job, we can just add the signature of the user
            if (userCreatedJob)
            {
                //4  bytes    USER magic number
                //4  bytes    jobid length
                //n  bytes    jobid
                //8  bytes    deletion time
                //m  bytes    signature                
                byte[] jobIdBytes = JobId.ToByteArray();
                byte[] jobIdLengthBytes = BitConverter.GetBytes((uint)jobIdBytes.Length);
                byte[] text = new byte[4 + jobIdLengthBytes.Length + jobIdBytes.Length + 8];
                text[0] = (byte)'U';
                text[1] = (byte)'S';
                text[2] = (byte)'E';
                text[3] = (byte)'R';
                int offset = 4;
                Array.Copy(jobIdLengthBytes, 0, text, offset, jobIdLengthBytes.Length);
                offset += jobIdLengthBytes.Length;
                Array.Copy(jobIdBytes, 0, text, offset, jobIdBytes.Length);
                offset += jobIdBytes.Length;
                Array.Copy(BitConverter.GetBytes(DateTime.UtcNow.ToBinary()), 0, text, offset, 8);
                offset += 8;
                byte[] signature = CertificateService.GetCertificateService().SignData(text);
                byte[] data = new byte[text.Length + signature.Length];
                Array.Copy(text, 0, data, 0, text.Length);
                Array.Copy(signature, 0, data, text.Length, signature.Length);
                JobDeletionSignatureData = data;
                IsDeleted = true;
                return true;
            }
            //case 2) When the user is admin, we can add the admin deletion signature
            else if (userIsAdmin)
            {
                //4  bytes    ADMN magic number
                //4  bytes    jobid length
                //n  bytes    jobid
                //8  bytes    deletion time
                //4  bytes    certificate length
                //n  bytes    certificate data
                //m  bytes    signature                
                byte[] jobIdBytes = JobId.ToByteArray();
                byte[] jobIdLengthBytes = BitConverter.GetBytes(jobIdBytes.Length);
                byte[] certificateDataBytes = CertificateService.GetCertificateService().OwnCertificate.GetRawCertData();
                byte[] certificateDataLengthBytes = BitConverter.GetBytes(certificateDataBytes.Length);
                byte[] text = new byte[4 + jobIdLengthBytes.Length + jobIdBytes.Length + 8 + certificateDataBytes.Length + certificateDataLengthBytes.Length];
                text[0] = (byte)'A';
                text[1] = (byte)'D';
                text[2] = (byte)'M';
                text[3] = (byte)'N';
                int offset = 4;
                Array.Copy(jobIdLengthBytes, 0, text, offset, jobIdLengthBytes.Length);
                offset += jobIdLengthBytes.Length;
                Array.Copy(jobIdBytes, 0, text, offset, jobIdBytes.Length);
                offset += jobIdBytes.Length;
                Array.Copy(BitConverter.GetBytes(DateTime.UtcNow.ToBinary()), 0, text, offset, 8);
                offset += 8;
                Array.Copy(certificateDataLengthBytes, 0, text, offset, certificateDataLengthBytes.Length);
                offset += certificateDataLengthBytes.Length;
                Array.Copy(certificateDataBytes, 0, text, offset, certificateDataBytes.Length);
                offset += certificateDataBytes.Length;
                byte[] signature = CertificateService.GetCertificateService().SignData(text);
                byte[] data = new byte[text.Length + signature.Length];
                Array.Copy(text, 0, data, 0, text.Length);
                Array.Copy(signature, 0, data, text.Length, signature.Length);
                JobDeletionSignatureData = data;
                IsDeleted = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if job has a valid deletion signature
        /// </summary>
        /// <returns></returns>
        public bool HasValidDeletionSignature
        {
            get
            {
                try
                {
                    //1. Check if we have a DeletionSignature
                    if (JobDeletionSignatureData == null || JobDeletionSignatureData.Length == 0)
                    {
                        return false;
                    }

                    //2. Check magic number
                    string magicNumber = UTF8Encoding.UTF8.GetString(JobDeletionSignatureData, 0, 4);

                    // user deleted
                    if (magicNumber.Equals("USER"))
                    {
                        int offset = 4;
                        byte[] jobIdLengthBytes = new byte[4];
                        Array.Copy(JobDeletionSignatureData, offset, jobIdLengthBytes, 0, 4);
                        uint jobIdLength = BitConverter.ToUInt32(jobIdLengthBytes, 0);
                        offset += 4;
                        byte[] jobIdData = new byte[jobIdLength];
                        Array.Copy(JobDeletionSignatureData, offset, jobIdData, 0, jobIdData.Length);
                        BigInteger jobid = new BigInteger(jobIdData);
                        if (jobid != JobId)
                        {
                            //invalid JobId in deletion signature
                            Logger.LogText(string.Format("Job has invalid deletion signature {0}", ConvertJobId(JobId)), this, Logtype.Debug);
                            return false;
                        }
                        offset += jobIdData.Length;
                        offset += 8; //+8 for deletion time in structure

                        byte[] data = new byte[offset];
                        Array.Copy(JobDeletionSignatureData, 0, data, 0, data.Length);
                        byte[] signature = new byte[JobDeletionSignatureData.Length - offset];
                        Array.Copy(JobDeletionSignatureData, offset, signature, 0, signature.Length);

                        //finally check signature
                        bool validDeletionSignature = CertificateService.GetCertificateService().VerifySignature(data, signature, new X509Certificate2(CreatorCertificateData)).Equals(CertificateValidationState.Valid);
                        Logger.LogText(string.Format("Job has valid deletion signature (USER) {0}: {1}", ConvertJobId(JobId), validDeletionSignature), this, Logtype.Debug);
                        return validDeletionSignature;

                    }
                    // admin deleted
                    else if (magicNumber.Equals("ADMN"))
                    {
                        int offset = 4;
                        byte[] jobIdLengthBytes = new byte[4];
                        Array.Copy(JobDeletionSignatureData, offset, jobIdLengthBytes, 0, 4);
                        uint jobIdLength = BitConverter.ToUInt32(jobIdLengthBytes, 0);
                        offset += 4;
                        byte[] jobIdData = new byte[jobIdLength];
                        Array.Copy(JobDeletionSignatureData, offset, jobIdData, 0, jobIdData.Length);
                        BigInteger jobid = new BigInteger(jobIdData);
                        if (jobid != JobId)
                        {
                            //invalid JobId in deletion signature
                            return false;
                        }
                        offset += jobIdData.Length;
                        offset += 8; //+8 for deletion time in structure
                        uint certificateDataLength = BitConverter.ToUInt32(JobDeletionSignatureData, offset);
                        offset += 4;
                        byte[] certificateData = new byte[certificateDataLength];
                        Array.Copy(JobDeletionSignatureData, offset, certificateData, 0, certificateDataLength);
                        offset += certificateData.Length;
                        byte[] data = new byte[offset];
                        Array.Copy(JobDeletionSignatureData, 0, data, 0, data.Length);
                        byte[] signature = new byte[JobDeletionSignatureData.Length - offset];
                        Array.Copy(JobDeletionSignatureData, offset, signature, 0, signature.Length);

                        X509Certificate2 adminCertificate = new X509Certificate2(certificateData);
                        //check, if the certificate is an admin certificate
                        if (!CertificateService.GetCertificateService().IsAdminCertificate(adminCertificate))
                        {
                            Logger.LogText(string.Format("Job {0} was deleted by a non-admin user: {1}", ConvertJobId(JobId), CertificateService.GetCertificateService().GetSubjectNameFromCertificate(adminCertificate)), this, Logtype.Debug);
                            return false;
                        }

                        //finally check signature
                        bool validDeletionSignature = CertificateService.GetCertificateService().VerifySignature(data, signature, adminCertificate).Equals(CertificateValidationState.Valid);
                        Logger.LogText(string.Format("Job has valid deletion signature (ADMIN) {0}: {1}", ConvertJobId(JobId), validDeletionSignature), this, Logtype.Debug);
                        return validDeletionSignature;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Problems with deserialization of job {0}: {1}", ConvertJobId(JobId), ex), this, Logtype.Debug);
                    Logger.LogException(ex, this, Logtype.Debug);
                }
                return false;
            }
        }

        private string ConvertJobId(BigInteger value)
        {
            byte[] idbytes = value.ToByteArray();
            return BitConverter.ToString(idbytes).Replace("-", "");
        }

        /// <summary>
        /// Notify that a property changed
        /// </summary>
        /// <param name="propertyName"></param>
        internal void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
            {
                return;
            }
            //if we are in a WPF application, we use the UI thread
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }));
            }
            else
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Returns a free block id; returns -1 if there is none
        /// </summary>
        /// <returns></returns>
        internal BigInteger GetFreeBlockId()
        {
            int freebit = JobEpochState.Bitmask.GetRandomFreeBit();
            if (freebit == -1)
            {
                return -1;
            }
            return JobEpochState.Bitmask.MaskSize * JobEpochState.EpochNumber * 8 + freebit;
        }

        /// <summary>
        /// Returns the amount of free blocks in the current epoch
        /// </summary>
        /// <returns></returns>
        internal BigInteger FreeBlocksInEpoch()
        {
            return JobEpochState.Bitmask.GetFreeBits();
        }

        /// <summary>
        /// Checks and updates the epoch and bitmask of this job.
        /// </summary>
        public void CheckAndUpdateEpochAndBitmask()
        {
            if (JobEpochState == null)
            {
                return;
            }
            if (JobEpochState.EpochNumber == NumberOfEpochs - 1)
            {
                //we are in the last epoch, thus, we fill the rest of the bitmask with ones
                BigInteger bitsToFill = NumberOfBlocks % (JobEpochState.Bitmask.MaskSize * 8);
                if (bitsToFill > 0)
                {
                    uint offset = JobEpochState.Bitmask.MaskSize - 1;
                    while (bitsToFill >= 8)
                    {
                        JobEpochState.Bitmask.SetMaskByte(offset, 0xFF);
                        bitsToFill -= 8;
                        offset -= 1;
                    }
                    //Set last byte which may not be completely filled
                    switch ((uint)bitsToFill)
                    {
                        case 1:
                            JobEpochState.Bitmask.SetMaskByte(offset, 128);
                            break;
                        case 2:
                            JobEpochState.Bitmask.SetMaskByte(offset, 128 + 64);
                            break;
                        case 3:
                            JobEpochState.Bitmask.SetMaskByte(offset, 128 + 64 + 32);
                            break;
                        case 4:
                            JobEpochState.Bitmask.SetMaskByte(offset, 128 + 64 + 32 + 16);
                            break;
                        case 5:
                            JobEpochState.Bitmask.SetMaskByte(offset, 128 + 64 + 32 + 16 + 8);
                            break;
                        case 6:
                            JobEpochState.Bitmask.SetMaskByte(offset, 128 + 64 + 32 + 16 + 8 + 4);
                            break;
                        case 7:
                            JobEpochState.Bitmask.SetMaskByte(offset, 128 + 64 + 32 + 16 + 8 + 4 + 2);
                            break;
                        case 8:
                            JobEpochState.Bitmask.SetMaskByte(offset, 128 + 64 + 32 + 16 + 8 + 4 + 2 + 1);
                            break;
                    }
                }
            }
            else if (FreeBlocksInEpoch() == 0)
            {
                Logger.LogText("Epoch is complete. Going to next epoch now", this, Logtype.Debug);
                JobEpochState.Bitmask.Clear();
                JobEpochState.EpochNumber++;
            }
        }

        /// <summary>
        /// Number of calculated blocks of this job
        /// </summary>
        public BigInteger NumberOfCalculatedBlocks
        {
            get
            {
                if (JobEpochState == null)
                {
                    return BigInteger.Zero;
                }
                //special case: last epoch size != all other epoch's size
                if (JobEpochState.EpochNumber == NumberOfEpochs - 1 && NumberOfBlocks % (JobEpochState.Bitmask.MaskSize * 8) > 0)
                {
                    return JobEpochState.EpochNumber * JobEpochState.Bitmask.MaskSize * 8 + JobEpochState.Bitmask.GetSetBitsCount() - (((JobEpochState.Bitmask.MaskSize * 8) - NumberOfBlocks % (JobEpochState.Bitmask.MaskSize * 8)));
                }
                else
                {
                    //usual case: not in last epoch
                    return JobEpochState.EpochNumber * JobEpochState.Bitmask.MaskSize * 8 + JobEpochState.Bitmask.GetSetBitsCount();
                }
            }
        }

        /// <summary>
        /// Number of epochs of this job
        /// </summary>
        public BigInteger NumberOfEpochs
        {
            get
            {
                if (JobEpochState == null || JobEpochState.Bitmask.MaskSize == 0)
                {
                    return BigInteger.Zero;
                }
                BigInteger numberOfEpochs = NumberOfBlocks / (JobEpochState.Bitmask.MaskSize * 8);
                if (NumberOfBlocks % JobEpochState.Bitmask.MaskSize > 0)
                {
                    numberOfEpochs++;
                }
                return numberOfEpochs;
            }
        }

        /// <summary>
        /// Returns true, if all blocks have been calculated
        /// </summary>
        /// <returns></returns>
        public bool IsFinished()
        {
            return NumberOfCalculatedBlocks == NumberOfBlocks;
        }

        /// <summary>
        /// Computes and sets the current progress and epoch progress of this job.
        /// Also calls the property change event for the progress if it changed
        /// </summary>
        internal void UpdateProgessAndEpochProgress()
        {
            if (JobEpochState != null)
            {
                double oldEpochProgress = EpochProgress;
                EpochProgress = (JobEpochState.Bitmask.GetSetBitsCount() / (JobEpochState.Bitmask.mask.Length * 8.0) * 100.0);
                if (oldEpochProgress != EpochProgress)
                {
                    Progress = (double)((NumberOfCalculatedBlocks * 10000000000) / (NumberOfBlocks)) / 100000000;
                    ProgressText = string.Format("{0:n0}", NumberOfCalculatedBlocks) + " / " + string.Format("{0:n0}", NumberOfBlocks);
                    OnPropertyChanged("Progress");
                    OnPropertyChanged("ProgressText");

                    EpochProgressText = string.Format("{0:n0}", JobEpochState.Bitmask.GetSetBitsCount()) + " / " + string.Format("{0:n0}", JobEpochState.Bitmask.mask.Length * 8);
                    OnPropertyChanged("EpochProgress");
                    OnPropertyChanged("EpochProgressText");
                }
            }
        }

        // help variable to check if we need to update the bitmap
        private BigInteger LastNumberOfCalculatedBlocks = 0;
        private readonly Mutex UpdateEpochVisualizationMutex = new Mutex();

        /// <summary>
        /// Updates visualization of current epoch 
        /// </summary>
        internal void UpdateEpochVisualization()
        {
            if (JobEpochState == null)
            {
                return;
            }
            try
            {
                UpdateEpochVisualizationMutex.WaitOne();
                //we use the singleton pattern to save memory
                //and always update the same bitmap
                if (VisualizationBitmap == null)
                {
                    int width = (int)Math.Ceiling(Math.Sqrt(JobEpochState.Bitmask.MaskSize * 8));
                    VisualizationBitmap = new Bitmap(width, width);
                }
                if (NumberOfCalculatedBlocks != LastNumberOfCalculatedBlocks)
                {
                    int divisor = (int)Math.Ceiling(Math.Sqrt(JobEpochState.Bitmask.MaskSize * 8));
                    int offset = 0;
                    for (int byteNo = 0; byteNo < JobEpochState.Bitmask.mask.Length; byteNo++)
                    {
                        int bitNo = 0;
                        for (int bitValue = 1; bitValue <= 128; bitValue *= 2)
                        {
                            offset = byteNo * 8 + bitNo;
                            bitNo++;
                            if ((JobEpochState.Bitmask.mask[byteNo] & bitValue) > 0)
                            {
                                VisualizationBitmap.SetPixel(offset % divisor, offset / divisor, Color.Black);
                            }
                            else
                            {
                                VisualizationBitmap.SetPixel(offset % divisor, offset / divisor, Color.White);
                            }
                        }
                    }
                    offset++;
                    //color the rest of the bitmap in black
                    while (offset < VisualizationBitmap.Width * VisualizationBitmap.Height)
                    {
                        VisualizationBitmap.SetPixel(offset % divisor, offset / divisor, Color.Black);
                        offset++;
                    }
                    LastNumberOfCalculatedBlocks = NumberOfCalculatedBlocks;
                    OnPropertyChanged("EpochVisualization");
                }
            }
            catch (Exception)
            {
                //if an exception occurs here, we ignore it.
                //visualization is not so important. And a new one will be generated later.
            }
            finally
            {
                UpdateEpochVisualizationMutex.ReleaseMutex();
            }
        }

        private Bitmap VisualizationBitmap;
        /// <summary>
        /// Returns a graphical visualization of the job state of the job with the given jobid
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public BitmapImage EpochVisualization
        {
            get
            {
                if (JobEpochState == null || VisualizationBitmap == null)
                {
                    return new BitmapImage();
                }
                try
                {
                    return BitmapToBitmapImage(VisualizationBitmap);
                }
                catch (Exception)
                {
                    return new BitmapImage();
                }
            }
        }

        /// <summary>
        /// Converts a bitmap to a bitmap image
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        /// <summary>
        /// Deletes the job from the file system
        /// </summary>
        internal void DeleteSerializedJobFile(string localStoragePath)
        {
            try
            {
                string filename = localStoragePath + Path.DirectorySeparatorChar + BitConverter.ToString(JobId.ToByteArray()) + ".job";
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                    Logger.LogText(string.Format("Deleted serialized job file: {0}", filename), this, Logtype.Debug);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, this, Logtype.Error);
            }
        }
    }
}
