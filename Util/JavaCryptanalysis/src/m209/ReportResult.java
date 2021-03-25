package m209;

import common.CtBestList;
import common.CtAPI;

public class ReportResult {
    private static long startTimeMillis = System.currentTimeMillis();
    public static boolean simulation = false;
    public static boolean knownPlaintext = false;
    private static int pushed = 0;

    public static void reportResult(int task, int[] roundLayers, int layers, Key key, int currScore, String desc) {
        if (CtBestList.shouldPushResult(currScore)) {
            String decryption = key.encryptDecrypt(key.cipher, false);

            long elapsedMillis = System.currentTimeMillis() - ReportResult.startTimeMillis;
            String comment;
            if (simulation) {

                comment = String.format("[%2dL/%3dP][2**%2d][%,5d K/s][%2d: %3d/%2d/%2d/%2d %s] ",
                        key.getCountIncorrectLugs(), key.getCountIncorrectPins(),
                        (long) (Math.log(Key.evaluations) / Math.log(2)),
                        elapsedMillis == 0 ? 0 : Key.evaluations / elapsedMillis,
                        task,
                        roundLayers[0], layers > 1 ? roundLayers[1] : 0, layers > 2 ? roundLayers[2] : 0, layers > 3 ? roundLayers[3] : 0,
                        desc
                );
            } else {
                comment = String.format("[2**%2d][%,5d K/s][%2d: %3d/%2d/%2d/%2d %s] ",
                        (long) (Math.log(Key.evaluations) / Math.log(2)),
                        elapsedMillis == 0 ? 0 : Key.evaluations / elapsedMillis,
                        task,
                        roundLayers[0], layers > 1 ? roundLayers[1] : 0, layers > 2 ? roundLayers[2] : 0, layers > 3 ? roundLayers[3] : 0,
                        desc
                );

            }

            if (CtBestList.pushResult(currScore, key.toString(), key.lugs.getLugsString(), decryption, comment)) {
                pushed++;
                if (simulation) {
                    int error = key.getCountIncorrectLugs() * 5 + key.getCountIncorrectPins();
                    CtAPI.updateProgress(Math.max(100 - error, 0), 100);
                } else if (knownPlaintext) {
                    CtAPI.updateProgress(currScore - 120000, 10000);
                } else {
                    CtAPI.updateProgress(currScore, 58000);
                }
            }
        }
        if (currScore == key.originalScore) {
            long elapsedSec = (System.currentTimeMillis() - ReportResult.startTimeMillis)/1000;
            CtAPI.printf("Found key - Task %d - %,d decryptions - elapsed %,d seconds - reported %d results\n", task, Key.evaluations, elapsedSec, pushed);
            CtAPI.goodbye();
        }
    }

    public static void setOriginalKey(Key simulationKey, EvalType evalType) {
        CtBestList.setOriginal(simulationKey.eval(evalType), simulationKey.toString(), simulationKey.toString(), simulationKey.encryptDecrypt(simulationKey.cipher, false),
                "                                                        ");
    }
    public static void setDummyOriginalKeyForCrib(String crib) {
        CtBestList.setOriginal(130_000, "", "", crib, "");
    }
    public static void setThreshold(EvalType evalType) {
        switch (evalType) {
            case CRIB:
                CtBestList.setScoreThreshold(127_500);
                break;
            case MONO:
                CtBestList.setScoreThreshold(40_000);
                break;
        }
    }
}
