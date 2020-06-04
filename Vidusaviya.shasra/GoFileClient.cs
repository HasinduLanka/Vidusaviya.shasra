using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Vidusaviya.shasra
{
    public class GoFileClient
    {
        readonly string getServerAPI = "https://apiv2.gofile.io/getServer";
        // readonly string uploadAPI1 = "https://{server}.gofile.io/upload";

        public string Server;

        public string GetBestServer()
        {

            HttpClient Http = new HttpClient();
            var res = Http.GetStringAsync(getServerAPI).Result;

            if (res != null)
            {
                var dic = JsonConvert.DeserializeObject<GoFileServerResult>(res);
                if (dic?.status == "ok")
                {
                    return dic.data?.server;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public string SelectBestServer()
        {
            Server = GetBestServer();
            return Server;
        }


        public GoFileUploadInfo Upload(byte[] bytes, string filename)
        {
            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent
            {
                { new StringContent("My file description"), "description" },

                { new ByteArrayContent(bytes), "filesUploaded", filename }
            };
            HttpResponseMessage response = httpClient.PostAsync($"https://{Server}.gofile.io/upload", form).Result;

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
            string sd = response.Content.ReadAsStringAsync().Result;


            var dic = JsonConvert.DeserializeObject<GoFileUploadResult>(sd);
            if (dic?.status == "ok")
            {
                return dic.data;
            }
            else
            {
                return null;
            }
        }

        public string UploadAndGetURL(byte[] bytes, string filename)
        {
            GoFileUploadInfo info = Upload(bytes, filename);
            //new Thread(new ParameterizedThreadStart(UnlockFile)).Start(info.code);
            UnlockFile(info.code);
            return $"https://{Server}.gofile.io/download/{info.code}/{filename}";

        }

        public void UnlockFile(object obj)
        {
            string code = (string)obj;
            HttpClient httpClient = new HttpClient();
            // var resgs = httpClient.GetAsync($"https://apiv2.gofile.io/getServer?c={code}").Result;
            _ = httpClient.GetAsync($"https://{Server}.gofile.io/getUpload?c={code}").Result;
            //var resd = httpClient.GetAsync($"https://gofile.io/d/{code}").Result;
            // Console.WriteLine($"upload \n {resgu.Content.ReadAsStringAsync().Result}\n");
        }

        public static string Download(string URL)
        {
            using HttpClient httpClient = new HttpClient();
            return httpClient.GetStringAsync(URL).Result;

        }
    }

    class GoFileServerResult
    {
        public string status;
        public GoFileServerInfo data;
    }
    class GoFileServerInfo
    {
        public string server;
    }


    public class GoFileUploadResult
    {
        public string status;
        public GoFileUploadInfo data;
    }
    public class GoFileUploadInfo
    {
        public string code;
        public string removalCode;
    }

    //{"status":"ok","data":{"code":"123Abc","removalCode":"3ZcBq12nTgb4cbSwJVYY"}}


}
