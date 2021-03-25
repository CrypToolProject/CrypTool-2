package common;

import org.cryptool.ipc.Ct2Connector;
import org.cryptool.ipc.messages.Ct2IpcMessages;

import java.util.HashMap;
import java.util.Map;

public class CtAPI {

    static int INPUT_VALUE_CIPHERTEXT = 1;
    static int INPUT_VALUE_CRIB = 2;
    static int INPUT_VALUE_ARGS = 3;
    static int INPUT_VALUE_THREADS = 100;
    static int INPUT_VALUE_CYCLES = 200;
    static int INPUT_VALUE_RESOURCES = 300;

    private static int OUTPUT_VALUE_PLAINTEXT = 1;
    private static int OUTPUT_VALUE_KEY = 2;
    private static int OUTPUT_VALUE_SCORE = 3;
    private static int OUTPUT_VALUE_BEST_RESULTS = 1000;

    private static Map<Integer, String> inputValuesMap = new HashMap<>();

    // Public methods.

    /**
     * Open the connector, amd read the input values.
     *
     * @param attackName    - Name of attack
     * @param attackVersion - Attack version
     */
    public static void openAndReadInputValues(String attackName, String attackVersion) {
        long start = System.currentTimeMillis();
        int received = 0;
        String prevReceiverState = "";
        String prevSenderState = "";
        try {
            Ct2Connector.start(attackName, attackVersion, null);
            do {
                shutdownIfNeeded();
                int previousReceived = received;
                if (Ct2Connector.hasValues()) {
                    Map<Integer, String> values = Ct2Connector.getValues();
                    for (int input : values.keySet()) {
                        inputValuesMap.put(input, values.get(input));
                        printf("Received value for %d: %s\n", input, inputValuesMap.get(input));
                        received++;
                    }
                }
                String newReceiverState = Ct2Connector.getReceiverState().toString();
                String newSenderState = Ct2Connector.getSenderState().toString();
                if (!newReceiverState.equals(prevReceiverState) || !newSenderState.equals(prevSenderState) || received > previousReceived) {
                    System.out.printf("Receiver: %-15s, Sender: %-15s, Read: %d inputs\n", newReceiverState, newSenderState, received);
                    prevSenderState = newSenderState;
                    prevReceiverState = newReceiverState;
                }
            } while (System.currentTimeMillis() - start < 1000);
            if (received > 0) {
                CtAPI.println("Available processors (cores): " + Runtime.getRuntime().availableProcessors());
                CtAPI.printf("Free memory (bytes): %,d\n\n", Runtime.getRuntime().freeMemory());
            }
            displayBestList("-");
        } catch (Exception e) {
            displayExceptionAndGoodbye(e);
        }
    }

    /**
     * Fetch the input values.
     * @return a Map<Integer, String> with input values.
     */
    public static Map<Integer, String> getInputValuesMap() {
        // return a copy.
        return new HashMap<>(inputValuesMap);
    }

    /**
     * Replacement of System.out.printf, so that output is displayed in the terminal
     * as well as in CrypTool.
     *
     * @param format  - format, e.g. "%s", or "%3d %s\n"
     * @param objects - list of object to print.
     */
    public static void printf(String format, Object... objects) {
        String s = String.format(format, objects);
        print(s);
    }

    /**
     * Replacement of System.out.print, so that output is displayed in the terminal
     * as well as in CrypTool.
     *
     * @param s - String to be printed.
     */
    public static void print(String s) {
        logInfo(s);
        System.out.print(s);
    }

    /**
     * Replacement of System.out.println, so that output is displayed in the terminal
     * as well as in CrypTool.
     *
     * @param s - String to be printed.
     */
    public static void println(String s) {
        logInfo(s);
        System.out.println(s);
    }

    /**
     * Update the progress bar.
     *
     * @param value    - progress value. Should be > 0 and <= maxValue (unless maxValue == 0, see below).
     * @param maxValue - maximum value. 0 should be specified if the max value is unknown, in which case
     *                 a default maxValue of 100 is used, and value modulo 100 is used.
     */
    public static synchronized void updateProgress(long value, long maxValue) {
        try {
            if (maxValue <= 0) {
                Ct2Connector.encodeProgress(value % 100, 100);
            } else {
                Ct2Connector.encodeProgress(value, maxValue);
            }
        } catch (Exception e) {
            displayExceptionAndGoodbye(e);
        }
    }

    /**
     * Update the template best key box.
     *
     * @param keyString - string representing the best key so far (highest score).
     */
    public static void displayBestKey(String keyString) {
        updateOutput(OUTPUT_VALUE_KEY, keyString);
    }

    /**
     * Display the best decryption. Also used to display error messages in case of exiting the program with
     * fatal errors, such as errors in the command line options.
     *
     * @param plaintextString - plaintext obtained after decrypting with best key (or fatal error message).
     */
    public static void displayBestPlaintext(String plaintextString) {
        updateOutput(OUTPUT_VALUE_PLAINTEXT, plaintextString);
    }

    /**
     * Shutdown the attack after fatal error. Error message is printed and displayed in the best plaintext box.
     * @param format - format as in System.out.format, for the error message.
     * @param objects - Items to be formatted and printed.
     */
    public static synchronized void goodbyeFatalError(String format, Object... objects) {
        goodbye(-1, String.format(format, objects));
    }

    /**
     * Shutdown attack without error.
     */
    public static void goodbye() {
        goodbye(0, "Shutting down ... ");
    }

    // Package private - to be used only by common/ code.

    /**
     * Update the best list.
     *
     * @param bestList - the baest list, formatted.
     */
    static void displayBestList(String bestList) {
        updateOutput(OUTPUT_VALUE_BEST_RESULTS, bestList);
    }

