namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal class DebugNMAPI : DebugSectionBase
{
    private static float vol;
    private static bool repeat;
    private static bool stopOnFocus;
    private static string path = "";
    public override void Draw()
    {
        ImGuiEx.Text($"已激活：{Lang.Bool(P.NotificationMasterApi.IsIPCReady())}");
        ImGui.InputText("路径", ref path, 500);
        ImGui.InputFloat("音量", ref vol);
        ImGui.Checkbox("重复播放", ref repeat);
        ImGui.Checkbox("窗口聚焦时停止", ref stopOnFocus);
        if(ImGui.Button("闪烁任务栏图标")) new TickScheduler(() => P.NotificationMasterApi.FlashTaskbarIcon(), 1000);
        if(ImGui.Button("显示带标题通知")) new TickScheduler(() => P.NotificationMasterApi.DisplayTrayNotification("标题", "内容"), 1000);
        if(ImGui.Button("显示无标题通知")) new TickScheduler(() => P.NotificationMasterApi.DisplayTrayNotification("内容"), 1000);
        if(ImGui.Button("播放声音")) new TickScheduler(() => P.NotificationMasterApi.PlaySound(path, vol, repeat, stopOnFocus), 1000);
        if(ImGui.Button("停止声音")) P.NotificationMasterApi.StopSound();
    }
}
