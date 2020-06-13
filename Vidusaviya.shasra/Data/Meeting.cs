using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Vidusaviya.shasra
{


    // Simple classes because JSON -> AES128 -> BASE64
    [Serializable]
    public class MeetingStreamerSettings
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CDNType CDNType { get; set; } //Content Delivery Network Type
        //Can select only one of those
        public List<GithubInfo> GithubInfos { get; set; }
        public List<FireBaseInfo> FireBaseInfos { get; set; }
        public List<FTPInfo> FTPInfos { get; set; }

        public int CamResolution { get; set; } = 360; //360, 480, 720, 1280
        public int CamFPS { get; set; } = 8; //8, 10, 16, 20, 24
        public int ScreenResolution { get; set; } = 360; //360, 480, 720, 1280
        public int ScreenFPS { get; set; } = 8; //8, 10, 16, 20, 24
        public float SegmentLength { get; set; } = 4; //1, 2, 3, 4, 5, 8, 10 (Live),    20, 60, 120 (Non live)

        public EncryptionType EncType { get; set; } = EncryptionType.None;
        public string Key { get; set; }

        public AesRij GetAes()
        {
            int ChunkSize;
            CipherMode mode;

            switch (EncType)
            {
                case EncryptionType.None:
                    return null;

                case EncryptionType.AES128EBC:
                    ChunkSize = 128;
                    mode = CipherMode.ECB;
                    break;
                case EncryptionType.AES128CBC:
                    ChunkSize = 128;
                    mode = CipherMode.CBC;
                    break;
                case EncryptionType.AES256CBC:
                    ChunkSize = 256;
                    mode = CipherMode.CBC;
                    break;
                default:
                    ChunkSize = 128;
                    mode = CipherMode.ECB;
                    break;
            }

            return new AesRij(Key, ChunkSize, mode);
        }
        //Auto Gen
        public MeetingPeerSettings PeerSettings()
        {
            MeetingPeerSettings meetingPeerSettings = new MeetingPeerSettings()
            {
                ID = ID,
                CDNType = CDNType,
                EncType = EncType,
                Key = Key
            };

            switch (CDNType)
            {
                case CDNType.FTP:
                    break;

                case CDNType.Github:

                    meetingPeerSettings.URLPrefixes = new List<string>(GithubInfos.Count);

                    foreach (var item in GithubInfos)
                    {
                        meetingPeerSettings.URLPrefixes.Add(item.PeerURLPrefix);
                    }

                    break;

                case CDNType.Firestore:
                    break;

                default:
                    break;
            }
            return meetingPeerSettings;
        }
    }
    [Serializable]
    public class MeetingPeerSettings
    {
        public int ID { get; set; }//ID
        public CDNType CDNType { get; set; }
        public EncryptionType EncType { get; set; } = EncryptionType.None;
        public string Key { get; set; } //BASE64 decryption key
        public List<string> URLPrefixes { get; set; }
        public bool AllowViewerOpenCamera { get; set; }
        public bool AllowViewerToShareScreen { get; set; }
        public bool AllowViewerToUnmuteAudio { get; set; }
    }
    [Serializable]
    public class GithubInfo
    {
        public string ps { get; set; }
        public string UName { get; set; }
        public string Repo { get; set; }
        public string Branch { get; set; }
        public string Path { get; set; }
        public string PeerURLPrefix => $"https://raw.githubusercontent.com/{UName}/{Repo}/{Branch}/{Path}";
    }
    [Serializable]
    public class FireBaseInfo
    {
        public string JsonCredentials { get; set; }
        public string ProjectId { get; set; }
        public string Collection { get; set; }
    }
    [Serializable]
    public class FTPInfo
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Path { get; set; }
    }
    [Serializable]
    public enum CDNType
    {
        FTP, //Host yourself
        Github, //Free Forever, 1-6 s delay
        Firestore //Fastets, Comming soon
    }
    [Serializable]
    public enum EncryptionType
    {
        None, //Fastest
        AES128EBC, //2nd Fastest
        AES128CBC,
        AES256CBC //Most Secure
    }
}
