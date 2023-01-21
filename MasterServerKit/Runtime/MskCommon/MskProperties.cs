using System.Collections.Generic;
using Newtonsoft.Json;

namespace MasterServerKit
{
    public class MskProperties
    {
        [JsonProperty]
        private Dictionary<string, string> properties = new Dictionary<string, string>();

        [JsonIgnore]
        public Dictionary<string, string> Properties { get { return this.properties; } }


        #region Has

        public bool ContainsKey(string key)
        {
            return properties.ContainsKey(key);
        }

        #endregion

        #region Add
        public void Add(string key, object value)
        {
            if (properties.ContainsKey(key))
            {
                properties[key] = value.ToString();
            }
            else
            {
                properties.Add(key, value.ToString());
            }
        }

        public void Append(MskProperties mskProperties)
        {
            Dictionary<string, string> appendProperties = mskProperties.Properties;
            foreach (string key in appendProperties.Keys)
            {
                if (this.properties.ContainsKey(key))
                {
                    this.properties[key] = appendProperties[key];
                }
                else
                {
                    this.properties.Add(key, appendProperties[key]);
                }
            }
        }

        public void Append(Dictionary<string, string> appendProperties)
        {
            foreach (string key in appendProperties.Keys)
            {
                if (this.properties.ContainsKey(key))
                {
                    this.properties[key] = appendProperties[key];
                }
                else
                {
                    this.properties.Add(key, appendProperties[key]);
                }
            }
        }

        #endregion

        #region Clear
        public void Clear()
        {
            this.properties.Clear();
        }

        #endregion

        #region Get
        public bool GetBool(string key)
        {
            if (properties.ContainsKey(key))
            {
                return bool.Parse(properties[key]);
            }

            return false;
        }

        public byte GetByte(string key)
        {
            if (properties.ContainsKey(key))
            {
                return byte.Parse(properties[key]);
            }

            return 0;
        }

        public int GetInt(string key)
        {
            if (properties.ContainsKey(key))
            {
                return int.Parse(properties[key]);
            }

            return 0;
        }

        public long GetLong(string key)
        {
            if (properties.ContainsKey(key))
            {
                return long.Parse(properties[key]);
            }

            return 0;
        }

        public short GetShort(string key)
        {
            if (properties.ContainsKey(key))
            {
                return short.Parse(properties[key]);
            }

            return 0;
        }

        public float GetFloat(string key)
        {
            if (properties.ContainsKey(key))
            {
                return float.Parse(properties[key]);
            }

            return 0f;
        }

        public string GetString(string key)
        {
            if (properties.ContainsKey(key))
            {
                return properties[key];
            }

            return "";
        }

        #endregion

        #region Serialize
        public string SerializeJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static MskProperties Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<MskProperties>(json);
        }


        #endregion
    }
}
