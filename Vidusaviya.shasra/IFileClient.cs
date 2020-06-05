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
        public Task<T> Upload(string FileSuffix, T Data);
        public Task<T> Update(string FileSuffix, T Data);
        public Task<T> Delete(string FileSuffix);
        public Task<T> Download(string FileSuffix);
        public Task<string> DownloadFromURL(string URL);
        public int GetLastFileIndex();
    }


}
