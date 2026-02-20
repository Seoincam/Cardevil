using Cardevil.NewCard.Common;
using System.Collections.Generic;

namespace Cardevil.NewCard.InStage
{
    public static class CardRegistry
    {
        private static readonly Dictionary<uint, InteractionCard> InteractionCards = new();
        private static readonly Dictionary<InteractionCard, uint> ReverseInteractionCards = new();
        
        private static readonly Dictionary<uint, HandBarCard> HandBarCards = new();
        private static readonly Dictionary<HandBarCard, uint> ReverseHandBarCards = new();

        private static uint _nextId = 1;

        public static uint Register(InteractionCard card)
        {
            var id = _nextId++;
            InteractionCards.Add(id, card);
            ReverseInteractionCards.Add(card, id);
            return id;
        }

        public static uint Register(HandBarCard card)
        {
            var id = _nextId++;
            HandBarCards.Add(id, card);
            ReverseHandBarCards.Add(card, id);
            return id;
        }

        public static void Unregister(InteractionCard card)
        {
            if (ReverseInteractionCards.Remove(card, out var id))
            {
                InteractionCards.Remove(id);
            }
        }

        public static void Unregister(HandBarCard card)
        {
            if (ReverseHandBarCards.Remove(card, out var id))
            {
                HandBarCards.Remove(id);
            }
        }

        public static InteractionCard GetInteractionCard(uint id)
        {
            return InteractionCards[id];
        }

        public static HandBarCard GetHandBarCard(uint id)
        {
            return HandBarCards[id];
        }
        
        public static uint GetId(InteractionCard card) => ReverseInteractionCards[card];
        
        public static uint GetId(HandBarCard card) => ReverseHandBarCards[card];
    }
}