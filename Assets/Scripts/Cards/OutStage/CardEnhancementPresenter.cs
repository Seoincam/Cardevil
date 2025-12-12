using Cardevil.Cards.Data;
using Cardevil.Cards.Data.Enhancement;
using Cardevil.DebugConsole;
using Cardevil.Utils;
using System;
using System.Collections.Generic;

namespace Cardevil.Cards.OutStage
{
    public class CardEnhancementPresenter
    {
        private IReadOnlyCardLibrary _library;
        private EnhancementDataLibrary _enhancementDataLibrary;
        private CardPipelineModifierService _service;
        
        public void Init(IReadOnlyCardLibrary library, EnhancementDataLibrary enhancementDataLibrary, CardPipelineModifierService service)
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

        private bool TryGetPossibleEnhancements(int id, out List<EnhancementData> possibles)
        {
            possibles = new();
            
            var pipeline = _library.GetReadOnlyPipelineById(id);
            if (pipeline == null)
            {
                LogEx.LogError($"Pipeline을 찾을 수 없음! (id: {id})");
                return false;
            }

            if (pipeline.NextEnhancementIds == null || pipeline.NextEnhancementIds.Count == 0)
            {
                return false;
            }

            foreach (var guid in pipeline.NextEnhancementIds)
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

        private void Enhance(int id, EnhancementData enhancementData)
        {
            var type = enhancementData.Type;
            var pipeline = _library.GetReadOnlyPipelineById(id);
            if (pipeline == null)
            {
                LogEx.LogError($"id({id})에 해당하는 Pipeline이 존재하지 않음.");
                return;
            }
            
            // 필요한 Modifier 개수 계산
            int remaining = enhancementData.ModifierCount;
            foreach (var modifier in pipeline.Modifiers)
            {
                if (modifier.Type == type)
                    remaining--;
            }

            if (remaining <= 0)
            {
                LogEx.LogError($"강화 가능한 {type} Modifier 수량이 부족함. (id:{id})");
                return;
            }
            
            Guid nextId = _enhancementDataLibrary.GetNextId(enhancementData);
            _service.Enhance(id, enhancementData.Id, enhancementData.Type, remaining, nextId);
            
            // TODO: UI 갱신
        }

        #region Command

        /*
        [ConsoleCommand("getPossibleEnhancements", "Log all possible enhancements.", "getPossibleEnhancements [int: ID (0~49])")]
        public static void GetPossibleEnhancementsCommand(string[] args)
        {
            int id;
            if (args.Length == 0)
            {
                DebugConsole.Console.MessageError("This command needs ID. Pls try one more with ID.");
                return;
            }
            if (!int.TryParse(args[0], out id))
            {
                DebugConsole.Console.MessageWarning("Invalid ID. Pls try one more with valid ID.");
                return;
            }
            if (id < 0 || id > 49)
            {
                DebugConsole.Console.MessageWarning("ID should be between 0 and 49.");
                return;
            }
            
            bool canEnhance = Managers.Card.EnhancementPresenter.TryGetPossibleEnhancements(id, out List<EnhancementData> possibles);

            if (!canEnhance)
            {
                DebugConsole.Console.Message("강화가 불가능합니다.");
                return;
            }
            
            DebugConsole.Console.Message($"강화가 가능합니다! 가능 개수: {possibles.Count}");
            foreach (var data in possibles)
                DebugConsole.Console.Message($"{data.Description} / type: {data.Type} / count: {data.ModifierCount}");
        }
        
        [ConsoleCommand("enhance", "Enhance a card by enhancementIndex.", "enhance [int: ID (0~49)] [int: enhancementIndex (usually 0~1)")]
        public static void EnhanceById(string[] args)
        {
            int id;
            int enhancementIndex;

            if (args.Length != 2)
            {
                DebugConsole.Console.MessageError("This command needs ID and index. Pls try one more with ID and index.");
                return;
            }
            if (!int.TryParse(args[0], out id))
            {
                DebugConsole.Console.MessageWarning("Invalid ID. Pls try one more with valid ID.");
                return;
            }
            if (!int.TryParse(args[1], out enhancementIndex))
            {
                DebugConsole.Console.MessageWarning("Invalid index. Pls try one more with valid index.");
                return;
            }
            if (id < 0 || id > 49)
            {
                DebugConsole.Console.MessageWarning("ID should be between 0 and 49.");
                return;
            }
            
            bool canEnhance = Managers.Card.EnhancementPresenter.TryGetPossibleEnhancements(id, out List<EnhancementData> possibles);

            if (!canEnhance)
            {
                DebugConsole.Console.Message("강화가 불가능합니다.");
                return;
            }

            if (enhancementIndex < 0 || enhancementIndex >= possibles.Count)
            {
                DebugConsole.Console.MessageWarning("Invalid index. Pls try one more with valid index.");
                return;
            }
            
            Managers.Card.EnhancementPresenter.Enhance(id, possibles[enhancementIndex]);
            var data = possibles[enhancementIndex];
            DebugConsole.Console.Message($"{data.Description} / type: {data.Type} / count: {data.ModifierCount} <-- 적용됐습니다.");
            DebugConsole.Console.Message("Inspector의 Managers.Card.CardLibrary에서 확인할 수 있습니다.");
        }

        */
        #endregion
    }
}