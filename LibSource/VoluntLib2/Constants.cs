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

namespace VoluntLib2
{
    /// <summary>
    /// This class contains all constants used by VoluntLib2
    /// Do not change UNLESS YOU EXACTLY KNOW what you do
    /// </summary>
    internal class Constants
    {
        //Bitmask class
        public const int BITMASK_MAX_MASKSIZE = 30720; //30 kiB

        //ComputationManager class
        public const int COMPUTATIONMANAGER_MAX_TERMINATION_WAIT_TIME = 5000; //5 s
        public const int COMPUTATIONMANAGER_WORKER_THREAD_SLEEPTIME = 1; //1 ms

        //CheckJobsCompletionState class
        public const int CHECKJOBSCOMPLETIONSTATE_CHECK_INTERVAL = 5000; //5 sec

        //ConnectionManager class
        public const int CONNECTIONMANAGER_RECEIVE_TIMEOUT = 100; //100 ms
        public const int CONNECTIONMANAGER_MAX_TERMINATION_WAIT_TIME = 5000; //5 s
        public const int CONNECTIONMANAGER_MAX_UDP_MESSAGE_PAYLOAD_SIZE = 65507; //maximum size of UDP payload
        public const int CONNECTIONMANAGER_WORKER_THREAD_SLEEPTIME = 1; //1 ms

        //ConnectionLayer.Message class
        public const string MESSAGE_VOLUNTLIB2 = "VoluntLib2";  //Magic number to identify voluntlib protocol messages
        public const byte MESSAGE_VOLUNTLIB2_VERSION = 0x02;    //Protocol version number

        //HelloOperation class
        public const long HELLOOPERATION_TIMEOUT = 30000;
        public const long HELLOOPERATION_RETRY_TIMESPAN = 5000;

        //CheckContactsOperation class
        public const int CHECKCONTACTSOPERATION_SAY_HELLO_INTERVAL = 30000; //30 sec
        public const int CHECKCONTACTSOPERATION_SET_CONTACT_OFFLINE = 300000; //5 min
        public const int CHECKCONTACTSOPERATION_REMOVE_OFFLINE_CONTACT = 86400000; //24 h

        //RequestNeighborListOperation class
        public const long REQUESTNEIGHBORLISTOPERATION_TIMEOUT = 30000; //30 sec
        public const long REQUESTNEIGHBORLISTOPERATION_RETRY_TIMESPAN = 5000; //5 sec

        //MyStatusOperation class
        public const uint MYSTATUSOPERATION_STATUS_SHOW_INTERVAL = 5000; // 5 sec

        //AskForNeighborListOperation class
        public const int ASKFORNEIGHBORLISTOPERATION_ASK_FOR_NEIGHBORLIST_INTERVAL = 300000; // 5 min

        //CheckMyConnectionsNumberOperation class
        public const int CHECKMYCONNECTIONSNUMBEROPERATION_CHECK_CONNECTIONS_INTERVAL = 10000; //10 sec
        public const int CHECKMYCONNECTIONSNUMBEROPERATION_MIN_CONNECTIONS_NUMBER = 10;
        public const int CHECKMYCONNECTIONSNUMBEROPERATION_MAX_CONNECTIONS_NUMBER = 20;

        //BootstrapOperation class
        public const int BOOTSTRAPOPERATION_CHECK_INTERVAL = 120000; //2 min

        //HousekeepReceivedNeighborlistOperation class
        public const int HOUSEKEEPRECEIVEDNEIGHBORLISTOPERATION_CHECK_INTERVAL = 60000; //1 min

        //HousekeepExternalIpAddressesOperation class
        public const int HOUSEKEEPEXTERNALIPADDRESSESOPERATION_CHECK_INTERVAL = 60000; //1 min
        public const int HOUSEKEEPEXTERNALIPADDRESSESOPERATION_REMOVE_INTERVAL = 300000; //5 min

        //Job class
        public const int JOB_STRING_MAX_LENGTH = 255;
        public const int JOB_STRING_MAX_JOB_DESCRIPTION_LENGTH = 1024; //1 KiB
        public const string JOB_ANONYMOUS_USER = "anonymous";

        //JobManager class
        public const int JOBMANAGER_MAX_TERMINATION_WAIT_TIME = 5000; //5 s
        public const int JOBMANAGER_WORKER_THREAD_SLEEPTIME = 1; // 1 ms
        public const int JOBMANAGER_MAX_JOB_PAYLOAD_SIZE = 20 * 1024; // 20 KiB

        //ManagementLayer.MessageHeader class
        public const int MESSAGES_STRING_MAX_LENGTH = 255;

        //ManagementLayer.Message class
        public const string MESSAGE_VLIB2MNGMT = "VLib2Mngmt";  //Magic Number to identify VoluntLib2 management protocol
        public const byte MGM_MESSAGE_VOLUNTLIB2_VERSION = 0x01;    //Protocol version number

        //ShareJobListAndJobsOperation class
        public const int SHAREJOBLISTANDJOBSOPERATION_SHARE_INTERVAL = 300000; //5 min

        //RequestJobListOperation class
        public const int REQUESTJOBLISTOPERATION_REQUEST_INTERVAL = 60000; //1 min 

        //CheckJobsPayloadOperation class
        public const int CHECKJOBSPAYLOADOPERATION_REQUEST_INTERVAL = 60000; //1 min

        //JobsSerializationOperation class
        public const int JOBSSERIALIZATIONOPERATION_SERIALIZATION_INTERVAL = 300000; //5 min

        //UpdateJobsProgressOperation class
        public const int UPDATEJOBSPROGRESSOPERATION_UPDATE_TIME_INTERVAL = 1000; //1 sec

        //HousekeepOldJobsOperation class (half year)
        public const int HOUSEKEEPOLDJOBSOPERATION_CHECK_INTERVAL = 60; //1 min


        //HousekeepOldJobsOperation class (half year)
        public const int HOUSEKEEPOLDJOBSOPERATION_DELETE_INTERVAL = 60 * 24 * 182; //60 sec * 24 * 128 = 6 months

        /// <summary>
        /// Set of private IP ranges for IsPrivateIP-method of IPTools
        /// </summary>
        public static uint[][] IPTOOLS_PRIVATE_IP_RANGES = new uint[][]
        {
            new uint[]{167772160u,   184549375u}, /*    10.0.0.0 -  10.255.255.255 */
            new uint[]{3232235520u, 3232301055u}, /* 192.168.0.0 - 192.168.255.255 */
            new uint[]{2130706432u, 2147483647u}, /*   127.0.0.0 - 127.255.255.255 */
            new uint[]{2851995648u, 2852061183u}, /* 169.254.0.0 - 169.254.255.255 */
            new uint[]{2886729728u, 2887778303u}, /*  172.16.0.0 -  172.31.255.255 */
            new uint[]{3758096384u, 4026531839u}, /*   224.0.0.0 - 239.255.255.255 */
            new uint[]{0u, 0u},                   /*     0.0.0.0 - 0.0.0.0.        */
            new uint[]{4294967295u, 4294967295u}, /*  255.255.255.255 - 255.255.255.255 */
        };
    }
}
