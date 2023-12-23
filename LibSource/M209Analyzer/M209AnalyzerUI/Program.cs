using M209AnalyzerLib;
using M209AnalyzerLib.Common;
using M209AnalyzerLib.Enums;
using M209AnalyzerLib.M209;
using System;
using System.Diagnostics;
using System.Threading;

namespace M209AnalyzerUI
{
    internal class Program
    {
        private static double _bestScore = 0;
        private static Stopwatch _stopwatch;

        private static object LOCK = new object();

        private static void createCommandLineArguments()
        {

            CommandLine.createCommonArguments();

            CommandLine.add(new CommandLine.Argument(
                    Flag.LANGUAGE,
                    "Language",
                    "Language used for statistics and for simulation random text.",
                    false,
                    "ENGLISH",
                    new String[] { "ENGLISH", "FRENCH", "ITALIAN", "GERMAN", }));

            CommandLine.add(new CommandLine.Argument(
                    Flag.VERSION,
                    "Version",
                    "Version of operating instructions.",
                    false,
                    "V1947",
                    new String[] { "V1942", "V1944", "V1947", "V1953", "UNRESTRICTED", }));

            CommandLine.add(new CommandLine.Argument(
                    Flag.SIMULATION,
                    "Simulation",
                    "Create ciphertext from random key and plaintext. Simulation modes: 0 (default) - no simulation, 1 - ciphertext only, 2 - with crib.",
                    false,
                    0, 2, 0));

            CommandLine.add(new CommandLine.Argument(
                    Flag.SIMULATION_TEXT_LENGTH,
                    "Length of text for simulation",
                    "Length of random plaintext encrypted for simulation.",
                    false,
                    1, 5000, 1500));

            CommandLine.add(new CommandLine.Argument(
                    Flag.SIMULATION_OVERLAPS,
                    "Number of lug overlaps for simulation key",
                    "Number of lug overlaps for simulation key i.e. bars involving two wheels.",
                    false,
                    0, 14, 2));
        }
        static void Main(string[] args)
        {
            createCommandLineArguments();

            CommandLine.parseAndPrintCommandLineArgs(args);

            M209AttackManager m209Analyzer = new M209AttackManager(new M209Scoring());

            m209Analyzer.OnLogMessage += M209Analyzer_OnLogMessage;
            m209Analyzer.OnNewBestListEntry += M209Analyzer_OnNewBestListEntry;
            m209Analyzer.OnProgressStatusChanged += M209Analyzer_OnProgressStatusChanged;


            m209Analyzer.Threads = CommandLine.getIntegerValue(Flag.THREADS);
            var simulationMode = CommandLine.getIntegerValue(Flag.SIMULATION);
            m209Analyzer.SimulationTextLength = CommandLine.getIntegerValue(Flag.SIMULATION_TEXT_LENGTH);

            var logFilePath = $"log_{DateTime.Now.Year}_{DateTime.Now.Month.ToString("00")}_{DateTime.Now.Day.ToString("00")}_";

            if (simulationMode == 1)
            {
                logFilePath += "CiphertextOnly_";
            }
            else if (simulationMode == 2)
            {
                logFilePath += "KnownPlaintext_";
            }
            logFilePath += $"{m209Analyzer.SimulationTextLength}";
            logFilePath += ".txt";

            Logger.LogPath = System.Reflection.Assembly.GetEntryAssembly().Location.Replace(".exe", logFilePath);
            Logger.WriteLog("Application started.");


            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            simulationMode = 1;

            if (simulationMode != 0)
            {
                m209Analyzer.UseSimulationValues();
                if (simulationMode == 1)
                {
                    m209Analyzer.CipherTextOnlyAttack();
                }
                else if (simulationMode == 2)
                {
                    m209Analyzer.KnownPlainTextAttack();
                }
            }

            Console.ReadLine();


        }

        public static void StopM209Analyzer(M209AttackManager attackManager)
        {
            Thread.Sleep(122000);
            attackManager.ShouldStop = true;
        }

