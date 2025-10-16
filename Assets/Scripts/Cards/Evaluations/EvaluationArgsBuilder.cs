using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
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
    public class EvaluationArgsBuilder : IClearable
    {
        private EvaluationResultsModel _model;
        private AsyncEvaluationEvent _event;
        
        private EvaluationResult _pending;
        
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
        
        // TODO: 현재 args 자동 등록 안됨!
        public void BuildEvaluationArgs(IReadOnlyList<Card> cards)
        {
            EvaluationArg arg;
            
            List<Card> numberCards = cards.Where(c => c.Data.Kind == CardKind.Number).ToList();
            List<Card> moveCards = cards.Where(c => c.Data.Kind == CardKind.Move).ToList();
            List<BuiltMoveData> moves = moveCards.Select(c => c.Data.Move).ToList();
            
            // Move Only
            if (numberCards.Count == 0 && moveCards.Count > 0)
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
            HandRanking handRanking = GetPrimaryHandRanking(numberCards, out List<Card> cardsInHandRanking);
            _pending = new EvaluationResult(moves);
            
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
                var top = numberCards.Aggregate((best, cur) =>
                    cur.Data.Number.SelectState.FinalValue > best.Data.Number.SelectState.FinalValue ? cur : best);
                using (arg = EvaluationArg.Get())
                {
                    arg.SetEvent(_event);
                    arg.SetValue(0, EffectEvaluation.Plus, (float)top.Data.Number.SelectState.FinalValue);
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
                        arg.SetValue(0, EffectEvaluation.Plus, (float)card.Data.Number.SelectState.FinalValue);
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

        public void UpdateHandRankingVisual(IEnumerable<Card> cards)
        {
            var handRanking = GetPrimaryHandRanking(cards, out var _);
            _event.Animator.UpdateHandRankingText(handRanking);
        } 
        
        public static HandRanking GetPrimaryHandRanking(IEnumerable<Card> cards, out List<Card> cardsInHandRanking)
        {
            cardsInHandRanking = new List<Card>();
            
            var numberCards = cards.Where(c => c.Data.Kind == CardKind.Number)
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
                var handRanking = numberCards[0].Data.Number.Color switch
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

            return HandRanking.None;
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

            var values = numberCards.Select(c => c.Data.Number.SelectState.FinalValue)
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
            
            bool value = numberCards.Select(c => c.Data.Number.SelectState.FinalValue)
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
                .GroupBy(c => c.Data.Number.SelectState.FinalValue)
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
                .GroupBy(c => c.Data.Number.SelectState.FinalValue)
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

            var groupCount = numberCards.GroupBy(c => c.Data.Number.SelectState.FinalValue)
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

            var groupCount = numberCards.GroupBy(c => c.Data.Number.SelectState.FinalValue)
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