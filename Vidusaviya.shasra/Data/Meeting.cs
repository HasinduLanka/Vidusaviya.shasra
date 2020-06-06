using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Vidusaviya.shasra
{


    // Simple classes because JSON -> AES128 -> BASE64
    public class MeetingStreamerSettings
    {
        public int ID;
        public string Name;

        public CDNType CDNType; //Content Delivery Network Type
        //Can select only one of those
        public List<GithubInfo> GithubInfos;
        public List<FireBaseInfo> FireBaseInfos;
        public List<FTPInfo> FTPInfos;

        public int Resolution = 360; //360, 480, 720, 1280
        public int FPS = 8; //8, 10, 16, 20, 24
        public int SegmentLength = 4; //1, 2, 3, 4, 5, 8, 10 (Live),    20, 60, 120 (Non live)

        public EncryptionType EncType = EncryptionType.None;
        public string Key;

        //Auto Gen
        public List<MeetingPeerSettings> PeerSettings()
        {
            var lst = new List<MeetingPeerSettings>();
            switch (CDNType)
            {
                case CDNType.FTP:
                    break;
                case CDNType.Github:
                    foreach (var item in GithubInfos)
                    {
                        lst.Add(new MeetingPeerSettings()
                        {
                            ID = ID,
                            CDNType = CDNType,
                            EncType = EncType,
                            Key = Key,
                            URLPrefix = item.PeerURLPrefix
                        });
                    }
                    break;
                case CDNType.Firestore:
                    break;
                default:
                    break;
            }

            return lst;

        }

    }

    public class MeetingPeerSettings
    {
        public int ID; //ID

        public CDNType CDNType;
        public string URLPrefix;

        public EncryptionType EncType = EncryptionType.None;
        public string Key; //BASE64 decryption key

    }

    public class GithubInfo
    {
        public string ps;
        public string UName;
        public string Repo;
        public string Branch;
        public string Path;

        public string PeerURLPrefix => $"https://raw.githubusercontent.com/{UName}/{Repo}/{Branch}/{Path}";
    }

    public class FireBaseInfo
    {
        public string JsonCredentials;
        public string ProjectId;
        public string Collection;
    }

    public class FTPInfo
    {
        public string Server;
        public string Username;
        public string Password;
        public string Path;
    }


    public enum CDNType
    {
        FTP, //Host yourself
        Github, //Free Forever, 1-6 s delay
        Firestore //Fastets, Comming soon
    }


    public enum EncryptionType
    {
        None, //Fastest
        AES128EBC, //2nd Fastest
        AES128CBC,
        AES256CBC //Most Secure
    }
}
