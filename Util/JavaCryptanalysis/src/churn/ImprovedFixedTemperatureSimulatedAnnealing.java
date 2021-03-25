package churn;

import java.util.Random;

public class ImprovedFixedTemperatureSimulatedAnnealing {

    private Random random = new Random();

    // Fixed temperature optimized for hexagram scoring
    private static final double FIXED_TEMPERATURE = 20_000.0;

    /**
     * Simulated Annealing acceptance function.
     *
     * @param newKeyScore - score for the ney key
     * @param currentKeyScore - score for the current key
     * @return true if new key should be accepted.
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

        return acceptanceProbability > 0.0085
                && random.nextDouble() < acceptanceProbability;
    }
}
