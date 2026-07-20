using AutoRetainer.Internal;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskRedeployVessel
{
    internal static void Enqueue(string name, VoyageType type)
    {
        VoyageUtils.Log($"任务已加入队列：{nameof(TaskRedeployVessel)}，名称={name}，类型={Lang.VoyageTypeNames[type]}");
        P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon), "WaitForSelectStringAddon");
        TaskIntelligentRepair.Enqueue(name, type);
        TaskRedeployPreviousLog.Enqueue(name, type);
    }
}
