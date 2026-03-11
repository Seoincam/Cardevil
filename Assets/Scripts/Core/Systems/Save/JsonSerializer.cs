using UnityEngine;

namespace Cardevil.Core.Systems.Save
{
    public class JsonSerializer : ISerializer
    {
        public string Serialize<T>(T obj)
        {
            // if (obj is IDictionary)
            // {
            //     var dict = obj as IDictionary;
            //     var serializableDictType = typeof(SerializableDict<,>).MakeGenericType(dict.GetType().GetGenericArguments());
            //     var serializableDict = (IDictionary)System.Activator.CreateInstance(serializableDictType);
            //     foreach (var key in dict.Keys)
            //     {
            //         serializableDict.Add(key, dict[key]);
            //     }
            //     return JsonUtility.ToJson(serializableDict);
            // }
            return JsonUtility.ToJson(obj,prettyPrint:true);
        }

        public T Deserialize<T>(string raw)
        {
            return JsonUtility.FromJson<T>(raw);
        }
    }
}