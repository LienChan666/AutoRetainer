namespace AutoRetainer.Modules.GcHandin;

internal static class AutoGCHandinUI
{
    internal static void Draw()
    {
        ImGui.Checkbox("交纳完成后托盘通知（需要 NotificationMaster）", ref C.GCHandinNotify);
    }
}
