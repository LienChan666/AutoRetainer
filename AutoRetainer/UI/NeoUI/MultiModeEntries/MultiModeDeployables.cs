using ECommons.Throttlers;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeDeployables : NeoUIEntry
{
public override string Path => "多角色模式/远航探索";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("多角色模式 - 远航探索")
        .Checkbox("等待飞空艇/潜水艇返航", () => ref C.MultiModeWorkshopConfiguration.MultiWaitForAll, """启用后，AutoRetainer 会等待所有飞空艇和潜水艇返航后再登录该角色。若你因其他原因已登录，仍会再次派遣已完成的飞空艇和潜水艇；除非全局也启用了“即使已登录也等待”。""")
        .Indent()
        .Checkbox("即使已登录也等待", () => ref C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn, """修改“等待飞空艇/潜水艇返航”（全局与角色级）行为：在已登录状态下不再逐艘再次派遣飞空艇或潜水艇，而是等待全部飞空艇和潜水艇返航后再处理。""")
        .InputInt(120f, "最大等待时间（分钟）", () => ref C.MultiModeWorkshopConfiguration.MaxMinutesOfWaiting.ValidateRange(0, 9999), 10, 60, """若等待其他飞空艇或潜水艇返航的时间超过该分钟数，AutoRetainer 会忽略“等待飞空艇/潜水艇返航”和“即使已登录也等待”两项设置。""")
        .Unindent()
        .DragInt(60f, "提前重登阈值（秒）", () => ref C.MultiModeWorkshopConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300, "该角色的飞空艇或潜水艇可再次派遣前，AutoRetainer 提前登录的秒数。")
        .DragInt(120f, "雇员探险处理截止时间（分钟）", () => ref C.DisableRetainerVesselReturn.ValidateRange(0, 60), "若该值大于 0，AutoRetainer 会在任意角色计划再次派遣飞空艇或潜水艇前指定分钟数停止处理雇员（综合前述设置）。")
        .Checkbox("派遣后立即出售“无条件出售列表”中的物品（需启用雇员流程）", () => ref C.VendorItemAfterVoyage)
        .Checkbox("进入部队工房时定期检查部队储物柜中的金币", () => ref C.FCChestGilCheck, "进入部队工房时会定期检查部队储物柜，以保持金币统计最新。")
        .Indent()
        .SliderInt(150f, "检查频率（小时）", () => ref C.FCChestGilCheckCd, 0, 24 * 5)
        .Widget("重置冷却", (x) =>
        {
            if(ImGuiEx.Button(x, C.FCChestGilCheckTimes.Count > 0)) C.FCChestGilCheckTimes.Clear();
        })
        .Unindent()
        .Checkbox("全部飞空艇/潜水艇处理完成后关闭游戏", () => ref C.ShutdownOnSubExhaustion)
        .Indent()
        .SliderFloat(150f, "若有飞空艇或潜水艇在该小时数内返航则不关机", () => ref C.HoursForShutdown, 0f, 10f)
        .Widget(() =>
        {
            ImGuiEx.HelpMarker($"""
                当前状态：{(Utils.CanShutdownForSubs() ? "可以关机" : "无法关机")}
                距离强制关机剩余：{EzThrottler.GetRemainingTime("ForceShutdownForSubs")}
                """);
        })
        .Unindent()
        .TextWrapped("进入部队工房后自动购买桶装青磷水：")
        .Indent()
        .Widget(() =>
        {
            if(Data != null)
            {
                ImGui.Checkbox($"在 {Data.NameWithWorldCensored} 上启用", ref Data.AutoFuelPurchase);
            }
            ImGuiEx.TextWrapped($"要为其他角色启用/禁用燃料购买，请前往“功能、排除与顺序”分区。");
        })
        .InputInt(150f, "触发购买时的剩余桶装青磷水数量", () => ref C.AutoFuelPurchaseLow.ValidateRange(100, 99999))
        .InputInt(150f, "购买至背包数量达到此值", () => ref C.AutoFuelPurchaseMax)
        .Checkbox("仅在工作站未锁定时购买", () => ref C.AutoFuelPurchaseOnlyWsUnlocked)
        .Unindent()
        .Checkbox("远航探索处理完成后退出游戏", () => ref C.ExitOnSubCompletion, "重要：启用后，多角色模式将只处理远航探索，不处理雇员。")
        .Indent()
        .InputInt(150f, "等待飞空艇/潜水艇返航的最长时间（分钟）", () => ref C.ExitOnSubCompletionTime)
        .Unindent()
        ;
}
