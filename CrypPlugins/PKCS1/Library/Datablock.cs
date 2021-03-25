using System.Text;

using Org.BouncyCastle.Utilities.Encoders;

namespace PKCS1.Library
{
    class Datablock
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
                this.m_hashFuncIdent = (HashFunctionIdent)value;
                OnRaiseParamChangedEvent(ParameterChangeType.HashfunctionType);
            }
            get { return this.m_hashFuncIdent; }
        }

        protected byte[] m_Message = new byte[0];
        public byte[] Message
        {
            set 
            {
                //string tmpString = (string)value;
                //this.m_Message = Encoding.ASCII.GetBytes(tmpString);
                this.m_Message = value;
                OnRaiseParamChangedEvent(ParameterChangeType.Message);
            }
            //get { return Encoding.ASCII.GetString(this.m_Message); }
            get { return this.m_Message; }
        }

        #endregion

        #region Eventhandling

        public event ParamChanged RaiseParamChangedEvent;

        private void OnRaiseParamChangedEvent( ParameterChangeType type )
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
            byte[] bMessage = this.Message;
            HashFunctionIdent hashIdent = this.HashFunctionIdent;
            return Encoding.ASCII.GetString(Hex.Encode(Hashfunction.generateHashDigest(ref bMessage, ref hashIdent)));
        }
        
        #endregion
    }
}
