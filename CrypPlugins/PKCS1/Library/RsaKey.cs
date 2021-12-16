using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using PKCS1.Resources.lang.Gui;
using System;
using System.Windows;

namespace PKCS1.Library
{
    internal class RsaKey
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
                m_RsaKeySize = value;
                OnRaiseKeyGenerated(ParameterChangeType.ModulusSize);
            }
            get => m_RsaKeySize;
        }

        private BigInteger m_Modulus = BigInteger.Zero;
        public void setModulus(string value, int radix)
        {
            m_Modulus = new BigInteger(value, radix);
            //this.m_RsaKeySize = this.m_Modulus.BitLength;
        }

        private BigInteger m_PrivKey = BigInteger.Zero;
        public void setPrivKey(string value, int radix)
        {
            m_PrivKey = new BigInteger(value, radix);
        }

        private BigInteger m_PubExponent = BigInteger.ValueOf(3); // default
        public int PubExponent //TODO ändern in String
        {
            set
            {
                m_PubExponent = BigInteger.ValueOf(value);
                OnRaiseKeyGenerated(ParameterChangeType.PublicExponent);
            }
            get => m_PubExponent.IntValue;
        }

        public void setInputParams()
        {
            try
            {
                AsymmetricKeyParameter publicKey = new RsaKeyParameters(false, m_Modulus, m_PubExponent);
                AsymmetricKeyParameter privateKey = new RsaKeyParameters(true, m_Modulus, m_PrivKey);
                keyPair = new AsymmetricCipherKeyPair(publicKey, privateKey);
            }
            catch (ArgumentException)
            {
                MessageBox.Show(RsaKeyInputCtrl.invalidInput, RsaKeyInputCtrl.invalidInput, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            m_bRsaKeyGenerated = true;
            OnRaiseKeyGenerated(ParameterChangeType.RsaKey);
        }

        // Rsa Schlüssel generieren       
        public void genRsaKeyPair(int certainty)
        {
            BigInteger pubExp = BigInteger.ValueOf(PubExponent);
            int strength = RsaKeySize;
            RsaKeyPairGenerator fact = new RsaKeyPairGenerator();

            RsaKeyGenerationParameters factParams = new RsaKeyGenerationParameters(pubExp, new SecureRandom(), strength, certainty);
            fact.Init(factParams);

            keyPair = fact.GenerateKeyPair();
            m_bRsaKeyGenerated = true;
            OnRaiseKeyGenerated(ParameterChangeType.RsaKey);
        }

        public bool isKeyGenerated()
        {
            return m_bRsaKeyGenerated;
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
            return keyPair.Private;
        }

        public BigInteger getPrivKeyToBigInt()
        {
            RsaKeyParameters privKeyParam = (RsaKeyParameters)getPrivKey();
            return privKeyParam.Exponent;
        }

        public AsymmetricKeyParameter getPubKey()
        {
            return keyPair.Public;
        }

        public BigInteger getPubKeyToBigInt()
        {
            RsaKeyParameters pubKeyParam = (RsaKeyParameters)getPubKey();
            return pubKeyParam.Exponent;
        }

        public BigInteger getModulusToBigInt()
        {
            RsaKeyParameters pubkeyParam = (RsaKeyParameters)RsaKey.Instance.getPubKey();
            return pubkeyParam.Modulus;
        }
    }
}
