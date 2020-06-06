using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Vidusaviya.shasra.DataBases
{

    public class EncryptedLink
    {

        public static AesRij Aes = new AesRij("Created by Harindu Wijesinha and Hasindu Lanka"); //If changed, every link will not work

        public byte[] Key;
        public string URL;

        public static EncryptedLink FromParams(string ekey, string eurl)
        {
            try
            {
                return new EncryptedLink()
                {
                    Key = Aes.DecryptFromBase64String(ekey),
                    URL = Aes.DecryptBase64String(eurl)
                };

            }
            catch (Exception)
            {
                return null;
            }

        }

        public string ToURLParams()
        {
            return Aes.EncryptToBase64String(Key) + '/' + Aes.EncryptBase64String(URL);
        }

        public string[] ToParams()
        {
            return new string[] { Aes.EncryptToBase64String(Key), Aes.EncryptBase64String(URL) };
        }

        public async Task<byte[]> GetFile()
        {
            HttpClient client = new HttpClient();

            var raw = await client.GetAsync(URL);
            var ciph = await raw.Content.ReadAsByteArrayAsync();

            return Aes.Decrypt(ciph);

        }

        public async Task<string> GetDocument()
        {
            return Convert.ToBase64String(await GetFile());
        }


    }
}
