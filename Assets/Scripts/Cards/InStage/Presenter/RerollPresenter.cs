using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Core;
using Cardevil.Systems;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.View;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Core.Bootstrap;
using Object = UnityEngine.Object;

namespace Cardevil.Cards.InStage.Presenter
{
    public class RerollPresenter : ITurnRerollInput, IClearable
    {
        private IReadOnlyCardLibrary _library;
        
        private StageCardsModel _model;
        private RerollView _view;
        private CardVisualSettingSO _visualSetting;
        
        private int _maxHand;
        
        private bool _isPreviewing;
        private UniTaskCompletionSource _cmp;
        private bool _isInitialized;
        
        /// <summary>
        /// RerollPresenter를 초기화.  
        /// model 참조를 저장, 카드 시각 효과 설정용 So를 로드.  
        /// 이미 초기화된 경우 중복 실행을 방지.
        /// </summary>
        public void Init(IReadOnlyCardLibrary library, StageCardsModel model)
        {
            if (_isInitialized) return;

            if (library == null)
            {
                LogEx.LogError("library is null");
                return;
            }
            _library = library;
            
            if (model == null)
            {
                LogEx.LogError("model is null");
                return;
            }
            _model = model;
            
            // SO 로드
            string path = "ScriptableObjects/Cards/CardVisualSetting";
            _visualSetting = Resources.Load<CardVisualSettingSO>(path);
            if (!_visualSetting)
            {
                LogEx.LogError($"CardVisualSettingSO 로드 실패. 경로가 올바른지 확인하세요: {path}");
                return;
            }
            
            _isInitialized = true;
        }

        /// <summary>
        /// 리롤 UI를 구성, 초기 상태를 설정.  
        /// RerollView를 씬에서 찾거나 존재하지 않을 경우 새로 생성,  
        /// 슬롯 수를 지정하고 버튼 이벤트를 바인딩.  
        /// </summary>
        /// <param name="maxHand">한 번에 표시할 최대 손패(카드) 개수.</param>
        /// <returns>UI 초기화 및 등장 애니메이션 완료 후 완료되는 <see cref="UniTask"/>.</returns>
        public async UniTask SetUp(int maxHand)
        {
            _maxHand = maxHand;
            _cmp = new UniTaskCompletionSource();
            
            // Model
            _model.SetUp(maxHand, 3);

            for (int i = 0; i < _library.Count; i++)
            {
                var data = _library.GetCardDataById(i);
                if (data == null)
                {
                    LogEx.LogError("data is null");
                    continue;
                }
                
                _model.AddDataInDeck(data);
            }
            
            _model.Shuffle();
            
            // View
            var views = Object.FindObjectsByType<RerollView>(FindObjectsSortMode.None);
            if (views is { Length: > 0} ) _view = views[0];
            else
            {
                Transform canvas = GameObject.Find("CardCanvas").transform;
                GameObject go = Managers.Resource.Instantiate("UI/CardUI/RerollView", canvas);
                _view = go.GetComponent<RerollView>();
            }
            
            _view.Init(_visualSetting);
            _view.ConfigureSlots(maxHand);
            _view.BindButtonEvents(DoReroll, EndReroll);
            
            Bootstrapper.Instance.Game.PlayerStatus.RerollTicket = 5; // 임시
            await _view.EnterRerollAsync();
        }
        
        /// <summary>
        /// 리롤 단계를 종료, 관련 UI를 비활성화.  
        /// 리롤 종료 애니메이션이 완료될 때까지 대기.
        /// </summary>
        /// <returns>리롤 종료 애니메이션이 완료되면 완료되는 <see cref="UniTask"/>.</returns>
        public async UniTask Exit()
        {
            await _view.ExitRerollAsync();
        }
        
        /// <summary>
        /// RerollPresenter의 상태를 초기화,  
        /// View와 관련 리소스를 정리.  
        /// UI 객체를 파괴하여 메모리를 반환.
        /// <para> TODO: View object를 풀링 해야할까 고민 </para>
        /// </summary>
        public void Clear()
        {
            _cmp = null;
            
            if (!_view) return;
            _view.Clear();
            Managers.Resource.Destroy(_view.gameObject);
        }
        
        // ITurnRerollInput
        public async UniTask Reroll()
        {
            _ = RerollAsync();
            await _cmp.Task;
        }
        
        private void DoReroll()
        {
            var old = Bootstrapper.Instance.Game.PlayerStatus.RerollTicket;
            Bootstrapper.Instance.Game.PlayerStatus.RerollTicket--;
            _ = _view.AnimateTicketChangeAsync(old, Bootstrapper.Instance.Game.PlayerStatus.RerollTicket);
            _ = RerollAsync();
        }

        private void EndReroll()
        {
            _cmp.TrySetResult();
        }

        private async UniTask RerollAsync()
        {
            var draw = _visualSetting.RerollDrawInterval;
            var discard = _visualSetting.RerollDiscardInterval;
            var drawDiscard = _visualSetting.RerollDrawDiscardInterval;
            var autoEnd = _visualSetting.EndRerollInterval;
            var end = .5f;
            
            _view.SetInteractable(false);

            try
            {
                // 버리기 Tween
                foreach (var card in _model.Hand)
                {
                    card.DoRerollDiscard();
                    await UniTask.Delay(TimeSpan.FromSeconds(discard));
                }

                // 대기 후 셔플
                await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.RerollDrawDiscardInterval));
                _model.Shuffle();

                // 카드 소환
                for (int i = 0; i < _maxHand; i++)
                {
                    var card = Spawn();
                    card.DoRerollDraw();
                    await UniTask.Delay(TimeSpan.FromSeconds(draw));
                }

                // 리롤권 소진시 자동 종료
                if (Bootstrapper.Instance.Game.PlayerStatus.RerollTicket <= 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(autoEnd));
                    EndReroll();
                    return;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(end));
                _view.SetInteractable(true);
            }
            catch (Exception ex)
            {
                LogEx.LogError($"리롤 중 오류!: {ex}");
            }
        }
        
        private Card Spawn()
        {
            var cardData = _model.PopCard();
            if (cardData == null) return null;
            
            var card = Managers.Resource.Instantiate("Cards/Card").GetComponent<Card>();
            card.Init(cardData, _model);
            card.SetRerollState(true);

            _model.Draw(card);
            if (!_model.TryGetIndex(card, out var idx)) return null;
            _view.SetCardToSlot(card, idx);

            return card;
        }
    }
}