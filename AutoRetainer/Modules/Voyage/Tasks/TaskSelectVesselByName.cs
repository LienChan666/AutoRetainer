using AutoRetainer.Internal;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static class TaskSelectVesselByName
{
    internal static void Enqueue(string name, VoyageType type)
    {
        VoyageUtils.Log($"任务已加入队列：{nameof(TaskSelectVesselByName)}（{name}）");
        P.TaskManager.Enqueue(() => VoyageScheduler.SelectVesselByName(name, type), $"按名称选择{Lang.VoyageTypeNames[type]}：{name}");
    }
}
