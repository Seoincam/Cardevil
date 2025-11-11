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
        public List<RoomData> RoomDataList = new List<RoomData>();
        public List<CustomClassTest> CustomClassTestList = new List<CustomClassTest>();
        public List<Heal> HealList = new List<Heal>();
        public List<HandRankingData> HandRankingDataList = new List<HandRankingData>();
        public List<MachineReward> MachineRewardList = new List<MachineReward>();
        public List<MachineProbabillity> MachineProbabillityList = new List<MachineProbabillity>();
        public List<RelicData> RelicDataList = new List<RelicData>();
        public List<RelicEffectOnEvaluationData> RelicEffectOnEvaluationDataList = new List<RelicEffectOnEvaluationData>();
        public readonly List<string> ClassNames = new List<string> {
            "RoomData",
            "CustomClassTest",
            "Heal",
            "HandRankingData",
            "MachineReward",
            "MachineProbabillity",
            "RelicData",
            "RelicEffectOnEvaluationData"
        };


        public T FindByName<T>(string name) where T : class
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
            RoomDataList.Clear();
            CustomClassTestList.Clear();
            HealList.Clear();
            HandRankingDataList.Clear();
            MachineRewardList.Clear();
            MachineProbabillityList.Clear();
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
                    case "RoomData":
                        RoomDataList = CreateInstance<RoomData>(df);
                        break;
                    case "CustomClassTest":
                        CustomClassTestList = CreateInstance<CustomClassTest>(df);
                        break;
                    case "Heal":
                        HealList = CreateInstance<Heal>(df);
                        break;
                    case "HandRankingData":
                        HandRankingDataList = CreateInstance<HandRankingData>(df);
                        break;
                    case "MachineReward":
                        MachineRewardList = CreateInstance<MachineReward>(df);
                        break;
                    case "MachineProbabillity":
                        MachineProbabillityList = CreateInstance<MachineProbabillity>(df);
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
                case "RoomData":
                    var newRoomDataItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RoomData>>(json);
                    RoomDataList.AddRange(newRoomDataItems);
                    break;
                case "CustomClassTest":
                    var newCustomClassTestItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CustomClassTest>>(json);
                    CustomClassTestList.AddRange(newCustomClassTestItems);
                    break;
                case "Heal":
                    var newHealItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Heal>>(json);
                    HealList.AddRange(newHealItems);
                    break;
                case "HandRankingData":
                    var newHandRankingDataItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HandRankingData>>(json);
                    HandRankingDataList.AddRange(newHandRankingDataItems);
                    break;
                case "MachineReward":
                    var newMachineRewardItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MachineReward>>(json);
                    MachineRewardList.AddRange(newMachineRewardItems);
                    break;
                case "MachineProbabillity":
                    var newMachineProbabillityItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MachineProbabillity>>(json);
                    MachineProbabillityList.AddRange(newMachineProbabillityItems);
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
                case "RoomData":
                    return typeof(RoomData);
                case "CustomClassTest":
                    return typeof(CustomClassTest);
                case "Heal":
                    return typeof(Heal);
                case "HandRankingData":
                    return typeof(HandRankingData);
                case "MachineReward":
                    return typeof(MachineReward);
                case "MachineProbabillity":
                    return typeof(MachineProbabillity);
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
