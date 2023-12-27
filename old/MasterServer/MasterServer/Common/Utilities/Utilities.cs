using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        Random rand = new Random();

        int random1, random2;
        T temp;

        for (int i = 0; i < array.Length; ++i)
        {
            random1 = rand.Next(0, array.Length);
            random2 = rand.Next(0, array.Length);

            temp = array[random1];
            array[random1] = array[random2];
            array[random2] = temp;
        }

        return array;
    }
}
