using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CadoNFS.Processing
{
    public static class PythonEnvironment
    {
        public static void Prepare(string cadoNfsDir)
        {
            Environment.SetEnvironmentVariable("BWC_BINDIR", Path.Combine(cadoNfsDir, "linalg", "bwc"));

            var currentPathEnvVarEntries = Environment.GetEnvironmentVariable("PATH")?.Split(';');
            var newPathEnvVarEntries = GetRequiredPathEnvVarEntries(cadoNfsDir).Union(currentPathEnvVarEntries).Distinct();
            Environment.SetEnvironmentVariable("PATH", string.Join(";", newPathEnvVarEntries));

            PythonEngine.PythonHome = Path.Combine(cadoNfsDir, "runtimes", "python-3.5.4-embed-amd64");

            PythonEngine.Initialize();

            //PythonEngine.BeginAllowThreads();
        }

        /// <summary>
        /// Returns alls paths which need to be included in the "PATH" environment variable for Cado NFS to work.
        /// </summary>
        private static IEnumerable<string> GetRequiredPathEnvVarEntries(string cadoNfsDir)
        {
            yield return Path.Combine(cadoNfsDir, "dlls32");
            yield return Path.Combine(cadoNfsDir, "runtimes", "python-3.5.4-embed-amd64");
            yield return Path.Combine(cadoNfsDir, "runtimes", "perl-5.24.0", "bin");
        }
    }
}
