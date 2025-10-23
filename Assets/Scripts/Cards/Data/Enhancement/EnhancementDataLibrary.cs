using Cardevil.Attributes;
using Cardevil.Cards.Data.Modifiers;
using Cardevil.DataStructure;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Data.Enhancement
{
    [Serializable]
    public class EnhancementDataLibrary
    {
        // <Id, Data>
        [SerializeField, VisibleOnly] private SerializableDict<Guid, EnhancementData> table = new();
        private Dictionary<EnhancementData, Guid> _reverseTable = new();
        
        public IReadOnlyDictionary<Guid, EnhancementData> Table => table;

        public void Init()
        {
            // TODO: DB 콜백에 등록
            MakeTestData();
        }
        
        private void MakeTestData()
        {
            EnhancementData testData;

            // AttackNumSelectable Lv1
            testData = new EnhancementData(
                ModifierType.AttackNumSelectable, 1, 2, 3, 2,
                0.7f, 0.3f,
                "테스트 데이터: 공격 숫자 선택 Lv1 (기본 강화)");
            AddData(testData.Id, testData);

            // AttackNumSelectable Lv2
            testData = new EnhancementData(
                ModifierType.AttackNumSelectable, 2, 3, 4, 2,
                0.52f, 0.48f,
                "테스트 데이터: 공격 숫자 선택 Lv2 (중간 강화)");
            AddData(testData.Id, testData);

            // AttackNumSelectable Lv3
            testData = new EnhancementData(
                ModifierType.AttackNumSelectable, 3, 9, 11, 4,
                0.4f, 0.55f,
                "테스트 데이터: 공격 숫자 선택 Lv3 (최대 강화)");
            AddData(testData.Id, testData);

            // AttackDamage Lv1
            testData = new EnhancementData(
                ModifierType.AttackDamage, 1, 1, 3, 2,
                0.7f, 0.3f,
                "테스트 데이터: 공격 데미지 Lv1 (+5% 증가)");
            AddData(testData.Id, testData);

            // AttackDamage Lv2
            testData = new EnhancementData(
                ModifierType.AttackDamage, 2, 2, 4, 2,
                0.52f, 0.48f,
                "테스트 데이터: 공격 데미지 Lv2 (+10% 증가)");
            AddData(testData.Id, testData);

            // AttackDamage Lv3
            testData = new EnhancementData(
                ModifierType.AttackDamage, 3, 4, 5, 2,
                0.45f, 0.5f,
                "테스트 데이터: 공격 데미지 Lv3 (+20% 증가)");
            AddData(testData.Id, testData);

            // MoveDirSelectable Lv1
            testData = new EnhancementData(
                ModifierType.MoveDirSelectable, 1, 2, 3, 2,
                0.7f, 0.3f,
                "테스트 데이터: 이동 방향 선택 Lv1 (2방향 가능)");
            AddData(testData.Id, testData);

            // MoveDirSelectable Lv2
            testData = new EnhancementData(
                ModifierType.MoveDirSelectable, 2, 4, 5, 2,
                0.45f, 0.5f,
                "테스트 데이터: 이동 방향 선택 Lv2 (4방향 가능)");
            AddData(testData.Id, testData);
        }

        private void AddData(Guid id, EnhancementData data)
        {
            table.Add(id, data);
            _reverseTable.Add(data, id);
        }

        public Guid GetId(ModifierType type, int level)
        {
            foreach ((EnhancementData data, Guid id) in _reverseTable)
            {
                if (data.Type == type && data.Level == level)
                    return id;
            }
            
            LogEx.LogError($"{type} 타입의 {level}레벨 데이터가 존재하지 않습니다.");
            return Guid.Empty;
        }

        public EnhancementData GetData(Guid id)
        {
            return table.GetValueOrDefault(id);
        }

        public Guid GetNextId(Guid guid)
        {
            var curData = GetData(guid);
            return GetNextId(curData);
        }

        public Guid GetNextId(EnhancementData curData)
        {
            var newLevel = curData.Level + 1;

            foreach ((EnhancementData data, Guid id) in _reverseTable)
            {
                if (data.Type == curData.Type && data.Level == newLevel)
                    return data.Id;
            }
            
            return Guid.Empty;
        }
    }
}