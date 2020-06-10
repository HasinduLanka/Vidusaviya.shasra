using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Vidusaviya.shasra.Testing
{
    class Program
    {


        static string filecontent;


        static FileAsyncManager<string> fileAsyncManager;
        static FileAsyncManager<string> DownloadAsyncManager;


        static void Main()
        {
            Console.WriteLine("Vidusaviya Shasra");

            // TestFirebase();



            int Thrds = 1;
            switch (RepoInfo.TestMeeting.CDNType)
            {
                case CDNType.FTP:
                    break;
                case CDNType.Github:
                    Thrds = RepoInfo.TestMeeting.GithubInfos.Count;
                    break;
                case CDNType.Firestore:
                    break;
                default:
                    break;
            }


            fileAsyncManager = new FileAsyncManager<string>(Thrds);

            for (int i = 0; i < Thrds; i++)
            {
                var thr = new FileAsyncThread<string>(new GitFileClient(RepoInfo.TestMeeting.GithubInfos[i]));
                fileAsyncManager.Threads[i] = thr;
                //thr.FileClient.DeleteAll().Wait();
                thr.LastFileIndex = thr.FileClient.GetLastFileIndex().Result;
                Console.WriteLine($"{i}. Selected LastFileIndex {thr.LastFileIndex}");
            }



            filecontent = "";

            for (int i = 0; i < 4000; i++)
            {
                filecontent += $"Hello github {i} times!\n";
            }

            var peersettings = RepoInfo.TestMeeting.PeerSettings();

            DownloadAsyncManager = new FileAsyncManager<string>(Thrds);
            for (int i = 0; i < Thrds; i++)
            {
                DownloadAsyncManager.Threads[i] = new FileAsyncThread<string>(new GitDownloadCient(peersettings.URLPrefixes[i]));
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
