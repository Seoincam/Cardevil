using Cardevil.Core.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor.Components
{
    [UxmlElement]
    public partial class RelicInformationBox : VisualElement
    {
        private readonly Image _iconPreview;
        private readonly Label _titlePreview;
        private readonly Label _descPreview;

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
        }
        
        public void BindRelic(SerializedObject serializedRelic)
        {
            this.Bind(serializedRelic);
        }
    }
}