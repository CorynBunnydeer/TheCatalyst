using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Catalyst.CatalystCode.Relics;

/// <summary>
/// Starter relic placeholder for Catalyst's Mass Differential ownership.
/// This will work for other characters to be able to use this mod's Grow/Shrink mechanics;
/// In-Lore, Nanairo is the one that carries the type of power to cause such distortions.
/// Given this is a parallel universe instance, I'm adapting it to the Relic on this mod for clarity's
/// sake, as a form of abstraction
/// </summary>
public class BlueSleighBell : CatalystRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
}