    /**
     * Display the best result in the best plaintext, best key and best score output boxed. Also prints the details.
     * @param result - best result.
     */
    static void displayBestResult(CtBestList.Result result) {
        updateOutput(OUTPUT_VALUE_SCORE, String.format("%,d", result.score));
        updateOutput(OUTPUT_VALUE_KEY, result.keyString);
        updateOutput(OUTPUT_VALUE_PLAINTEXT, result.plaintextString);
        // In CrypTool log, we only show a short version of the key.
        logInfoFormatted("Best: %,12d %s %s %s\n", result.score, plaintextCapped(result.plaintextString), result.commentString, result.keyStringShort);
        // but we print the longer one in the terminal.
        System.out.printf("Best: %,12d %s %s %s\n", result.score, plaintextCapped(result.plaintextString), result.commentString, result.keyString);
    }

    /**
     * Display the best result key in the key box as well as the original key used (e.g. key used for
     * simulation).
     * @param bestResult - best result
     * @param original   - original (correct) result if known.
     */
    static void displayBestResult(CtBestList.Result bestResult, CtBestList.Result original) {
        if (original.keyString.isEmpty()) {
            updateOutput(OUTPUT_VALUE_KEY, bestResult.keyString);
        } else {
            updateOutput(OUTPUT_VALUE_KEY, bestResult.keyString + " (Original:" + original.keyString + ")");
        }

        updateOutput(OUTPUT_VALUE_SCORE, String.format("%,d (Original:%,d)", bestResult.score, original.score));

        if (original.plaintextString.isEmpty()) {
            updateOutput(OUTPUT_VALUE_PLAINTEXT, bestResult.plaintextString);
        } else {
            updateOutput(OUTPUT_VALUE_PLAINTEXT, bestResult.plaintextString + " (Original:" + original.plaintextString + ")");
        }
        logInfoFormatted("Best: %,12d %s %s \n%s\n", bestResult.score, plaintextCapped(bestResult.plaintextString), bestResult.commentString, bestResult.keyStringShort);
        System.out.printf("Best: %,12d %s %s %s\n", bestResult.score, plaintextCapped(bestResult.plaintextString), bestResult.commentString, bestResult.keyString);
        logInfoFormatted("      %,12d %s %s \n%s\n", original.score, plaintextCapped(original.plaintextString), original.commentString, original.keyStringShort);
        System.out.printf("      %,12d %s %s %s\n", original.score, plaintextCapped(original.plaintextString), original.commentString, original.keyString);
    }

    // Private.

    public static void shutdownIfNeeded() {
        if (Ct2Connector.getShutdownRequested()) {
            println("Received request to shutdown ....");
            java.awt.Toolkit.getDefaultToolkit().beep();
            System.exit(0);
        }
    }

    private static void logInfoFormatted(String format, Object... objects) {
        String s = String.format(format, objects);
        logInfo(s);
    }

    private static String plaintextCapped(String plaintext) {
        if (plaintext.length() <= 1000) {
            return plaintext;
        }
        return plaintext.substring(0, Math.min(100, plaintext.length())) + "...";
    }

    private static synchronized void goodbye(int exitCode, String message) {
        if (exitCode != 0) {
            String fullMessage = String.format("Shutting down (%d) - %s\n", exitCode, message);
            logError(fullMessage);
            CtAPI.displayBestPlaintext(fullMessage);
        } else {
            printf(message + "\n");
        }

        //TODO: remove.
        printf("Shutting down in 30 seconds ...\n");
        for (int i = 30; i > 0; i--) {
            try {
                Thread.sleep(1_000);
            } catch (InterruptedException ignored) {
            }
        }
        printf("Shutting down in 5 seconds ...\n");

        CtBestList.display();
        long start = System.currentTimeMillis();
        while (System.currentTimeMillis() - start < 5_000) {
            try {
                Ct2Connector.encodeGoodbye(exitCode, message);
                Thread.sleep(100);
                if (Ct2Connector.getShutdownRequested()) {
                    println("Received shutdown command from CrypTool");
                    break;
                }
            } catch (InterruptedException e) {
                displayExceptionAndGoodbye(e);
            }
        }

        java.awt.Toolkit.getDefaultToolkit().beep();
        System.exit(exitCode);
    }

    private static void displayExceptionAndGoodbye(Exception e) {
        logError(e.getMessage());
        e.printStackTrace();
        goodbyeFatalError(e.getMessage());
    }

    private static void log(String s, Ct2IpcMessages.Ct2LogEntry.LogLevel level) {

        try {
            while (!s.isEmpty() && (s.charAt(s.length() - 1) == '\n' || s.charAt(s.length() - 1) == '\r')) {
                s = s.substring(0, s.length() - 1);
            }
            Ct2Connector.encodeLogEntry(s, level);
        } catch (Exception e) {
            displayExceptionAndGoodbye(e);
        }
    }

    private static void logInfo(String s) {
        if (s.length() > 300) {
            s = s.substring(0, 300) + " ..... (truncated)";
        }
        log(s, Ct2IpcMessages.Ct2LogEntry.LogLevel.CT2INFO);
    }

    private static void logError(String message) {
        log(message, Ct2IpcMessages.Ct2LogEntry.LogLevel.CT2ERROR);
        System.out.println("Error: " + message);
    }

    private static void updateOutput(int i, String value) {

        try {
            Map<Integer, String> valuemap = new HashMap<>();
            valuemap.put(i, value);
            Ct2Connector.enqueueValues(valuemap);
        } catch (Exception e) {
            displayExceptionAndGoodbye(e);
        }
    }
}
