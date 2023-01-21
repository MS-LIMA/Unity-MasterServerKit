using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MasterServerKit
{
    public class JsonSerializer
    {
        public static string ToJson(object item)
        {
            return JsonConvert.SerializeObject(item);
        }

        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);

            //JObject jObject = JObject.FromObject(data);
            //return jObject.ToObject<T>();
        }
    }
}
