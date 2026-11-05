namespace AutoRetainer.UI.NeoUI;
public class MainSettings : NeoUIEntry
{
    public override string Path => "通用";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("延迟")
        .Widget(100f, "时间不同步补偿（秒）", (x) => ImGuiEx.SliderInt(x, ref C.UnsyncCompensation.ValidateRange(-60, 0), -10, 0), "为缓解游戏时间与本机时间不同步问题，在探险结束时间基础上额外提前扣除的秒数。")
        .Widget(100f, "额外交互延迟（帧）", (x) => ImGuiEx.SliderInt(x, ref C.ExtraFrameDelay.ValidateRange(-10, 100), 0, 50), "该值越小，插件执行动作越快。若帧率较低或延迟较高，建议适当增大；若希望插件更快，可适当减小。")
        .Widget("额外日志", (x) => ImGui.Checkbox(x, ref C.ExtraDebug), "启用详细调试日志。开启后会产生大量日志并影响性能；重载插件或重启游戏后会自动关闭。")

            .Section("操作")
        .Widget("委托并重新委托", (x) =>
        {
            if(ImGui.RadioButton(x, C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = true;
                C.DontReassign = false;
            }
        }, "当已启用雇员没有进行中的探险时，会自动执行“自由探索委托”；若已有进行中的探险，则会重新委托当前探险。")
        .Widget("仅领取", (x) =>
        {
            if(ImGui.RadioButton(x, !C.EnableAssigningQuickExploration && C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = true;
            }
        }, "仅领取雇员探险奖励，不会重新委托。\n与传唤铃交互时按住 CTRL，可临时应用此模式。")
        .Widget("仅重新委托", (x) =>
        {
            if(ImGui.RadioButton("仅重新委托", !C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = false;
            }
        }, "仅重新委托雇员当前正在进行的探险。")
        .Widget("雇员感知", (x) => ImGui.Checkbox(x, ref C.RetainerSense), "当玩家进入传唤铃交互范围时，AutoRetainer 会自动启用。你需要保持静止，否则会取消启用。")
        .Widget(200f, "激活时间（秒）", (x) => ImGuiEx.SliderIntAsFloat(x, ref C.RetainerSenseThreshold, 1000, 100000));


}
