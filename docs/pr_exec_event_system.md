# 🚀 FEAT : 우선순위 기반 비동기 실행 이벤트 시스템 구현

## 📑 개요
우선순위에 따라 순차적으로 비동기 실행되는 이벤트 시스템을 구현했습니다.
- `UniTask`를 활용한 비동기 이벤트 처리
- 우선순위 기반 실행 순서 제어 (`ExecPriority` enum 지원)
- 이벤트 체인 중단 기능 (`BreakChain`)
- 이벤트 풀링을 통한 메모리 효율성
- 자동 버스 초기화 및 런타임 관리
- Unity Editor 플레이모드 전환 시 자동 정리 기능
- 동적 버스와 정적 버스 분리 설계
- 병합 실행 모드 (Merge Mode) 지원

---

## ✏️ 변경(추가) 사항

### 1) ExecEventArgs (이벤트 인자)
- `Events.Core`의 `EventArgs<T>`를 상속받는 실행 이벤트 인자 베이스 클래스
- `BreakChain` 속성을 통한 이벤트 체인 중단 기능 제공
- 이벤트 풀링 시스템 지원 (`Get()`, `Release()`, `Dispose()`)
- 새로운 이벤트 타입을 정의하면 자동으로 해당 EventBus가 생성됨

### 2) ExecPriority (우선순위 Enum)
- 우선순위를 명확하게 표현하기 위한 enum 타입
- `First` (Int32.MinValue): 최우선 실행
- `Normal` (0): 기본 우선순위
- `Last` (Int32.MaxValue): 가장 나중에 실행 (UI 등)
- enum 사용 시 가독성과 유지보수성 향상

### 3) ExecQueue (우선순위 실행 큐)
- 우선순위 값을 가진 액션들을 저장하는 큐 클래스
- `ActionWrapper` 내부 클래스를 통한 우선순위 비교 및 정렬
  - Primary Priority, Extra Priorities, Enqueued Order를 기준으로 정렬
  - `IComparable<T>` 인터페이스 구현으로 정렬 지원
  - 객체 풀링을 통한 메모리 최적화
- **등록 메서드:**
  - `Enqueue(ExecPriority priority, ExecAction<TEventArgs> action)`: enum 타입 우선순위로 액션 등록
  - `Enqueue(int priority, ExecAction<TEventArgs> action, int[] extraPriorities)`: 정수 우선순위로 액션 등록 (오버로드)
  - `EnqueueBinarySearch(int priority, ...)`: 이진 탐색을 통한 등록 (이미 정렬된 상태에서 사용)
  - `EnqueueSafeBinarySearch(int priority, ...)`: 자동 정렬 후 이진 탐색 등록
- `SortByPriority()`: 우선순위에 따라 정렬 (dirty 플래그 관리)
- `ExecuteAll(TEventArgs eventArgs)`: 정렬된 순서대로 모든 액션을 비동기 실행
- 실행 시 스냅샷을 사용하여 실행 중 큐 변경에 안전
- `Remove(ExecAction)`: 특정 액션 제거 기능

### 4) ExecDynamicEventBus (동적 이벤트 버스)
- 제네릭 정적 클래스로 각 이벤트 타입별 버스 인스턴스 제공
- **핸들러 관리:**
  - `Register(ExecEventHandler<TEvent> handler)`: 핸들러 등록
  - `Unregister(ExecEventHandler<TEvent> handler)`: 핸들러 해제
  - `ClearHandlers()`: 모든 핸들러 제거
- **이벤트 호출:**
  - `Invoke(TEvent eventArgs)`:
    - 등록된 모든 핸들러에 큐와 이벤트 인자 전달
    - 핸들러들이 큐에 우선순위와 함께 액션 등록
    - 우선순위에 따라 정렬 후 순차 실행
  - `InvocationQueue(TEvent eventArgs)`: 핸들러 호출 후 큐 반환 (병합 모드용)
- 실행 시마다 큐를 초기화하고 재정렬하므로 유연한 우선순위 변경 가능
- 빈번한 이벤트 호출에는 `ExecStaticEventBus` 사용 권장

### 5) ExecStaticEventBus (정적 이벤트 버스)
- 우선순위가 미리 정해진 정적 핸들러를 관리하는 버스
- 핸들러 등록 시 우선순위를 함께 지정하여 최적화
- **핸들러 관리:**
  - `Register(int priority, ExecAction<TEvent> handler, int[] extraPriorities)`: 우선순위와 함께 등록
  - `RegisterBinarySearch(...)`: 이진 탐색으로 등록 (정렬된 상태 유지)
  - `RegisterSafeBinarySearch(...)`: 자동 정렬 후 이진 탐색 등록
  - `Unregister(ExecAction<TEvent> handler)`: 핸들러 해제
  - `ClearHandlers()`: 모든 핸들러 제거
- **이벤트 호출:**
  - `Invoke(TEvent eventArgs)`: 등록된 액션들을 우선순위에 따라 순차 실행
  - `GetExecQueue()`: 내부 큐 직접 접근 (병합 모드용)
- 동적 버스보다 성능이 우수하며 자주 호출되는 이벤트에 적합

