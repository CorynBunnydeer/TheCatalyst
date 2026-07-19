using Catalyst.CatalystCode.Character;

namespace Catalyst.CatalystCode.Cards.Debug;

/// <summary>
/// Registration-only home for development cards. No character references this pool,
/// and it is not shared, so its cards remain spawnable without entering normal pools.
/// </summary>
public class CatalystDebugCardPool : CatalystCardPool
{
    // ((REFERENCE)) BaseLib: custom pools must be character pools or shared pools to
    // be discovered by ModelDb. Shared keeps these cards out of Catalyst's character
    // pool while still making them available to the dev console and prototype kit.
    public override bool IsShared => true;

    public override string Title => "CatalystDebug";

    public override bool SeenByDefault => false;
}
