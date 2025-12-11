using Cardevil.Manager;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Cardevil.Core.Bootstrap
{
    public class GameManagerInstaller : MonoBehaviour, IManager
    {
        [SerializeField] private GameManager manager;

        public GameManager Game
        {
            get => manager;
            private set => manager = value;
        }
        
        public async UniTask InitializeAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
            manager = new GameManager();
        }
    }
}