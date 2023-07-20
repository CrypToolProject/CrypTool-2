using M209AnalyzerLib.Common;

namespace M209AnalyzerLib.M209
{
    public class Simulation
    {
        public static Key CreateSimulationValues(M209AttackManager attackManager)
        {

            Key simulationKey = new Key();
            simulationKey.Pins.Randomize();
            simulationKey.Lugs.Randomize(attackManager.SimulationOverlaps);

            string plainText = Utils.ReadPlaintextSegmentFromResource(attackManager.Language, -1, attackManager.SimulationTextLength, true).Substring(0, attackManager.SimulationTextLength);
            string cipherText = simulationKey.EncryptDecrypt(plainText, true);

            simulationKey.SetCipherTextAndCrib(cipherText, plainText);

            return simulationKey;
        }
    }
}
