using AutoRetainer.Internal;
using AutoRetainer.Scheduler.Tasks;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskFinalizeVessel
{
    internal static void Enqueue(string name, VoyageType type, bool quit)
    {
        VoyageUtils.Log($"任务已加入队列：{nameof(TaskFinalizeVessel)}，名称={name}，类型={Lang.VoyageTypeNames[type]}，退出={Lang.Bool(quit)}");
        TaskSelectVesselByName.Enqueue(name, type);
        P.TaskManager.Enqueue(VoyageScheduler.WaitUntilFinalizeDeployAddonExists);
        P.TaskManager.Enqueue(VoyageScheduler.FinalizeVessel);
        P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon), "WaitForSelectStringAddon");
        TaskIntelligentRepair.Enqueue(name, type);
        if(quit)
        {
            P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselMenu);
        }
    }
}
