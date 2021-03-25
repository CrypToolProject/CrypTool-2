using Python.Runtime;
using System.IO;

namespace CadoNFS.Processing
{
    public class CadoNFSClient
    {
        private readonly string cadoNfsDir;

        public LoggingInterception Logging { get; }
        public int WorkerNr { get; }

        public CadoNFSClient(int workerNr, string cadoNfsDir)
        {
            WorkerNr = workerNr;
            this.cadoNfsDir = cadoNfsDir;
            Logging = new LoggingInterception();
        }

        public void Run()
        {
            using (Py.GIL())
            {
                //TODO: Individual attachment of logger to client not working (logs get mixed up):
                //Logging.Attach();

                dynamic sys = Py.Import("sys");
                sys.path.append(cadoNfsDir);
                Py.SetArgv(
                    Path.Combine(cadoNfsDir, "cado-nfs-client.py"),
                    $"--basepath={CreateTempFolder()}",
                    $"--clientid={WorkerNr}",
                    "--server=http://localhost:8012");  //TODO: Configurable

                dynamic cadoNfsClient = PythonEngine.ImportModule(@"cado-nfs-client");
                cadoNfsClient.main();
            }
        }

        private string CreateTempFolder()
        {
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }
    }
}
