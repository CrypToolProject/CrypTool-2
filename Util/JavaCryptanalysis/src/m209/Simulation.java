package m209;

import common.Utils;
import common.Language;

import static common.CtAPI.printf;

public class Simulation {
    public static Key simulation(String dirname, Language language, int len, int overlaps) {
        String plain = Utils.readPlaintextSegmentFromFile(dirname, language, -1, len, true).substring(0, len);

        Key simulationKey = new Key();
        simulationKey.pins.randomize();
        simulationKey.lugs.randomize(overlaps);
        String cipher = simulationKey.encryptDecrypt(plain, true);
        simulationKey.setCipherAndCrib(cipher, plain);
        printf("Simulation\n");
        printf("%s\n%s\n", simulationKey.crib, simulationKey.cipher);
        simulationKey.pins.print();
        simulationKey.lugs.print();

        return simulationKey;
    }
}
