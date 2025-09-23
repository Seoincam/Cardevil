using Cardevil.Systems;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards
{
    public class CardManager
    {
        public ICardHandBar handBar;
        public ITurnPlayerInput playerInput;

        private List<CardData> _runtimeBaseDeck;

        private int _maxHandCount = 6;
        private int _defaultRemainDiscardCount = 3;

        private float _redFlushEffectMultiply = 3.0f;
        private int _redFlushEffectTurnCount = 1;
        private int _blueFlushEffectCount = 3;
        private int _greenFlushEffectCount = 1;
        private float _blackFlushEffectMultiply = 2.0f;



        public IReadOnlyList<CardData> RuntimeBaseDeck => _runtimeBaseDeck;

        /// <summary>
        /// 최대 핸드 수. 현재는 6-7장 사이로 고정.
        /// </summary>
        public int MaxHandCount
        {
            get => _maxHandCount;
            set => _maxHandCount = Mathf.Clamp(_maxHandCount, 6, 7);
        }

        /// <summary>
        /// 기본 남은 버리기 수.
        /// </summary>
        public int DefalutRemainDiscardCount
        {
            get => _defaultRemainDiscardCount;
            set => _defaultRemainDiscardCount = value;
        }   

        /// <summary>
        /// 데미지 배율.
        /// </summary>
        public float RedFlushEffectMultiply
        {
            get => _redFlushEffectMultiply;
        }

        /// <summary>
        /// 효과 유지 횟수.
        /// </summary>
        public int RedFlushEffectTurnCount
        {
            get => _redFlushEffectTurnCount;
            set => RedFlushEffectTurnCount = value;
        }

        /// <summary>
        /// 버리기 횟수 n회 추가 또는 묘지→덱으로 불러오는 카드가 n장.
        /// </summary>
        public int BlueFlushEffectCount
        {
            get => _blueFlushEffectCount;
            set => _blueFlushEffectCount = value;
        }

        /// <summary>
        /// 보호막 추가량.
        /// </summary>
        public int GreenFlushEffectCount
        {
            get => _greenFlushEffectCount;
            set => _greenFlushEffectCount = value;
        }

        /// <summary>
        /// 데미지 배율.
        /// </summary>
        public float BlackFlushEffectMultiply
        {
            get => _blackFlushEffectMultiply;
            set => _blackFlushEffectMultiply = value;
        }



        public void Init()
        {
            var handBarObj = GameObject.Find("CardHandBar");
            if (handBarObj == null) Debug.LogError("CardHandBar이 씬 내 존재하지 않습니다.");
            handBar = handBarObj.GetComponent<ICardHandBar>();
            playerInput = handBarObj.GetComponent<ITurnPlayerInput>();
            if (playerInput == null) Debug.LogError("playerinput이 없습니다.");

            _runtimeBaseDeck = DeckFactory.CreateRuntimeBaseDeck();
            handBar.Init();
        }



        public ILockable GetCard()
        {
            return handBar.StageCardsCtx.GetRandomCard();
        }

        public int GetCurrentCardRankScore()
        {
            if (handBar.Context.CurrentResult is not { } result)
            {
                Debug.LogError("설정 안 된 CardResult에 접근.");
                return 0;
            }

            HandRanking rank = result.Ranking;

            // enum → int 변환
            int score = (int)rank;

            return score;
        }
    }
}


