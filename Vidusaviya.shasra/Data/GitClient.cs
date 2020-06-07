using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Octokit;

namespace Vidusaviya.shasra
{
    public class GitClient
    {
        private readonly GitHubClient Client;
        private readonly HttpClient httpClient;
        private readonly string Username;
        public GitClient(string username, string password)
        {
            Username = username;
            Client = new GitHubClient(new ProductHeaderValue("ViduSaviya" + DateTime.Now.Millisecond.ToString()))
            {
                Credentials = new Credentials(username, password)
            };

            httpClient = new HttpClient();
        }
        public async Task<string> UploadFile(string Repo, string gitPath, string data, string branch = "master")
        {
            await Client.Repository.Content.CreateFile(Username, Repo, gitPath, new CreateFileRequest($"{gitPath}", data, branch));
            return $"https://raw.githubusercontent.com/{Username}/{Repo}/{branch}/{gitPath}";
        }
        public async Task<string> UpdateFile(string Repo, string gitPath, string data, string branch = "master")
        {
            var res = (await Client.Repository.Content.GetAllContents(Username, Repo, gitPath)).First();
            await Client.Repository.Content.UpdateFile(Username, Repo, gitPath, new UpdateFileRequest($"{gitPath}", data, res.Sha, branch));
            return $"https://raw.githubusercontent.com/{Username}/{Repo}/{branch}/{gitPath}";
        }
        public async Task<string> DeleteFile(string Repo, string gitPath, string branch = "master")
        {
            var res = (await Client.Repository.Content.GetAllContents(Username, Repo, gitPath)).First();
            await Client.Repository.Content.DeleteFile(Username, Repo, gitPath, new DeleteFileRequest($"{gitPath}", res.Sha, branch));
            return $"https://raw.githubusercontent.com/{Username}/{Repo}/{branch}/{gitPath}";
        }


        public async Task DeleteAll(string Repo, string gitPath, string branch = "master")
        {
            foreach (var item in await Client.Repository.Content.GetAllContents(Username, Repo, gitPath))
            {
                await Client.Repository.Content.DeleteFile(Username, Repo, gitPath + '/' + item.Name, new DeleteFileRequest($"{gitPath}", item.Sha, branch));
                //  Console.WriteLine($"Deleted {gitPath}/{item.Name}");
            }

        }



        public async Task<string> DownloadFile(string Repo, string gitPath, string branch = "master")
        {
            return await Download($"https://raw.githubusercontent.com/{Username}/{Repo}/{branch}/{gitPath}");
        }

        public async Task<string> Download(string URL)
        {
            return await httpClient.GetStringAsync(URL);

        }

        public async Task<Stream> StreamFromURL(string URL)
        {
            return await httpClient.GetStreamAsync(URL);
        }

        public IEnumerable<string> GetFiles(string Repo, string Path)
        {
            IReadOnlyList<RepositoryContent> result;

            try
            {
                result = Client.Repository.Content.GetAllContents(Username, Repo, Path).Result;
            }
            catch (Exception)
            {
                yield break;
            }

            foreach (var item in result)
            {
                yield return item.Name;
            }
        }

        public async Task<int> GetLastFileIndex(string Repo, string Path)
        {
            IReadOnlyList<RepositoryContent> result;

            try
            {
                result = await Client.Repository.Content.GetAllContents(Username, Repo, Path);
            }
            catch (Exception)
            {
                return -1;
            }

            int max = 0;
            foreach (var file in result)
            {
                if (int.TryParse(file.Name, out int fi))
                {
                    max = max > fi ? max : fi;
                }
            }
            return max;
        }
    }

    public class GitDownloadCient : IFileClient<string>
    {
        public string URLPrefix;
        public HttpClient HttpClient;
        public HttpListener HttpListener;

        public bool IsReadOnly => true;

        public GitDownloadCient(string uRLPrefix)
        {
            URLPrefix = uRLPrefix;
            HttpClient = new HttpClient();
        }

        public GitDownloadCient(string Username, string Repo, string gitPath, string branch = "master")
        {
            URLPrefix = $"https://raw.githubusercontent.com/{Username}/{Repo}/{branch}/{gitPath}";
            HttpClient = new HttpClient();
            HttpListener = new HttpListener();

        }

        public async Task<string> Upload(string FileSuffix, string Data)
        {
            return HttpListener.IsSupported ? "HttpListener Supported" : "HttpListener Not Supported";
        }

        public Task<string> Update(string FileSuffix, string Data)
        {
            return null;
        }

        public Task<string> Delete(string FileSuffix)
        {
            return null;
        }


        public async Task<string> Download(string FileSuffix)
        {
            return await HttpClient.GetStringAsync(URLPrefix + FileSuffix);
        }

        public async Task<string> DownloadFromURL(string URL)
        {
            return await HttpClient.GetStringAsync(URL);
        }

        public Task<int> GetLastFileIndex() => new Task<int>(() => -1);

        public Task DeleteAll() => null;

        public async Task<Stream> StreamFromURL(string URL)
        {
            return await HttpClient.GetStreamAsync(URL);
        }
    }

    public class GitFileClient : IFileClient<string>
    {
        public string Username;
        public string Repo;
        public string Branch;
        public string Path;

        public GitClient GitClient;

        public bool IsReadOnly => false;

        public GitFileClient(string username, string password, string repo, string path, string branch = "master")
        {
            Username = username;
            Repo = repo;
            Branch = branch;
            Path = path;

            GitClient = new GitClient(username, password);

        }

        public GitFileClient(GithubInfo settings)
        {
            Username = settings.UName;
            Repo = settings.Repo;
            Branch = settings.Branch;
            Path = settings.Path;

            GitClient = new GitClient(Username, settings.ps);
        }

        public async Task<string> Upload(string FileName, string Data)
        {
            return await GitClient.UploadFile(Repo, Path + '/' + FileName, Data, Branch);
        }
        public async Task<string> Update(string FileName, string Data)
        {
            return await GitClient.UpdateFile(Repo, Path + '/' + FileName, Data, Branch);
        }
        public async Task<string> Delete(string FileName)
        {
            return await GitClient.DeleteFile(Repo, Path + '/' + FileName, Branch);
        }

        public async Task<string> Download(string FileName)
        {
            return await GitClient.DownloadFile(Repo, Path + '/' + FileName, Branch);
        }

        public async Task<string> DownloadFromURL(string URL)
        {
            return await GitClient.Download(URL);
        }


        public IEnumerable<string> GetFiles() => GitClient.GetFiles(Repo, Path);
        public async Task<int> GetLastFileIndex()
        {
            int i = await GitClient.GetLastFileIndex(Repo, Path);

            if (i == -1)
            {
                //Create room
                i = 0;
                await GitClient.UploadFile(Repo, Path + '/' + "index", i.ToString(), Branch);
            }
            else
            {
                try
                {
                    await GitClient.UpdateFile(Repo, Path + '/' + "index", i.ToString(), Branch);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex} {ex.Message} \n {ex.StackTrace}");
                    await GitClient.UploadFile(Repo, Path + '/' + "index", i.ToString(), Branch);
                }

            }
            return i;
        }

        public async Task DeleteAll()
        {
            await GitClient.DeleteAll(Repo, Path, Branch);
        }

        public async Task<Stream> StreamFromURL(string URL) => await GitClient.StreamFromURL(URL);

    }
}
