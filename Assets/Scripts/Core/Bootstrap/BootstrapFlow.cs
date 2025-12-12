using Cardevil.DebugConsole.Commands;
using Cardevil.Events.ExecEvents;
using Cardevil.Item;
using Cardevil.SceneManagement;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.SceneManagement;

namespace Cardevil.Core.Bootstrap
{
    public static class BootstrapFlow
    {
        // loaded / totalToLoad
        public static event Action<int, int> ProgressChanged;
        
        private static int _totalToLoad;
        private static int _loaded;
        
        private static int Loaded
        {
            get => _loaded;
            set
            {
                _loaded = value;
                ProgressChanged?.Invoke(_totalToLoad, value);
            }
        }
        
        // TODO: ct 사용 및 전파하기
        public static async UniTask BootstrapAsync(Bootstrapper ctx, int totalToLoadCount, CancellationToken ct)
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
            Loaded++;
         
            // TODO: SaveLoad InitAsync/LoadAsync 만들기
            
            // 3. Save data
            ctx.Game.Init();
            Loaded++;
            /*
             * TODO: 로드된 세이브 있다면 적용
             * 없다면 새 게임 진입하게
             */
            Loaded++;
            
            // 4. Object Pool
            ctx.Pool.Init(ctx.transform);
            
            // 5. Sound
            ctx.Sound.Init(ctx.transform, ctx.Pool);                //!!!!!!!!주의 나중에 사운드 작업할때 반드시 켜야함.
            Loaded++;
            
            // 6. Flow
            ctx.GameFlow.Init();
            Loaded++;

            await SceneLoader.LoadSceneAsync(Scenes.Title, LoadSceneMode.Single);
        }
    }
}