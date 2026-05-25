using Cardevil.Card.Common.Core.Upgrade;

namespace Cardevil.Card.InWorld.UI.Upgrade
{
    public static class UpgradeTextResolver
    {
        public static (string title, string description) GetTooltipText(UpgradePath path, int stage)
        {
            switch (path)
            {
                case UpgradePath.MultiColor:
                    return stage >= 4
                        ? ($"다중 색 카드 ({stage})",
                            "여러 색 보석을 함께 사용할 수 있습니다.\n이번 강화 이후 사용할 색을 선택하세요.")
                        : ($"다중 색 카드 ({stage})",
                            "보석 색이 하나 추가됩니다.\n추가 색은 무작위로 결정됩니다.\n원하는 강화 루트를 선택하세요.");

                case UpgradePath.MultiNumber:
                    return stage >= 4
                        ? ($"다중 숫자 카드 ({stage})",
                            "여러 숫자 중 하나를 선택해 사용할 수 있습니다.\n이번 강화 이후 사용할 숫자를 선택하세요.")
                        : ($"다중 숫자 카드 ({stage})",
                            "숫자 후보가 하나 추가됩니다.\n추가 숫자는 무작위로 결정됩니다.\n원하는 강화 루트를 선택하세요.");

                case UpgradePath.MultiDirection:
                    return stage >= 4
                        ? ($"다중 방향 카드 ({stage})",
                            "여러 방향 중 하나를 선택해 이동할 수 있습니다.\n이번 강화 이후 사용할 방향을 선택하세요.")
                        : ($"다중 방향 카드 ({stage})",
                            "방향 후보가 하나 추가됩니다.\n추가 방향은 무작위로 결정됩니다.\n원하는 강화 루트를 선택하세요.");

                default:
                    return ("카드 강화", "새로운 능력이 개방됩니다.");
            }
        }
    }
}
