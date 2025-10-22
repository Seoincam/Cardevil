using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Data.Modifiers;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Core;
using System.Collections.Generic;
using System.Linq;
using Cardevil.Relics;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;

namespace Cardevil.Cards.Evaluations
{
    /// <summary>
    /// 스테이지에서 사용된 카드 평가를 단계적으로 구성,
    /// UI 연출(<see cref="AsyncEvaluationEvent"/>)과 모델(<see cref="EvaluationResultsModel"/>)에 반영하는 Builder.
    /// </summary>
    public class EvaluationArgsBuilder : IClearable
    {
        private EvaluationResultsModel _model;
        private AsyncEvaluationEvent _event;
        
        private EvaluationResult _pending;
        
        /// <summary>
        /// 결과를 기록할 <see cref="EvaluationResultsModel"/>을 주입,
        /// 평가 연출을 담당할 <see cref="AsyncEvaluationEvent"/>를 준비.
        /// </summary>
        /// <param name="model">평가 결과 이력을 저장할 모델 인스턴스</param>
        /// <remarks>
        /// 이 메서드는 반드시 한 번 이상 호출되어야 함.
        /// </remarks>

        public void Init(EvaluationResultsModel model)
        {
            if (model == null)
            {
                LogEx.LogError("Init() 실패 - Stage Evaluation Results Model이 null입니다.");
                return;
            }
            _model = model;

            _event = new AsyncEvaluationEvent(this);
        }
        
        public void Clear()
        {
            _pending = null;
        }

        public void SetDamage(int damage)
        {
            if (_pending == null)
            {
                LogEx.LogError("설정된 pending이 없음!");
                return;
            }
            
            _pending.SetDamage(damage);
            _model.Add(_pending);
            _pending = null;
        }
        
        
        /// <summary>
        /// 주어진 카드 목록을 기반으로 평가 단계 인자를 구성.  
        /// (예: 데미지 합산, 이동 결과, 족보 판정 등)  
        /// 각 단계는 <see cref="AsyncEvaluationEvent"/>에 등록되어
        /// 순차적인 UI 연출과 함께 적용.
        /// </summary>
        /// <param name="cards">평가 대상 카드 목록</param>
        /// <remarks>
        /// 내부적으로 <c>_pending</c> 결과를 생성/갱신하고,  
        /// 단계별 인자(효과/값/우선순위)를 우선순위 <c>SortedList</c>에 쌓음.
        /// 실제 모델 반영은 <see cref="SetDamage(int)"/> 호출 시 수행.
        /// </remarks>
        // TODO: event 우선 순위 설정 로직 추가해야함
        public void BuildEvaluationArgs(IReadOnlyList<Card> cards)
        {
            EvaluationArg arg;
            
            List<Card> attackCards = cards.Where(c => c.Data.Kind == CardKind.Attack).ToList();
            List<Card> moveCards = cards.Where(c => c.Data.Kind == CardKind.Move).ToList();
            List<CardData> moves = moveCards.Select(c => c.Data).ToList();
            
            // Move Only
            if (attackCards.Count == 0 && moveCards.Count > 0)
            {
                using (arg = EvaluationArg.Get())
                {
                    arg.SetEvent(_event);
                    arg.SetValue(0, EffectEvaluation.None);
                    arg.SetVisual(moveCards);
                }

                _pending = new EvaluationResult(moves);
                return;
            }
            
            // 족보 계산
            HandRanking handRanking = GetPrimaryHandRanking(attackCards, out List<Card> cardsInHandRanking);
            _pending = new EvaluationResult(moves, handRanking);
            
            // 기본 족보 보너스
            if (handRanking > HandRanking.High)
            {
                using (arg = EvaluationArg.Get())
                {
                    var data = Managers.Database.Database.HandRankingDataList
                        .FirstOrDefault(d => d.Ranking == handRanking);

                    if (data == null)
                    {
                        LogEx.LogError($"Database에 족보 데이터가 존재하지 않음! : {handRanking}");
                        return;
                    }
                    
                    arg.SetEvent(_event);
                    arg.SetValue(0, EffectEvaluation.Plus, data.Value);
                    arg.SetVisual(cardsInHandRanking); // 족보에 포함되는 카드들만 추가함
                }
            }

            // 기본 데미지
            // 4장의 카드를 모두 쓰지 않는 경우를 따로 계산
            if (handRanking == HandRanking.High)
            {
                var top = attackCards.Aggregate((best, cur) =>
                    cur.Data.NumberSelectState.FinalValue > best.Data.NumberSelectState.FinalValue ? cur : best);
                using (arg = EvaluationArg.Get())
                {
                    arg.SetEvent(_event);
                    arg.SetValue(0, EffectEvaluation.Plus, (float)top.Data.NumberSelectState.FinalValue);
                    arg.SetVisual(top);
                }
            }
            else if (handRanking != HandRanking.None)
            {
                foreach (var card in cardsInHandRanking)
                {
                    using (arg = EvaluationArg.Get())
                    {
                        arg.SetEvent(_event);
                        arg.SetValue(0, EffectEvaluation.Plus, (float)card.Data.NumberSelectState.FinalValue);
                        arg.SetVisual(card);
                    }
                }    
            }
            

            // TODO: 추가 데미지

            /*
            // 유물
            var relics = Managers.Relic;
            if (relics == null)
            {
                Debug.LogWarning("[EvaluateResult] RelicDataManager.Instance is null");
                return;
            }

            var effects = relics.GetPlayerEffect(EffectType.OnEvaluation)
                .Where(e => e.EffectType == EffectType.OnEvaluation)
                .ToList();

            int Priority(EffectEvaluation type) => type switch
            {
                EffectEvaluation.MultiplyRanking => 0,
                EffectEvaluation.Plus => 1,
                EffectEvaluation.MultiplyAll => 2,
                _ => 99
            };

            // TODO: 순서 더 정확히
            var pr = 300;
            foreach (var e in effects.OrderBy(e => Priority(e.OnEvaluationValues.EvaluationType)))
            {
                if (e.CanTriggerOnEvaluation(primaryRanking))
                {
                    var data = e.OnEvaluationValues;
                    using (var r = EvaluationAction.Get())
                    {
                        r.SetValue(priority: pr++, data.EvaluationType, data.EffectValue);
                        // r.SetVisual();
                    }
                }
            }

            return;
            */
        }

