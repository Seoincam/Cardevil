using Cardevil.Cards.Data;
using Cardevil.Cards.Data.Enhancement;
using Cardevil.Utils;
using System.Collections.Generic;

namespace Cardevil.Cards.OutStage
{
    public class CardEnhancementPresenter
    {
        private CardLibrary _library;
        private EnhancementDataLibrary _enhancementDataLibrary;
        private CardPipelineModifierService _service;
        
        public void Init(CardLibrary library, EnhancementDataLibrary enhancementDataLibrary, CardPipelineModifierService service)
        {
            if (library == null)
            {
                LogEx.LogError("card library == null");
                return;
            }
            _library = library;

            if (enhancementDataLibrary == null)
            {
                LogEx.LogError("enhancementDataLibrary == null");
                return;
            }
            _enhancementDataLibrary = enhancementDataLibrary;

            if (service == null)
            {
                LogEx.LogError("service == null");
                return;
            }
            _service = service;
        }

        public bool TryGetPossibleEnhancements(int id, out List<EnhancementData> possibles)
        {
            possibles = new();
            
            var pipeline = _library.GetPipelineById(id);
            if (pipeline == null)
            {
                LogEx.LogError($"Pipeline을 찾을 수 없음! (id: {id})");
                return false;
            }

            if (pipeline.PossibleEnhancementIds == null || pipeline.PossibleEnhancementIds.Count == 0)
            {
                return false;
            }

            foreach (var guid in pipeline.PossibleEnhancementIds)
            {
                var data = _enhancementDataLibrary.GetData(guid);
                if (data == null)
                {
                    LogEx.LogError($"Enhancement Data를 찾을 수 없음!");
                    continue;
                }
                
                possibles.Add(data);
            }
            return true;
        }

        public void Enhance(int id, EnhancementData enhancementData)
        {
            _service.Enhance(id, enhancementData.Type, enhancementData.ModifierCount);
            // TODO: UI 갱신
        }
    }
}