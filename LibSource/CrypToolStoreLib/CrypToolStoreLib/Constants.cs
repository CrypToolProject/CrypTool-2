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

namespace CrypToolStoreLib
{
    /// <summary>
    /// This class contains all constants used by the CrypToolStoreLib
    /// Do not change UNLESS YOU EXACTLY KNOW what you do
    /// </summary>
    public class Constants
    {
        //Constants of Client class
        public const int CLIENT_DEFAULT_PORT = 15151;
        public const string CLIENT_DEFAULT_ADDRESS = "localhost";
        public const int CLIENT_READ_TIMEOUT = 5000;
        public const int CLIENT_WRITE_TIMEOUT = 5000;
        public const int CLIENT_FILE_BUFFER_SIZE = 81920; // 80kb

        //Constants of Database class
        public const int DATABASE_PBKDF2_ITERATION_COUNT = 10000;
        public const int DATABASE_PBKDF2_HASH_LENGTH = 32;

        //Constants for MessageHeader class
        public const string MESSAGEHEADER_MAGIC = "CrypToolStore";       // 13 byte (string); each message begins with that                

        //Constants for Server class
        public const int SERVER_MESSAGE_MAX_PAYLOAD_SIZE = 1048576; //1mb
        public const int SERVER_DEFAULT_PORT = 15151;

        //Constants for ClientHandler class
        public const int CLIENTHANDLER_READ_TIMEOUT = 5000;
        public const int CLIENTHANDLER_WRITE_TIMEOUT = 5000;
        public const int CLIENTHANDLER_MAX_ICON_FILE_SIZE = 65536;
        public const string CLIENTHANDLER_PLUGIN_SOURCE_FOLDER = "Sources";
        public const string CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER = "Assemblies";
        public const string CLIENTHANDLER_RESOURCES_FOLDER = "Resources";
        public const int CLIENTHANDLER_FILE_BUFFER_SIZE = 81920; // 80kb
        public const string CLIENTHANDLER_RESOURCEDATA_FOLDER = "ResourceData";
    }
}
