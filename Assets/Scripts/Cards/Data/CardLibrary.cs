using Cardevil.Core;
using System.Collections.Generic;

namespace Cardevil.Cards.Data
{
    public interface IReadOnlyCardLibrary
    {
        IReadOnlyCollection<CardPipeline> Pipelines { get; }
    }
    
    public class CardLibrary : IClearable, IReadOnlyCardLibrary
    {
        private readonly HashSet<CardPipeline> _pipelines = new();
        private readonly CardDataModifierService _modifierService = new();
        
        public IReadOnlyCollection<CardPipeline> Pipelines => _pipelines;

        public void Init()
        {
            Clear();
            _modifierService.Init();
            
            // TODO: 세이브파일 로드 체크 로직 넣어야함.
            _pipelines.CreateBasePipelines(_modifierService);
        }

        public void Clear()
        {
            _pipelines.Clear();
        }
    }
}