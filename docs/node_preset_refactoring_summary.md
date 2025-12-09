# Node Behaviour → Node Preset 리팩토링 요약

## 📋 작업 개요

모든 `NodeBehaviour`를 `NodePreset`으로 이름을 변경하고, ScriptableObject가 직접 노드 UI를 그리도록 개선했습니다.
**DungeonNodeSettingSO의 SpriteSet 역할을 제거**하고, 각 Preset이 상태별 스프라이트를 직접 관리합니다.

## ✅ 주요 변경 사항

### 1. **클래스 이름 변경**
모든 NodeBehaviour를 NodePreset으로 변경:

| 이전 | 변경 후 |
|------|---------|
| `DungeonNodeBehaviour` | `DungeonNodePreset` |
| `FinalBossNodeBehaviour` | `FinalBossNodePreset` |
| `MiniBossBehaviour` | `MiniBossNodePreset` |
| `MobNodeBehaviour` | `MobNodePreset` |
| `HealNodeBehaviour` | `HealNodePreset` |
| `RandomNodeBehaviour` | `RandomNodePreset` |
| `ReinforcementNodeBehaviour` | `ReinforcementNodePreset` |

### 2. **새로운 Preset 추가**
- `BlackMarketNodePreset` - 암시장 노드 (확률 기반 출현)
- `ShopNodePreset` - 상점 노드

### 3. **DungeonNodePreset 기본 클래스 개선**

#### 상태별 스프라이트 필드 추가 (DungeonNodeSettingSO 역할 대체)
```csharp
[Header("상태별 스프라이트")]
[SerializeField] protected Sprite lockedSprite;      // 잠김 상태
[SerializeField] protected Sprite activeSprite;      // 활성화 상태
[SerializeField] protected Sprite completedSprite;   // 완료 상태
[SerializeField] protected Sprite completedOverlaySprite; // 완료 오버레이
```

#### GetSpriteForState 메서드 추가
```csharp
public virtual Sprite GetSpriteForState(NodeState state)
{
    return state switch
    {
        NodeState.Locked => lockedSprite,
        NodeState.Available => activeSprite,
        NodeState.Current => activeSprite,
        NodeState.Completed => completedSprite,
        _ => lockedSprite
    };
}
```

#### DrawNodeUI 메서드 (상태 파라미터 추가)
```csharp
public virtual void DrawNodeUI(Image nodeImage, TextMeshProUGUI nodeText, Image overlayImage, NodeState state)
{
    // 스프라이트 설정
    if (nodeImage != null)
    {
        Sprite sprite = GetSpriteForState(state);
        if (sprite != null) nodeImage.sprite = sprite;
        nodeImage.color = nodeColor;
    }

    // 텍스트 설정 (잠김/완료 상태에서는 숨김)
    if (nodeText != null)
    {
        if (state == NodeState.Locked || state == NodeState.Completed)
            nodeText.text = "";
        else
        {
            nodeText.text = string.IsNullOrEmpty(displayName) ? name : displayName;
            nodeText.color = textColor;
        }
    }
    
    // 오버레이 설정
    if (overlayImage != null)
    {
        if (state == NodeState.Completed && completedOverlaySprite != null)
        {
            overlayImage.gameObject.SetActive(true);
            overlayImage.sprite = completedOverlaySprite;
        }
        else
            overlayImage.gameObject.SetActive(false);
    }
}
```

### 4. **BlackMarketNodePreset - 확률 기반 출현**

```csharp
[Header("암시장 설정")]
[SerializeField, Range(0f, 1f)] private float _appearChance = 0.5f;
[SerializeField] private bool _alwaysAppear; // true면 확률 무시하고 반드시 출현

public bool ShouldAppear()
{
    if (_alwaysAppear) return true;
    return Random.value < _appearChance;
}
```

### 5. **블랙마켓 로직 개선**

**핵심 변경**: 블랙마켓은 **던전에 미리 배치**되지만, **현재 노드에서 다음 노드로 이동할 때** 확률을 체크합니다.

#### DungeonManager.EnterNode()에서 블랙마켓 확률 체크
```csharp
// 다음 노드들 중 블랙마켓이 있으면 확률 체크
foreach (var nextNode in currentNode.NextNodes)
{
    if (nextNode.Type == DungeonNodeTypes.BlackMarket)
    {
        bool appeared = nextNode.CheckBlackMarketAppearance();
        if (appeared)
            nextNode.State = NodeState.Available;
        else
            // UI에서 숨김 처리
    }
    else
        nextNode.State = NodeState.Available;
}

// UI에 블랙마켓 상태 업데이트 요청
UI?.UpdateBlackMarketVisibility(currentNode);
```

