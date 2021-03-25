using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyOneCiphers
{
    class Cipher16Bit16DifferentialKeyRecoveryAttack : DifferentialKeyRecoveryAttack
    {
        
        public bool recoveredSubkey3;
        public bool recoveredSubkey2;
        public bool recoveredSubkey1;
        public bool recoveredSubkey0;

        public int subkey4;
        public int subkey3;
        public int subkey2;
        public int subkey1;

        public Cipher16Bit16DifferentialKeyRecoveryAttack()
        {
            recoveredSubkey1 = false;
            recoveredSubkey2 = false;
            recoveredSubkey3 = false;
            recoveredSubkey0 = false;

            subkey1 = -1;
            subkey2 = -1;
            subkey3 = -1;
            subkey4 = -1;

            RoundConfigurations = new List<DifferentialAttackRoundConfiguration>();
            RoundResults = new List<DifferentialAttackRoundResult>();
            LastRoundResult = new DifferentialAttackLastRoundResult();
        }

        public override string printRecoveredSubkeyBits()
        {
            throw new NotImplementedException();
        }
    }
}
