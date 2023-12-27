using System.Collections.Generic;
using Newtonsoft.Json;

namespace Msk
{
    public class MskProperties
    {
        [JsonProperty]
        private Dictionary<string, string> m_properties = new Dictionary<string, string>();

        [JsonIgnore]
        public Dictionary<string, string> Properties { get { return this.m_properties; } }

        public MskProperties()
        {
            this.m_properties = new Dictionary<string, string>();
        }

        public MskProperties(Dictionary<string, string> properties)
        {
            this.m_properties = properties;
        }

        #region Has

        public bool ContainsKey(string key)
        {
            return m_properties.ContainsKey(key);
        }

        public int Count()
        {
            return m_properties.Count;
        }

        #endregion

        #region Add
        public void Add(string key, object value)
        {
            if (m_properties.ContainsKey(key))
            {
                m_properties[key] = value.ToString();
            }
            else
            {
                m_properties.Add(key, value.ToString());
            }
        }

        public void Append(MskProperties mskProperties)
        {
            Dictionary<string, string> appendProperties = mskProperties.Properties;
            foreach (string key in appendProperties.Keys)
            {
                if (this.m_properties.ContainsKey(key))
                {
                    this.m_properties[key] = appendProperties[key];
                }
                else
                {
                    this.m_properties.Add(key, appendProperties[key]);
                }
            }
        }

        public void Append(Dictionary<string, string> appendProperties)
        {
            foreach (string key in appendProperties.Keys)
            {
                if (this.m_properties.ContainsKey(key))
                {
                    this.m_properties[key] = appendProperties[key];
                }
                else
                {
                    this.m_properties.Add(key, appendProperties[key]);
                }
            }
        }

        #endregion

        #region Clear
        public void Clear()
        {
            this.m_properties.Clear();
        }

        #endregion

        #region Get
        public bool GetBool(string key)
        {
            if (m_properties.ContainsKey(key))
            {
                return bool.Parse(m_properties[key]);
            }

            return false;
        }

        public byte GetByte(string key)
        {
            if (m_properties.ContainsKey(key))
            {
                return byte.Parse(m_properties[key]);
            }

            return 0;
        }

        public int GetInt(string key)
        {
            if (m_properties.ContainsKey(key))
            {
                return int.Parse(m_properties[key]);
            }

            return 0;
        }

        public long GetLong(string key)
        {
            if (m_properties.ContainsKey(key))
            {
                return long.Parse(m_properties[key]);
            }

            return 0;
        }

        public short GetShort(string key)
        {
            if (m_properties.ContainsKey(key))
            {
                return short.Parse(m_properties[key]);
            }

            return 0;
        }

        public float GetFloat(string key)
        {
            if (m_properties.ContainsKey(key))
            {
                return float.Parse(m_properties[key]);
            }

            return 0f;
        }

        public string GetString(string key)
        {
            if (m_properties.ContainsKey(key))
            {
                return m_properties[key];
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
