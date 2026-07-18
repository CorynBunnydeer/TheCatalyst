using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

using Catalyst.CatalystCode.Cards.Infrastructure;

namespace Catalyst.CatalystCode.Cards.Tokens.Props;

/// <summary>
/// Marker base for physical token cards Nanairo can manipulate as Props.
///
/// Prop is deliberately only a classification. Individual Props define their own
/// lifecycle, charges, enlargement behavior, or other physical rules.
/// </summary>

