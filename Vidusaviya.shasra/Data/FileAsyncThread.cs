using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vidusaviya.shasra
{
    public class FileAsyncThread<TU, TD>
    {
        public IFileClient<TU, TD> FileClient;
        public AesRij Aes;

        public Task<object> task;

        public int LastFileIndex = 0;
        public long LastTime = 0;

        public FileAsyncThread(IFileClient<TU, TD> fileClient)
        {
            FileClient = fileClient;
            task = null;
        }
    }

    public class FileAsyncManager<TU, TD>
    {
        public int Size;
        public FileAsyncThread<TU, TD>[] Threads;


        public FileAsyncManager(int NumberOfThreads)
        {
            Size = NumberOfThreads;
            Threads = new FileAsyncThread<TU, TD>[Size];
        }

        public FileAsyncManager(params FileAsyncThread<TU, TD>[] threads)
        {
            Size = threads.Length;
            Threads = threads;
        }



        public int CurrThread { get; private set; } = 0;
        public FileAsyncThread<TU, TD> Thread => Threads[CurrThread];


        /// <summary>
        /// Return Current Thread and Advance Thread Index (Wrapped)
        /// </summary>
        public FileAsyncThread<TU, TD> Advance()
        {
            int ret = CurrThread;
            CurrThread++;
            if (CurrThread == Size) CurrThread = 0;
            return Threads[ret];
        }
    }

}
