using AutoRetainer.Internal;
using AutoRetainerAPI.Configuration;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskDeployOnPointPlan
{
    internal static void Enqueue(string name, VoyageType type, SubmarinePointPlan unlock)
    {
        VoyageUtils.Log($"任务已加入队列：{nameof(TaskDeployOnPointPlan)}（方案：{unlock}）");
        TaskIntelligentRepair.Enqueue(name, type);
        P.TaskManager.Enqueue(TaskDeployOnBestExpVoyage.SelectDeploy);
        EnqueuePick(unlock);
        P.TaskManager.Enqueue(TaskDeployOnBestExpVoyage.Deploy);
        TaskDeployAndSkipCutscene.Enqueue(true);
    }
    internal static void EnqueuePick(SubmarinePointPlan unlock)
    {
        P.TaskManager.Enqueue(() => PickFromPlan(unlock), $"PickFromPlan({unlock})");
    }

    internal static void PickFromPlan(SubmarinePointPlan unlock)
    {
        var points = unlock.Points;
        VoyageUtils.Log($"目的地：{points.Select(x => $"{x}").Join("\n")}");
        TaskPickSubmarineRoute.EnqueueImmediate(unlock.GetMapId(), points.ToArray());
    }
}
