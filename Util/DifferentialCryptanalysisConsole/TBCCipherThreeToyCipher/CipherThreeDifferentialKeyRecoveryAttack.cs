using Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBCCipherThreeToyCipher
{
    class CipherThreeDifferentialKeyRecoveryAttack : DifferentialKeyRecoveryAttack
    {
        public bool RecoveredSubkey3;
        public bool RecoveredSubkey2;
        public bool RecoveredSubkey1;
        public bool RecoveredSubkey0;

        public int Subkey3;
        public int Subkey2;
        public int Subkey1;
        public int Subkey0;

        public CipherThreeDifferentialKeyRecoveryAttack()
        {
            RecoveredSubkey0 = false;
            RecoveredSubkey1 = false;
            RecoveredSubkey2 = false;
            RecoveredSubkey3 = false;

            Subkey0 = -1;
            Subkey1 = -1;
            Subkey2 = -1;
            Subkey3 = -1;

            RoundConfigurations = new List<DifferentialAttackRoundConfiguration>();
            RoundResults = new List<DifferentialAttackRoundResult>();
            LastRoundResult = new DifferentialAttackLastRoundResult();
        }

        public override string printRecoveredSubkeyBits()
        {
            var sb = new StringBuilder();
            BitArray keyBits = null;
            bool first = true;

            if (RecoveredSubkey0)
            {
                first = false;
                sb.Append("Binary K0: ");
                keyBits = new BitArray(BitConverter.GetBytes(Subkey0));

                for (int i = keyBits.Length - 29; i >= 0; i--)
                {
                    if (((i + 1) % 4) == 0)
                        sb.Append(" ");

                    char c = keyBits[i] ? '1' : '0';
                    sb.Append(c);
                }

                sb.Append(" Decimal K0: ");
                sb.Append(Subkey0);
            }

            if (RecoveredSubkey1)
            {
                if (!first)
                {
                    sb.Append(Environment.NewLine);
                }
                first = false;
                sb.Append("Binary K1: ");
                keyBits = new BitArray(BitConverter.GetBytes(Subkey1));

                for (int i = keyBits.Length - 29; i >= 0; i--)
                {
                    if (((i + 1) % 4) == 0)
                        sb.Append(" ");

                    char c = keyBits[i] ? '1' : '0';
                    sb.Append(c);
                }

                sb.Append(" Decimal K1: ");
                sb.Append(Subkey1);
            }

            if (RecoveredSubkey2)
            {
                if (!first)
                {
                    sb.Append(Environment.NewLine);
                }
                first = false;
                sb.Append("Binary K2: ");
                keyBits = new BitArray(BitConverter.GetBytes(Subkey2));

                for (int i = keyBits.Length - 29; i >= 0; i--)
                {
                    if (((i + 1) % 4) == 0)
                        sb.Append(" ");

                    char c = keyBits[i] ? '1' : '0';
                    sb.Append(c);
                }

                sb.Append(" Decimal K2: ");
                sb.Append(Subkey2);
            }

            if (RecoveredSubkey3)
            {
                if (!first)
                {
                    sb.Append(Environment.NewLine);
                }
                first = false;
                sb.Append("Binary K3: ");
                keyBits = new BitArray(BitConverter.GetBytes(Subkey3));

                for (int i = keyBits.Length - 29; i >= 0; i--)
                {
                    if (((i + 1) % 4) == 0)
                        sb.Append(" ");

                    char c = keyBits[i] ? '1' : '0';
                    sb.Append(c);
                }

                sb.Append(" Decimal K3: ");
                sb.Append(Subkey3);
            }

            return sb.ToString();
        }
    }
}
