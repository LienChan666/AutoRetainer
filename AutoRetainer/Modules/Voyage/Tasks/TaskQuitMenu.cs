namespace AutoRetainer.Modules.Voyage.Tasks;

internal static class TaskQuitMenu
{
    internal static void Enqueue()
    {
        VoyageUtils.Log($"任务已加入队列：{nameof(TaskQuitMenu)}");
        P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselSelectorMenu);
    }
}
