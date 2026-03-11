using System;
using System.Collections.Generic;
using UnityEngine; 
using Cardevil.InGame.Enemy; // IGimmick, IUpdatableGimmick 등이 있는 네임스페이스
using System.Reflection;
using System.Linq;

public class GimmickFactory
{

    private static GimmickFactory _instance;
    public static GimmickFactory Instance
    {
        get
        {
            // 인스턴스가 아직 생성되지 않았다면
            if (_instance == null)
            {
                // 새 인스턴스를 생성하고 즉시 초기화(등록)
                _instance = new GimmickFactory();
                _instance.Initialize();
            }
            return _instance;
        }
    }

  
    // Key: JSON에 저장된 기믹 이름 (string)
    // Value: 해당 기믹 클래스의 '새 인스턴스를 생성하는 함수' (Func<IGimmick>)
    private Dictionary<string, Func<IGimmick>> gimmickRegistry = new Dictionary<string, Func<IGimmick>>();


    // 외부에서 'new GimmickFactory()'를 호출하지 못하도록 private으로 설정
    private GimmickFactory() { }

    // 게임 시작 시(또는 첫 접근 시) 모든 기믹을 등록합니다.
    private void Initialize()
    {
        Debug.Log("GimmickFactory 초기화 (어셈블리 스캔 자동 등록)...");

        // 1. IGimmick 인터페이스가 정의된 어셈블리(C# 프로젝트)를 가져옵니다.
        Assembly assembly = typeof(IGimmick).Assembly;

        // 2. IGimmick 인터페이스 자체의 Type을 가져옵니다.
        Type gimmickInterfaceType = typeof(IGimmick);

        // 3. (핵심) 어셈블리 내의 모든 'Type'들을 스캔합니다.
        var gimmickTypes = assembly.GetTypes()
            .Where(type =>
                // 조건 1: IGimmick 인터페이스를 '구현'해야 함 (Gimmick_Test가 해당됨)
                gimmickInterfaceType.IsAssignableFrom(type) &&
                // 조건 2: 인터페이스(IGimmick, IUpdatableGimmick) 자체는 등록에서 제외
                !type.IsInterface &&
                // 조건 3: 추상 클래스(abstract) 제외
                !type.IsAbstract
            );

        // 4. 찾은 모든 기믹 C# 클래스 (Gimmick_Test 등)를 순회합니다.
        foreach (Type gimmickType in gimmickTypes)
        {
            // 5. 클래스의 '이름'을 키로 사용합니다. (예: "Gimmick_Test")
            string gimmickName = gimmickType.Name;

            // 6. 팩토리에 등록합니다.
            if (!gimmickRegistry.ContainsKey(gimmickName))
            {
                Register(gimmickName, () => (IGimmick)Activator.CreateInstance(gimmickType));
                // Debug.Log($"기믹 등록: {gimmickName}"); // 확인용 로그
            }
        }

        Debug.Log($"총 {gimmickRegistry.Count}개의 기믹이 (자동 스캔) 등록되었습니다.");
    }

    public void Register(string gimmickName, Func<IGimmick> creationFunction)
    {
        gimmickRegistry[gimmickName] = creationFunction;
    }


    //  기믹 생성 함수 (MonsterSpawner가 호출하는 함수) ---
    /// <summary>
    /// JSON에서 읽어온 기믹 이름(string)으로
    /// 실제 기믹 C# 클래스 인스턴스(IGimmick)를 생성하여 반환합니다.
    /// </summary>
    /// <param name="gimmickName">JSON에 정의된 기믹 이름</param>
    /// <returns>새로 생성된 IGimmick 인스턴스</returns>
    public IGimmick CreateGimmick(string gimmickName)
    {
        // 1. 등록부에서 'gimmickName' 키로 '생성 함수'를 찾습니다.
        if (gimmickRegistry.TryGetValue(gimmickName, out Func<IGimmick> createFunc))
        {
            // 2. 함수를 찾았다면, 해당 함수를 실행(invoke)하여 '새 인스턴스'를 생성합니다.
            //    예: () => new HealthThresholdRankUpgradeGimmick() 함수가 실행됨
            return createFunc();
        }

        // 3. 팩토리에 등록되지 않은 기믹 이름이 JSON에 들어온 경우 (오타 등)
        Debug.LogWarning($"[GimmickFactory] 경고: '{gimmickName}'(이)라는 이름의 기믹이 등록부에 없습니다. JSON 오타이거나 등록이 누락되었습니다.");
        return null;
    }
}