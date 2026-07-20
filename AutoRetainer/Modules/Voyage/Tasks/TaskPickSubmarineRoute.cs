using AutoRetainer.Modules.Voyage.Readers;
using Dalamud.Utility;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskPickSubmarineRoute
{
    internal static void Enqueue(uint map, params uint[] points)
    {
        VoyageUtils.Log($"任务已加入队列：{nameof(TaskPickSubmarineRoute)}，航海图={map}，目的地={points.Print()}");
        if(Svc.Data.GetExcelSheet<SubmarineMap>().GetRow(map).Name.GetText() == "") throw new ArgumentOutOfRangeException(nameof(map));
        if(points.Length < 1 || points.Length > 5) throw new ArgumentOutOfRangeException(nameof(points));
        P.TaskManager.Enqueue(() => PickMap(map), $"PickMap({map})");
        foreach(var point in points)
        {
            var name = C.SimpleTweaksCompat ? Svc.Data.GetExcelSheet<SubmarineExploration>().GetRow(point).Location.ToDalamudString().GetText().Trim() : Svc.Data.GetExcelSheet<SubmarineExploration>().GetRow(point).Destination.ToDalamudString().GetText().Trim();
            P.TaskManager.Enqueue(() => PickPoint(name), $"PickPoint({name})");
        }
    }

    internal static void EnqueueImmediate(uint map, params uint[] points)
    {
        P.TaskManager.BeginStack();
        try
        {
            VoyageUtils.Log($"任务已立即加入队列：{nameof(TaskPickSubmarineRoute)}，航海图={map}，目的地={points.Print()}");
            if(Svc.Data.GetExcelSheet<SubmarineMap>().GetRow(map).Name.GetText() == "") throw new ArgumentOutOfRangeException(nameof(map));
            if(points.Length < 1 || points.Length > 5) throw new ArgumentOutOfRangeException(nameof(points));
            P.TaskManager.Enqueue(() => PickMap(map), $"PickMap({map})");
            foreach(var point in points)
            {
                var name = C.SimpleTweaksCompat ? Svc.Data.GetExcelSheet<SubmarineExploration>().GetRow(point).Location.ToDalamudString().GetText().Trim() : Svc.Data.GetExcelSheet<SubmarineExploration>().GetRow(point).Destination.ToDalamudString().GetText().Trim();
                P.TaskManager.Enqueue(() => PickPoint(name), $"PickPoint({name})");
            }
        }
        catch(Exception e) { e.Log(); }
        P.TaskManager.InsertStack();
    }

    internal static bool? PickMap(uint which)
    {
        if(TryGetAddonByName<AtkUnitBase>("AirShipExploration", out _)) return true;
        if(TryGetAddonByName<AtkUnitBase>("SubmarineExplorationMapSelect", out var addon) && IsAddonReady(addon))
        {
            var cnt = new ReaderSubmarineExplorationMapSelect(addon).Maps.Count;
            if(which < 1 || which > cnt)
            {
                PluginLog.Error($"指定的航海图索引无效（指定值 {which}，最大值 {cnt}）");
                return false;
            }
            if(Utils.GenericThrottle && EzThrottler.Throttle("PickMapVoyage", 2000))
            {
                Callback.Fire(addon, true, 2, Utils.ZeroAtkValue, which);
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }

    internal static bool? PickPoint(string name)
    {
        if(TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var addon) && IsAddonReady(addon))
        {
            if(Utils.GenericThrottle)
            {
                VoyageUtils.SelectRoutePointSafe(name);
                return true;
            }
        }
        else
        {
            Utils.RethrottleGeneric();
        }
        return false;
    }
}
