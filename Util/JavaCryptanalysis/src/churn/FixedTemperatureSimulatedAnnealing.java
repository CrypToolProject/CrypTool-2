package churn;

import java.util.Random;

public class FixedTemperatureSimulatedAnnealing {

    private Random random = new Random();

    // Fixed temperature optimized for hexagram scoring
    private static final double FIXED_TEMPERATURE = 20_000.0;

    /**
     * Simulated annealing acceptance function.
     *
     * @param newKeyScore - score for the ney key
     * @param currentKeyScore - score for the current key
     * @return true if new key should be accepted
     */
    boolean accept(double newKeyScore, double currentKeyScore) {

        // Always accept better keys
        if (newKeyScore > currentKeyScore) {
            return true;
        }

        // Degradation between current key and new key.
        double degradation = currentKeyScore - newKeyScore;

        double acceptanceProbability =
                Math.pow(Math.E, - degradation / FIXED_TEMPERATURE);

        return random.nextDouble() < acceptanceProbability;
    }
}
