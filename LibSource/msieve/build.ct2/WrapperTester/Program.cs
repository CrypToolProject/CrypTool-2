using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace WrapperTester
{
    class Program
    {
        private static Queue relationpackagequeue;
        private static bool running;
        private static bool threadStopped;

        static bool prepareSieving(IntPtr conf, int update, IntPtr core_sieve_fcn, int max_relations)
        {
            Console.WriteLine("Update: " + update);

            relationpackagequeue = Queue.Synchronized(new Queue());

            //Create a thread:
            IntPtr clone = Msieve.msieve.cloneSieveConf(conf);
            WaitCallback worker = new WaitCallback(MSieveJob);
            running = true;
            ThreadPool.QueueUserWorkItem(worker, new object[] { clone, update, core_sieve_fcn, relationpackagequeue });

            Thread.Sleep(TimeSpan.FromSeconds(2));

            Task.Run(() =>
            {
                Msieve.msieve.stop(Msieve.msieve.getObjFromConf(clone));

                Msieve.msieve.stop(Msieve.msieve.getObjFromConf(conf));
                running = false;
            });

            while (running || !threadStopped)
            {
                Thread.Sleep(0);
            }

            return true;
        }

        public static void MSieveJob(object param)
        {
            object[] parameters = (object[])param;
            IntPtr clone = (IntPtr)parameters[0];
            int update = (int)parameters[1];
            IntPtr core_sieve_fcn = (IntPtr)parameters[2];
            Queue relationpackagequeue = (Queue)parameters[3];

            while (running)
            {
                Msieve.msieve.collectRelations(clone, update, core_sieve_fcn);
                IntPtr relationPackage = Msieve.msieve.getRelationPackage(clone);
                relationPackage = Msieve.msieve.deserializeRelationPackage(Msieve.msieve.serializeRelationPackage(relationPackage));
                relationpackagequeue.Enqueue(relationPackage);
            }
            threadStopped = true;
        }

        static void Main(string[] args)
        {            
            Msieve.callback_struct callbacks = new Msieve.callback_struct();
            callbacks.prepareSieving = prepareSieving;
            callbacks.putTrivialFactorlist = delegate(IntPtr list, IntPtr obj)
            {
                foreach (Object o in Msieve.msieve.getPrimeFactors(list))
                    Console.Out.WriteLine((String)o);
                foreach (Object o in Msieve.msieve.getCompositeFactors(list))
                    Console.Out.WriteLine((String)o);

                list = Msieve.msieve.msieve_run_core(obj, (String)Msieve.msieve.getCompositeFactors(list)[0]);
                foreach (Object o in Msieve.msieve.getPrimeFactors(list))
                    Console.Out.WriteLine("Prim: " + (String)o);
                foreach (Object o in Msieve.msieve.getCompositeFactors(list))
                    Console.Out.WriteLine("Composite: " + (String)o);
            };
            
            Msieve.msieve.initMsieve(callbacks);

            //ArrayList factors = Msieve.msieve.factorize("8490874917243147254909119 * 6760598565031862090687387", null);
            //Msieve.msieve.start("(2^300-1)/2", null);
            Msieve.msieve.start("(2^204-1)/2-1", null);
            //ArrayList factors = Msieve.msieve.factorize("(2^200 - 1) / 2", null);            

            Console.ReadLine();
        }
    }
}
