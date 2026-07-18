using BaseLib.Abstracts;
using BaseLib.Utils;
using Catalyst.CatalystCode.Character;

namespace Catalyst.CatalystCode.Potions;

[Pool(typeof(CatalystPotionPool))]
public abstract class CatalystPotion : CustomPotionModel;