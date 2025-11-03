
using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Emoting;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Trauma.Shared.Animals.Monkey.Components;

namespace Content.Trauma.Shared.Animals.Monkey.System;

public abstract class SharedMonkeyBusinessSystem : EntitySystem
{

    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly SharedChatSystem _chatSystem = default!;

    // [Dependency] private readonly EmoteSystem _emoteSystem = default!;
    // [Dependency] private readonly ChatManager _chatManager = default!;
    // [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    // [Dependency] private readonly ThrowingSystem _throwingSystem = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("SharedMonkeySystem");
        /** // TODO:
            we really want this to be a do after and such, so we should add a TryDoMonkeyBusiness
            event for starting doing monkey business, which then should result in the
            MonkeyBusinessEvent if it completes successfully.
        */
        // TODO: figure out how this system should be split between server\shared\client.

        SubscribeLocalEvent<MonkeyBusinessComponent, MonkeyBusinessEvent>(DoMonkeyBusiness);
        SubscribeLocalEvent<MonkeyBusinessComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<MonkeyBusinessComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<MonkeyBusinessComponent> ent, ref MapInitEvent args)
    {
        _sawmill.Debug("Starting up SharedMonkeySystem for entity: {EntityUid}", ent.Owner);
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<MonkeyBusinessComponent> ent, ref ComponentShutdown args)
    {
        _sawmill.Debug("Shutting down SharedMonkeySystem for entity: {EntityUid}", ent.Owner);
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);
    }

    private bool TryDoMonkeyBusiness(EntityUid uid, MonkeyBusinessComponent comp)
    {
        if (TryComp<HungerComponent>(uid, out var hunger))
        {
            if (_hunger.GetHunger(hunger) < comp.HungerUsage)
            {
                _popup.PopupEntity("Ain't nothing in the ''tank''!", uid, uid);
                return false;
            }

            _hunger.ModifyHunger(uid, -comp.HungerUsage, hunger);
        }

        return true;
    }

    private void DoMonkeyBusiness(Entity<MonkeyBusinessComponent> ent, ref MonkeyBusinessEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;
        var user = args.Performer;

        _handsSystem.TryGetEmptyHand(user, out var emptyHand);
        if (emptyHand is null)
        {
            _popup.PopupEntity("You need a free hand to do monkey business!", user, user);
            return;
        }

        var targetPos = _transform.GetMapCoordinates(target).Position;
        var userPos = _transform.GetMapCoordinates(user).Position;

        var direction = targetPos - userPos;
        if (direction == Vector2.Zero)
            return;

        if (!TryDoMonkeyBusiness(user, ent.Comp))
            return;

        ent.Comp.MonkeyBusinessTarget = target;

        // Gonna assume that our hands are still empty and spawn the monkeyball and pick it up

        var monkeyball = EntityManager.SpawnEntity(ent.Comp.HairballPrototype, Transform(user).Coordinates);
        _sawmill.Debug("Trying to pick up monkey business: {Monkeyball} with {EmptyHand}", monkeyball, emptyHand);

        var pickedUpMonkeyBusiness = _handsSystem.TryPickup(user, monkeyball, emptyHand);
        if (pickedUpMonkeyBusiness is false)
        {
            // TODO: figure out why we sometimes cannot pick up the monkeyball after spawning it.
            _sawmill.Debug("Failed to pick up monkey business: {Monkeyball}", monkeyball);
            EntityManager.DeleteEntity(monkeyball);
            return;
        }
        _sawmill.Debug("Picked up monkey business: {PickedUpMonkeyBusiness}", pickedUpMonkeyBusiness);

        // Business in hand, we now shall inflict our wrath upon the populace.

        // TODO: using chat system like this feels weird, figure out how this actually should be done.
        _chatSystem.TrySendInGameICMessage(user, "EEEK OOOK AAAAH!!!!", InGameICChatType.Emote, false);
        // TODO: figure out if we're trying to throw this too fast after picking it up.
        // _throwingSystem.TryThrow(monkeyball, direction);

        args.Handled = true;
    }

}
