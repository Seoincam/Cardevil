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
        public readonly List<string> ClassNames = new List<string> {
            "Example",
            "mob"
        };


        public void ClearAll()
        {
            ExampleList.Clear();
            mobList.Clear();
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
                    default:
                        Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 이름: {df.name}");
                        break;
                }
            }
        }


        public void AddInstancesFromJsonList(string className, string jsonList)
        {
            switch (className)
            {
                case "Example":
                    var newExampleItems = JsonUtilExtend.FromJsonList<Example>(jsonList);
                    ExampleList.AddRange(newExampleItems);
                    break;
                case "mob":
                    var newmobItems = JsonUtilExtend.FromJsonList<mob>(jsonList);
                    mobList.AddRange(newmobItems);
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
                default:
                    Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 이름: {className}");
                    return null;
            }
        }
    }
}
