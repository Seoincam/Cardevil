using Cardevil.Core.Bootstrap;
using Database;
using System;
using System.Collections.Generic;



namespace Cardevil.UI
{
    using RawTooltipData = Database.Generated.TooltipData;
    
    public static class TooltipResolver
    {
        private static readonly Dictionary<string, RawTooltipData> TooltipCache = new(StringComparer.Ordinal);

        public static TooltipData Resolve(string tooltipKey)
        {
            if (string.IsNullOrWhiteSpace(tooltipKey))
            {
                return CreateMissingTooltip(string.Empty);
            }

            RawTooltipData rawData = GetRawTooltipData(tooltipKey);
            if (rawData == null)
            {
                return CreateMissingTooltip(tooltipKey);
            }

            return new TooltipData
            {
                Title = ResolveText(rawData.titleTextID),
                Description = ResolveText(rawData.decriptionTextID)
            };
        }

        private static RawTooltipData GetRawTooltipData(string tooltipKey)
        {
            if (TooltipCache.TryGetValue(tooltipKey, out RawTooltipData cached))
            {
                return cached;
            }

            var tooltipDataList = CardevilCore.Instance?.DatabaseManager?.Database?.TooltipDataList;
            if (tooltipDataList == null)
            {
                return null;
            }

            foreach (RawTooltipData data in tooltipDataList)
            {
                if (data.identifier != tooltipKey)
                {
                    continue;
                }

                TooltipCache[tooltipKey] = data;
                return data;
            }

            return null;
        }

        private static string ResolveText(string textKey)
        {
            if (string.IsNullOrWhiteSpace(textKey))
            {
                return string.Empty;
            }

            if (textKey.StartsWith("text.", StringComparison.Ordinal))
            {
                return textKey;
            }

            return textKey;
        }

        private static TooltipData CreateMissingTooltip(string tooltipKey)
        {
            return new TooltipData
            {
                Title = string.Empty,
                Description = string.IsNullOrWhiteSpace(tooltipKey)
                    ? "Missing Tooltip"
                    : $"Missing Tooltip: {tooltipKey}"
            };
        }
    }
}
