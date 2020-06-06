using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Vidusaviya.shasra
{
    public interface IFileClient<T>
    {
        bool IsReadOnly { get; }
        Task<T> Upload(string FileName, T Data);
        Task<T> Update(string FileName, T Data);
        Task<T> Delete(string FileName);
        Task DeleteAll();
        Task<T> Download(string FileName);
        Task<T> DownloadFromURL(string URL);
        Task<Stream> StreamFromURL(string URL);
        Task<int> GetLastFileIndex();
    }


}
