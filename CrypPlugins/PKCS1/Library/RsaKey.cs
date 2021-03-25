using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Windows;
using PKCS1.Resources.lang.Gui;

namespace PKCS1.Library
{
    class RsaKey
    {
        #region Singleton

        private static RsaKey m_Instance = null;
        public static RsaKey Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new RsaKey();
                }
                return m_Instance;
            }
        }

        private RsaKey()
        {
        }

        #endregion

        public event ParamChanged RaiseKeyGeneratedEvent;
              
        private AsymmetricCipherKeyPair keyPair = null;
        private bool m_bRsaKeyGenerated = false;

        private int m_RsaKeySize = 2048; // default
        public int RsaKeySize
        {
            set 
            {
                this.m_RsaKeySize = (int)value;
                OnRaiseKeyGenerated(ParameterChangeType.ModulusSize);
            }
            get { return this.m_RsaKeySize; }
        }

        private BigInteger m_Modulus = BigInteger.Zero;
        public void setModulus(string value, int radix)
        {
            this.m_Modulus = new BigInteger(value,radix);
            //this.m_RsaKeySize = this.m_Modulus.BitLength;
        }

        private BigInteger m_PrivKey = BigInteger.Zero;
        public void setPrivKey(string value, int radix)
        {
            this.m_PrivKey = new BigInteger(value,radix);
        }

        private BigInteger m_PubExponent = BigInteger.ValueOf(3); // default
        public int PubExponent //TODO ändern in String
        {
            set 
            { 
                this.m_PubExponent = BigInteger.ValueOf(value);
                OnRaiseKeyGenerated(ParameterChangeType.PublicExponent);
            }
            get { return this.m_PubExponent.IntValue; }
        }

        public void setInputParams()
        {
            try
            {
                AsymmetricKeyParameter publicKey = new RsaKeyParameters(false, this.m_Modulus, this.m_PubExponent);
                AsymmetricKeyParameter privateKey = new RsaKeyParameters(true, this.m_Modulus, this.m_PrivKey);
                this.keyPair = new AsymmetricCipherKeyPair(publicKey, privateKey);
            }
            catch (ArgumentException)
            {
                MessageBox.Show(RsaKeyInputCtrl.invalidInput, RsaKeyInputCtrl.invalidInput, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.m_bRsaKeyGenerated = true;
            OnRaiseKeyGenerated(ParameterChangeType.RsaKey);
        }

        // Rsa Schlüssel generieren       
        public void genRsaKeyPair(int certainty)
        {
            BigInteger pubExp = BigInteger.ValueOf(this.PubExponent);
            int strength = this.RsaKeySize;
            RsaKeyPairGenerator fact = new RsaKeyPairGenerator();

            RsaKeyGenerationParameters factParams = new RsaKeyGenerationParameters(pubExp, new SecureRandom(), strength, certainty);
            fact.Init(factParams);

            this.keyPair = fact.GenerateKeyPair();
            this.m_bRsaKeyGenerated = true;
            OnRaiseKeyGenerated(ParameterChangeType.RsaKey);
        }

        public bool isKeyGenerated()
        {
            return this.m_bRsaKeyGenerated;
        }

        private void OnRaiseKeyGenerated(ParameterChangeType type)
        {
            if (RaiseKeyGeneratedEvent != null)
            {
                RaiseKeyGeneratedEvent(type);
            }
        }


        public AsymmetricKeyParameter getPrivKey()
        {
            return this.keyPair.Private;          
        }

        public BigInteger getPrivKeyToBigInt()
        {
            RsaKeyParameters privKeyParam = (RsaKeyParameters)this.getPrivKey();
            return privKeyParam.Exponent;
        }

        public AsymmetricKeyParameter getPubKey()
        {
             return this.keyPair.Public;
        }

        public BigInteger getPubKeyToBigInt()
        {
            RsaKeyParameters pubKeyParam = (RsaKeyParameters)this.getPubKey();
            return pubKeyParam.Exponent;
        }

        public BigInteger getModulusToBigInt()
        {            
            RsaKeyParameters pubkeyParam = (RsaKeyParameters)RsaKey.Instance.getPubKey();           
            return pubkeyParam.Modulus;
        }
    }
}
