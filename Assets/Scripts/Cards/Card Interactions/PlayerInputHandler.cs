using Cardevil.Systems;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Cardevil.Cards.CardInteractinos
{
    public class PlayerInputHandler : MonoBehaviour, IPlayerInputHandler
    {
        public event Action<CardResult> OnPlayerInputReceived;

        private UniTaskCompletionSource<CardResult> inputTcs;


        void Start()
        {
            TurnManager.Instance.playerInputHandler = this;
            SubscribePlayerInput();

            var cardManager = FindAnyObjectByType<CardManager>();
            cardManager.OnUseCard += OnCardUsed;
        }


        public async UniTask HandlePlayerInputAsync()
        {
            TurnManager.Instance.SetGameState(GameState.PlayerInput);

            inputTcs = new();
            var result = await inputTcs.Task;
            inputTcs = null;

            OnPlayerInputReceived?.Invoke(result);
        }

        public void SubscribePlayerInput()
        {
            TurnManager.Instance.PlayerInputAsync += HandlePlayerInputAsync;
        }

        public void OnCardUsed(CardResult result)
        {
            inputTcs?.TrySetResult(result);
        }
    }
}
