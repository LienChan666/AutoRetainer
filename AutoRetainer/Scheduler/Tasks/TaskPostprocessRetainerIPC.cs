namespace AutoRetainer.Scheduler.Tasks;

internal static class TaskPostprocessRetainerIPC
{
    internal static void Enqueue(string retainer, string pluginToProcess = null)
    {
        P.TaskManager.Enqueue(() =>
        {
            SchedulerMain.RetainerPostprocess = SchedulerMain.RetainerPostprocess.Clear();
            IPC.FireRetainerPostprocessTaskRequestEvent(retainer);
        }, "TaskRetainerPostprocessIPCEnqueue");
        P.TaskManager.Enqueue(() =>
        {
            P.TaskManager.BeginStack();
            try
            {
                DebugLog($"SchedulerMain.RetainerPostprocess 包含：{SchedulerMain.RetainerPostprocess.Print()}，待处理插件 = {pluginToProcess}");
                foreach(var x in SchedulerMain.RetainerPostprocess.Where(x => pluginToProcess == null || x == pluginToProcess))
                {
                    P.TaskManager.Enqueue(() =>
                    {
                        SchedulerMain.RetainerPostprocess = SchedulerMain.RetainerPostprocess.Remove(x);
                        SchedulerMain.RetainerPostProcessLocked = true;
                        IPC.FireRetainerPostprocessEvent(x, retainer);
                    }, $"来自 {x} 的雇员后处理请求");
                    P.TaskManager.Enqueue(() => !SchedulerMain.RetainerPostProcessLocked, $"来自 {x} 的雇员后处理任务", new(timeLimitMS: int.MaxValue));
                }
            }
            catch(Exception ex) { ex.Log(); }
            P.TaskManager.InsertStack();
        }, "TaskRetainerPostprocessProcessEntries");
    }
}
