using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

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
        static byte[] B;


        static void Main()
        {
            Console.WriteLine("Vidusaviya Shasra");



            filecontent = "";

            for (int i = 0; i < 4000; i++)
            {
                filecontent += $"Hello github {i} times!\n";
            }

            // B = Convert.FromBase64String(filecontent);
            Console.WriteLine($"File size {filecontent.Length / 1024} kB");
            B = new byte[10];


            GitFileClient octo = new GitFileClient(GitUsername, GitPassword, GitRepo, GitPath, FilePrefix);
            foreach (var item in octo.GetFiles())
            {
                Console.WriteLine(item);
            }



            for (int i = 1; i < 3; i++)
            {
                new Thread(GithubBenchmarkUpload) { Name = "Upload " + i.ToString() }.Start(i.ToString());
                Thread.Sleep(500);
                new Thread(GithubBenchmarkDownload) { Name = "Download " + i.ToString() }.Start();
                Thread.Sleep(500);

            }



            Console.ReadLine();
        }


        public static Queue<string> uploads = new Queue<string>();
        private async static void GithubBenchmarkUpload(object o)
        {

            string ThrID = (string)o;
            var octo1 = new GitFileClient(GitUsername, GitPassword, GitRepo + ThrID, GitPath, FilePrefix);

            int LastFileIndex = octo1.GetLastFileIndex() + 1;
            Console.WriteLine($"{ThrID}. Selected LastFileIndex " + LastFileIndex.ToString());

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            long last = 0;


            for (int j = 0; j < 1000; j++)
            {

                string FileSuffix = (++LastFileIndex).ToString();
                string url = await octo1.Upload(FileSuffix, filecontent);

                lock (uploads)
                {
                    uploads.Enqueue(url);
                }

                long elps = stopwatch.ElapsedMilliseconds;
                long d = elps - last;
                last = elps;

                Console.WriteLine($"{ThrID}. Uploaded {url} in {d}");

            }

        }

        private static async void GithubBenchmarkDownload()
        {


            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            long last;


            for (int j = 0; j < 1000; j++)
            {

                string url;

                lock (B)
                {

                    while (uploads.Count == 0)
                    {
                        Thread.Sleep(100);
                    }

                    lock (uploads)
                    {
                        url = uploads.Dequeue();
                    }
                }

                last = stopwatch.ElapsedMilliseconds;

                int DownloadedLength;
                try
                {
                    DownloadedLength = (await GitFileClient.DownloadFromURL(url)).Length;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error {ex} : {ex.Message} \n {ex.StackTrace}");
                    DownloadedLength = -1;
                }


                long elps = stopwatch.ElapsedMilliseconds;
                long d = elps - last;
                Console.WriteLine($" Got {url}   {DownloadedLength / 1024} kB in { d }");


            }

        }


        public static void GoBenchmark()
        {
            GoFileClient goFileClient = new GoFileClient();
            Console.WriteLine($"Server is {goFileClient.SelectBestServer()}");


            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            long last = 0;


            for (int j = 0; j < 10; j++)
            {

                var upres = goFileClient.UploadAndGetURL(B, "file1.txt");

                long elps = stopwatch.ElapsedMilliseconds;
                long d = elps - last;
                last = elps;

                Console.WriteLine($"Uploaded to {upres} in {d}");


                string downres = GoFileClient.Download(upres);

                elps = stopwatch.ElapsedMilliseconds;
                d = elps - last;
                last = elps;

                Console.WriteLine($"Got {downres.Length} bytes in { d }");

            }
        }
    }
}
