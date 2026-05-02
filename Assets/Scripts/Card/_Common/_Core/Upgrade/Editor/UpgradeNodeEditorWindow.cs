using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cardevil.Card.Common.Core.Upgrade
{
    public class UpgradeNodeEditorWindow : EditorWindow
    {
        private List<UpgradeNodeSO> _allNodes = new();
        private UpgradeNodeDatabaseSO _database;
        private UpgradeNodeSO _selectedNode;

        // UI Elements
        private UpgradeGraphView _graphView;
        private ScrollView _detailContainer;

        private const string SavePath = "Assets/Resources/ScriptableObjects/NewCard/UpgradeNodes";

        [MenuItem("Cardevil/Upgrade Node Editor")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<UpgradeNodeEditorWindow>();
            wnd.titleContent = new GUIContent("강화 노드 편집기");
            wnd.minSize = new Vector2(1200, 800);
            wnd.Show();
        }

        public void CreateGUI()
        {
            SetupUI();
            RefreshData();
            PopulateGraph();
        }

        private void SetupUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;

            // 좌측: GraphView 패널
            var leftPane = new VisualElement { style = { flexGrow = 1, borderRightWidth = 1, borderRightColor = Color.gray } };
            var toolbar = new Toolbar();
            toolbar.Add(new ToolbarButton(OnAddNode) { text = "Add Node" });
            toolbar.Add(new ToolbarButton(RefreshDataAndGraph) { text = "Refresh Graph" });

            _graphView = new UpgradeGraphView(this) { style = { flexGrow = 1 } };
            
            leftPane.Add(toolbar);
            leftPane.Add(_graphView);

            // 우측: Inspector 패널
            _detailContainer = new ScrollView { style = { width = 350, paddingLeft = 15, paddingRight = 15, paddingTop = 15 } };
            
            root.Add(leftPane);
            root.Add(_detailContainer);
        }

        private void RefreshDataAndGraph()
        {
            RefreshData();
            PopulateGraph();
        }

        private void RefreshData()
        {
            string[] guids = AssetDatabase.FindAssets("t:UpgradeNodeSO");
            _allNodes = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<UpgradeNodeSO>(AssetDatabase.GUIDToAssetPath(g)))
                .ToList();
            
            // 2. 데이터베이스 자동 갱신
            if (_database == null)
            {
                string[] dbGuids = AssetDatabase.FindAssets("t:UpgradeNodeDatabaseSO");
                if (dbGuids.Length > 0)
                {
                    _database = AssetDatabase.LoadAssetAtPath<UpgradeNodeDatabaseSO>(AssetDatabase.GUIDToAssetPath(dbGuids[0]));
                }
            }

            if (_database != null)
            {
                _database.SyncNodes(_allNodes);
            }
        }

        private void PopulateGraph()
        {
            _graphView.IsPopulating = true;
            _graphView.ClearGraph();
    
            // 1. 모든 노드를 시각적으로 생성
            foreach (var nodeSO in _allNodes)
            {
                _graphView.CreateGraphNode(nodeSO);
            }

            // 🌟 2. 생성된 노드들을 바탕으로 선(Edge) 그리기
            _graphView.DrawEdges(_allNodes);

            _graphView.IsPopulating = false;
        }

        public void OnNodeSelected(UpgradeNodeSO nodeSO)
        {
            _selectedNode = nodeSO;
            ShowDetailPane();
        }

        // =========================================================================================
        // 우측 디테일 창 (Inspector & 다형성 메뉴)
        // =========================================================================================
        private void ShowDetailPane()
        {
            _detailContainer.Clear();
            if (_selectedNode == null) return;
        
            var serializedObject = new SerializedObject(_selectedNode);
        
            // 1. 에셋 이름 변경
            var nameField = new TextField("Asset Name") { value = _selectedNode.name };
            nameField.RegisterValueChangedCallback(evt =>
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(_selectedNode), evt.newValue);
                RefreshData();
            });
            _detailContainer.Add(nameField);
            _detailContainer.Add(new VisualElement { style = { height = 15 } });
        
            // 2. 기본 프로퍼티들 수동 렌더링
            _detailContainer.Add(new Label("Constraints") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            _detailContainer.Add(new PropertyField(serializedObject.FindProperty("targetCardType"))); // 🌟 추가
            _detailContainer.Add(new VisualElement { style = { height = 10 } });
            
            _detailContainer.Add(new Label("Settings") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            _detailContainer.Add(new PropertyField(serializedObject.FindProperty("path")));
            _detailContainer.Add(new PropertyField(serializedObject.FindProperty("stage")));
            _detailContainer.Add(new PropertyField(serializedObject.FindProperty("upgradeType")));
            // _detailContainer.Add(new PropertyField(serializedObject.FindProperty("nextNodes")));
        
            // 🌟 3. 새로 추가된 Cost 변수들 렌더링
            _detailContainer.Add(new VisualElement { style = { height = 15 } });
            _detailContainer.Add(new Label("Cost Settings") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            _detailContainer.Add(new PropertyField(serializedObject.FindProperty("blackMarketCost")));
            _detailContainer.Add(new PropertyField(serializedObject.FindProperty("marketCost")));
        
            // 4. Elements 리스트 수동 렌더링
            _detailContainer.Add(new VisualElement { style = { height = 20 } });
            _detailContainer.Add(new Label("Upgrade Elements") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
        
            var elementsProp = serializedObject.FindProperty("elements");
            var elementsListView = new ListView
            {
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                showBorder = true,
                reorderable = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                bindingPath = elementsProp.propertyPath
            };
            _detailContainer.Add(elementsListView);
        
            // 5. 다형성 Element 추가 버튼 및 드롭다운 메뉴
            var addElementBtn = new Button(() => ShowAddElementMenu(serializedObject, elementsProp)) 
            { 
                text = "+ Add Element", 
                style = { marginTop = 10, backgroundColor = new Color(0.2f, 0.4f, 0.2f) } 
            };
            _detailContainer.Add(addElementBtn);
            
            // 🌟 6. 핵심 추가: detailContainer 내의 값이 바뀔 때 발생하는 이벤트를 가로채서 노드 갱신
            _detailContainer.RegisterCallback<SerializedPropertyChangeEvent>(evt => 
            {
                if (_selectedNode != null)
                {
                    _graphView.UpdateNodeVisual(_selectedNode);
                }
            });
        
            // 🌟 핵심: detailContainer 전체에 SerializedObject를 바인딩합니다.
            // 이렇게 하면 추가된 모든 PropertyField와 ListView가 자동으로 값 변경을 감지하고 에셋에 저장합니다.
            _detailContainer.Bind(serializedObject);
        }

        private void ShowAddElementMenu(SerializedObject serializedObject, SerializedProperty elementsProp)
        {
            var menu = new GenericMenu();
            
            // ISpecElement를 상속받는 모든 클래스를 리플렉션(TypeCache)으로 가져옵니다.
            var types = TypeCache.GetTypesDerivedFrom<ISpecElement>()
                .Where(t => !t.IsAbstract && !t.IsInterface);

            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    serializedObject.Update();
                    
                    // 리스트 끝에 새 항목 추가
                    elementsProp.arraySize++;
                    var newElementProp = elementsProp.GetArrayElementAtIndex(elementsProp.arraySize - 1);
                    
                    // 선택한 타입의 인스턴스를 생성하여 managedReferenceValue에 할당 ([SerializeReference] 전용)
                    newElementProp.managedReferenceValue = Activator.CreateInstance(type);
                    
                    serializedObject.ApplyModifiedProperties();
                    _graphView.UpdateNodeVisual(_selectedNode); 
                    ShowDetailPane(); // UI 새로고침
                });
            }

            menu.ShowAsContext();
        }

        private void OnAddNode()
        {
            var newNode = CreateInstance<UpgradeNodeSO>();

            if (!AssetDatabase.IsValidFolder(SavePath))
            {
                Debug.LogError($"폴더가 존재하지 않습니다. path: {SavePath}");
                return;
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{SavePath}/NewUpgradeNode.asset");
            AssetDatabase.CreateAsset(newNode, assetPath);
            AssetDatabase.SaveAssets();

            RefreshDataAndGraph();
        }
    }

    // =========================================================================================
    // 시각적 노드 연결을 위한 GraphView 클래스
    // =========================================================================================
    public class UpgradeGraphView : GraphView
    {
        private readonly UpgradeNodeEditorWindow _window;
        
        // 🌟 노드 에셋(SO)과 시각적 노드(Node)를 매핑해두는 딕셔너리
        private Dictionary<UpgradeNodeSO, Node> _nodeMap = new();

        public bool IsPopulating { get; set; }
    
        public UpgradeGraphView(UpgradeNodeEditorWindow window)
        {
            _window = window;
            
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
    
            Insert(0, new GridBackground());
            
            graphViewChanged += OnGraphViewChanged;
        }
    
        public void ClearGraph()
        {
            IsPopulating = true;
            DeleteElements(graphElements.ToList());
            _nodeMap.Clear(); // 맵 초기화
            IsPopulating = false;
        }
    
        public void CreateGraphNode(UpgradeNodeSO nodeSO)
        {
            var node = new Node { title = nodeSO.name, userData = nodeSO };

            // 1. 포트 생성
            var inputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "In";
            node.inputContainer.Add(inputPort);

            var outputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "Out";
            node.outputContainer.Add(outputPort);

            // 2. 컨테이너 및 레이블 선언 (🌟 생성할 때부터 텍스트를 바로 주입합니다!)
            
            // 🌟 추가: TargetCardType에 따른 타이틀 바 색상 설정
            // 노드를 생성하는 시점에 스타일을 직접 지정합니다.
            var titleContainer = node.titleContainer;
            titleContainer.style.backgroundColor = nodeSO.TargetCardType switch
            {
                CardType.Attack => new Color(0.5f, 0.15f, 0.15f), // 공격: 어두운 빨강
                CardType.Move => new Color(0.15f, 0.3f, 0.5f),   // 이동: 어두운 파랑
                _ => new Color(0.25f, 0.25f, 0.25f)              // 기본: 회색
            };
            
            var detailsContainer = new VisualElement { style = { paddingBottom = 5, paddingLeft = 5, paddingRight = 5 } };
            
            // 🌟 Target Type 레이블 추가
            detailsContainer.Add(new Label 
            { 
                name = "targetTypeLabel", 
                text = $"Target: {nodeSO.TargetCardType}", 
                style = { color = new Color(0.6f, 0.9f, 0.6f), fontSize = 11 } // 연녹색으로 강조
            });

            detailsContainer.Add(new Label 
            { 
                name = "pathStage", 
                text = $"[{nodeSO.Stage}단계] {nodeSO.Path}", // 🌟 초기값 주입
                style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.white } 
            });

            detailsContainer.Add(new Label 
            { 
                name = "typeLabel", 
                text = $"Type: {nodeSO.UpgradeType}", // 🌟 초기값 주입
                style = { color = new Color(0.8f, 0.8f, 0.8f) } 
            });

            detailsContainer.Add(new Label 
            { 
                name = "costLabel", 
                text = $"Cost: M {nodeSO.MarketCost} / BM {nodeSO.BlackMarketCost}", // 🌟 초기값 주입
                style = { color = Color.yellow } 
            });

            detailsContainer.Add(new Label 
            { 
                name = "elementsLabel", 
                text = $"Elements: {nodeSO.Elements?.Count ?? 0}", // 🌟 초기값 주입
                style = { color = new Color(0.8f, 0.8f, 0.8f) } 
            });

            node.extensionContainer.style.minWidth = 160;
            node.extensionContainer.Add(detailsContainer);
            node.expanded = true;

            // 3. 노드를 그래프에 추가
            _nodeMap[nodeSO] = node;
            AddElement(node);

            // 4. 확장 상태 강제 활성화 및 포트 갱신
            node.expanded = true; 
            node.RefreshExpandedState(); 
            node.RefreshPorts();

            // 나머지 콜백 및 위치 설정
            node.RegisterCallback<MouseDownEvent>(evt => { _window.OnNodeSelected(nodeSO); });
            var savedPos = nodeSO.nodePosition;
            node.SetPosition(new Rect(savedPos.x, savedPos.y, 0, 0)); 
        }
        
        // 🌟 새로 추가: 데이터가 바뀌었을 때 텍스트를 실시간으로 다시 그려주는 메서드
        public void UpdateNodeVisual(UpgradeNodeSO nodeSO)
        {
            if (!_nodeMap.TryGetValue(nodeSO, out var node)) return;

            // 이름(타이틀) 업데이트
            node.title = nodeSO.name;
            
            // 🌟 타입에 따른 타이틀 바 색상 변경 (가시성 업그레이드)
            var titleContainer = node.titleContainer;
            titleContainer.style.backgroundColor = nodeSO.TargetCardType switch
            {
                CardType.Attack => new Color(0.5f, 0.15f, 0.15f), // 공격: 어두운 빨강
                CardType.Move => new Color(0.15f, 0.3f, 0.5f),   // 이동: 어두운 파랑
                _ => new Color(0.25f, 0.25f, 0.25f)              // 기본: 회색
            };
            
            var targetTypeLabel = node.Q<Label>("targetTypeLabel");
            if (targetTypeLabel != null)
                targetTypeLabel.text = $"Target: {nodeSO.TargetCardType}";

            var pathStageLabel = node.Q<Label>("pathStage");
            if (pathStageLabel != null)
                pathStageLabel.text = $"[{nodeSO.Stage}단계] {nodeSO.Path}";

            var typeLabel = node.Q<Label>("typeLabel");
            if (typeLabel != null)
                typeLabel.text = $"Type: {nodeSO.UpgradeType}"; // UpgradeApplyType 열거형이 깔끔하게 출력됩니다.

            var costLabel = node.Q<Label>("costLabel");
            if (costLabel != null)
                costLabel.text = $"Cost: M {nodeSO.MarketCost} / BM {nodeSO.BlackMarketCost}";

            var elementsLabel = node.Q<Label>("elementsLabel");
            if (elementsLabel != null)
                elementsLabel.text = $"Elements: {nodeSO.Elements?.Count ?? 0}";
        }
    
        // 🌟 기존 데이터를 바탕으로 선(Edge)을 그려주는 로직
        public void DrawEdges(List<UpgradeNodeSO> allNodes)
        {
            foreach (var parentSO in allNodes)
            {
                if (parentSO.NextNodes == null) continue;
    
                foreach (var childSO in parentSO.NextNodes)
                {
                    // 둘 다 그래프 상에 존재하는 노드일 때만 연결
                    if (childSO == null || !_nodeMap.ContainsKey(parentSO) || !_nodeMap.ContainsKey(childSO)) 
                        continue;
    
                    var parentNode = _nodeMap[parentSO];
                    var childNode = _nodeMap[childSO];
    
                    // 생성했던 포트 가져오기
                    var outputPort = parentNode.outputContainer.Q<Port>();
                    var inputPort = childNode.inputContainer.Q<Port>();
    
                    // 시각적으로 선 연결
                    var edge = outputPort.ConnectTo(inputPort);
                    AddElement(edge);
                }
            }
        }
    
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                // 자기 자신과 연결 방지, In-Out 방향이 다를 때만 연결 허용
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }
        
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (IsPopulating) return graphViewChange;
            
            // 1. 노드 이동 저장 최적화
            if (graphViewChange.movedElements != null)
            {
                // 🌟 최적화: 이동된 모든 SO를 한꺼번에 리스트로 모음
                var movedSOs = graphViewChange.movedElements
                    .OfType<Node>()
                    .Select(n => n.userData as UpgradeNodeSO)
                    .Where(so => so != null)
                    .ToArray();
        
                if (movedSOs.Length > 0)
                {
                    // 🌟 최적화: 단 한 번의 Undo 기록
                    Undo.RecordObjects(movedSOs, "Move Upgrade Nodes");
                    
                    foreach (var node in graphViewChange.movedElements.OfType<Node>())
                    {
                        if (node.userData is UpgradeNodeSO nodeSO)
                        {
                            nodeSO.nodePosition = node.GetPosition().position;
                            EditorUtility.SetDirty(nodeSO);
                        }
                    }
                    // 🌟 주의: AssetDatabase.SaveAssets()는 여기서 절대 호출하지 마세요!
                }
            }
        
            // 2. 선 연결 저장
            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    var parentSO = edge.output.node.userData as UpgradeNodeSO;
                    var childSO = edge.input.node.userData as UpgradeNodeSO;
        
                    if (parentSO != null && childSO != null)
                    {
                        Undo.RecordObject(parentSO, "Connect Node");
                        parentSO.AddNextNode(childSO);
                        EditorUtility.SetDirty(parentSO);
                    }
                }
            }
        
            // 3. 요소 삭제 (중복 제거 및 통합)
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    // 선(Edge) 삭제 시 연결 해제
                    if (element is Edge edge)
                    {
                        var parentSO = edge.output.node.userData as UpgradeNodeSO;
                        var childSO = edge.input.node.userData as UpgradeNodeSO;
        
                        if (parentSO != null && childSO != null)
                        {
                            Undo.RecordObject(parentSO, "Disconnect Node");
                            parentSO.RemoveNextNode(childSO);
                            EditorUtility.SetDirty(parentSO);
                        }
                    }
                    // 노드(Node) 삭제 시 에셋 삭제
                    else if (element is Node node && node.userData is UpgradeNodeSO nodeSO)
                    {
                        DeleteNodeAsset(nodeSO);
                    }
                }
            }
        
            return graphViewChange;
        }
        
        private void DeleteNodeAsset(UpgradeNodeSO nodeSO)
        {
            // 실수 방지를 위해 확인 팝업을 띄우는 것이 안전합니다.
            bool confirm = EditorUtility.DisplayDialog(
                "강화 노드 삭제",
                $"정말 '{nodeSO.name}' 에셋을 영구적으로 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.",
                "삭제",
                "취소"
            );

            if (confirm)
            {
                string path = AssetDatabase.GetAssetPath(nodeSO);
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.SaveAssets();
            
                    // 윈도우의 리스트 데이터를 갱신하기 위해 호출
                    // (이미 PopulateGraph가 내부적으로 리스트를 다시 읽도록 설계되어 있다면 생략 가능)
                    // _window.RefreshData(); 
                }
            }
        }
    }
}