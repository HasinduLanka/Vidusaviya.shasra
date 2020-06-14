using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
        public async Task<string> CreateRepo(string Name)
        {
            NewRepository newRepo = new NewRepository(Name);
            newRepo.AutoInit = true;
            await Client.Repository.Create(newRepo);
            return $"https://raw.githubusercontent.com/{Username}/{Name}/";
        }
        public async Task<List<GitHubCommit>> GetCommits(string Repo)
        {
            var res = await Client.Repository.Get(Username, Repo);
            var commits = await Client.Repository.Commit.GetAll(res.Id);
            List<GitHubCommit> commitList = new List<GitHubCommit>();
            foreach (GitHubCommit commit in commits)
            {
                commitList.Add(commit);
            }
            return commitList;
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
        public async Task<byte[]> BytesFromURL(string URL)
        {
            return await httpClient.GetByteArrayAsync(URL);
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
        [Obsolete]
        public async Task<List<CommitInfo>> GetCommits(string repo, string branch = "master")
        {
            string url = $"https://github.com/{Username}/{repo}/commits/{branch}";
            var cli = new HttpClient();
            var res = await cli.GetAsync(url);
            var body = await res.Content.ReadAsStringAsync();
            int start = body.IndexOf("<div class=" + '"' + "commits-listing");
            int end = body.IndexOf("<div class=" + '"' + "paginate-container" + '"');
            body = body.Substring(start, end - start);
            string regularExpressionPattern1 = @"<li(.*?)<\/li>";
            Regex regex = new Regex(regularExpressionPattern1, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            int s1;
            int s2;
            var tmp = new List<CommitInfo>();
            foreach (var item in regex.Matches(body))
            {
                s1 = item.ToString().IndexOf("<a aria-label=" + '"') + ("<a aria-label=" + '"').Length;
                //Title
                string Title = item.ToString().Substring(s1);
                Title = Title.Substring(0, Title.IndexOf('"'));
                //Hash
                string Hash = item.ToString().Substring(s1);
                s1 = Hash.IndexOf("commit/") + "commit/".Length;
                s2 = Hash.IndexOf('"' + ">");
                Hash = Hash.Substring(s1, s2 - s1);
                //Time
                s1 = item.ToString().IndexOf("datetime=" + '"') + ("datetime=" + '"').Length;
                string Time = item.ToString().Substring(s1);
                Time = Time.Substring(0, Time.IndexOf('"'));
                // By
                s1 = item.ToString().IndexOf("AvatarStack-body" + '"' + " aria-label=" + '"') + ("AvatarStack-body" + '"' + " aria-label=" + '"').Length;
                string By = item.ToString().Substring(s1);
                By = By.Substring(0, By.IndexOf('"'));
                tmp.Add(new CommitInfo { Title = Title, Hash = Hash, Time = Time, By = By });
            }
            cli.Dispose();
            return tmp;
        }

    }
    public class CommitInfo
    {
        public string Title { get; set; }
        public string Hash { get; set; }
        public string Time { get; set; }
        public string By { get; set; }
        public override string ToString()
        {
            return $"[Title : {Title}]  [Hash : {Hash}] [Time : {Time}] [By : {By}]";
        }
    }
    public class GitDownloadCient : IFileClient<byte[], string>
    {
        public string URLPrefix;
        public HttpClient HttpClient = new HttpClient();
        //public VideoInjector Injector;
        // public Task TListen;

        public int LastQueueIndex = 0;

        public bool IsReadOnly => true;

        public GitDownloadCient(string uRLPrefix)
        {
            URLPrefix = uRLPrefix;

            //if (injector == null) injector = new VideoInjector();
            //Injector = injector;

            //if (!Injector.Running) TListen = Injector.Start();

        }


        public GitDownloadCient(string Username, string Repo, string gitPath, string branch = "master")
        {
            URLPrefix = $"https://raw.githubusercontent.com/{Username}/{Repo}/{branch}/{gitPath}";


            //if (injector == null) injector = new VideoInjector();
            //Injector = injector;

            //if (!Injector.Running) TListen = Injector.Start();

        }



#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        public Task<string> Upload(string FileSuffix, byte[] Data)
        {
            return null;
        }

#pragma warning restore CS1998

        public async Task<string> Update(string FileSuffix, byte[] Data)
        {
            return await Upload(FileSuffix, Data);
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

        public async Task<byte[]> BytesFromURL(string URL)
        {
            return await HttpClient.GetByteArrayAsync(URL);
        }

        public Task<int> GetLastFileIndex() => new Task<int>(() => -1);

        public Task DeleteAll() => null;

        public async Task<Stream> StreamFromURL(string URL)
        {
            return await HttpClient.GetStreamAsync(URL);
        }
    }

    public class GitFileClient : IFileClient<string, string>
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

        public async Task<byte[]> BytesFromURL(string URL) => await GitClient.BytesFromURL(URL);


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
