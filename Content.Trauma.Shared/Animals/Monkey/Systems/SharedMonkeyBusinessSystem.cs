
using Content.Shared.Actions;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Trauma.Shared.Animals.Monkey.Components;

namespace Content.Trauma.Shared.Animals.Monkey.System;

public abstract class SharedMonkeyBusinessSystem : EntitySystem
{

    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("SharedMonkeySystem");

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

        if (!TryDoMonkeyBusiness(user, ent.Comp))
            return;

        // var targetCoords = _transform.GetWorldPosition(target);

        ent.Comp.MonkeyBusinessTarget = target;

        _popup.PopupEntity("MONKEY BUSINESS!", user, PopupType.LargeCaution);
        _popup.PopupEntity("WATCH OUT", target, PopupType.LargeCaution);

        args.Handled = true;
    }

}
