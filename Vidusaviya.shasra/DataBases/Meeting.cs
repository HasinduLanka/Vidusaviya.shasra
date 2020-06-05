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
        public MeetingPeerSettings PeerSettings;

    }

    public class MeetingPeerSettings
    {
        public int I; //ID
        public string U; //URL prefix
        public int Z; //Last segment index (URL suffix)

        public int C; //CDN type
        public string key; //BASE64 decryption key

    }

    public class GithubInfo
    {
        public string ps;
        public string Username;
        public string Repo;
        public string Path;
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
        FTP,
        Github,
        Firestore
    }


    public enum EncryptionType
    {
        None, //Fastest
        AES128EBC, //2nd Fastest
        AES128CBC,
        AES256CBC //Most Secure
    }
}
