package churn;
import java.util.Random;

public class ChurnSimulatedAnnealing {

    private Random random = new Random();

    // Fixed temperature optimized for hexagram scoring
    private static final double FIXED_TEMPERATURE = 20_000.0;

    // Size of degradation threshold lookup table.
    private static final int LOOKUP_TABLE_SIZE = 100;

    // The churn algorithm lookup table of degradation thresholds.
    private final double[] degradationLookupTable = new double[LOOKUP_TABLE_SIZE];

    // Compute the churn algorithm lookup table of degradation thresholds.
    void computeDegradationLookupTable() {
        for (int index = 0; index < LOOKUP_TABLE_SIZE; index++)
            degradationLookupTable[index] =
                    FIXED_TEMPERATURE * Math.log(LOOKUP_TABLE_SIZE / (index + 1));
    }

    /**
     * Simulated Annealing acceptance function - Churn implementation.
     *
     * @param newKeyScore - score for the ney key
     * @param currentKeyScore - score for the current key
     * @return true if new key should be accepted.
     */
    boolean accept(double newKeyScore, double currentKeyScore) {

        // Always accept better keys
        if (newKeyScore > currentKeyScore) return true;

        // Fetch a random degradation threshold from the lookup table.
        int randomIndex = random.nextInt(LOOKUP_TABLE_SIZE);
        double degradationRandomThreshold = degradationLookupTable[randomIndex];

        // Degradation between current key and new key.
        double degradation = currentKeyScore - newKeyScore;

        return degradation < degradationRandomThreshold;
    }
}


