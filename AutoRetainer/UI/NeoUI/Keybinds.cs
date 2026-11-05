namespace AutoRetainer.UI.NeoUI;
public class Keybinds : NeoUIEntry
{
    public override string Path => "快捷键";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("传唤铃/航行管制面板快捷键")
        .Widget("临时阻止在使用传唤铃/航行管制面板时自动启用 AutoRetainer", (x) =>
        {
            UIUtils.DrawKeybind(x, ref C.Suppress);
        })
        .Widget("临时切换为“仅领取”模式（当前轮次不委托或重新委托探险）/临时将“远航探索”模式设为仅结算奖励", (x) =>
        {
            UIUtils.DrawKeybind(x, ref C.TempCollectB);
        })

        .Section("雇员快速操作")
        .Widget("出售", (x) => UIUtils.QRA(x, ref C.SellKey))
        .Widget("交给雇员保管", (x) => UIUtils.QRA(x, ref C.EntrustKey))
        .Widget("从雇员处取回", (x) => UIUtils.QRA(x, ref C.RetrieveKey))
        .Widget("到市场出售", (x) => UIUtils.QRA(x, ref C.SellMarketKey));
}
