namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeFPSLimiter : NeoUIEntry
{
    public override string Path => "多角色模式/帧率限制";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("帧率限制")
        .TextWrapped("帧率限制器仅在多角色模式启用时生效")
        .Widget("空闲时目标帧率", (x) =>
        {
            ImGui.SetNextItemWidth(100f);
            UIUtils.SliderIntFrameTimeAsFPS(x, ref C.TargetMSPTIdle, C.ExtraFPSLockRange ? 1 : 10);
        })
        .Widget("运行时目标帧率", (x) =>
        {
            ImGui.SetNextItemWidth(100f);
            UIUtils.SliderIntFrameTimeAsFPS("运行时目标帧率", ref C.TargetMSPTRunning, C.ExtraFPSLockRange ? 1 : 20);
        })
        .Checkbox("游戏激活时解除 FPS 限制", () => ref C.NoFPSLockWhenActive)
        .Checkbox($"允许更低 FPS 限制值", () => ref C.ExtraFPSLockRange, "若启用此选项后在多角色模式出现任何错误，将不提供支持。")
        .Checkbox($"仅在设置关机计时器时启用限制器", () => ref C.FpsLockOnlyShutdownTimer);
}
