using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Animals.Monkey.Components;

// <summary>
// Handles the main actions of a monkey business
// </summary>
[RegisterComponent]
public sealed partial class MonkeyBusinessComponent : Component
{
    /**
        /// <summary>
        /// The hairball prototype to use.
        /// </summary>
        [DataField("hairballPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string HairballPrototype = "Hairball";

        //[DataField("hairballAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        //public string HairballAction = "ActionHairball";

        [DataField("hairballActionId",
            customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? HairballActionId = "ActionHairball";

        [DataField("hairballAction")]
        public EntityUid? HairballAction;
    */

    [DataField("hairballPrototype")]
    public string HairballPrototype = "MonkeyBusinessBall";

    [DataField]
    public EntProtoId ActionId = "ActionMonkeyBusiness";

    [DataField]
    public EntityUid? ActionEnt;

    [DataField]
    public EntityUid MonkeyBusinessTarget;

    [DataField]
    public float Cooldown;

    [DataField]
    public float HungerUsage;

    [DataField]
    public SoundSpecifier? MonkeyBusinessSound;

}
