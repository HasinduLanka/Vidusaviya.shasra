using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Vidusaviya.shasra
{
    public interface IFileClient<T>
    {
        public bool IsReadOnly { get; }
        public Task<T> Upload(string FileName, T Data);
        public Task<T> Update(string FileName, T Data);
        public Task<T> Delete(string FileName);
        public Task DeleteAll();
        public Task<T> Download(string FileName);
        public Task<T> DownloadFromURL(string URL);
        public Task<int> GetLastFileIndex();
    }


}
