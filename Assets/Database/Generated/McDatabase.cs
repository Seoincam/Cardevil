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
        public List<RelicData> RelicDataList = new List<RelicData>();
        public List<RelicEffectOnEvaluationData> RelicEffectOnEvaluationDataList = new List<RelicEffectOnEvaluationData>();
        public readonly List<string> ClassNames = new List<string> {
            "Example",
            "mob",
            "shop",
            "RelicData",
            "RelicEffectOnEvaluationData"
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
            RelicDataList.Clear();
            RelicEffectOnEvaluationDataList.Clear();
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
                    case "RelicData":
                        RelicDataList = CreateInstance<RelicData>(df);
                        break;
                    case "RelicEffectOnEvaluationData":
                        RelicEffectOnEvaluationDataList = CreateInstance<RelicEffectOnEvaluationData>(df);
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
                case "shop":
                    var newshopItems = JsonUtilExtend.FromJsonList<shop>(jsonList);
                    shopList.AddRange(newshopItems);
                    break;
                case "RelicData":
                    var newRelicDataItems = JsonUtilExtend.FromJsonList<RelicData>(jsonList);
                    RelicDataList.AddRange(newRelicDataItems);
                    break;
                case "RelicEffectOnEvaluationData":
                    var newRelicEffectOnEvaluationDataItems = JsonUtilExtend.FromJsonList<RelicEffectOnEvaluationData>(jsonList);
                    RelicEffectOnEvaluationDataList.AddRange(newRelicEffectOnEvaluationDataItems);
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
                case "RelicData":
                    return typeof(RelicData);
                case "RelicEffectOnEvaluationData":
                    return typeof(RelicEffectOnEvaluationData);
                default:
                    Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 이름: {className}");
                    return null;
            }
        }
    }
}
