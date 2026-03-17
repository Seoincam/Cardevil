using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor.Components
{
    [UxmlElement]
    public partial class RelicList : VisualElement
    {
        /// <summary>
        /// 리스트 뷰에서 선택한 유물이 변했을 때 발행되는 이벤트.
        /// </summary>
        public event Action<RelicSO> SelectionChanged;
        
        /// <summary>
        /// <see cref="RelicRow"/>에서 삭제 버튼이 눌렸을 때 발행되는 이벤트.
        /// 유물의 Id를 인자로 함.
        /// </summary>
        public event Action<string> DeleteClicked;
        
        private readonly ListView _listView;
        
        public RelicList()
        {
            const string uxmlPath = "Assets/Scripts/Gameplay/Relics/Editor/Components/RelicList/RelicList.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (!visualTree)
            {
                LogEx.LogError($"UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }

            visualTree.CloneTree(this);
            
            _listView = this.Q<ListView>("MainListView");
        }

        public void Setup(List<RelicSO> relics)
        {
            _listView.Clear();

            _listView.itemsSource = relics;
            _listView.makeItem = () => new RelicRow();
            _listView.bindItem = (element, index) =>
            {
                var row = element as RelicRow;
                var relic = (_listView.itemsSource as List<RelicSO>)![index];
                var relicData = relic.Data;

                row!.SetupData(
                    relicData.DisplayIcon,
                    relicData.Id,
                    relicData.Rarity,
                    relicData.DisplayName,
                    relicData.DisplayDescription,
                    relic.isLocal
                );

                row.DeleteClicked = id => DeleteClicked?.Invoke(id);
            };
            _listView.unbindItem = (element, index) =>
            {
                var row = element as RelicRow;
                row!.DeleteClicked = null;
            };

            _listView.selectionChanged += OnSelectionChanged;
        }

        public void UpdateSource(List<RelicSO> newSource)
        {
            _listView.itemsSource = newSource;
            Refresh();
        }

        public void Refresh()
        {
            _listView.RefreshItems();
        }

        public void SelectItemByObject(RelicSO target)
        {
            if (!target || _listView.itemsSource == null) return;

            var source = _listView.itemsSource as List<RelicSO>;
            int newIndex = source!.IndexOf(target);

            if (newIndex >= 0)
            {
                _listView.SetSelectionWithoutNotify(new[] { newIndex });
                _listView.ScrollToItem(newIndex);
            }
            else
            {
                _listView.ClearSelection();
            }
        }

        public void RefreshSelectedRow()
        {
            int currentIndex = _listView.selectedIndex;

            if (currentIndex >= 0)
            {
                _listView.RefreshItem(currentIndex);
            }
        }

        public void ClearSelection()
        {
            _listView.ClearSelection();
        }

        private void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            var selectedRelic = selectedItems?.FirstOrDefault() as RelicSO;
            if (selectedRelic == null) 
                return;
            
            SelectionChanged?.Invoke(selectedRelic);
        }
    }
}