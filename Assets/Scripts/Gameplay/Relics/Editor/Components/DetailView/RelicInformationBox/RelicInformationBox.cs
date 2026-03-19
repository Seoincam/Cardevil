using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor.Components
{
    [UxmlElement]
    public partial class RelicInformationBox : VisualElement
    {
        public event Action DataChanged;
        
        // 미리보기
        private readonly Image _iconPreview;
        private readonly Label _titlePreview;
        private readonly Label _descPreview;

        private readonly VisualElement _helpBoxContainer;
        
        private readonly TextField _idField;
        private readonly Button _copyIdButton;
        
        // 입력 필드
        private readonly EnumField _rarityField;
        private readonly ObjectField _iconField;
        private readonly TextField _nameField;
        private readonly TextField _descField;
        
        public RelicInformationBox()
        {
            const string uxmlPath = "Assets/Scripts/Gameplay/Relics/Editor/Components/DetailView/RelicInformationBox/RelicInformationBox.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (!visualTree)
            {
                LogEx.LogError($"UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }

            visualTree.CloneTree(this);
            
            _iconPreview = this.Q<Image>("IconPreview");
            _titlePreview = this.Q<Label>("TitlePreview");
            _descPreview = this.Q<Label>("DescriptionPreview");
            
            _helpBoxContainer = this.Q("HelpBoxContainer");
            
            _idField = this.Q<TextField>("IdField");
            _copyIdButton = this.Q<Button>("CopyId");
            _rarityField = this.Q<EnumField>("RarityField");
            _iconField = this.Q<ObjectField>("IconField");
            _nameField = this.Q<TextField>("NameField");
            _descField = this.Q<TextField>("DescriptionField");
            
            // 텍스트/이미지 변경 시 미리보기 동기화
            _iconField.RegisterValueChangedCallback(evt => _iconPreview.sprite = evt.newValue as Sprite);
            _nameField.RegisterValueChangedCallback(evt => _titlePreview.text = evt.newValue);
            _descField.RegisterValueChangedCallback(evt => _descPreview.text = evt.newValue);

            // 데이터가 사용자 입력으로 인해 변경될 때만 DataChanged 이벤트 발생
            _idField.RegisterValueChangedCallback(evt => { if (evt.target == _idField) DataChanged?.Invoke(); });
            _rarityField.RegisterValueChangedCallback(evt => { if (evt.target == _rarityField) DataChanged?.Invoke(); });
            _iconField.RegisterValueChangedCallback(evt => { if (evt.target == _iconField) DataChanged?.Invoke(); });
            _nameField.RegisterValueChangedCallback(evt => { if (evt.target == _nameField) DataChanged?.Invoke(); });
            _descField.RegisterValueChangedCallback(evt => { if (evt.target == _descField) DataChanged?.Invoke(); });

            _copyIdButton.clicked += OnCopyIdButtonClicked;
        }

        public void BindRelic(SerializedObject serializedRelic)
        {
            // 기존 바인딩 해제
            this.Unbind();
            
            // 새로 선택된 SerializedObject로 바인딩
            this.Bind(serializedRelic);

            var relic = serializedRelic.targetObject as RelicSO;
            if (!relic) return;

            bool isLocal = relic.FromLocal;
            
            _rarityField.SetEnabled(isLocal);
            _nameField.SetEnabled(isLocal);
            _descField.SetEnabled(isLocal);
            
            _helpBoxContainer.Clear();
            
            AddSourceDescriptionBox(_helpBoxContainer, relic.Source);
            if (relic.FromSheet)
            {
                AddComment(_helpBoxContainer, relic.Data.CommentForEditor);
            }
            
            // 바인딩 직후에 미리보기 영역 수동 갱신 (선택이 바뀔 때 값이 채워지도록)
            _iconPreview.sprite = relic.Data.DisplayIcon;
            _titlePreview.text = relic.Data.DisplayName;
            _descPreview.text = relic.Data.DisplayDescription;
        }

        private void OnCopyIdButtonClicked()
        {
            EditorGUIUtility.systemCopyBuffer = _idField.value;

            var originalText = _copyIdButton.text;
            _copyIdButton.text = "<b><color=#2ECC71>복사 성공!</color></b>";

            _copyIdButton.schedule.Execute(() =>
            {
                _copyIdButton.text = originalText;
            }).StartingIn(1000);
        }

        private void AddSourceDescriptionBox(VisualElement container, RelicSO.DataSource dataSource)
        {
            string message = dataSource switch
            {
                RelicSO.DataSource.Local => "<b>로컬 유물:</b> 이 유물은 테스트용 유물입니다. 실제 빌드본엔 사용되지 않습니다.",
                RelicSO.DataSource.Sheet => "<b>시트 연동 유물:</b> Id와 희귀도, 이름, 설명은 구글 시트에서 수정하세요.",
                RelicSO.DataSource.Missing => "<b>구글 시트에서 사라진 유물:</b> 구글 시트를 확인하거나 삭제하세요."
            };
            
            var type =dataSource switch
            {
                RelicSO.DataSource.Local => HelpBoxMessageType.Warning,
                RelicSO.DataSource.Sheet => HelpBoxMessageType.Info,
                RelicSO.DataSource.Missing => HelpBoxMessageType.Error
            };

            HelpBox helpBox = new(message, type);
            container.Add(helpBox);
        }

        private void AddComment(VisualElement container, string comment)
        {
            if (string.IsNullOrEmpty(comment)) return;

            const string prefix = "<b>에디터 코멘트: </b>";

            HelpBox helpBox = new(prefix + comment, HelpBoxMessageType.Info);
            container.Add(helpBox);
        }
    }
}