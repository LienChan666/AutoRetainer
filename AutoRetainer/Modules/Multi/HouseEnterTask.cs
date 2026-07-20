using ECommons.Automation;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;

namespace AutoRetainer.Modules.Multi;

internal static unsafe class HouseEnterTask
{
    internal static bool? Approach()
    {
        DebugLog($"正在启用自动移动");
        Utils.RegenerateRandom();
        Chat.ExecuteCommand("/automove on");
        return true;
    }

    internal static bool? SelectYesno()
    {
        if(!ResidentalAreas.List.Contains((ushort)Svc.ClientState.TerritoryType))
        {
            return null;
        }
        var addon = Utils.GetSpecificYesno(Lang.ConfirmHouseEntrance);
        if(addon != null)
        {
            if(IsAddonReady(addon) && EzThrottler.Throttle("HET.SelectYesno"))
            {
                DebugLog("选择“是”");
                new AddonMaster.SelectYesno((nint)addon).Yes();
                return true;
            }
        }
        else
        {
            if(Utils.TrySelectSpecificEntry(Lang.GoToYourApartment, () => EzThrottler.Throttle("HET.SelectYesno")))
            {
                DebugLog("已确认前往公寓");
                return true;
            }
        }
        return false;
    }

    internal static bool? WaitUntilLeavingZone()
    {
        return !ResidentalAreas.List.Contains((ushort)Svc.ClientState.TerritoryType);
    }

    internal static bool? LockonBell()
    {
        var bell = Utils.GetNearestRetainerBell(out var d);
        if(bell != null && d < 20f)
        {
            if(Svc.Targets.Target?.Address == bell.Address)
            {
                if(EzThrottler.Throttle("HET.LockonBell"))
                {
                    Chat.ExecuteCommand("/lockon on");
                    return true;
                }
            }
            else
            {
                if(EzThrottler.Throttle("HET.SetTargetBell", 200))
                {
                    DebugLog($"正在设置传唤铃目标（{bell}）");
                    Svc.Targets.Target = bell;
                }
            }
        }
        return false;
    }


    internal static bool? AutorunOffBell()
    {
        var bell = Utils.GetReachableRetainerBell(false);
        if(bell != null) PluginLog.Information($"距离：{Vector3.Distance(Player.Object.Position, bell.Position)}");
        if(bell != null && Vector3.Distance(Player.Object.Position, bell.Position) < 4f + Utils.Random * 0.25f)
        {
            DebugLog($"正在禁用自动移动");
            Chat.ExecuteCommand("/automove off");
            return true;
        }
        return false;
    }
}
