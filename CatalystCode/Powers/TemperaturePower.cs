using MegaCrit.Sts2.Core.Entities.Powers;

namespace Catalyst.CatalystCode.Powers;

public class TemperaturePower : CatalystPower
{
    public const int Minimum = -3;
    public const int Maximum = 3;

    public override PowerType Type => PowerType.None;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => true;
}
