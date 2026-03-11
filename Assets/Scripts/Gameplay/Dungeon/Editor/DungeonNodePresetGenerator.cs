using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Cardevil.Dungeon;
using Cardevil.Gameplay.Dungeon.Node;
using Cardevil.Gameplay.Dungeon.NodePresets;

namespace Cardevil.Dungeon.Editor
{
    /// <summary>
    /// 던전 노드 Preset을 자동으로 생성하는 에디터 도구
    /// </summary>
    public class DungeonNodePresetGenerator : EditorWindow
    {
        private const string PRESET_OUTPUT_PATH = "Assets/ScriptableObjects/Dungeon/NodePresets";
        private const string SPRITE_BASE_PATH = "Assets/Sprites/Dungeon/Icon";
        
        private Dictionary<DungeonNodeTypes, PresetConfig> _presetConfigs = new Dictionary<DungeonNodeTypes, PresetConfig>();
        
        private class PresetConfig
        {
            public string DisplayName;
            public string ActiveSpriteName;
            public string InactiveSpriteName;
            public string CompletedSpriteName;
            public string CompletedOverlaySpriteName;
            public Color NodeColor = Color.white;
            public Color TextColor = Color.white;
        }

        [MenuItem("Cardevil/Generate Dungeon Node Presets")]
        public static void ShowWindow()
        {
            var window = GetWindow<DungeonNodePresetGenerator>("노드 Preset 생성기");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            InitializePresetConfigs();
        }

        private void InitializePresetConfigs()
        {
            _presetConfigs.Clear();
            
            // Dev
            _presetConfigs[DungeonNodeTypes.None] = new PresetConfig
            {
                DisplayName = "개발용 노드",
                ActiveSpriteName = "Dev_active",
                InactiveSpriteName = "Dev_Inactive",
                CompletedSpriteName = "Dev_active",
                CompletedOverlaySpriteName = "After_Kill_S",
                NodeColor = Color.magenta,
                TextColor = Color.white
            };
            
            // Mob
            _presetConfigs[DungeonNodeTypes.Mob] = new PresetConfig
            {
                DisplayName = "몬스터",
                ActiveSpriteName = "Mob_active",
                InactiveSpriteName = "Mob_Inactive",
                CompletedSpriteName = "Mob_active",
                CompletedOverlaySpriteName = "After_Kill_S",
                NodeColor = Color.white,
                TextColor = Color.white
            };
            
            // Heal
            _presetConfigs[DungeonNodeTypes.Heal] = new PresetConfig
            {
                DisplayName = "회복",
                ActiveSpriteName = "Heal_active",
                InactiveSpriteName = "Heal_Inactive",
                CompletedSpriteName = "Heal_active",
                CompletedOverlaySpriteName = "After_Kill_S",
                NodeColor = Color.white,
                TextColor = Color.white
            };
            
            // Random
            _presetConfigs[DungeonNodeTypes.Random] = new PresetConfig
            {
                DisplayName = "랜덤",
                ActiveSpriteName = "Random_active",
                InactiveSpriteName = "Random_Inactive",
                CompletedSpriteName = "Random_active",
                CompletedOverlaySpriteName = "After_Kill_S",
                NodeColor = Color.white,
                TextColor = Color.white
            };
            
            // MiniBoss
            _presetConfigs[DungeonNodeTypes.MiniBoss] = new PresetConfig
            {
                DisplayName = "중간 보스",
                ActiveSpriteName = "Middle_Boss_active",
                InactiveSpriteName = "Middle_Boss_Inactive",
                CompletedSpriteName = "Middle_Boss_active",
                CompletedOverlaySpriteName = "After_Kill_M",
                NodeColor = Color.white,
                TextColor = Color.white
            };
            
            // FinalBoss
            _presetConfigs[DungeonNodeTypes.FinalBoss] = new PresetConfig
            {
                DisplayName = "최종 보스",
                ActiveSpriteName = "Main_Boss_active",
                InactiveSpriteName = "Main_Boss_Inactive",
                CompletedSpriteName = "Main_Boss_active",
                CompletedOverlaySpriteName = "After_Kill_L",
                NodeColor = Color.white,
                TextColor = Color.white
            };
            
            // BlackMarket
            _presetConfigs[DungeonNodeTypes.BlackMarket] = new PresetConfig
            {
                DisplayName = "암시장",
                ActiveSpriteName = "Black_Market_active",
                InactiveSpriteName = "Black_Market_Inactive",
                CompletedSpriteName = "Black_Market_active",
                CompletedOverlaySpriteName = "After_Kill_S",
                NodeColor = Color.white,
                TextColor = Color.white
            };
            
            // Shop
            _presetConfigs[DungeonNodeTypes.Shop] = new PresetConfig
            {
                DisplayName = "상점",
                ActiveSpriteName = "Shop_active",
                InactiveSpriteName = "Shop_Inactive",
                CompletedSpriteName = "Shop_active",
                CompletedOverlaySpriteName = "After_Kill_S",
                NodeColor = Color.white,
                TextColor = Color.white
            };
        }

