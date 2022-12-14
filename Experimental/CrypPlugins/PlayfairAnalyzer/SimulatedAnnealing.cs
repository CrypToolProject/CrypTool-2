using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CrypTool.Plugins.PlayfairAnalyzer
{
    class SimulatedAnnealing
    {
        private static int[] CHURN_VALUES = {
            0, 0, 0, 0, 0, 0, 0,
            1, 1, 1, 1, 1, 1, 1,
            2, 2, 2, 2, 2, 2, 2,
            3, 3, 3, 3,
            4, 4, 4, 4, 4, 4, 4,
            5, 5, 5, 5, 5,
            6, 6, 6, 6, 6,
            7, 7,
            8, 8, 8, 8,
            9, 9, 9, 9, 9,
            10, 10, 10, 10,
            11, 11, 11, 11,
            12, 12,
            13, 13,
            14, 14,
            15, 15, 15, 15,
            16, 16,
            17, 17,
            18, 18,
            19,
            20, 20,
            21,
            22, 22,
            23, 24, 25, 26, 27, 28, 29, 30, 32, 33, 36, 38, 40, 42, 47, 56, 65
        };

        static Stopwatch sw = new Stopwatch();
        //private static long startTime = sw.currentTimeMillis();
        private static long evaluations = 0;
        private static long bestScoreOverall = 0;
        //private static ReentrantLock lock = new ReentrantLock();

        private static bool accept(long newScore, long previousScore)
        {
            return newScore > (previousScore - 20 * CHURN_VALUES[Utils.random.Next(100)]);
        }

        private static long score(byte[] plainText, int plainTextLength, String crib)
        {
            evaluations++;

            if (crib != null && crib.Length > 0)
            {
                int count = 0;
                for (int i = 0; i < crib.Length; i++)
                    if (plainText[i] == Utils.getCharIndex(crib[i]))
                        count++;

                return (25000 * count) / crib.Length;
            }

            return Stats.evalPlaintextPentagram(plainText, plainTextLength);
            //return Stats.evalPlaintextHexagram(plainText, plainTextLength);
        }

        public static void solveSA(PlayfairAnalyzer solver, int taskNumber, int maxCycles, bool removeXZ, byte[] cipherText, int cipherTextLength, String crib)
        {
            byte[] plainText = new byte[1000];
            byte[] plainTextFull = new byte[1000];

            Key currentKey = new Key(solver);
            Key newKey = new Key(solver);

            long serialCounter = 0;
            bestScoreOverall = 0;

            sw.Start();

            for (int cycle = 0; cycle < maxCycles || maxCycles == 0; cycle++)
            {
                if (maxCycles == 0) solver.ProgressChanged(50, 100);
                else solver.ProgressChanged(cycle, maxCycles);

                solver.transform.randomize();
                currentKey.random();

                int plainTextLength = solver.decrypt(cipherText, cipherTextLength, plainText, removeXZ, currentKey);
                long currentScore = score(plainText, plainTextLength, crib);

                for (int counter = 0; counter < 200000; counter++)
                {
                    if (solver.stopped) return;

                    solver.transform.applyTransformation(currentKey, newKey, serialCounter++);

                    plainTextLength = solver.decrypt(cipherText, cipherTextLength, plainText, removeXZ, newKey);
                    long newScore = score(plainText, plainTextLength, crib);

                    if (accept(newScore, currentScore))
                    {
                        newKey.copy(currentKey);
                        currentScore = newScore;
                        if (currentScore > bestScoreOverall)
                        {
                            bool print = false;
                            //lock.lock () ;

                            if (currentScore > bestScoreOverall)
                            {
                                print = true;
                                bestScoreOverall = currentScore;
                            }

                            if (print)
                            {
                                long elapsed = sw.ElapsedMilliseconds + 1;
                                string msg = String.Format("\nTask {0}, Elapsed: {1:D8} seconds, Evaluations: {2:D12}, Evaluations per sec: {3:D10}\n", taskNumber, elapsed / 1000, evaluations, (evaluations * 1000) / elapsed)
                                           + String.Format("Score: {0:D10}, Length (cipher/plain): {1:D3}/{2:D3}\n", currentScore, cipherTextLength, plainTextLength)
                                           + String.Format("Key: \n{0}", newKey);

                                solver.decrypt(cipherText, cipherTextLength, plainTextFull, false, newKey);

                                msg += String.Format("Ciphertext:       {0} {1:D3}\n", Utils.getString(cipherText, cipherTextLength), cipherTextLength);
                                if (crib != null && crib.Length > 0)
                                    msg += String.Format("Crib:             {0} {1:D3}\n", crib, crib.Length);
                                msg += String.Format("Plaintext(full):  {0} {1:D3}\n", Utils.getString(plainTextFull, cipherTextLength), cipherTextLength);
                                if (plainTextLength != cipherTextLength)
                                    msg += String.Format("Plaintext(clean): {0} {1:D3}\n", Utils.getString(plainText, plainTextLength), plainTextLength);

                                solver.GuiLogMessage(msg, PluginBase.NotificationLevel.Debug);
                                solver.Plaintext = Utils.getString(plainTextFull, plainTextLength);
                                //solver.Key = String.Format("{0}", newKey);
                                solver.Key = newKey.ToString();
                            }

                            //lock.unlock();
                        }
                    }
                }
            }
        }
    }
}