#### DungeonNode.CheckBlackMarketAppearance()
```csharp
public bool CheckBlackMarketAppearance()
{
    if (Type != DungeonNodeTypes.BlackMarket) return true;
    
    if (Preset is BlackMarketNodePreset blackMarketPreset)
    {
        bool shouldAppear = blackMarketPreset.ShouldAppear();
        _isBlackMarketHidden = !shouldAppear;
        return shouldAppear;
    }
    return true;
}
```

#### DungeonNodeUI - 블랙마켓 숨김 처리
```csharp
public void HideAsBlackMarketNotAppeared()
{
    _isHidden = true;
    gameObject.SetActive(false);
}
```

### 6. **DungeonNodeUI 단순화**

- `DungeonNodeSettingSO` 참조 제거
- Preset의 `DrawNodeUI()`가 직접 UI를 그림
- 버튼 상태만 별도로 처리

```csharp
public void UpdateView()
{
    if (dungeonNode == null || _isHidden) return;
    
    // Preset이 직접 UI를 그림
    if (dungeonNode.Preset != null)
        dungeonNode.Preset.DrawNodeUI(nodeImage, nodeText, overlayImage, dungeonNode.State);
    
    // 버튼 상태 처리
    UpdateButtonState(dungeonNode.State);
}
```

### 7. **DungeonUI - 블랙마켓 가시성 업데이트**

```csharp
public void UpdateBlackMarketVisibility(DungeonNode currentNode)
{
    foreach (var nextNode in currentNode.NextNodes)
    {
        if (nextNode.Type == DungeonNodeTypes.BlackMarket && nextNode.IsBlackMarketHidden)
        {
            var nodeUI = chapterUI.GetNodeUI(nextNode.NodeId);
            nodeUI?.HideAsBlackMarketNotAppeared();
        }
    }
}
```

## 🎯 장점

1. **명확한 네이밍**: Behaviour보다 Preset이 ScriptableObject의 역할을 더 명확하게 표현
2. **UI 제어 향상**: 각 Preset이 자신의 UI를 직접 그릴 수 있어 확장성 증가
3. **DungeonNodeSettingSO 역할 제거**: Enum 기반 스프라이트 매핑 대신 SO가 직접 관리
4. **블랙마켓 확률 체크 시점 개선**: Initialize가 아닌 실제 이동 시점에 확률 체크
5. **반드시 출현하는 블랙마켓 지원**: `AlwaysAppear` 옵션으로 필수 블랙마켓 설정 가능

## 📌 사용 방법

### Preset 생성
Unity 에디터에서 우클릭 → Create → Cardevil → Dungeon → Node Presets → 원하는 타입 선택

### UI 커스터마이징
각 Preset에서 다음 항목을 설정:
- **Locked Sprite**: 잠김 상태 스프라이트
- **Active Sprite**: 활성화 상태 스프라이트
- **Completed Sprite**: 완료 상태 스프라이트
- **Completed Overlay Sprite**: 완료 오버레이 (옵션)
- **Display Name**: 노드에 표시될 이름
- **Node Color / Text Color**: 색상 설정

### 블랙마켓 설정
BlackMarketNodePreset에서:
- **Appear Chance**: 출현 확률 (0.0 ~ 1.0)
- **Always Appear**: 체크하면 확률 무시하고 반드시 출현

## ⚠️ 주의사항

1. **Unity 리로드 필요**: 파일 이름이 변경되었으므로 Unity 에디터를 재시작하세요
2. **Preset 스프라이트 설정 필수**: 각 Preset에 상태별 스프라이트를 설정해야 합니다
3. **DungeonNodeSettingSO는 더 이상 스프라이트를 관리하지 않음**

## 🔍 테스트 체크리스트

- [ ] 모든 Preset에 상태별 스프라이트가 설정되었는지 확인
- [ ] 던전 UI에서 노드가 올바르게 표시되는지 확인
- [ ] 블랙마켓이 확률에 따라 나타나거나 숨겨지는지 확인
- [ ] AlwaysAppear가 true인 블랙마켓이 항상 나타나는지 확인
- [ ] 블랙마켓이 숨겨졌을 때 다음 노드로 정상적으로 이동하는지 확인

