namespace Cardevil.Card.Common.Core
{
    public enum CardType : byte
    {
        None,
        Attack, 
        Move    
    }
    
    public enum CardColor : byte
    {
        None,
        
        Red,
        Green,
        Blue,
        Black
    }

    public enum HandRank : byte
    {
        None,
        
        HighCard,
        OnePair,
        TwoPair,
        Triple,
        Straight,
        Flush,
        TwoPairFlush,
        FourCard,
        StraightFlush,
        FourCardFlush,
    }
}