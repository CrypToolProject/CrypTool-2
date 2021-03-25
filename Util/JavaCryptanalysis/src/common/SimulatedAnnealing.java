package common;

import churn.ChurnSimulatedAnnealing;
import churn.FixedTemperatureSimulatedAnnealing;
import churn.ImprovedFixedTemperatureSimulatedAnnealing;

public class SimulatedAnnealing {
    public static boolean acceptHexaScore(long newScore, long currLocalScore, int multiplier) {

        //return accept(newScore, currLocalScore, 275.0 * multiplier / 20.0);
        return accept(newScore, currLocalScore, 13.75 * multiplier);

    }

    private static double minRatio = Math.log(0.0085);
    public static boolean accept(long newScore, long currLocalScore, double temperature) {

        long diffScore = newScore - currLocalScore;
        if (diffScore > 0) {
            return true;
        }
        if (temperature == 0.0) {
            return false;
        }
        double ratio = diffScore / temperature;
        return ratio > minRatio && Math.pow(Math.E, ratio) > Utils.random.nextFloat();

    }
}
