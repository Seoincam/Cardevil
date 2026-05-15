using UnityEditor;
using UnityEngine;

namespace Cardevil.Card.Common.Core.Upgrade
{
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cardevil.Card.Common.Core.Upgrade
{
    [CustomEditor(typeof(UpgradeNodeSO))]
    public class UpgradeNodeInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // 1. 루트 컨테이너 생성
            var root = new VisualElement();
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;

            // 2. 히어로 섹션 (Banner)
            var banner = new VisualElement();
            banner.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            banner.style.borderBottomWidth = 2;
            banner.style.borderBottomColor = new Color(0.1f, 0.6f, 0.9f); // 포인트 컬러 (파란색)
            banner.style.paddingTop = 15;
            banner.style.paddingBottom = 15;
            banner.style.marginBottom = 10;
            banner.style.borderTopLeftRadius = 5;
            banner.style.borderTopRightRadius = 5;

            var title = new Label("강화 노드 데이터");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 14;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.color = new Color(0.8f, 0.8f, 0.8f);
            banner.Add(title);
            root.Add(banner);

            // 3. 안내 메시지
            var helpBox = new HelpBox("이 에셋은 전용 그래프 에디터에서만 수정 가능합니다.", HelpBoxMessageType.Info);
            helpBox.style.marginBottom = 10;
            root.Add(helpBox);

            // 4. 세련된 버튼 생성 (USS 스타일링)
            var openButton = new Button(() => 
            {
                UpgradeNodeEditorWindow.ShowWindow();
                var window = EditorWindow.GetWindow<UpgradeNodeEditorWindow>();
                window.OnNodeSelected((UpgradeNodeSO)target);
            }) 
            { 
                text = "강화 에디터 열기" 
            };

            // 버튼 스타일링
            var s = openButton.style;
            s.height = 45;
            s.fontSize = 15;
            s.unityFontStyleAndWeight = FontStyle.Bold;
            s.backgroundColor = new Color(0.18f, 0.35f, 0.55f); // 차분한 네이비 블루
            s.color = Color.white;
            s.borderTopLeftRadius = 8;
            s.borderBottomLeftRadius = 8;
            s.borderTopRightRadius = 8;
            s.borderBottomRightRadius = 8;
            s.marginTop = 5;
            s.marginBottom = 15;
            s.borderLeftWidth = s.borderRightWidth = s.borderTopWidth = s.borderBottomWidth = 0; // 테두리 제거

            // 호버 효과 추가
            openButton.RegisterCallback<MouseEnterEvent>(evt => openButton.style.backgroundColor = new Color(0.22f, 0.45f, 0.7f));
            openButton.RegisterCallback<MouseLeaveEvent>(evt => openButton.style.backgroundColor = new Color(0.18f, 0.35f, 0.55f));

            root.Add(openButton);

            // 5. 구분선
            var line = new VisualElement();
            line.style.height = 1;
            line.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            line.style.marginBottom = 10;
            root.Add(line);

            // 6. 데이터 프리뷰 (읽기 전용)
            var foldout = new Foldout { text = "Data Preview (Read Only)", value = false };
            var inspectorFields = new VisualElement();
            InspectorElement.FillDefaultInspector(inspectorFields, serializedObject, this);
            inspectorFields.SetEnabled(false); // 수정 방지
            foldout.Add(inspectorFields);
            root.Add(foldout);

            return root;
        }
    }
}
}