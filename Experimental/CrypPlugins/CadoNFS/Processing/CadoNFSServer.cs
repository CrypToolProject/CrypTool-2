using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CadoNFS.Processing
{
    public class CadoNFSServer
    {
        private readonly string cadoNfsDir;

        public LoggingInterception Logging { get; }

        public CadoNFSServer(string cadoNfsDir)
        {
            this.cadoNfsDir = cadoNfsDir;
            Logging = new LoggingInterception();
        }

        public IEnumerable<BigInteger> Factorize(BigInteger number)
        {
            try
            {
                using (Py.GIL())
                {
                    Logging.Attach();

                    dynamic sys = Py.Import("sys");
                    sys.path.append(cadoNfsDir);
                    Py.SetArgv(
                        Path.Combine(cadoNfsDir, "cado-nfs.py"), 
                        number.ToString(), 
                        "server.whitelist=0.0.0.0/0",
                        "server.ssl=no",
                        "server.port=8012", //TODO: make configurable
                        "--server");

                    dynamic cadoNfs = PythonEngine.ImportModule("cado-nfs");
                    var result = cadoNfs.main();
                    return ConvertFactors(result);
                }
            }
            catch (PythonException ex)
            {
                return null;
            }
        }

        private IEnumerable<BigInteger> ConvertFactors(PyObject result)
        {
            var factors = (string[]) result.AsManagedObject(typeof(string[]));
            return factors.Select(fac => BigInteger.Parse(fac));
        }
    }
}
