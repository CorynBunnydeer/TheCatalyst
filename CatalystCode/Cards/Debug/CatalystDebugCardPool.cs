using Catalyst.CatalystCode.Character;

namespace Catalyst.CatalystCode.Cards.Debug;

/// <summary>
/// Registration-only home for development cards. No character references this pool,
/// and it is not shared, so its cards remain spawnable without entering normal pools.
/// </summary>
public class CatalystDebugCardPool : CatalystCardPool
{
    public override bool IsShared => true;

    public override string Title => "CatalystDebug";

    public override bool SeenByDefault => false;
}