        private void OnGUI()
        {
            GUILayout.Label("던전 노드 Preset 자동 생성", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "모든 던전 노드 타입에 대한 Preset ScriptableObject를 자동으로 생성합니다.\n" +
                $"생성 경로: {PRESET_OUTPUT_PATH}\n" +
                $"스프라이트 경로: {SPRITE_BASE_PATH}",
                MessageType.Info
            );
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("모든 Preset 생성", GUILayout.Height(40)))
            {
                GenerateAllPresets();
            }
            
            GUILayout.Space(10);
            
            GUILayout.Label("개별 생성:", EditorStyles.boldLabel);
            
            foreach (var kvp in _presetConfigs)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{kvp.Value.DisplayName} ({kvp.Key})", GUILayout.Width(150));
                if (GUILayout.Button("생성"))
                {
                    GeneratePreset(kvp.Key, kvp.Value);
                }
                GUILayout.EndHorizontal();
            }
        }

        private void GenerateAllPresets()
        {
            if (!EditorUtility.DisplayDialog(
                "모든 Preset 생성",
                $"모든 던전 노드 Preset을 생성하시겠습니까?\n경로: {PRESET_OUTPUT_PATH}",
                "생성",
                "취소"))
            {
                return;
            }
            
            int successCount = 0;
            int failCount = 0;
            
            foreach (var kvp in _presetConfigs)
            {
                if (GeneratePreset(kvp.Key, kvp.Value))
                    successCount++;
                else
                    failCount++;
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog(
                "생성 완료",
                $"성공: {successCount}개\n실패: {failCount}개",
                "확인"
            );
        }

        private bool GeneratePreset(DungeonNodeTypes nodeType, PresetConfig config)
        {
            try
            {
                // 출력 디렉토리 생성
                if (!Directory.Exists(PRESET_OUTPUT_PATH))
                {
                    Directory.CreateDirectory(PRESET_OUTPUT_PATH);
                }
                
                // Preset 생성
                DungeonNodePreset preset = CreatePresetInstance(nodeType);
                if (preset == null)
                {
                    Debug.LogError($"[PresetGenerator] {nodeType}에 해당하는 Preset 클래스를 찾을 수 없습니다.");
                    return false;
                }
                
                // 필드 설정
                var so = new SerializedObject(preset);
                
                so.FindProperty("displayName").stringValue = config.DisplayName;
                so.FindProperty("nodeType").enumValueIndex = (int)nodeType;
                so.FindProperty("nodeColor").colorValue = config.NodeColor;
                so.FindProperty("textColor").colorValue = config.TextColor;
                
                // 스프라이트 로드 및 설정
                SetSprite(so, "lockedSprite", config.InactiveSpriteName);
                SetSprite(so, "activeSprite", config.ActiveSpriteName);
                SetSprite(so, "completedSprite", config.CompletedSpriteName);
                
                so.ApplyModifiedProperties();
                
                // 파일로 저장
                string assetPath = $"{PRESET_OUTPUT_PATH}/{nodeType}NodePreset.asset";
                AssetDatabase.CreateAsset(preset, assetPath);
                
                Debug.Log($"[PresetGenerator] {nodeType} Preset 생성 완료: {assetPath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PresetGenerator] {nodeType} Preset 생성 실패: {e.Message}");
                return false;
            }
        }

        private DungeonNodePreset CreatePresetInstance(DungeonNodeTypes nodeType)
        {
            return nodeType switch
            {
                DungeonNodeTypes.Mob => ScriptableObject.CreateInstance<MobNodePreset>(),
                DungeonNodeTypes.Heal => ScriptableObject.CreateInstance<HealNodePreset>(),
                DungeonNodeTypes.Random => ScriptableObject.CreateInstance<RandomNodePreset>(),
                DungeonNodeTypes.MiniBoss => ScriptableObject.CreateInstance<MiniBossNodePreset>(),
                DungeonNodeTypes.FinalBoss => ScriptableObject.CreateInstance<FinalBossNodePreset>(),
                DungeonNodeTypes.BlackMarket => ScriptableObject.CreateInstance<BlackMarketNodePreset>(),
                DungeonNodeTypes.Shop => ScriptableObject.CreateInstance<ShopNodePreset>(),
                _ => null
            };
        }

        private void SetSprite(SerializedObject so, string propertyName, string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
                return;
            
            // Active 폴더에서 찾기
            string activePath = $"{SPRITE_BASE_PATH}/Active/{spriteName}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(activePath);
            
            // Inactive 폴더에서 찾기
            if (sprite == null)
            {
                string inactivePath = $"{SPRITE_BASE_PATH}/Inactive/{spriteName}.png";
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(inactivePath);
            }
            
            if (sprite != null)
            {
                so.FindProperty(propertyName).objectReferenceValue = sprite;
            }
            else
            {
                Debug.LogWarning($"[PresetGenerator] 스프라이트를 찾을 수 없습니다: {spriteName}");
            }
        }
    }
}

