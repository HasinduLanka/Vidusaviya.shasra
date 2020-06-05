using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Vidusaviya.shasra.Testing
{
    class Program
    {
        private readonly static string GitUsername = RepoInfo.GitUsername;
        private readonly static string GitPassword = RepoInfo.vidup;
        private readonly static string GitRepo = RepoInfo.GitRepo;
        private readonly static string GitPath = RepoInfo.GitPath;
        private readonly static string FilePrefix = RepoInfo.FilePrefix;

        static string filecontent;


        static FileAsyncManager<string> fileAsyncManager;
        static FileAsyncManager<string> DownloadAsyncManager;


        static void Main()
        {
            Console.WriteLine("Vidusaviya Shasra");

            FireBaseClient fc = new FireBaseClient("", File.ReadAllText(@""));

            // Write
            var writeRes = fc.WriteDocument("rooms", "room1", new Dictionary<string, object> { { "Data", "Your Data" } });
            writeRes.Wait();
            Console.WriteLine($"Writing Result : {writeRes.Result}");

            // Read Docs In Collection
            var readcolRes = fc.ReadDocuments("rooms");
            readcolRes.Wait();
            Console.WriteLine($"Reading Collection :");
            foreach (var item in readcolRes.Result)
            {
                Console.WriteLine($"[Id : {item.Id}] [Path : {item.Reference.Path}]");
            }
            // Read Doc
            var readRes = fc.ReadDocument("rooms", "room1");
            readRes.Wait();
            Console.WriteLine($"Reading :");
            foreach (var item in readRes.Result.ToDictionary())
            {
                Console.WriteLine($"[Key : {item.Key}] [Value : {item.Value}]");
            }

            // B = Convert.FromBase64String(filecontent);
            Console.WriteLine($"File size {filecontent.Length / 1024} kB");
            // Read Data In Doc
            var readDocRes = fc.ReadData("rooms", "room1","Data");
            readDocRes.Wait();
            Console.WriteLine($"Reading Data : {readDocRes.Result}");

            ////Delete
            //var deleteRes = fc.DeleteDocument("rooms", "room1");
            //deleteRes.Wait();
            //Console.WriteLine($"Delete Result : {deleteRes.Result}");

            //filecontent = "";

            //for (int i = 0; i < 4000; i++)
            //{
            //    filecontent += $"Hello github {i} times!\n";
            //}

            //// B = Convert.FromBase64String(filecontent);
            //Console.WriteLine($"File size {filecontent.Length / 1024} kB");
            //B = new byte[10];

            fileAsyncManager = new FileAsyncManager<string>(2);

            for (int i = 0; i < 2; i++)
            {
                fileAsyncManager.Threads[i] = new FileAsyncThread<string>(new GitFileClient(GitUsername, GitPassword, GitRepo + (i + 1).ToString(), GitPath, FilePrefix));
                fileAsyncManager.Threads[i].LastFileIndex = fileAsyncManager.Threads[i].FileClient.GetLastFileIndex();
                Console.WriteLine($"{i}. Selected LastFileIndex {fileAsyncManager.Threads[i].LastFileIndex}");
            }


            DownloadAsyncManager = new FileAsyncManager<string>(2);
            for (int i = 0; i < 2; i++)
            {
                DownloadAsyncManager.Threads[i] = new FileAsyncThread<string>(new GitDownloadCient(GitUsername, GitRepo + (i + 1).ToString(), GitPath, FilePrefix));
            }


            stopwatchUpload = new Stopwatch();
            stopwatchUpload.Start();

            stopwatchDownload = new Stopwatch();
            stopwatchDownload.Start();

            Timer timer = new Timer(100);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();


            //for (int i = 1; i < 3; i++)
            //{
            //    new Thread(GithubBenchmarkUpload) { Name = "Upload " + i.ToString() }.Start(i.ToString());
            //    Thread.Sleep(500);
            //    new Thread(GithubBenchmarkDownload) { Name = "Download " + i.ToString() }.Start();
            //    Thread.Sleep(500);

            //}



            Console.ReadLine();
        }




        static bool TimerVoiding = false;

        public static Queue<string> uploads = new Queue<string>();
        public static Stopwatch stopwatchUpload;
        public static Stopwatch stopwatchDownload;

        private async static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (TimerVoiding) return;


            TimerVoiding = true;

            await GithubBenchmarkUpload();
            await GithubBenchmarkDownload();

            TimerVoiding = false;

        }



        private async static Task GithubBenchmarkUpload()
        {


            var thread = fileAsyncManager.Advance();
            string ThrID = fileAsyncManager.CurrThread.ToString();

            if (thread.task != null)
            {
                string url = await thread.task;

                lock (uploads)
                {
                    uploads.Enqueue(url);
                }

                long now = stopwatchUpload.ElapsedMilliseconds;
                long d = now - thread.LastTime;
                thread.LastTime = now;

                Console.WriteLine($"{ThrID}. Uploaded {url} in {d}");
            }
            else
            {
                thread.LastTime = stopwatchUpload.ElapsedMilliseconds;
            }




            string FileSuffix = (++thread.LastFileIndex).ToString();
            thread.task = thread.FileClient.Upload(FileSuffix, filecontent);


        }

        private static async Task GithubBenchmarkDownload()
        {

            string url;


            if (uploads.Count == 0)
            {
                return;
            }

            lock (uploads)
            {
                url = uploads.Dequeue();
            }


            var thread = DownloadAsyncManager.Advance();
            string ThrID = DownloadAsyncManager.CurrThread.ToString();

            if (thread.task != null)
            {
                int DownloadedLength;
                try
                {
                    DownloadedLength = (await thread.task).Length;

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ThrID}. Error {ex} : {ex.Message} \n {ex.StackTrace}");
                    DownloadedLength = -1;
                }

                long now = stopwatchUpload.ElapsedMilliseconds;
                long d = now - thread.LastTime;
                thread.LastTime = now;


                Console.WriteLine($"{ThrID}. Got {url}   {DownloadedLength / 1024} kB in { d }");
            }
            else
            {
                thread.LastTime = stopwatchUpload.ElapsedMilliseconds;
            }


            thread.task = thread.FileClient.DownloadFromURL(url);

        }

    }
}
