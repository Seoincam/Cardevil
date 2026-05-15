namespace Cardevil.Card.Common.Core.Upgrade
{
    public enum UpgradePath
    {
        None,
        MultiNumber,
        MultiColor,
        MultiDirection
    }

	public enum UpgradeApplyType
	{
        None,
        Add,
        OverrideColors,
        OverrideNumbers,
        OverrideDirections
	}
}