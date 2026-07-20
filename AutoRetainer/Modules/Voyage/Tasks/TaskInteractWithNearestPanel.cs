using ECommons.GameHelpers;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static class TaskInteractWithNearestPanel
{
    internal static void Enqueue(bool interact = true)
    {
        VoyageUtils.Log($"任务已加入队列：{nameof(TaskInteractWithNearestPanel)}，交互={interact}");
        if(!VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType))
        {
            TaskNeoHET.TryEnterWorkshop(() =>
            {
                Data.WorkshopEnabled = false;
                DuoLog.Error($"未能找到部队工房，已排除该角色的远航探索处理");
                P.TaskManager.Abort();
            });
        }
        P.TaskManager.Enqueue(() =>
        {
            if(VoyageUtils.TryGetNearestVoyagePanel(out var obj) && Vector3.Distance(Player.Object.Position, obj.Position) > 4.25f)
            {
                P.TaskManager.BeginStack();
                P.TaskManager.Enqueue(VoyageScheduler.Lockon);
                P.TaskManager.Enqueue(VoyageScheduler.Approach);
                P.TaskManager.Enqueue(VoyageScheduler.AutomoveOffPanel);
                P.TaskManager.InsertStack();
            }
        }, "ApproachPanelIfNeeded");
        if(interact) P.TaskManager.Enqueue(VoyageScheduler.InteractWithVoyagePanel);
    }
}
