using Cardevil.DebugConsole.Commands;
using Cardevil.Events.ExecEvents;
using Cardevil.Item;
using Cardevil.Save;
using Cardevil.SceneManagement;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.SceneManagement;

namespace Cardevil.Core.Bootstrap
{
    /// <summary>
    /// 게임 부트스트랩 흐름 제어.
    /// 초기화 단계 순차 실행 및 진행률 관리.
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// 부트스트랩 진행률 변경 이벤트.
        /// 로드 완료 개수와 전체 개수 전달.
        /// </summary>
        public static event Action<int, int> ProgressChanged;
        
        private static int _totalToLoad;
        private static int _loaded;
        
        private static int Loaded
        {
            get => _loaded;
            set
            {
                _loaded = value;
                ProgressChanged?.Invoke(value, _totalToLoad);
            }
        }
        
        // TODO: ct 사용 및 전파하기
        
        /// <summary>
        /// 게임 초기화 비동기 실행.
        /// 매니저 및 시스템 초기화 후 시작 씬 로드.
        /// </summary>
        /// <param name="ctx">부트스트랩 컨텍스트</param>
        /// <param name="totalToLoadCount">전체 로드 단계 개수</param>
        /// <param name="ct">취소 토큰</param>
        public static async UniTask BootstrapAsync(CardevilCore ctx, int totalToLoadCount, CancellationToken ct)
        {
            _totalToLoad = totalToLoadCount;
            
            // TODO: 오래 걸릴 것들은 비동기로 전환하기
            
            // 1. Util
            ExecEventUtil.Initialize();
            SceneReference.InitializeCache();
            BuiltInCommands.RegisterCommands();
            ItemLibrary.Initialize();
            // TODO: Console도 할 지 고민하기

            Loaded++;
            
            // 2. Database
            await ctx.Database.InitializeAsync();
            ctx.CardEnhancementData.Init();
            Loaded++;
            
            // 3. Save data
            ctx.SaveLoad.Init();
            Loaded++;
            
            // 4. Object Pool
            ctx.Pool.Init(ctx.transform);
            Loaded++;
            
            // 5. Sound
            ctx.Sound.Init(ctx.transform, ctx.Pool);                //!!!!!!!!주의 나중에 사운드 작업할때 반드시 켜야함.
            Loaded++;
            
            // 6. Flow
            ctx.Game.Init();
            ctx.GameFlow.Init();
            Loaded++;

            if (ctx.CanSelectSaveSlot)
            {
                SceneLoader.LoadSceneAsync(Scenes.Title, LoadSceneMode.Single).Forget();    
            }
            else
            {
                // 개발용으로 세이브 로드 생략하고 항상 새 게임에 진입
                var slot = SaveSlot.DevSlot;
                ctx.SaveLoad.MakeNewSave(slot, "DevSave");
                ctx.SaveLoad.LoadGame(slot);
                await SceneLoader.LoadSceneAsync(Scenes.World, LoadSceneMode.Single);
            }
        }
    }
}