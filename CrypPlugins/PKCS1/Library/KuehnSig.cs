using CrypTool.PluginBase;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Text;

namespace PKCS1.Library
{
    public class KuehnSig : Signature, IGuiLogMsg
    {
        private int m_Iterations = 250000;
        public int Iterations
        {
            get => m_Iterations;
            set => m_Iterations = value;
        }

        public KuehnSig()
        {
            registerHandOff();
        }

        public override bool GenerateSignature()
        {
            m_KeyLength = RsaKey.Instance.RsaKeySize; // Länge des RSA Modulus

            // drei Leerzeichen an Msg anhängen
            string sMsgModifier = "   ";
            byte[] bMsgModifier = Encoding.ASCII.GetBytes(sMsgModifier);
            byte[] bMessage = new byte[Datablock.getInstance().Message.Length + bMsgModifier.Length];
            Array.Copy(Datablock.getInstance().Message, bMessage, Datablock.getInstance().Message.Length);
            Array.Copy(bMsgModifier, 0, bMessage, Datablock.getInstance().Message.Length, bMsgModifier.Length);

            HashFunctionIdent hashFuncIdent = Datablock.getInstance().HashFunctionIdent; // Die vom User gewählte Hashfunktion
            byte[] hashIdent = Hex.Decode(Encoding.ASCII.GetBytes(hashFuncIdent.DERIdent)); // ASN.1 codierter Ident-string
            int significantByteLength = 11 + hashIdent.Length + hashFuncIdent.digestLength / 8;

            // Datenblock wird konstruiert
            byte[] A = new byte[significantByteLength];
            A[0] = 0x00;
            A[1] = 0x01;
            for (int i = 2; i < 10; i++)
            {
                A[i] = 0xFF;
            }
            A[10] = 0x00;
            Array.Copy(hashIdent, 0, A, 11, hashIdent.Length);
            // Datenblock noch ohne Hashwert, wird in while Schleife unten hinzugefügt

            // byte array der kompletten Signatur, wird zuerst mit 'FF' gefüllt und dann nachher Datenblock an den Anfang kopiert
            byte[] S = new byte[m_KeyLength / 8];
            for (int i = A.Length; i < S.Length; i++)
            {
                S[i] = 0xFF;
            }

            BigInteger finalSignature = null;
            int iMsgLength = bMessage.Length;
            bool isEqual = false;
            int countLoops = 0;

            SendGuiLogMsg("Signature Generation started", NotificationLevel.Info);
            byte[] hashDigest = new byte[0]; // Hashwert wird in dieser var gespeichert
            BigInteger T = new BigInteger("0"); // hilfsvar
            byte[] resultArray = new byte[m_KeyLength / 8];

            while (!isEqual && (countLoops < m_Iterations))
            {
                hashDigest = Hashfunction.generateHashDigest(ref bMessage, ref hashFuncIdent); // Hashwert wird erzeugt
                Array.Copy(hashDigest, 0, A, 11 + hashIdent.Length, hashDigest.Length); // erzeugter Hashwert wird in den Datenblock kopiert
                Array.Copy(A, 0, S, 0, A.Length); // erzeugter Datenblock wird in erzeugte Signatur S kopiert

                finalSignature = MathFunctions.cuberoot4(new BigInteger(S), m_KeyLength); // Kubikwurzel ziehen          
                byte[] test2 = finalSignature.ToByteArray();
                T = finalSignature.Pow(3); // mit 3 potenzieren       
                byte[] test = T.ToByteArray();
                resultArray[0] = 0x00; // erstes byte is 0
                // durch Konvertierung in BigInteger werden führende Nullen abgeschnitten, daher wird an Stelle 1 in byte array kopiert.
                Array.Copy(T.ToByteArray(), 0, resultArray, 1, T.ToByteArray().Length);

                isEqual = MathFunctions.compareByteArray(ref resultArray, ref S, significantByteLength); // byte arrays vergleichen, wird in meinen Tests nicht erreicht
                if (!isEqual)
                {
                    int value1 = bMessage[iMsgLength - 1];
                    if (++value1 >= 256)
                    {
                        value1 = 0;
                        int value2 = bMessage[iMsgLength - 2];
                        if (++value2 >= 256)
                        {
                            value2 = 0;
                            int value3 = bMessage[iMsgLength - 3];
                            ++value3;
                            bMessage[iMsgLength - 3] = (byte)value3;
                        }
                        bMessage[iMsgLength - 2] = (byte)value2;
                    }
                    bMessage[iMsgLength - 1] = (byte)value1;

                    countLoops++;
                }
            }
            if (countLoops != m_Iterations)
            {
                Datablock.getInstance().Message = bMessage;
                byte[] returnByteArray = new byte[m_KeyLength / 8];
                Array.Copy(finalSignature.ToByteArray(), 0, returnByteArray, returnByteArray.Length - finalSignature.ToByteArray().Length, finalSignature.ToByteArray().Length);

                m_Signature = returnByteArray;
                m_bSigGenerated = true;
                OnRaiseSigGenEvent(SignatureType.Kuehn);
                return true;
            }
            else
            {
                m_bSigGenerated = false;
            }
            return false;
        }

        public event GuiLogHandler OnGuiLogMsgSend;

        public void registerHandOff()
        {
            GuiLogMsgHandOff.getInstance().registerAt(ref OnGuiLogMsgSend);
        }

        public void SendGuiLogMsg(string message, NotificationLevel logLevel)
        {
            if (null != OnGuiLogMsgSend)
            {
                OnGuiLogMsgSend(message, logLevel);
            }
        }
    }
}
