using Cardevil.Cards.Data.InStage;
using Cardevil.Core;
using Cardevil.Utils;
using System.Collections.Generic;

namespace Cardevil.Cards.Data
{
    public interface IReadOnlyCardLibrary
    {
        IReadOnlyDictionary<int, CardPipeline> Pipelines { get; }
    }
    
    public class CardLibrary : IClearable, IReadOnlyCardLibrary
    {
        // <id, data>
        private readonly Dictionary<int, CardPipeline> _pipelines = new();
        private readonly Dictionary<int, CardData> _cardDatas = new();
        // TODO: Visual도 추가

        public IReadOnlyDictionary<int, CardPipeline> Pipelines => _pipelines;
        
        public void Init()
        {
            Clear();
            // TODO: 세이브파일 로드 체크 로직 넣어야함.
            
            _pipelines.CreateBasePipelines();
        }

        public void Clear()
        {
            _pipelines.Clear();
        }
        
        public CardPipeline GetPipelineById(int id)
        {
            if (id < 0 || id > 50)
            {
                LogEx.LogError("잘못된 id를 입력했습니다.");
                return null;
            }

            if (!_pipelines.TryGetValue(id, out var pipeline))
            {
                LogEx.LogError($"존재하지 않는 id입니다. : {id}");
                return null;
            }

            return pipeline;
        }

    }
}