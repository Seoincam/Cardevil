using Cardevil.Systems;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Cardevil.Cards.CardInteractinos
{
    public class PlayerInputHandler : IPlayerInputHandler
    {
        public event Action<CardContext> OnPlayerInputReceived;

        private UniTaskCompletionSource<CardContext> inputTcs;


        public PlayerInputHandler()
        {
            SubscribePlayerInput();
            Managers.Card.OnCardUsed += OnCardUsed;
        }


        public async UniTask HandlePlayerInputAsync()
        {
            Managers.Turn.SetGameState(GameManager.GameState.PlayerInput);

            inputTcs = new();
            var result = await inputTcs.Task;
            inputTcs = null;

            OnPlayerInputReceived?.Invoke(result);
        }

        public void SubscribePlayerInput()
        {
            Managers.Turn.PlayerInputAsync += HandlePlayerInputAsync;
        }

        public void OnCardUsed(CardContext result)
        {
            inputTcs?.TrySetResult(result);
        }
    }
}
