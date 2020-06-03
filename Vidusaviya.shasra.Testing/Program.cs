using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Vidusaviya.shasra.Testing
{
    class Program
    {

        static byte[] B;


        static void Main(string[] args)
        {
            Console.WriteLine("Vidusaviya Shasra");



            //string filecontent = "";

            //for (int i = 0; i < 20000; i++)
            //{
            //    filecontent += $"Hello go files {i} times!\n";
            //}

            //B = Encoding.UTF8.GetBytes(filecontent);


            //for (int i = 0; i < 2; i++)
            //{
            //    new Thread(Benchmark).Start();
            //    Thread.Sleep(5000);

            //}
            var cli = new GitClient("hariwij", "fuckyoubitch");
            foreach (var item in cli.GetFiles("Network-Usage-Meter","path"))
            {
                Console.WriteLine(item);
            }

            Console.ReadLine();
        }

        private static void Benchmark()
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


                string downres = goFileClient.Download(upres);

                elps = stopwatch.ElapsedMilliseconds;
                d = elps - last;
                last = elps;

                Console.WriteLine($"Got {downres.Length} bytes in { d }");

            }
        }
    }
}
