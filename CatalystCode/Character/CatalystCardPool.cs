using BaseLib.Abstracts;
using Catalyst.CatalystCode.Extensions;
using Godot;

namespace Catalyst.CatalystCode.Character;

/// <summary>
/// Main card pool metadata for Catalyst's character color and UI.
/// </summary>
public class CatalystCardPool : CustomCardPoolModel
{
    public override string Title => "Catalyst";

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();

    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    public override float H => 1f;

    public override float S => 1f;

    public override float V => 1f;

    public override Color DeckEntryCardColor => new("ffffff");

    public override bool SeenByDefault => true;

    public override bool IsColorless => false;
}
