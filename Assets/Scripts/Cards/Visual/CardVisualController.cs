using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Visual.StateMachine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Visual
{
    [RequireComponent(typeof(CardVisualBase))]
    public class CardVisualController : MonoBehaviour
    {
        private CardVisualBase _visual;
        private CardVisualPhaseStateMachine _fsm;

        public void Init(CardData data)
        {
            _visual = GetComponent<CardVisualBase>();

            var spriteSet = ConfigureSpriteSet(data);
            var phase = (VisualPhase)spriteSet.sprites.Count;
            
            _fsm ??= new CardVisualPhaseStateMachine(_visual, spriteSet);
            _fsm.InitPhase(phase, spriteSet).Forget();
        }

        public async UniTask UpdateData(CardData data)
        {
            var spriteSet = ConfigureSpriteSet(data);
            var phase = (VisualPhase)spriteSet.sprites.Count;

            await _fsm.SetPhase(phase, spriteSet);
        }

        private CardVisualSpriteSet ConfigureSpriteSet(CardData data)
        {
            if (data.Kind == CardKind.Attack)
                return UpdateAttackData(data);
            if (data.Kind == CardKind.Move)
                return UpdateMoveData(data);

            return new CardVisualSpriteSet();
        }
        
        // TODO 강화 데이터 핸들링
        private CardVisualSpriteSet UpdateAttackData(CardData data)
        {
            Sprite innerFrame = CardSpriteCache.GetInnerFrame(data.Color);
            List<Sprite> sprites = new();

            // 오망성인 경우 따로 분류
            if (data.NumberSelectState.Selectables.Count == 9)
            {
                sprites.Add(CardSpriteCache.GetStar(data.Color));
            }
            else
            {
                foreach (var item in data.NumberSelectState.Selectables)
                {
                    sprites.Add(item.hasValue
                        ? CardSpriteCache.GetNumber(data.Color, item.value)
                        : CardSpriteCache.GetQuestionMark(data.Color));
                }
            }
            
            return new CardVisualSpriteSet(innerFrame, sprites);
        }

        private CardVisualSpriteSet UpdateMoveData(CardData data)
        {
            Sprite innerFrame = CardSpriteCache.GetInnerFrame(data.DirectionFlag);
            List<Sprite> sprites = new() { CardSpriteCache.GetArrow(data.DirectionFlag) };
            
            return new CardVisualSpriteSet(innerFrame, sprites);
        }
    }
}