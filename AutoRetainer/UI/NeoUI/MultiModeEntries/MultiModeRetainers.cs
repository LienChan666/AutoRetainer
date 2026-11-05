namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeRetainers : NeoUIEntry
{
    public override string Path => "多角色模式/雇员";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("多角色模式 - 雇员")
        .Checkbox("等待探险完成", () => ref C.MultiModeRetainerConfiguration.MultiWaitForAll, "多角色模式下，AutoRetainer 会等待全部雇员探险返回后再切换到下一个角色。")
        .DragInt(60f, "提前重登阈值（秒）", () => ref C.MultiModeRetainerConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300)
        .SliderInt(100f, "继续运行所需最小背包空位", () => ref C.MultiMinInventorySlots.ValidateRange(2, 9999), 2, 30)
        .Checkbox("同步雇员（一次）", () => ref MultiMode.Synchronize, "AutoRetainer 会等待所有已启用雇员完成探险。完成后该设置会自动关闭，并继续处理全部角色。")
        .Checkbox($"强制完整角色轮换", () => ref C.CharEqualize, "推荐拥有 15 个以上角色的用户启用。会强制多角色模式按顺序处理完所有角色后再回到循环起点。")
        .Indent()
        .Checkbox("按探险完成时间排序角色", () => ref C.LongestVentureFirst, "探险完成时间更久的角色会优先检查。")
        .Checkbox("按雇员等级与等级封顶状态排序角色", () => ref C.CappedLevelsLast, "优先处理有可升级雇员的角色；其次是雇员已满级角色；最后是未满级但受等级上限限制的角色。")
        .Unindent();
}
