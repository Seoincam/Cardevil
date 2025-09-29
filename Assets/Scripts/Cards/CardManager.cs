using Cardevil.Systems;
using System.Collections.Generic;
using UnityEngine;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using System.Linq;
using Cardevil.Cards.Interactions;

namespace Cardevil.Cards
{
    public class CardManager : IClearable
    {
        private ITurnPlayerInput _playerInput;

        private ITurnRerollInput _rerollInput;
        private GameObject _rerollGO;

        private CardHandBar _handBar;
        private List<CardData> _runtimeBaseDeck;
        private StageCardsContext _stageCardsCtx = new();
        private CardResultContext _resultCtx = new();
        private AsyncEvaluationEvent _evaluations = new();
        private int _maxHandCount = 6;



        public ITurnPlayerInput PlayerInput
        {
            get
            {
                _playerInput ??= GetPlayerInput();
                if (_playerInput == null)
                {
                    Debug.LogError("PlayerInput을 찾을 수 없습니다.");
                    return null;
                }
                return _playerInput;
            }
        }

        public ITurnRerollInput RerollInput
        {
            get => _rerollInput ??= CreateRerollInput();
        }

        public IReadOnlyList<CardData> RuntimeBaseDeck
        {
            get => _runtimeBaseDeck;
        }


        public CardHandBar HandBar
        {
            get
            {
                if (_handBar == null)
                    _handBar = FindHandBar();

                return _handBar;
            }
        }

        public StageCardsContext StageCardsCtx
        {
            get => _stageCardsCtx;
        }

        public CardResultContext ResultCtx
        {
            get => _resultCtx;
        }

        public AsyncEvaluationEvent Evaluations
        {
            get => _evaluations;
        }

        public int MaxHandCount
        {
            get => _maxHandCount;
        }



        public void Init()
        {
            HandBar.Init(this, _stageCardsCtx);          
            _runtimeBaseDeck = DeckFactory.CreateRuntimeBaseDeck();
        }

        public void Clear()
        {
            _handBar.Clear();
            _resultCtx.Clear();
            _stageCardsCtx.Clear();
        }

        public void OnEnterStage()
        {
            Clear();
        }

        private ITurnRerollInput CreateRerollInput()
        {
            var prefab = Resources.Load<GameObject>("Prefabs/UI/PopUp/RerollHandler");
            if (prefab == null)
            {
                Debug.LogError("Resources에서 RerollHandler 프리팹을 찾을 수 없습니다.");
                return null;
            }

            var parent = GameObject.Find("CardCanvas")?.transform;
            if (parent == null)
            {
                Debug.LogError("CardCanvas를 찾을 수 없습니다.");
                return null;
            }

            _rerollGO = GameObject.Instantiate(prefab, parent);
            if (!_rerollGO.TryGetComponent<RerollHandler>(out var handler))
            {
                Debug.LogError("RerollHandler 컴포넌트를 찾을 수 없습니다.");
                GameObject.Destroy(_rerollGO);
                _rerollGO = null;
                return null;
            }

            handler.Init();

            _rerollInput = handler;
            if (_rerollInput == null)
            {
                Debug.LogError("RerollHandler가 ITurnRerollInput을 구현하지 않습니다.");
                GameObject.Destroy(_rerollGO);
                _rerollGO = null;
                return null;
            }

            return _rerollInput;
        }

        private CardHandBar FindHandBar()
        {
            var handBarObj = GameObject.Find("CardHandBar");
            if (handBarObj == null) Debug.LogError("CardHandBar이 씬 내 존재하지 않습니다.");
            _handBar = handBarObj.GetComponent<CardHandBar>();

            return _handBar;
        }

        private ITurnPlayerInput GetPlayerInput()
        {
            return HandBar.GetComponent<ITurnPlayerInput>();
        }


        public ILockable GetCard()
        {
            return StageCardsCtx.GetRandomCard();
        }

        public int GetCurrentCardRankScore()
        {
            var result = _resultCtx.CurrentResult;
            if (result == null)
            {
                Debug.LogError("잘못된 시점에 족보에 접근.");
                result = _resultCtx.PreviousResult;
            }
            HandRanking rank = result.Ranking;

            var data = Managers.Database.Database.HandRankingDataList
                .FirstOrDefault(r => r.Ranking == rank);

            int score = data?.Value ?? 0;
            return score;
        }
    }
}


