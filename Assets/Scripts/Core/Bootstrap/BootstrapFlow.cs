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
        public static async UniTask RunAsync(Bootstrapper ctx, int totalToLoadCount, CancellationToken ct)
        {
            _totalToLoad = totalToLoadCount;
            
            await ctx.Database.InitializeAsync();
            Loaded++;
         
            // TODO: SaveLoad InitAsync/LoadAsync 만들기
            
            ctx.Game.Init();
            Loaded++;
            /*
             * TODO: 로드된 세이브 있다면 적용
             * 없다면 새 게임 진입하게
             */
            Loaded++;
            
            ctx.Sound.Init(ctx.transform);                //!!!!!!!!주의 나중에 사운드 작업할때 반드시 켜야함.
            Loaded++;

            await SceneLoader.LoadSceneAsync(Scenes.Title, LoadSceneMode.Single);
        }
    }
}