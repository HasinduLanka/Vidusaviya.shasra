using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vidusaviya.shasra
{
    public static class Core
    {
        public static string ObjectToJSON(object O)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(O, Newtonsoft.Json.Formatting.None);
        }
        public static object JSONoObject(string O)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(O);
        }
    }
}
