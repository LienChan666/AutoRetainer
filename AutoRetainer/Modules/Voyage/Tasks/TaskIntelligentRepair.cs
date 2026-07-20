using AutoRetainer.Internal;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static class TaskIntelligentRepair
{
    internal static void Enqueue(string name, VoyageType type)
    {
        VoyageUtils.Log($"任务已加入队列：{nameof(TaskIntelligentRepair)}，名称={name}，类型={Lang.VoyageTypeNames[type]}");
        P.TaskManager.Enqueue(() =>
        {
            var rep = VoyageUtils.GetIsVesselNeedsRepair(name, type, out var log);
            if(rep.Count > 0)
            {
                TaskRepairAll.EnqueueImmediate(rep, name, type);
            }
            DebugLog($"修理检查日志：{log.Join("，")}");
        }, "IntelligentRepairTask");
    }
}
