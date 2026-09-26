/*
   Copyright (C) CrypTool 2 Team

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
using Org.BouncyCastle.Utilities.Encoders;
using System.Text;

namespace PKCS1.Library
{
    internal class Datablock
    {
        #region Singleton
        // Singleton
        //
        private static Datablock instance = null;

        public static Datablock getInstance()
        {
            if (instance == null) { instance = new Datablock(); }
            return instance;
        }

        private Datablock()
        {
        }
        //
        // end Singleton
        #endregion

        #region Member

        private HashFunctionIdent m_hashFuncIdent = HashFuncIdentHandler.SHA1; // default SHA-1
        public HashFunctionIdent HashFunctionIdent
        {
            set
            {
                m_hashFuncIdent = value;
                OnRaiseParamChangedEvent(ParameterChangeType.HashfunctionType);
            }
            get => m_hashFuncIdent;
        }

        protected byte[] m_Message = new byte[0];
        public byte[] Message
        {
            set
            {
                //string tmpString = (string)value;
                //this.m_Message = Encoding.ASCII.GetBytes(tmpString);
                m_Message = value;
                OnRaiseParamChangedEvent(ParameterChangeType.Message);
            }
            //get { return Encoding.ASCII.GetString(this.m_Message); }
            get => m_Message;
        }

        #endregion

        #region Eventhandling

        public event ParamChanged RaiseParamChangedEvent;

        private void OnRaiseParamChangedEvent(ParameterChangeType type)
        {
            if (null != RaiseParamChangedEvent)
            {
                RaiseParamChangedEvent(type);
            }
        }

        #endregion 

        #region Functions

        public string GetHashDigestToHexString()
        {
            byte[] bMessage = Message;
            HashFunctionIdent hashIdent = HashFunctionIdent;
            return Encoding.ASCII.GetString(Hex.Encode(Hashfunction.generateHashDigest(ref bMessage, ref hashIdent)));
        }

        #endregion
    }
}
