using System;
using System.Collections.Generic;
using UnityEngine;
using Database.Generated;

namespace Database
{
    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class McDatabase
    {
        public List<Example> ExampleList = new List<Example>();
        public List<mob> mobList = new List<mob>();
        public List<shop> shopList = new List<shop>();
        public readonly List<string> ClassNames = new List<string> {
            "Example",
            "mob",
            "shop"
        };


        public T FindByName<T>(string name) where T : class
        {
            if (typeof(T) == null) return null;
            switch (typeof(T).Name)
            {
                case "Example":
                    foreach (var instance in ExampleList)
                    {
                        if (instance.name == name)
                            return instance as T;
                    }
                    break;
                default:
                    Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 타입: {typeof(T).Name}");
                    return null;
            }
            return null;
        }

        public T FindByIdentifier<T>(string identifier) where T : class
        {
            if (typeof(T) == null) return null;
            switch (typeof(T).Name)
            {
                default:
                    Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 타입: {typeof(T).Name}");
                    return null;
            }
            return null;
        }

        public void ClearAll()
        {
            ExampleList.Clear();
            mobList.Clear();
            shopList.Clear();
        }



        private List<T> CreateInstance<T>(DataFrame df) where T : new()
        {
            object[] instances = ClassInstanceFactory.CreateInstance(df);
            List<T> list = new List<T>();
            foreach (var instance in instances)
            {
                if (instance is T typedInstance)
                {
                    list.Add(typedInstance);
                }
            }
            return list;
        }

        public void InitializeAll(List<DataFrame> dataFrames)
        {
            foreach (var df in dataFrames)
            {
                switch (df.name)
                {
                    case "Example":
                        ExampleList = CreateInstance<Example>(df);
                        break;
                    case "mob":
                        mobList = CreateInstance<mob>(df);
                        break;
                    case "shop":
                        shopList = CreateInstance<shop>(df);
                        break;
                    default:
                        Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 이름: {df.name}");
                        break;
                }
            }
        }


        public void AddInstancesFromJsonList(string className, string json)
        {
            switch (className)
            {
                case "Example":
                    var newExampleItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Example>>(json);
                    ExampleList.AddRange(newExampleItems);
                    break;
                case "mob":
                    var newmobItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<mob>>(json);
                    mobList.AddRange(newmobItems);
                    break;
                case "shop":
                    var newshopItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<shop>>(json);
                    shopList.AddRange(newshopItems);
                    break;
                default:
                    Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 이름: {className}");
                    break;
            }
        }


        public Type GetTypeByName(string className)
        {
            switch (className)
            {
                case "Example":
                    return typeof(Example);
                case "mob":
                    return typeof(mob);
                case "shop":
                    return typeof(shop);
                default:
                    Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 이름: {className}");
                    return null;
            }
        }
    }
}