        public async UniTask InvokeAsync()
        {
            await _event.InvokeAsync();
        }

        public void UpdateHandRankingVisual(IEnumerable<Card> cards = null)
        {
            var handRanking = cards == null || cards.Count() == 0 ? HandRanking.None : GetPrimaryHandRanking(cards, out var _);
            _event.Animator.UpdateHandRankingText(handRanking);
        } 
        
        public static HandRanking GetPrimaryHandRanking(IEnumerable<Card> cards, out List<Card> cardsInHandRanking)
        {
            cardsInHandRanking = new List<Card>();
            
            var numberCards = cards.Where(c => c.Data.Kind == CardKind.Attack)
                .ToList();
            
            if (numberCards.Count == 0)
                return HandRanking.None;

            if (IsStraightFlush(numberCards, out cardsInHandRanking)) 
                return HandRanking.StraightFlush;
            
            if (IsFourCard(numberCards, out cardsInHandRanking))
                return HandRanking.FourCard;
            
            if (IsStraight(numberCards, out cardsInHandRanking))
                return HandRanking.Straight;

            if (IsFlush(numberCards, out cardsInHandRanking))
            {
                var handRanking = numberCards[0].Data.Color switch
                {
                    CardColor.Red => HandRanking.RedFlush,
                    CardColor.Green => HandRanking.GreenFlush,
                    CardColor.Blue => HandRanking.BlueFlush,
                    CardColor.Black => HandRanking.BlackFlush,
                    _ => HandRanking.None
                };
                
                if (handRanking == HandRanking.None)
                    LogEx.LogError("Flush 분류에 실패했습니다.");

                return handRanking;
            }

            if (IsTriple(numberCards, out cardsInHandRanking))
                return HandRanking.Triple;
            
            if (IsTwoPair(numberCards, out cardsInHandRanking))
                return HandRanking.TwoPair;
            
            if (IsOnePair(numberCards, out cardsInHandRanking))
                return HandRanking.OnePair;

            return HandRanking.High;
        }
        
        #region HandRanking Helper

