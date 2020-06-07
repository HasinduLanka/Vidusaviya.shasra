using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vidusaviya.shasra
{
    public class FileAsyncThread<T>
    {
        public IFileClient<T> FileClient;
        public Task<T> task;
        public int LastFileIndex = 0;
        public long LastTime = 0;

        public FileAsyncThread(IFileClient<T> fileClient)
        {
            FileClient = fileClient;
            task = null;
        }
    }

    public class FileAsyncManager<T>
    {
        public int Size;
        public FileAsyncThread<T>[] Threads;


        public FileAsyncManager(int NumberOfThreads)
        {
            Size = NumberOfThreads;
            Threads = new FileAsyncThread<T>[Size];
        }

        public FileAsyncManager(params FileAsyncThread<T>[] threads)
        {
            Size = threads.Length;
            Threads = threads;
        }



        public int CurrThread { get; private set; } = 0;
        public FileAsyncThread<T> Thread => Threads[CurrThread];


        /// <summary>
        /// Return Current Thread and Advance Thread Index (Wrapped)
        /// </summary>
        public FileAsyncThread<T> Advance()
        {
            int ret = CurrThread;
            CurrThread++;
            if (CurrThread == Size) CurrThread = 0;
            return Threads[ret];
        }
    }

}
