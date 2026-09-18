/*                              
   Copyright 2026 Marc Philipp Kray, CrypTool Project

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

/// <summary>
/// This file establishes an isolated diagnostic telemetry abstraction (port) for the CrypLLM subsystem.
/// By explicitly decoupling internal message propagation from the primary host environment's routines,
/// it ensures that telemetry faults do not induce computational cascade failures during LLM integration logic.
/// </summary>

using System;

namespace CrypTool.CrypLLM.Ports
{
    /// <summary>
    /// Categorizes diagnostic log emission events to establish severity semantics harmonized 
    /// with the overarching CrypTool 2 host environment configurations.
    /// </summary>
    public enum LLMNotificationLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Balloon
    }

    /// <summary>
    /// Defines a contractual boundary for cross-layer diagnostic telemetries, enforcing 
    /// inversion of control between subsystem executions and the GUI dispatcher.
    /// </summary>
    public interface ILoggingPort
    {
        /// <summary>
        /// Dispatches a diagnostic notification trajectory.
        /// </summary>
        /// <param name="message">The substantive content payload of the log.</param>
        /// <param name="level">The categorical severity prioritizing application responses.</param>
        void Log(string message, LLMNotificationLevel level = LLMNotificationLevel.Info);
    }

    /// <summary>
    /// Serves as the static event aggregator bridging bounded internal subsystem routines 
    /// to subscribed external logging evaluators managed by the GUI.
    /// </summary>
    public static class LoggingPort
    {
        /// <summary>
        /// Synchronous multicast delegation resolving subsystem log emissions outward constraints.
        /// </summary>
        public static event Action<string, LLMNotificationLevel> OnLog;

        /// <summary>
        /// Triggers the cross-boundary event stream cautiously to ensure transient subscriber faults 
        /// do not contaminate the integrity of the semantic orchestration algorithms.
        /// </summary>
        public static void Log(string message, LLMNotificationLevel level = LLMNotificationLevel.Info)
        {
            try { OnLog?.Invoke(message, level); } catch { /* never throw */ }
        }
    }
}

namespace CrypTool.CrypLLM
{
    /// <summary>
    /// A localized utility abstraction facilitating uniform semantic notification injections 
    /// natively across the LLM component architecture without reiterating severity boundaries.
    /// </summary>
    public class Log
    {
        public static void Info(string message) => TryLog(message, Ports.LLMNotificationLevel.Info);
        public static void Warning(string message) => TryLog(message, Ports.LLMNotificationLevel.Warning);
        public static void Error(string message) => TryLog(message, Ports.LLMNotificationLevel.Error);
        public static void Debug(string message) => TryLog(message, Ports.LLMNotificationLevel.Debug);

        /// <summary>
        /// Evaluates and channels the respective diagnostic signature structurally outward.
        /// The unconditional suppression catch guarantees that ancillary telemetry anomalies 
        /// strictly cannot impede primary active execution topologies involving model inference paths.
        /// </summary>
        private static void TryLog(string message, Ports.LLMNotificationLevel level)
        {
            try
            {
                Ports.LoggingPort.Log(message, level);
            }
            catch
            {
                // Exception neutralization asserts telemetry independence from computational safety guarantees.
            }
        }
    }
}
