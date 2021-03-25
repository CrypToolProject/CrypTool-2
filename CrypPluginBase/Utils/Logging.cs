/*
   Copyright 2019 Simon Leischnig, based on the work of Soeren Rinne

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.PluginBase.Utils
{
    public static class LogLevels
    {
        public static Logging.LogLevel Debug    = new Logging.LogLevel.Debug();
        public static Logging.LogLevel Info     = new Logging.LogLevel.Info();
        public static Logging.LogLevel Warning  = new Logging.LogLevel.Warning();
        public static Logging.LogLevel Error    = new Logging.LogLevel.Error();
        public static Logging.LogLevel Balloon  = new Logging.LogLevel.Balloon();
    }
    namespace Logging
    {
        public static class GlobalLog
        {
            public static LogLevel defaultLevel = LogLevels.Debug;
            public static LogLevel maxOutLevel = LogLevels.Debug;
            public static Logger CErr = new WriterLogger(s => Console.Error.WriteLine(s));
            public static Logger COut = new WriterLogger(s => Console.Out.WriteLine(s));
            public static Logger VSDebug = new WriterLogger(s => System.Diagnostics.Debug.WriteLine(s));
        }
        public class LogLevel
        {
            public int Verbosity { get; }
            public LogLevel(int verbosity) { Verbosity = verbosity; }
            public LogLevel() : this(0) {}
            public class Debug   : LogLevel { public Debug() : base(6) { } }
            public class Info    : LogLevel { public Info() : base(5) { } }
            public class Warning : LogLevel { public Warning() : base(4) { }}
            public class Error   : LogLevel { public Error() : base(3) { }}
            public class Balloon : LogLevel { public Balloon() : base(2) { } }

        }
        public class LogMsg
        {
            public String Msg { get; }
            public LogLevel Level { get; }
            public LogMsg(string msg, LogLevel level)
            {
                this.Msg = msg;
                this.Level = level;
            }
            public LogMsg Map(String newMsg)
            {
                return new LogMsg(newMsg, this.Level);
            }
            public LogMsg MapV(LogLevel newLevel)
            {
                return new LogMsg(this.Msg, newLevel);
            }

            public static implicit operator LogMsg(String msg) => new LogMsg(msg, GlobalLog.defaultLevel);
            public static implicit operator String(LogMsg msg) => msg.Msg;
        }
        public class WriterLogger : Logger
        {
            public WriterLogger(Action<String> sink) : base("")
            {
                this.OnLog += msg => sink.Invoke(msg);
            }
            public WriterLogger(TextWriter writer) : this(s => writer.WriteLine(s)) { }
        }
        public class BufferLogger : Logger
        {
            public List<LogMsg> Buffer = new List<LogMsg>();
            public BufferLogger(string prefix="") : base(prefix) {
                this.OnLog += (msg) => Buffer.Add(msg);
            }
            public void Replay() => Buffer.ForEach(m => this.Log(m));
        }
        public class Logger
        {
            public String prefix;
            private int maxV = 10000;
            private LogLevel verbosityForward = null;

            public Logger(string prefix)
            {
                this.prefix = prefix;
            }

            public Logger Receiving(Logger l) { l.OnLog += this; return this; }

            public event Action<LogMsg> OnLog = (msg) => { };
            public void Log(LogMsg msg, LogLevel newVerbosity=null) {
                var m = msg;
                if (newVerbosity != null)
                {
                    m = m.MapV(newVerbosity);
                }
                if (verbosityForward != null)
                {
                    m = m.MapV(verbosityForward);
                }
                if (m.Level.Verbosity > maxV) return;
                if (prefix.Length > 0) { this.OnLog(m.Map($"{prefix} : {m.Msg}")); }
                else { this.OnLog(m); }
            }
            public Action<String> LogPrefixed(String prefix)
            {
                return msg => Log($"{prefix}");
            }
            public Logger WithVMax(LogLevel max) { this.maxV = max.Verbosity; return this; }
            public Logger Verbosity(LogLevel verbosity) { this.verbosityForward = verbosity; return this; }


            public static implicit operator Action<LogMsg>(Logger l) => (m) => l.Log(m);
        }
    }
}