        /// <summary>
        /// 숫자 카드를 바탕으로 '모든' 족보를 반환. 
        /// </summary>
        private static List<HandRanking> CalculateRanking(List<Card> numberCards)
        {
            var rankings = new List<HandRanking>();

            /*
            var numberDatas = numberCards.Where(c => c.Data.Kind == CardKind.Number)
                .Select(c => c.Data)
                .ToList();

            if (IsStraightFlush(numberDatas))
                rankings.Add(HandRanking.StraightFlush);
            if (IsFourCard(numberDatas))
                rankings.Add(HandRanking.FourCard);
            if (IsStraight(numberDatas))
                rankings.Add(HandRanking.Straight);
            if (IsFlush(numberDatas))
            {
                var ranking = numberDatas[0].Number.Color switch
                {
                    CardColor.Red => HandRanking.RedFlush,
                    CardColor.Green => HandRanking.GreenFlush,
                    CardColor.Blue => HandRanking.BlueFlush,
                    CardColor.Black => HandRanking.BlackFlush,
                    _ => HandRanking.None
                };
                if (ranking == HandRanking.None)
                    Debug.LogError("Flush 분류에 실패했습니다.");

                rankings.Add(ranking);
            }
            if (IsTriple(numberDatas))
                rankings.Add(HandRanking.Triple);
            if (IsTwoPair(numberDatas))
                rankings.Add(HandRanking.TwoPair);
            if (IsOnePair(numberDatas))
                rankings.Add(HandRanking.OnePair);
            if (rankings.Count() == 0)
                rankings.Add(HandRanking.High);
            */
            
            return rankings;
        }

        private static bool IsStraight(List<Card> numberCards, out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();
            
            if (numberCards.Count != 4) 
                return false;

            var values = numberCards.Select(c => c.Data.NumberSelectState.FinalValue)
                    .OrderBy(v => v)
                    .ToList();

            for (int i = 1; i < numberCards.Count; i++)
                if (values[i] != values[i - 1] + 1)
                    return false;
        
            cardsInRanking = numberCards.ToList();
            return true;
        }

        private static bool IsFlush(List<Card> numberCards, out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();
            
            if (numberCards.Count != 4) 
                return false;
            
            bool value = numberCards.Select(c => c.Data.NumberSelectState.FinalValue)
                    .Distinct()
                    .Count() == 1;
            
            if (value) cardsInRanking = numberCards.ToList();
            return value;
        }

        private static bool IsStraightFlush(List<Card> numberCards,  out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();
            
            if (numberCards.Count != 4) 
                return false;

            var value = IsStraight(numberCards, out var _) && IsFlush(numberCards, out var _);

            if (value) cardsInRanking = numberCards.ToList();
            return value;
        }

        private static bool IsFourCard(List<Card> numberCards, out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();

            if (numberCards.Count < 4)
                return false;

            // 같은 숫자 값으로 그룹화
            var group = numberCards
                .GroupBy(c => c.Data.NumberSelectState.FinalValue)
                .FirstOrDefault(g => g.Count() == 4);

            if (group != null)
            {
                cardsInRanking = group.ToList();
                return true;
            }

            return false;
        }

        private static bool IsTriple(List<Card> numberCards, out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();

            if (numberCards.Count < 3)
                return false;

            // 같은 숫자 값으로 그룹화
            var group = numberCards
                .GroupBy(c => c.Data.NumberSelectState.FinalValue)
                .FirstOrDefault(g => g.Count() == 3);

            if (group != null)
            {
                cardsInRanking = group.ToList();
                return true;
            }

            return false;
        }

        private static bool IsTwoPair(List<Card> numberCards, out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();
            
            if (numberCards.Count != 4)
                return false;

            var groupCount = numberCards.GroupBy(c => c.Data.NumberSelectState.FinalValue)
                .Count(g => g.Count() == 2);

            if (groupCount == 2)
            {
                cardsInRanking = numberCards.ToList();
                return true;
            }

            return false;
        }

        private static bool IsOnePair(List<Card> numberCards, out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();
            if (numberCards.Count < 2)
                return false;

            var groupCount = numberCards.GroupBy(c => c.Data.NumberSelectState.FinalValue)
                .Count(g => g.Count() == 2);

            if (groupCount == 1)
            {
                cardsInRanking = numberCards.ToList();
                return true;
            }

            return false;
        }


        #endregion
    }
}