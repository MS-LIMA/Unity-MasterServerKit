using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Msk
{
    public class Utilities
    {
        public static string ToJson(object t)
        {
            return JsonConvert.SerializeObject(t);
        }

        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T[] Shuffle<T>(T[] array)
        {
            int random1, random2;
            T temp;

            for (int i = 0; i < array.Length; ++i)
            {
                random1 = Random.Range(0, array.Length);
                random2 = Random.Range(0, array.Length);

                temp = array[random1];
                array[random1] = array[random2];
                array[random2] = temp;
            }

            return array;
        }
    }
}
