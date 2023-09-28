using System;
using System.Threading;
using System.Threading.Tasks;

namespace CadoNFS.Processing
{
    /// <summary>
    /// Python.NET has trouble executing subsequently in different threads.
    /// Therefore, this class serves as *the* python main thread.
    /// </summary>
    public class PythonThread
    {
        private readonly AutoResetEvent resetEvent = new AutoResetEvent(true);
        private Action threadAction;
        private bool stopped;

        public void Start()
        {
            var thread = new Thread(() => ThreadActionLoop());
            thread.Start();
        }

        public void Stop()
        {
            stopped = true;
            resetEvent.Set();
        }

        /// <summary>
        /// Execute a function in the python thread.
        /// </summary>
        public T Exec<T>(Func<T> function)
        {
            var completionSource = new TaskCompletionSource<T>();
            threadAction = () =>
            {
                var result = function();
                completionSource.SetResult(result);
            };
            resetEvent.Set();

            return completionSource.Task.Result;
        }

        private void ThreadActionLoop()
        {
            while (!stopped)
            {
                resetEvent.WaitOne();
                var action = threadAction;
                threadAction = null;
                action?.Invoke();
            }
        }
    }
}
