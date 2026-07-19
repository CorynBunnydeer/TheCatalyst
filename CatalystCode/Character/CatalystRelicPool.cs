using BaseLib.Abstracts;
using Catalyst.CatalystCode.Extensions;
using Godot;

namespace Catalyst.CatalystCode.Character;

/// <summary>
/// Relic-pool UI metadata for Catalyst.
/// </summary>
public class CatalystRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => Catalyst.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();

    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}
