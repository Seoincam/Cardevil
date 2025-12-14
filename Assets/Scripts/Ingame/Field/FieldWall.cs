using Cardevil.Events;
using Cardevil.Events.ExecEvents;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Cardevil.Ingame.Field
{
    public class FieldWall : MonoBehaviour
    {
        [SerializeField] private GameObject[] _walls;
        
        private void OnEnable()
        {
            ExecEventBus<PlayerHealthChangeArgs>.RegisterStatic(10, OnPlayerHealthChanged);
        }
        
        private void OnDisable()
        {
            ExecEventBus<PlayerHealthChangeArgs>.UnregisterStatic(OnPlayerHealthChanged);
        }
        
        
        private async UniTask OnPlayerHealthChanged(PlayerHealthChangeArgs args, CancellationToken cancellationToken)
        {
            for (int i = 0; i < _walls.Length; i++)
            {
                if (_walls[i] != null)
                {
                    bool isActive = i < args.ModifiedHealth;
                    _walls[i].SetActive(isActive);
                }
            }
            await UniTask.CompletedTask;
        }

    }
}
