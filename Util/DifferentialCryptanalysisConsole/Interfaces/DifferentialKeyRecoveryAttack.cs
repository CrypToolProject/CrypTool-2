using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public abstract class DifferentialKeyRecoveryAttack
    {
        public List<DifferentialAttackRoundConfiguration> RoundConfigurations;
        public List<DifferentialAttackRoundResult> RoundResults;
        public DifferentialAttackLastRoundResult LastRoundResult;

        public abstract string printRecoveredSubkeyBits();
    }
}
