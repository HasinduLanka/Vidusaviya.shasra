using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Vidusaviya.shasra
{
    public class VideoInjector
    {
        public HttpListener Listener = new HttpListener();
        public Queue<byte[]> Playlist = new Queue<byte[]>();

        public bool Running { get; private set; } = false;

        public int Port { get; private set; } = 0;
        public string URLPrefix { get; private set; }

        public VideoInjector()
        {

        }
        public VideoInjector(int port)
        {
            Port = port;
        }

        public int AutoPort()
        {
            //get an empty port
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            Port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return Port;
        }


        public async Task Start()
        {
            if (Port == 0) AutoPort();

            URLPrefix = "http://localhost:" + Port.ToString() + "/";

            Running = true;
            await Listen();
        }

        public void Enqueue(byte[] seg)
        {
            Playlist.Enqueue(seg);
        }

        public void Stop()
        {
            Running = false;
            Listener.Stop();
        }


        private async Task Listen()
        {
            Listener = new HttpListener();
            if (!HttpListener.IsSupported) throw new Exception("Not supported");
            Listener.Prefixes.Add(URLPrefix);
            Listener.Start();

            while (Running)
            {
                try
                {
                    HttpListenerContext context = await Listener.GetContextAsync();
                    await Process(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"VideoInjector.GetContext Error {ex} : {ex.Message} \n{ex.StackTrace}");
                }
            }
        }



        private async Task Process(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            Console.WriteLine(filename);
            //filename = filename.Substring(1);


            byte[] Input;

            Recheck:
            while (Playlist.Count == 0)
            {
                await Task.Delay(100);
            }

            try
            {
                Input = Playlist.Dequeue();
            }
            catch (InvalidOperationException)
            {
                goto Recheck;
            }

            int Length = Input.Length;


            try
            {
                //Adding permanent http response headers
                context.Response.ContentType = "application/octet-stream";
                context.Response.ContentLength64 = Length;
                context.Response.StatusCode = (int)HttpStatusCode.OK;

                await context.Response.OutputStream.WriteAsync(Input, 0, Length);
                await context.Response.OutputStream.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VideoInjector Error {ex} : {ex.Message} \n{ex.StackTrace}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }


            context.Response.OutputStream.Close();
        }



    }
}
