namespace AutoRetainer.Scheduler.Tasks;

public class TaskPostprocessCharacterIPC
{
    internal static void Enqueue(string pluginToProcess = null)
    {
        P.TaskManager.Enqueue(() =>
        {
            SchedulerMain.CharacterPostprocess = SchedulerMain.CharacterPostprocess.Clear();
            IPC.FireCharacterPostprocessTaskRequestEvent();
        }, "TaskCharacterPostprocessIPCEnqueue");
        P.TaskManager.Enqueue(() =>
        {
            P.TaskManager.BeginStack();
            try
            {
                DebugLog($"SchedulerMain.CharacterPostprocess 包含：{SchedulerMain.CharacterPostprocess.Print()}，待处理插件 = {pluginToProcess}");
                foreach(var x in SchedulerMain.CharacterPostprocess.Where(x => pluginToProcess == null || x == pluginToProcess))
                {
                    P.TaskManager.Enqueue(() =>
                        {
                            SchedulerMain.CharacterPostprocess = SchedulerMain.CharacterPostprocess.Remove(x);
                            SchedulerMain.CharacterPostProcessLocked = true;
                            IPC.FireCharacterPostprocessEvent(x);
                        }, $"来自 {x} 的角色后处理请求");
                    P.TaskManager.Enqueue(() => !SchedulerMain.CharacterPostProcessLocked, $"来自 {x} 的角色后处理任务", new(timeLimitMS: int.MaxValue));
                }
            }
            catch(Exception e) { e.Log(); }
            P.TaskManager.InsertStack();
        }, "TaskCharacterPostprocessProcessEntries");
    }
}
