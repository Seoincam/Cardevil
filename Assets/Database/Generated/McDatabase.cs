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
        public List<CustomClassTest> CustomClassTestList = new List<CustomClassTest>();
        public List<RoomData> RoomDataList = new List<RoomData>();
        public List<BaseMobBossData> BaseMobBossDataList = new List<BaseMobBossData>();
        public List<Heal> HealList = new List<Heal>();
        public List<MachineReward> MachineRewardList = new List<MachineReward>();
        public List<MachineProbabillity> MachineProbabillityList = new List<MachineProbabillity>();
        public List<RelicDisplayData> RelicDisplayDataList = new List<RelicDisplayData>();
        public readonly List<string> ClassNames = new List<string> {
            "CustomClassTest",
            "RoomData",
            "BaseMobBossData",
            "Heal",
            "MachineReward",
            "MachineProbabillity",
            "RelicDisplayData"
        };


        public T FindByName<T>(string name) where T : class
        {
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
            CustomClassTestList.Clear();
            RoomDataList.Clear();
            BaseMobBossDataList.Clear();
            HealList.Clear();
            MachineRewardList.Clear();
            MachineProbabillityList.Clear();
            RelicDisplayDataList.Clear();
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
                    case "CustomClassTest":
                        CustomClassTestList = CreateInstance<CustomClassTest>(df);
                        break;
                    case "RoomData":
                        RoomDataList = CreateInstance<RoomData>(df);
                        break;
                    case "BaseMobBossData":
                        BaseMobBossDataList = CreateInstance<BaseMobBossData>(df);
                        break;
                    case "Heal":
                        HealList = CreateInstance<Heal>(df);
                        break;
                    case "MachineReward":
                        MachineRewardList = CreateInstance<MachineReward>(df);
                        break;
                    case "MachineProbabillity":
                        MachineProbabillityList = CreateInstance<MachineProbabillity>(df);
                        break;
                    case "RelicDisplayData":
                        RelicDisplayDataList = CreateInstance<RelicDisplayData>(df);
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
                case "CustomClassTest":
                    var newCustomClassTestItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CustomClassTest>>(json);
                    CustomClassTestList.AddRange(newCustomClassTestItems);
                    break;
                case "RoomData":
                    var newRoomDataItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RoomData>>(json);
                    RoomDataList.AddRange(newRoomDataItems);
                    break;
                case "BaseMobBossData":
                    var newBaseMobBossDataItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BaseMobBossData>>(json);
                    BaseMobBossDataList.AddRange(newBaseMobBossDataItems);
                    break;
                case "Heal":
                    var newHealItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Heal>>(json);
                    HealList.AddRange(newHealItems);
                    break;
                case "MachineReward":
                    var newMachineRewardItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MachineReward>>(json);
                    MachineRewardList.AddRange(newMachineRewardItems);
                    break;
                case "MachineProbabillity":
                    var newMachineProbabillityItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MachineProbabillity>>(json);
                    MachineProbabillityList.AddRange(newMachineProbabillityItems);
                    break;
                case "RelicDisplayData":
                    var newRelicDisplayDataItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RelicDisplayData>>(json);
                    RelicDisplayDataList.AddRange(newRelicDisplayDataItems);
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
                case "CustomClassTest":
                    return typeof(CustomClassTest);
                case "RoomData":
                    return typeof(RoomData);
                case "BaseMobBossData":
                    return typeof(BaseMobBossData);
                case "Heal":
                    return typeof(Heal);
                case "MachineReward":
                    return typeof(MachineReward);
                case "MachineProbabillity":
                    return typeof(MachineProbabillity);
                case "RelicDisplayData":
                    return typeof(RelicDisplayData);
                default:
                    Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 이름: {className}");
                    return null;
            }
        }
    }
}
