namespace AutoRetainer.UI.NeoUI.Experiments;

internal class NightMode : ExperimentUIEntry
{
    public override string Name => "夜间模式";
    public override void Draw()
    {
        ImGuiEx.TextWrapped("夜间模式：\n" +
                "- 将强制启用“登录界面等待”选项\n" +
                "- 将强制应用内置 FPS 限制\n" +
                "- 窗口失焦且等待期间，游戏帧率将限制为 0.2 FPS\n" +
                "- 画面可能看起来像卡死；重新激活游戏窗口后最多等待 5 秒即可恢复。\n" +
            "- 夜间模式下默认仅启用远航探索流程\n" +
                $"- 关闭夜间模式后，脱困管理器会启动并将你重新登录回游戏。");
        if(ImGui.Checkbox("启用夜间模式", ref C.NightMode)) MultiMode.BailoutNightMode();
        ImGui.Checkbox("显示夜间模式复选框", ref C.ShowNightMode);
        ImGui.Checkbox("夜间模式处理雇员", ref C.NightModeRetainers);
        ImGui.Checkbox("夜间模式处理远航探索", ref C.NightModeDeployables);
        ImGui.Checkbox("记住夜间模式开关状态", ref C.NightModePersistent);
        ImGui.Checkbox("将关机命令改为启用夜间模式（不真正关机）", ref C.ShutdownMakesNightMode);
    }
}
