using Cardevil.Core.Utils;
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor.Components.Sync
{
    public class SyncSummaryPopup : VisualElement
    {
        public event Action ConfirmClicked;

        public SyncSummaryPopup(RelicSyncService.Summary summaryResult)
        {
            const string uxmlPath = "Assets/Scripts/Gameplay/Relics/Editor/Components/Sync/SyncSummaryPopup.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (!visualTree)
            {
                LogEx.LogError($"UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }
            
            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;
    
            // style.i = 999;

            visualTree.CloneTree(this);

            var createdCount = this.Q<Label>("CreatedCount");
            var updatedCount = this.Q<Label>("UpdatedCount");
            var missingCount = this.Q<Label>("MissingCount");
            var restoredCount = this.Q<Label>("RestoredCount");

            createdCount.text = $"신규 생성: {summaryResult.CreatedCount}건";
            updatedCount.text = $"갱신: {summaryResult.UpdatedCount}건";
            missingCount.text = $"시트 실종: {summaryResult.MissingCount}건";
            restoredCount.text = $"복구: {summaryResult.RestoredCount}건";

            var confirmButton = this.Q<Button>("Confirm");
            confirmButton.clicked += () =>
            {
                RemoveFromHierarchy();
                // ConfirmClicked?.Invoke();
            };
        }
    }
}