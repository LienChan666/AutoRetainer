namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeCommon : NeoUIEntry
{
    public override string Path => "多角色模式/通用设置";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("通用设置")
        .Checkbox($"在登录界面等待", () => ref C.MultiWaitOnLoginScreen, "若当前没有可处理探险的角色，系统会登出并等待，直到有角色可用。启用此选项且多角色模式运行时，会禁用标题界面动画。")
        .Checkbox($"手动登录时禁用多角色模式", () => ref C.MultiDisableOnRelog, "通过 AutoRetainer 界面或命令重登时，将禁用多角色模式。")
        .Checkbox($"手动登录时不重置优先角色", () => ref C.MultiNoPreferredReset, "通过 AutoRetainer 界面或命令重登时，不重置优先角色。")
        .Checkbox("允许进入共享房屋", () => ref C.SharedHET)
        .Checkbox("即使多角色模式关闭，也在登录后尝试进屋", () => ref C.HETWhenDisabled)
        .Checkbox("已在传唤铃附近时，雇员流程不会传送或进屋", () => ref C.NoTeleportHetWhenNextToBell)

        .Section("游戏启动")
        .Checkbox($"游戏启动时启用多角色模式", () => ref C.MultiAutoStart)
        .Checkbox($"插件加载时启用多角色模式", () => ref C.MultiOnPluginLoad)
        .Indent()
        .SliderInt(150f, "延迟（秒）", () => ref C.MultiModeOnPluginLoadDelay, 0, 20)
        .Unindent()
        .Widget("游戏启动后自动登录", (x) =>
        {
            ImGui.SetNextItemWidth(150f);
            var names = C.OfflineData.Where(s => !s.Name.IsNullOrEmpty()).Select(s => $"{s.Name}@{s.World}");
            var dict = names.ToDictionary(s => s, s => Censor.Character(s));
            dict.Add("", "已禁用");
            dict.Add("~", "上次登录角色");
            ImGuiEx.Combo(x, ref C.AutoLogin, ["", "~", .. names], names: dict);
        })
        .SliderInt(150f, "延迟（秒）", () => ref C.AutoLoginDelay.ValidateRange(0, 60), 0, 20, "设置合适延迟，让插件在登录前完成加载，并预留手动取消登录的时间。")
        .Checkbox("插件重载后保留多角色模式状态", () => ref C.PreserveMultiModeState)

        .Section("背包警告")
        .InputInt(100f, $"雇员列表：剩余背包空位警告阈值", () => ref C.UIWarningRetSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"雇员列表：剩余探险币警告阈值", () => ref C.UIWarningRetVentureNum.ValidateRange(2, 1000))
        .InputInt(100f, $"远航探索列表：剩余背包空位警告阈值", () => ref C.UIWarningDepSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"远航探索列表：剩余燃料警告阈值", () => ref C.UIWarningDepTanksNum.ValidateRange(20, 1000))
        .InputInt(100f, $"远航探索列表：剩余魔导机械修理材料警告阈值", () => ref C.UIWarningDepRepairNum.ValidateRange(5, 1000))

        .Section("传送")
        .Widget(() => ImGuiEx.Text("需要安装 Lifestream 插件"))
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("Lifestream", new Version("2.2.1.1"))]))
        .TextWrapped("若要让此选项生效，你需要在 Lifestream 中为每个角色登记住宅，或启用简单传送。")
        .TextWrapped("你可以在角色配置菜单中按角色单独设置这些选项。")
        .Widget(() =>
        {
            if(Data != null && Data.GetAreTeleportSettingsOverriden())
            {
                ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, "当前角色使用了自定义传送选项。");
            }
        })
        .Checkbox("启用", () => ref C.GlobalTeleportOptions.Enabled)
        .Indent()
        .Checkbox("雇员流程传送...", () => ref C.GlobalTeleportOptions.Retainers)
        .Indent()
        .Checkbox("...传送到个人房屋", () => ref C.GlobalTeleportOptions.RetainersPrivate)
        .Checkbox("...传送到共享房屋", () => ref C.GlobalTeleportOptions.RetainersShared)
        .Checkbox("...传送到部队房屋", () => ref C.GlobalTeleportOptions.RetainersFC)
        .Checkbox("...传送到公寓", () => ref C.GlobalTeleportOptions.RetainersApartment)
        .TextWrapped("若以上选项都禁用或失败，将传送到旅馆。")
        .Unindent()
        .Checkbox("远航探索流程传送到部队房屋", () => ref C.GlobalTeleportOptions.Deployables)
        .Checkbox("启用简单传送", () => ref C.AllowSimpleTeleport)
        .Unindent()
        .Widget(() => ImGuiEx.HelpMarker("""
            允许在未在 Lifestream 登记房屋时进行传送。注意：传送功能仍需安装 Lifestream 插件。

            警告：此选项稳定性低于在 Lifestream 正确登记房屋，仅在必要时使用。
            """, EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString()))

        .Section("脱困模块")
        .Checkbox("连接错误时自动关闭提示并重试登录", () => ref C.ResolveConnectionErrors, "断线后 AutoRetainer 会尝试自动关闭提示并重新登录。若会话已过期，则不会尝试登录。")
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("NoKillPlugin")]));
}
