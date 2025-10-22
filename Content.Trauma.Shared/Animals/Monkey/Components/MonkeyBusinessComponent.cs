using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Animals.Monkey.Components;

// <summary>
// Handles the main actions of a monkey business
// </summary>
[RegisterComponent]
public sealed partial class MonkeyBusinessComponent : Component
{

    [DataField]
    public EntProtoId ActionId = "ActionMonkeyBusiness";

    [DataField]
    public EntityUid? ActionEnt;

    [DataField]
    public EntityUid MonkeyBusinessTarget;
}
