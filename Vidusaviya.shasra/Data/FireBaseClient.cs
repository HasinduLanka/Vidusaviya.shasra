using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Vidusaviya.shasra
{
    public class FireBaseClient
    {
        private readonly FirestoreDb db;
        private readonly bool readOnly;
        public FireBaseClient(string ProjectId, string JsonCredentials, bool ReadOnly = false)
        {
            readOnly = ReadOnly;
            db = new FirestoreDbBuilder { ProjectId = ProjectId, JsonCredentials = JsonCredentials }.Build();
        }
        public async Task<WriteResult> WriteDocument(string Collection, string Document, Dictionary<string, object> Data)
        {
            if (readOnly) return null;
            return await db.Collection(Collection).Document(Document).SetAsync(Data);
        }
        public async Task<DocumentSnapshot> ReadDocument(string Collection, string Document)
        {
            return await db.Collection(Collection).Document(Document).GetSnapshotAsync();
        }
        public async Task<WriteResult> DeleteDocument(string Collection, string Document)
        {
            if (readOnly) return null;
            return await db.Collection(Collection).Document(Document).DeleteAsync();
        }
        public async Task<QuerySnapshot> ReadDocuments(string Collection)
        {
            return await db.Collection(Collection).GetSnapshotAsync();
        }
        public async Task<object> ReadData(string Collection, string Document, string Key)
        {
            var res = await ReadDocument(Collection, Document);
            var dic = res.ToDictionary();
            dic.TryGetValue(Key, out object v);
            return v;
        }
    }
}
