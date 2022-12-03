
using Content.Server.ControllerDevice;
using Content.Server.Mind.Components;
using Content.Server.MobState;
using Content.Server.Players;
using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.MobState.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Server.Player;
using Robust.Shared.Localization;
using Robust.Shared.Player;

namespace Content.Server.ControllableMob;

public sealed class ControllerDeviceSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly ControllableMobSystem _controllableMobSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<ControllerDeviceComponent, UseInHandEvent>(Control);
        SubscribeLocalEvent<ControllerDeviceComponent, GotUnequippedHandEvent>(Unequipped);
    }

    private void Control(EntityUid uid, ControllerDeviceComponent comp, UseInHandEvent args)
    {
        if (comp.Controlling == null && !_entityManager.EntityExists(comp.Controlling) && (TryComp<MobStateComponent>(uid, out var damageState) && _mobStateSystem.IsDead(uid, damageState)))
        {
            _popupSystem.PopupEntity(Loc.GetString("control-device-unable-to-connect"), uid, Filter.Entities(args.User));
            return;
        }

        if (!TryComp<ControllableMobComponent>(comp.Controlling, out var controllableComp)
            || controllableComp.CurrentEntityOwning != null)
        {
            _popupSystem.PopupEntity(Loc.GetString("control-device-already-controlled"), uid, Filter.Entities(args.User));
            return;
        }

    //    var calcDist = Comp<TransformComponent>(uid).WorldPosition.Length - Comp<TransformComponent>(comp.Controlling.Value).WorldPosition.Length;
    //    if (calcDist > controllableComp.Range)
    //    {
    //        _popupSystem.PopupEntity(Loc.GetString("device-control-out-of-range"), uid, Filter.Entities(args.User));
    //        return;
    //    }

        if (!TryComp<ControllerMobComponent>(args.User, out var controllerComp)
            || controllerComp.Controlling != null)
        {
            _popupSystem.PopupEntity(Loc.GetString("control-device-unable-to-use"), uid, Filter.Entities(args.User));
            return;
        }

        controllerComp.Controlling = comp.Controlling;
        controllableComp.CurrentEntityOwning = args.User;
        _controllableMobSystem.ChangeControl(controllableComp.CurrentEntityOwning.Value, comp.Controlling.Value);
    }

    private void Unequipped(EntityUid uid, ControllerDeviceComponent comp, GotUnequippedHandEvent args)
    {
        if (comp.Controlling != null && _entityManager.EntityExists(comp.Controlling) // CHONKY
            && TryComp<ControllableMobComponent>(comp.Controlling, out var controllableComp)
            && controllableComp.CurrentEntityOwning != null
            && TryComp<ControllerMobComponent>(args.User, out var controllerComp)
            && controllerComp.Controlling != null)
        {
            _controllableMobSystem.ChangeControl(comp.Controlling.Value, controllableComp.CurrentEntityOwning.Value);
            comp.Controlling = null;
            controllerComp.Controlling = null;
            controllableComp.CurrentEntityOwning = null;
        }
    }
}