using Python.Runtime;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

//Inspired from: https://github.com/pythonnet/pythonnet/issues/766

namespace CadoNFS.Processing
{
    public class Interrupter
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int PendingCall(IntPtr arg);
        private readonly PendingCall interruptAction;

        private bool ignore;

        public Interrupter()
        {
            interruptAction = DoInterrupt;
        }

        private int DoInterrupt(IntPtr arg)
        {
            if (!ignore)
            {
                Exceptions.SetError(Exceptions.KeyboardInterrupt, "interrupted");
            }
            return 0;
        }

        public void Interrupt()
        {
            IntPtr func = Marshal.GetFunctionPointerForDelegate(interruptAction);
            var Py_AddPendingCall = typeof(Runtime).GetMethod("Py_AddPendingCall", BindingFlags.Static | BindingFlags.NonPublic);
            Py_AddPendingCall.Invoke(null, new object[] { func, IntPtr.Zero });
        }

        public void Ignore()
        {
            ignore = true;
        }
    }
}
