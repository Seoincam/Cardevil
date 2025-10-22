using Cardevil.Cards.Data.Modifiers;

namespace Cardevil.Cards.Data
{
    public class CardPipelineModifierService
    {
        private CardLibrary _library;
        
        public void Init(CardLibrary library)
        {
            _library = library;
        }
        
        public void Enhancement(int id, EnhancementData enhancementData)
        {
            var pipeline = _library.GetPipelineById(id);
            
            // TODO: Data를 바탕으로 실제 Modifier를 생성 및 추가
        }
    }

}