### 6) ExecEventBus (통합 이벤트 버스)
- `ExecDynamicEventBus`와 `ExecStaticEventBus`의 기능을 통합한 Facade 클래스
- **동적 핸들러 관리:**
  - `RegisterDynamic(ExecEventHandler<TEvent> handler)`
  - `UnregisterDynamic(ExecEventHandler<TEvent> handler)`
  - `ClearDynamicHandlers()`
- **정적 핸들러 관리:**
  - `RegisterStatic(int priority, ExecAction<TEvent> handler, int[] extraPriorities)`
  - `RegisterStaticBinarySearch(...)`
  - `UnregisterStatic(ExecAction<TEvent> handler)`
  - `ClearStaticHandlers()`
- **이벤트 호출 모드:**
  - `InvokeSequentially(TEvent args)`: 동적 이벤트 → 정적 이벤트 순차 실행
  - `InvokeMerged(TEvent args)`: 두 큐를 병합하여 우선순위에 따라 통합 실행
    - Merge Sort 방식으로 동적/정적 액션을 우선순위 기준으로 통합
    - 같은 우선순위일 경우 동적 이벤트가 먼저 실행

### 7) ExecEventUtil (유틸리티 클래스)
- `RuntimeInitializeOnLoadMethod`를 통한 자동 초기화
- 리플렉션을 사용하여 모든 `ExecEventArgs` 타입 자동 감지
- 각 이벤트 타입에 대한 `ExecDynamicEventBus` 및 `ExecStaticEventBus` 자동 생성
- `ClearBus()`: 모든 버스의 핸들러 초기화
- Unity Editor에서 플레이모드 종료 시 자동 정리 기능
- 등록된 이벤트 타입 및 버스 타입 추적 (`EventTypes`, `EventBusTypes`, `StaticEventBusTypes`)

### 8) ExecEvents (예제 이벤트)
- `TestExecEventArgs`: 테스트용 예제 이벤트 클래스
- 새로운 이벤트 타입 생성 방법을 보여주는 예시

---

## 📖 사용 방법

### 기본 이벤트 사용법 (동적 버스)

```csharp
// 1. 이벤트 인자 클래스 정의
public class TestExecEventArgs : ExecEventArgs<TestExecEventArgs>
{
    public int Value;
    
    public override void Clear()
    {
        base.Clear();
        Value = 0;
    }
}

// 2. OnEnable에서 핸들러 등록
private void OnEnable()
{
    ExecEventBus<TestExecEventArgs>.RegisterDynamic(OnTestEvent);
}

// 3. 핸들러 메서드 구현 (로컬 함수를 사용한 액션 정의)
private void OnTestEvent(ExecQueue<TestExecEventArgs> queue, TestExecEventArgs args)
{
    // 로컬 함수로 액션 정의
    async UniTask FirstPriorityAction(TestExecEventArgs eventArgs)
    {
        await UniTask.Delay(100);
        LogEx.Log($"First Priority Action Executed with Value: {eventArgs.Value}");
    }
    
    async UniTask SecondPriorityAction(TestExecEventArgs eventArgs)
    {
        await UniTask.Delay(200);
        LogEx.Log($"Second Priority Action Executed with Value: {eventArgs.Value}");
    }
    
    // 우선순위 순서대로 등록 (1 -> 2 순서로 실행됨)
    queue.Enqueue(1, FirstPriorityAction);
    queue.Enqueue(2, SecondPriorityAction);
}

// 4. OnDisable에서 핸들러 해제
private void OnDisable()
{
    ExecEventBus<TestExecEventArgs>.UnregisterDynamic(OnTestEvent);
}

// 5. 이벤트 발생
public async void InvokeTestEvent()
{
    // 방법 1: 풀에서 이벤트 인자 가져오기 (권장)
    var args = TestExecEventArgs.Get();
    args.Value = Random.Range(0, 100);
    
    LogEx.Log($"Invoking TestExecEvent with Value: {args.Value}");
    await ExecEventBus<TestExecEventArgs>.InvokeSequentially(args);
    
    // 이벤트 인자 정리 (풀로 반환)
    args.Release();
    
    // 방법 2: using 사용
    using var args2 = new TestExecEventArgs { Value = Random.Range(1, 100) };
    await ExecEventBus<TestExecEventArgs>.InvokeSequentially(args2);
}
```

### 정적 버스 사용법

```csharp
// 등록 (우선순위와 함께)
void OnEnable()
{
    ExecEventBus<TestExecEventArgs>.RegisterStatic(10, OnStaticAction);
    ExecEventBus<TestExecEventArgs>.RegisterStatic(5, OnHighPriorityAction);
}

async UniTask OnStaticAction(TestExecEventArgs args)
{
    await UniTask.Delay(100);
    LogEx.Log($"Static Action: {args.Value}");
}

async UniTask OnHighPriorityAction(TestExecEventArgs args)
{
    LogEx.Log($"High Priority Action: {args.Value}");
}

void OnDisable()
{
    ExecEventBus<TestExecEventArgs>.UnregisterStatic(OnStaticAction);
    ExecEventBus<TestExecEventArgs>.UnregisterStatic(OnHighPriorityAction);
}
```

