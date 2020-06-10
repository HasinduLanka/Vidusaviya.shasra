using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Vidusaviya.shasra
{
    public interface IFileClient<TU, TD>
    {
        bool IsReadOnly { get; }
        Task<string> Upload(string FileName, TU Data);
        Task<string> Update(string FileName, TU Data);
        Task<string> Delete(string FileName);
        Task DeleteAll();
        Task<TD> Download(string FileName);
        Task<TD> DownloadFromURL(string URL);
        Task<Stream> StreamFromURL(string URL);
        Task<byte[]> BytesFromURL(string URL);
        Task<int> GetLastFileIndex();
    }


}
