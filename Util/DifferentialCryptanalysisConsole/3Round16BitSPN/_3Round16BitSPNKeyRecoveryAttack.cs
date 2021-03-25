using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace _3Round16BitSPN
{
    class _3Round16BitSPNKeyRecoveryAttack : DifferentialKeyRecoveryAttack
    {
        public bool recoveredSubkey3;
        public bool recoveredSubkey2;
        public bool recoveredSubkey1;
        public bool recoveredSubkey0;

        public int subkey3;
        public int subkey2;
        public int subkey1;
        public int subkey0;

        public _3Round16BitSPNKeyRecoveryAttack()
        {
            recoveredSubkey0 = false;
            recoveredSubkey1 = false;
            recoveredSubkey2 = false;
            recoveredSubkey3 = false;

            subkey0 = -1;
            subkey1 = -1;
            subkey2 = -1;
            subkey3 = -1;

            RoundConfigurations = new List<DifferentialAttackRoundConfiguration>();
            RoundResults = new List<DifferentialAttackRoundResult>();
            LastRoundResult = new DifferentialAttackLastRoundResult();
        }

        public override string printRecoveredSubkeyBits()
        {
            var sb = new StringBuilder();
            BitArray keyBits = null;
            bool first = true;

            if (recoveredSubkey0)
            {
                first = false;
                sb.Append("Binary K0: ");
                keyBits = new BitArray(BitConverter.GetBytes(subkey0));

                for (int i = keyBits.Length - 25; i >= 0; i--)
                {
                    if (((i + 1) % 4) == 0)
                        sb.Append(" ");

                    char c = keyBits[i] ? '1' : '0';
                    sb.Append(c);
                }

                sb.Append(" Decimal K0: ");
                sb.Append(subkey0);
            }

            if (recoveredSubkey1)
            {
                if (!first)
                {
                    sb.Append(Environment.NewLine);
                }
                first = false;
                sb.Append("Binary K1: ");
                keyBits = new BitArray(BitConverter.GetBytes(subkey1));

                for (int i = keyBits.Length - 25; i >= 0; i--)
                {
                    if (((i + 1) % 4) == 0)
                        sb.Append(" ");

                    char c = keyBits[i] ? '1' : '0';
                    sb.Append(c);
                }

                sb.Append(" Decimal K1: ");
                sb.Append(subkey1);
            }

            if (recoveredSubkey2)
            {
                if (!first)
                {
                    sb.Append(Environment.NewLine);
                }
                first = false;
                sb.Append("Binary K2: ");
                keyBits = new BitArray(BitConverter.GetBytes(subkey2));

                for (int i = keyBits.Length - 25; i >= 0; i--)
                {
                    if (((i + 1) % 4) == 0)
                        sb.Append(" ");

                    char c = keyBits[i] ? '1' : '0';
                    sb.Append(c);
                }

                sb.Append(" Decimal K2: ");
                sb.Append(subkey2);
            }

            if (recoveredSubkey3)
            {
                if (!first)
                {
                    sb.Append(Environment.NewLine);
                }
                first = false;
                sb.Append("Binary K3: ");
                keyBits = new BitArray(BitConverter.GetBytes(subkey3));

                for (int i = keyBits.Length - 25; i >= 0; i--)
                {
                    if (((i + 1) % 4) == 0)
                        sb.Append(" ");

                    char c = keyBits[i] ? '1' : '0';
                    sb.Append(c);
                }

                sb.Append(" Decimal K3: ");
                sb.Append(subkey3);
            }

            return sb.ToString();
        }
    }
}