### ExecPriority Enum 사용법

```csharp
// ExecPriority enum을 사용하여 명확한 우선순위 표현
queue.Enqueue(ExecPriority.First, async (args) => 
{
    // 가장 먼저 실행 (Int32.MinValue)
    await DoCriticalAction();
});

queue.Enqueue(ExecPriority.Normal, async (args) => 
{
    // 기본 우선순위 (0)
    await DoNormalAction();
});

queue.Enqueue(ExecPriority.Last, async (args) => 
{
    // 가장 나중에 실행 (Int32.MaxValue) - UI 등
    await DoUIAction();
});
```

### 병합 실행 모드 (Merge Mode)

```csharp
// 동적 핸들러와 정적 핸들러를 우선순위에 따라 통합 실행
await ExecEventBus<TestExecEventArgs>.InvokeMerged(args);

// 같은 우선순위라면 동적 이벤트가 먼저 실행됨
// 예: Dynamic(5) -> Static(5) -> Dynamic(10) -> Static(10)
```

### 추가 우선순위 (Extra Priorities)

```csharp
// Primary Priority가 같을 때 추가 우선순위로 세밀한 제어
queue.Enqueue(10, ActionA, 1, 5);  // Primary: 10, Extra: [1, 5]
queue.Enqueue(10, ActionB, 2, 3);  // Primary: 10, Extra: [2, 3]

// 실행 순서: ActionA (10, 1, 5) -> ActionB (10, 2, 3)
```

### 이벤트 체인 중단 기능

```csharp
private void OnPlayerDeath(ExecQueue<PlayerDeathEventArgs> queue, PlayerDeathEventArgs args)
{
    queue.Enqueue(0, async (eventArgs) =>
    {
        // 플레이어가 이미 죽었으면 체인 중단
        if (eventArgs.Player.IsDead)
        {
            eventArgs.BreakChain = true;
            return;
        }
        
        // 죽음 처리
        eventArgs.Player.Die();
    });
    
    queue.Enqueue(1000, async (eventArgs) =>
    {
        // BreakChain이 true면 이 액션은 실행되지 않음
        await ShowGameOverUI();
    });
}
```

---

## ⭐ 특징 및 주의사항

### 장점
- **우선순위 제어**: 숫자가 낮을수록 먼저 실행되어 실행 순서를 명확하게 제어 가능
- **ExecPriority Enum**: enum을 통한 가독성 높은 우선순위 표현 지원
- **비동기 처리**: `UniTask`를 통한 효율적인 비동기 이벤트 처리
- **메모리 효율성**: 이벤트 인자 및 액션 래퍼 풀링으로 GC 압력 감소
- **자동 관리**: 새로운 이벤트 타입이 추가되면 자동으로 버스 생성
- **안전성**: 실행 중 큐 변경에 안전한 스냅샷 방식 사용
- **체인 중단**: `BreakChain` 기능으로 조건부 실행 중단 가능
- **동적/정적 분리**: 유연성과 성능을 위한 두 가지 버스 제공
- **병합 실행**: 동적/정적 핸들러를 우선순위에 따라 통합 실행 가능
- **추가 우선순위**: Primary Priority가 같을 때 세밀한 순서 제어 가능
- **이진 탐색 등록**: 정렬된 상태 유지를 통한 최적화 옵션

### 주의사항
- 이벤트 인자는 `Get()` 정적 메서드를 사용하여 풀에서 가져온 후, 사용 후 반드시 `Release()` 또는 `Dispose()`를 호출하여 풀로 반환해야 합니다. (직접 생성도 가능하지만, 풀링 사용을 권장합니다)
- 우선순위는 숫자가 낮을수록 먼저 실행됩니다 (0이 기본, Int32.MinValue가 최우선)
- `ExecuteAll` 실행 중에는 큐에 직접 액션을 추가하지 말아야 합니다. 스냅샷이 사용되므로 반영되지 않습니다
- 핸들러는 `OnEnable`/`OnDisable`에서 등록/해제하여 라이프사이클 관리가 권장됩니다
- 동적 버스는 실행 시마다 정렬하므로 빈번한 호출에는 정적 버스 사용을 권장합니다
- 정적 버스의 `RegisterBinarySearch`는 이미 정렬된 상태에서만 사용해야 합니다
- `InvokeMerged` 사용 시 동적과 정적 핸들러가 우선순위에 따라 섞여 실행됩니다
- 로컬 함수를 사용하여 액션을 정의하면 코드 가독성과 유지보수성이 향상됩니다

---

## ⚠️ 알려진 문제 (Known Issues)

현재 알려진 문제는 없습니다.

---

## ✅ 체크리스트
- [x] Namespace 규칙 확인 (`Cardevil.Events.ExecEvents`)
- [x] public 함수의 경우 주석 확인 (모든 public API에 XML 주석 작성)
- [x] 코드 예제 작성 완료
- [x] 메모리 풀링 구현 완료
- [x] Unity Editor 자동 정리 기능 구현 완료

---

## 연관 PR

#32

---

## 연관 이슈

#19

