using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;

namespace Vidusaviya.shasra
{
    public class GitClient
    {
        private readonly GitHubClient Client;
        private readonly string Username;
        public GitClient(string username, string password)
        {
            Username = username;
            Client = new GitHubClient(new ProductHeaderValue("ViduSaviya"))
            {
                Credentials = new Credentials(username, password)
            };
        }
        public async Task<string> UploadFile(string Repo, string gitPath, string data, string branch = "master")
        {
            await Client.Repository.Content.CreateFile(Username, Repo, gitPath, new CreateFileRequest($"Add => {gitPath}", data, branch));
            return $"https://raw.githubusercontent.com/{Username}/{Repo}/{branch}/{gitPath}";
        }
        public async Task<string> UpdateFile(string Repo, string gitPath, string data, string branch = "master")
        {
            var res = Client.Repository.Content.GetAllContents(Username, Repo, gitPath).Result.First();
            await Client.Repository.Content.UpdateFile(Username, Repo, gitPath, new UpdateFileRequest($"Update => {gitPath}", data, res.Sha, branch));
            return $"https://raw.githubusercontent.com/{Username}/{Repo}/{branch}/{gitPath}";
        }
        public async Task<string> DeleteFile(string Repo, string gitPath, string branch = "master")
        {
            var res = Client.Repository.Content.GetAllContents(Username, Repo, gitPath).Result.First();
            await Client.Repository.Content.DeleteFile(Username, Repo, gitPath, new DeleteFileRequest($"Delete => {gitPath}", res.Sha, branch));
            return $"https://raw.githubusercontent.com/{Username}/{Repo}/{branch}/{gitPath}";
        }

        public async Task<string> DownloadFile(string Repo, string gitPath, string branch = "master")
        {
            return await Download($"https://raw.githubusercontent.com/{Username}/{Repo}/{branch}/{gitPath}");
        }

        public static async Task<string> Download(string URL)
        {
            using HttpClient httpClient = new HttpClient();
            return await httpClient.GetStringAsync(URL);

        }

        public IEnumerable<string> GetFiles(string Repo, string Path)
        {
            foreach (var item in Client.Repository.Content.GetAllContents(Username, Repo, Path).Result)
            {
                yield return item.Name;
            }
        }

        public int GetLastFileIndex(string Repo, string Path, string FilePrefix)
        {
            IReadOnlyList<RepositoryContent> result = Client.Repository.Content.GetAllContents(Username, Repo, Path).Result;

            int max = 0;
            foreach (var file in result)
            {
                if (file.Name.Length > FilePrefix.Length && file.Name.Substring(0,FilePrefix.Length) == FilePrefix && int.TryParse(file.Name.Substring(FilePrefix.Length), out int fi))
                {
                    max = max > fi ? max : fi;
                }
            }
            return max;
        }
    }


    public class GitFileClient
    {
        public string Username;
        public string Repo;
        public string Branch;
        public string Path;
        public string FilePrefix;

        public GitClient GitClient;

        public GitFileClient(string username, string password, string repo, string path, string filePrefix, string branch = "master")
        {
            Username = username;
            Repo = repo;
            Branch = branch;
            Path = path;
            FilePrefix = filePrefix;

            GitClient = new GitClient(username, password);

        }


        public async Task<string> Upload(string FileSuffix, string Data)
        {
            return await GitClient.UploadFile(Repo, Path + '/' + FilePrefix + FileSuffix, Data, Branch);
        }
        public async Task<string> Update(string FileSuffix, string Data)
        {
            return await GitClient.UpdateFile(Repo, Path + '/' + FilePrefix + FileSuffix, Data, Branch);
        }
        public async Task<string> Delete(string FileSuffix)
        {
            return await GitClient.DeleteFile(Repo, Path + '/' + FilePrefix + FileSuffix, Branch);
        }

        public async Task<string> Download(string FileSuffix)
        {
            return await GitClient.DownloadFile(Repo, Path + '/' + FilePrefix + FileSuffix, Branch);
        }

        public static async Task<string> DownloadFromURL(string URL)
        {
            return await Vidusaviya.shasra.GitClient.Download(URL);
        }


        public IEnumerable<string> GetFiles() => GitClient.GetFiles(Repo, Path);
        public int GetLastFileIndex() => GitClient.GetLastFileIndex(Repo, Path, FilePrefix);
    }
}
