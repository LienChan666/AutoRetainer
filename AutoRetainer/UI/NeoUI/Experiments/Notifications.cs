namespace AutoRetainer.UI.NeoUI.Experiments;
public class Notifications : ExperimentUIEntry
{
    public override string Name => "消息提醒";

    public override void Draw()
    {
        ImGui.Checkbox($"当任一雇员探险完成时显示覆盖层通知", ref C.NotifyEnableOverlay);
        ImGui.Checkbox($"在任务或战斗中不显示覆盖层", ref C.NotifyCombatDutyNoDisplay);
        ImGui.Checkbox($"包含其他角色", ref C.NotifyIncludeAllChara);
        ImGui.Checkbox($"忽略未在多角色模式中启用的其他角色", ref C.NotifyIgnoreNoMultiMode);
        ImGui.Checkbox($"在游戏聊天中显示通知", ref C.NotifyDisplayInChatX);
        ImGuiEx.Text($"当游戏处于后台时：（需安装并启用 NotificationMaster）");
        ImGui.Checkbox($"雇员可用时发送桌面通知", ref C.NotifyDeskopToast);
        ImGui.Checkbox($"闪烁任务栏", ref C.NotifyFlashTaskbar);
        ImGui.Checkbox($"AutoRetainer 已启用或多角色模式运行时不通知", ref C.NotifyNoToastWhenRunning);
    }
}
