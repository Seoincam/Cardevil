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
        public List<machineReward> machineRewardList = new List<machineReward>();
        public List<HandRankingData> HandRankingDataList = new List<HandRankingData>();
        public List<RelicData> RelicDataList = new List<RelicData>();
        public List<RelicEffectOnEvaluationData> RelicEffectOnEvaluationDataList = new List<RelicEffectOnEvaluationData>();
        public readonly List<string> ClassNames = new List<string> {
            "Example",
            "mob",
            "machineReward",
            "HandRankingData",
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
            machineRewardList.Clear();
            HandRankingDataList.Clear();
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
                    case "machineReward":
                        machineRewardList = CreateInstance<machineReward>(df);
                        break;
                    case "HandRankingData":
                        HandRankingDataList = CreateInstance<HandRankingData>(df);
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
                case "machineReward":
                    var newmachineRewardItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<machineReward>>(json);
                    machineRewardList.AddRange(newmachineRewardItems);
                    break;
                case "HandRankingData":
                    var newHandRankingDataItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HandRankingData>>(json);
                    HandRankingDataList.AddRange(newHandRankingDataItems);
                    break;
                case "RelicData":
                    var newRelicDataItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RelicData>>(json);
                    RelicDataList.AddRange(newRelicDataItems);
                    break;
                case "RelicEffectOnEvaluationData":
                    var newRelicEffectOnEvaluationDataItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RelicEffectOnEvaluationData>>(json);
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
                case "machineReward":
                    return typeof(machineReward);
                case "HandRankingData":
                    return typeof(HandRankingData);
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
