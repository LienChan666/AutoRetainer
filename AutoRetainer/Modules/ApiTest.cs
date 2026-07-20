using ECommons.Automation.LegacyTaskManager;
using ECommons.WindowsFormsReflector;

namespace AutoRetainer.Modules;

internal static class ApiTest
{
    internal static bool Enabled = false;
    internal static TaskManager TaskManager;

    internal static void Init()
    {
        P.API.OnRetainerPostprocessStep += API_OnRetainerPostprocessTask;
        P.API.OnRetainerReadyToPostprocess += API_OnRetainerReadyToPostprocess;
        TaskManager = new();
    }

    private static void API_OnRetainerPostprocessTask(string retainerName)
    {
        if(!Enabled) return;
        PluginLog.Information($"正在请求对雇员 {retainerName} 执行后处理");
        P.API.RequestRetainerPostprocess();
    }

    private static void API_OnRetainerReadyToPostprocess(string retainerName)
    {
        PluginLog.Information($"正在对雇员 {retainerName} 执行后处理");
        TaskManager.Enqueue(() =>
        {
            if(GenericHelpers.IsKeyPressed(Keys.Back))
            {
                return true;
            }
            return false;
        }, int.MaxValue);
        TaskManager.Enqueue(P.API.FinishRetainerPostProcess);
    }
}
