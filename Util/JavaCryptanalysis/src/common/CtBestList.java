package common;

import java.util.ArrayList;

public class CtBestList {
    /**
     * Class encapulating a result in best list.
     */
    static class Result {
        long score;
        String keyString;
        String keyStringShort;   // short version of the key
        String plaintextString;
        String commentString;
        Result(long score, String keyString,String keyStringShort, String plaintextString, String commentString) {
            set(score, keyString, keyStringShort, plaintextString, commentString);
        }
        void set(long score, String keyString,String keyStringShort, String plaintextString, String commentString) {
            this.score = score;
            this.keyString = keyString;
            this.keyStringShort = keyStringShort;
            this.plaintextString = plaintextString;
            this.commentString = commentString;
        }
        public String toString(int rank) {
            return String.format("%2d;%,12d;%s;%s;%s\n", rank, score, keyStringShort, plaintextString, commentString);
        }

    }

    private static ArrayList<Result> bestResults = new ArrayList<>();
    private static Result originalResult = null;
    private static long lastBestListUpdateMillis = 0;
    private static boolean shouldUpdateBestList = false;

    private static int maxNumberOfResults = 10;
    private static long scoreThreshold = 0;
    private static boolean discardSamePlaintexts = false;
    private static boolean throttle = false;

    /**
     * Set best list size.
     * @param size - max number of elements in best list.
     */
    public static synchronized void setSize(int size) {
        CtBestList.maxNumberOfResults = size;
        clean();
    }

    /**
     * Set a score threshold, below which result will not be included in best list.
     * @param scoreThreshold - threshold value
     */
    public static synchronized void setScoreThreshold(long scoreThreshold) {
        CtBestList.scoreThreshold = scoreThreshold;
        clean();
    }

    /**
     * If set to yes, ignore results with plaintext already seen (possibly with a different key).
     * @param discardSamePlaintexts
     */
    public static synchronized void setDiscardSamePlaintexts(boolean discardSamePlaintexts) {
        CtBestList.discardSamePlaintexts = discardSamePlaintexts;
        clean();
    }

    /**
     * If set to true, best list will be send to CrypTool no more than once every second.
     * This is useful in case there are many new results, in a short period of time, that would be one of the top best.
     * This can happen very often in hillclimbing processes which slowly progress.
     * @param throttle - if yes, throttle updates to CrypTool.
     */
    public static synchronized void setThrottle(boolean throttle) {
        CtBestList.throttle = throttle;
        clean();
    }

    /**
     * If known, keep the original key and/or plaintext, as well as the score value expected when decrypting with the
     * correct (original) key. If a new result has exactly this score value, the attack with stop.
     * @param score - expected score with the correct key.
     * @param keyString - the correct key.
     * @param keyStringShort - the correct key, short format.
     * @param plaintextString - the expected/original plaintext.
     * @param commentString - a comment
     */
    public static synchronized void setOriginal(long score, String keyString, String keyStringShort,String plaintextString, String commentString) {
        originalResult = new Result(score, keyString, keyStringShort, plaintextString, commentString);
    }

    /**
     * Resets the best list.
     */
    public static synchronized void clear() {
        bestResults.clear();
        CtAPI.displayBestList("-");
    }

    /**
     * Check whether a new result has a score that would allow it to be added to the best list.
     * Useful when some processing is required before pushing the result (e.g. formatting the key string). After formatting, then
     * pushResult should be called to push the result.
     * @param score - score of a new result.
     * @return - score is higher than the lower score in the best list.
     */
    public static synchronized boolean shouldPushResult(long score) {

        if (throttle) {
            if (shouldUpdateBestList && System.currentTimeMillis() - lastBestListUpdateMillis > 1000) {
                lastBestListUpdateMillis = System.currentTimeMillis();
                shouldUpdateBestList = false;
                display();
            }
        }

        if (score < scoreThreshold) {
            return false;
        }
        int size = bestResults.size();
        return size < maxNumberOfResults || score > bestResults.get(size - 1).score;
    }

    /**
     * Push a new result to the task list, if its score is highes that the lowest score in the best list.
     * Discard duplicate keys (and if the relevant option is set, keyes resulting in an already seen plaintext).
     * @param score
     * @param keyString
     * @param keyStringShort
     * @param plaintextString
     * @param commentString
     * @return
     */
    public static synchronized boolean pushResult(long score, String keyString, String keyStringShort, String plaintextString, String commentString) {
        if (discardSamePlaintexts) {
            for (Result be : bestResults) {
                if (be.plaintextString.equals(plaintextString)) {
                    return false;
                }
            }
        }
        for (Result be : bestResults) {
            if (be.keyString.equals(keyString)) {
                return false;
            }
        }
        int size = bestResults.size();
        boolean bestChanged = false;
        if (size == 0 || score > bestResults.get(0).score) {
            bestChanged = true;
        }
        if (size < maxNumberOfResults) {
            bestResults.add(new Result(score, keyString, keyStringShort, plaintextString, commentString));
        } else if (score > bestResults.get(size - 1).score) {
            bestResults.get(size - 1).set(score, keyString, keyStringShort, plaintextString, commentString);
        } else {
            return false;
        }
        sort();
        if (bestChanged) {
            Result bestResult = bestResults.get(0);
            if (originalResult == null) {
                CtAPI.displayBestResult(bestResult);
            } else {
                CtAPI.displayBestResult(bestResult, originalResult);
            }
        }
        if (throttle) {
            shouldUpdateBestList = true;
        } else {
            display();
        }
        return true;
    }

    // Package private.
    static synchronized void display() {
        StringBuilder s = new StringBuilder();
        sort();
        for (int i = 0; i < bestResults.size(); i++) {
            s.append(bestResults.get(i).toString(i + 1));
        }
        CtAPI.displayBestList(s.toString());
        //System.out.println(s.toString());
    }

    // Private.
    private static synchronized void clean() {
        sort();
        while (bestResults.size() > maxNumberOfResults) {
            bestResults.remove(bestResults.size() - 1);
        }
        while (!bestResults.isEmpty() && bestResults.get(bestResults.size() - 1).score < scoreThreshold) {
            bestResults.remove(bestResults.size() - 1);
        }
    }
    private static synchronized void sort() {
        bestResults.sort((o1, o2) -> (int) (o2.score - o1.score));
    }
}
