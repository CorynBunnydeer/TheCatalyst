using MegaCrit.Sts2.Core.Entities.Powers;

namespace Catalyst.CatalystCode.Powers;

/// <summary>
/// Signed room Temperature represented on Nanairo until a dedicated room indicator is
/// added. Negative amounts are cold, positive amounts are hot, and the valid range is
/// -3 through +3. Neutral zero is represented by the absence of this Power.
/// </summary>
public class TemperaturePower : CatalystPower
{
    public const int Minimum = -3;
    public const int Maximum = 3;


