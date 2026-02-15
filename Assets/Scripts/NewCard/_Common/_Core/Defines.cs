namespace Cardevil.NewCard.Common.Core
{
    public enum CardType : byte
    {
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
    
    /*
     * 저장되어야할 것:
     * 족보, 족보에 포함된 카드, 데미지, 
     */
}