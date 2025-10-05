namespace Cardevil.Pools
{
    /// <summary>
    /// Human Error 방지를 위한 enum
    /// </summary>
    /// <remarks>
    /// OriginalGameObject이름과 같아야함
    /// </remarks>
    public enum Poolables
    {
        None,
        TestPoolable,
        SoundEmitter,
        TileHighlight,
        Card,
        CardVisual,
    }
}