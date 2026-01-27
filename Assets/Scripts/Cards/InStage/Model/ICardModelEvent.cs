namespace Cardevil.Cards.InStage.Model
{
    public interface ICardModelEvent
    {
        /// <summary>
        /// 덱에 존재하는 랜덤한 카드를 <c>count</c>만큼 잠급니다.
        /// </summary>
        void LockRandomlyInDeck(int count);
    }
}