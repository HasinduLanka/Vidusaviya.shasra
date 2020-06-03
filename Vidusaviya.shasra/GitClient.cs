using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Octokit;

namespace Vidusaviya.shasra
{
    public class GitClient
    {
        private GitHubClient Client;
        private string Username;
        public GitClient(string username, string password)
        {
            Username = username;
            Client = new GitHubClient(new ProductHeaderValue("ViduSaviya"));
            Client.Credentials = new Credentials(username, password);
        }
        public void UploadFile(string Repo, string gitPath, string data, string branch = "master")
        {
            Client.Repository.Content.CreateFile(Username, Repo, gitPath, new CreateFileRequest($"Add => {gitPath}", data, branch));
        }
        public void UpdateFile(string Repo, string gitPath, string data, string branch = "master")
        {
            var res = Client.Repository.Content.GetAllContents(Username, Repo, gitPath).Result.First();
            Client.Repository.Content.UpdateFile(Username, Repo, gitPath, new UpdateFileRequest($"Update => {gitPath}", data, res.Sha, branch));
        }
        public void DeleteFile(string Repo, string gitPath, string data, string branch = "master")
        {
            var res = Client.Repository.Content.GetAllContents(Username, Repo, gitPath).Result.First();
            Client.Repository.Content.DeleteFile(Username, Repo, gitPath, new DeleteFileRequest($"Delete => {gitPath}", res.Sha, branch));
        }
        public List<string> GetFiles(string Repo)
        {
            var tmp = new List<string>();
            foreach (var item in Client.Repository.Content.GetAllContents(Username, Repo).Result)
            {
                tmp.Add(item.Name);
            }
            return tmp;
        }
        public List<string> GetFiles(string Repo, string Path)
        {
            var tmp = new List<string>();
            foreach (var item in Client.Repository.Content.GetAllContents(Username, Repo, Path).Result)
            {
                tmp.Add(item.Name);
            }
            return tmp;
        }
    }
}