        private static void M209Analyzer_OnProgressStatusChanged(object sender, M209AttackManager.OnProgressStatusChangedEventArgs args)
        {
            double percentage = Math.Round(((double)(args.Counter) / (double)args.TargetValue) * 100);
            Console.Write("\r");
            Console.Write($"[{args.ElapsedTime}] [Thread {Thread.CurrentThread.ManagedThreadId}] Progress Changed: [{args.AttackType}|{args.Phase}]  \t- {args.Counter}/{args.TargetValue} ({percentage}%) " +
                $"[2**{(long)(Math.Log(args.EvaluationCount) / Math.Log(2))}][{(args.ElapsedTime.TotalMilliseconds == 0 ? 0 : args.EvaluationCount / args.ElapsedTime.TotalMilliseconds)} K/s]");
            //Logger.WriteLog($"[{args.ElapsedTime}] [Thread {Thread.CurrentThread.ManagedThreadId}] Progress Changed: [{args.AttackType}|{args.Phase}]  \t- {args.Counter}/{args.TargetValue} ({percentage}%)");
        }

        static void M209Analyzer_OnLogMessage(object sender, M209AttackManager.OnLogMessageEventArgs args)
        {
            Console.Write(args.Message);
        }

        static void M209Analyzer_OnNewBestListEntry(object sender, M209AttackManager.OnNewBestListEntryEventArgs args)
        {
            string msg = "";
            M209AttackManager attackManager = sender as M209AttackManager;
            if (args.Score > _bestScore)
            {
                string decyptedText = Utils.GetString(args.Key.Decryption);
                msg = $"[{attackManager.ElapsedTime}] [Thread {Thread.CurrentThread.ManagedThreadId}/{attackManager.Threads}] New BestList entry: [Score: {args.Score}/{args.Key.OriginalScore}]" +
                    $" [{args.Key.GetCountIncorrectLugs()}L/{args.Key.GetCountIncorrectPins()}P]" +
                    $"[2**{(long)(Math.Log(attackManager.EvaluationCount) / Math.Log(2))} ({attackManager.EvaluationCount})][{(attackManager.ElapsedTime.TotalMilliseconds == 0 ? 0 : attackManager.EvaluationCount / attackManager.ElapsedTime.TotalMilliseconds)} K/s] Length:{decyptedText.Length} ::: {decyptedText} \n";
                //Console.WriteLine(msg);
                _bestScore = args.Score;
                //Logger.WriteLog($"{attackManager.CipherText.Length} {attackManager.ElapsedTime}, {attackManager.EvaluationCount}, 2^{(long)(Math.Log(attackManager.EvaluationCount) / Math.Log(2))}, " +
                //        $"{(attackManager.ElapsedTime.TotalMilliseconds == 0 ? 0 : attackManager.EvaluationCount / attackManager.ElapsedTime.TotalMilliseconds)} K/s, " +
                //        $" Score: {args.Score} " +
                //        $"{attackManager.Threads} Threads" +
                //        $"\n {Utils.GetString(args.Decryption)}" +
                //        $"\n {attackManager.Crib} \n -------------");
            }
            lock (LOCK)
            {
                if (args.Key.OriginalKey != null && args.Score >= args.Key.OriginalScore)
                {
                    Logger.WriteLog($"{attackManager.CipherText.Length} {attackManager.ElapsedTime}, {attackManager.EvaluationCount}, 2^{(long)(Math.Log(attackManager.EvaluationCount) / Math.Log(2))}, " +
                        $"{(attackManager.ElapsedTime.TotalMilliseconds == 0 ? 0 : attackManager.EvaluationCount / attackManager.ElapsedTime.TotalMilliseconds)} K/s, " +
                        $"{attackManager.Threads} Threads  Score: {args.Score} " +
                        $"\n {Utils.GetString(args.Decryption)}" +
                        $"\n {attackManager.Crib} \n -------------");
                    Console.WriteLine($"{attackManager.CipherText.Length} {attackManager.ElapsedTime}, {attackManager.EvaluationCount}, 2^{(long)(Math.Log(attackManager.EvaluationCount) / Math.Log(2))}, " +
                        $"{(attackManager.ElapsedTime.TotalMilliseconds == 0 ? 0 : attackManager.EvaluationCount / attackManager.ElapsedTime.TotalMilliseconds)} K/s, " +
                        $"{attackManager.Threads} Threads" +
                        $"\n {Utils.GetString(args.Decryption)}" +
                        $"\n {attackManager.Crib} \n -------------");
                    attackManager.ShouldStop = true;

                    //Start process, friendly name is something like MyApp.exe (from current bin directory)
                    //System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);

                    //Close the current process
                    Environment.Exit(0);

                }

            }
        }
    }
}
