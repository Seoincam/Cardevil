using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using Database.Generated;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Editor
{
    public class RelicSyncService
    {
        private readonly RelicDatabase _database;
        
        private const string RelicSavePath = "Assets/Resources/ScriptableObjects/Relics";
        private const string JsonPath = "Assets/StreamingAssets/Json/RelicDisplayData.json";
        
        public RelicSyncService(RelicDatabase database)
        {
            _database = database;
        }
        
        /// <returns>
        /// 변화가 있었는지 여부.
        /// </returns>
        public Summary ExecuteSync()
        {
            Summary summary = new();
            
            var jsonData = LoadJsonFileData();
            if (string.IsNullOrEmpty(jsonData)) return null;
            
            List<RelicDisplayData> sheetData = JsonConvert.DeserializeObject<List<RelicDisplayData>>(jsonData);
            if (sheetData == null || sheetData.Count == 0)
            {
                LogEx.LogError("JSON 파싱 실패 또는 데이터가 없음.");
                return null;
            }
            
            var currentDb = _database.relics
                .Where(r => r && !string.IsNullOrEmpty(r.Data.Id))
                .ToDictionary(r => r.Data.Id, r => r);

            var sheetIds = new HashSet<string>(sheetData.Select(d => d.Id));
            
            // 시트 데이터 갱신 및 생성
            foreach (RelicDisplayData sheetItem in sheetData)
            {
                if (currentDb.TryGetValue(sheetItem.Id, out RelicSO existingRelic))
                {
                    if (existingRelic.FromLocal) continue;

                    SerializedObject serializedRelic = new(existingRelic);

                    // Missing 상태였다가 시트에 다시 나타난 유물 복구
                    if (existingRelic.FromMissing)
                    {
                        serializedRelic.FindProperty("dataSource").enumValueIndex = (int)RelicSO.DataSource.Sheet;
                        summary.RestoredIds.Add(sheetItem.Id);
                    }

                    serializedRelic.FindProperty("data.rarity").enumValueIndex = (int)sheetItem.Rarity;
                    serializedRelic.FindProperty("data.displayName").stringValue = sheetItem.DisplayName;
                    serializedRelic.FindProperty("data.displayDescription").stringValue = sheetItem.DisplayDescription;
                    serializedRelic.FindProperty("data.commentForEditor").stringValue = sheetItem.CommentForEditor;

                    if (serializedRelic.ApplyModifiedProperties())
                    {
                        summary.UpdatedIds.Add(sheetItem.Id);
                    }
                }
                else
                {
                    // 존재하지 않는 유물은 새로 생성
                    CreateRelicFromSheet(sheetItem);
                    summary.CreatedIds.Add(sheetItem.Id);
                }
            }
            
            // Missing 감지 
            foreach (var relic in _database.relics)
            {
                if (!relic || string.IsNullOrEmpty(relic.Data.Id)) continue;
                
                if (relic.FromLocal) continue;

                if (!sheetIds.Contains(relic.Data.Id) && relic.FromSheet)
                {
                    SerializedObject serializedRelic = new(relic);
                    serializedRelic.FindProperty("dataSource").enumValueIndex = (int)RelicSO.DataSource.Missing;

                    if (serializedRelic.ApplyModifiedProperties())
                    {
                        summary.MissingIds.Add(relic.Data.Id);
                    }
                }
            }

            if (summary.HasAnyChange)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            LogEx.Log("구글 시트 데이터 동기화 완료!");
            return summary;
        }

        public class Summary
        {
            public List<string> CreatedIds { get; } = new();
            public List<string> UpdatedIds { get; } = new();
            public List<string> MissingIds { get; } = new();
            public List<string> RestoredIds { get; } = new(); // Missing에서 Sheet로
            
            public int CreatedCount => CreatedIds.Count;
            public int UpdatedCount => UpdatedIds.Count;
            public int MissingCount => MissingIds.Count;
            public int RestoredCount => RestoredIds.Count;
            
            public bool HasAnyChange => CreatedCount > 0 || UpdatedCount > 0 || MissingCount > 0 || RestoredCount > 0;
        }

        private void CreateRelicFromSheet(RelicDisplayData data)
        {
            RelicSO newRelic = ScriptableObject.CreateInstance<RelicSO>();
            newRelic.InitializeFromSheet(data.Id, data.Rarity, data.DisplayName, data.DisplayDescription, data.CommentForEditor);

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{RelicSavePath}/Relic_{data.Id}.asset");
            AssetDatabase.CreateAsset(newRelic, assetPath);

            _database.relics.Add(newRelic);
            EditorUtility.SetDirty(_database);
        }

        private string LoadJsonFileData()
        {
            if (!File.Exists(JsonPath))
            {
                LogEx.LogError($"JSON 파일이 존재하지 않습니다. 경로: {JsonPath}");
                return null;
            }

            try
            {
                string jsonText = File.ReadAllText(JsonPath);
                return jsonText;
            }
            catch (System.Exception e)
            {
                LogEx.LogError($"파일 읽기 중 오류 발생: {e.Message}");
                return null;
            }
        }
    }
}