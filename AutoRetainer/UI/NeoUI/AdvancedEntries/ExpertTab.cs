using ECommons.Configuration;
using ECommons.Reflection;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries;
public class ExpertTab : NeoUIEntry
{
    public override string Path => "高级/专家设置";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("行为")
        .EnumComboFullWidth(null, "打开传唤铃（无可领取探险）时的行为：", () => ref C.OpenBellBehaviorNoVentures, names: Lang.OpenBellBehaviorNames)
        .EnumComboFullWidth(null, "打开传唤铃（有可领取探险）时的行为：", () => ref C.OpenBellBehaviorWithVentures, names: Lang.OpenBellBehaviorNames)
        .EnumComboFullWidth(null, "访问传唤铃后任务完成行为：", () => ref C.TaskCompletedBehaviorAccess, names: Lang.TaskCompletedBehaviorNames)
        .EnumComboFullWidth(null, "手动启用后任务完成行为：", () => ref C.TaskCompletedBehaviorManual, names: Lang.TaskCompletedBehaviorNames)
        .EnumComboFullWidth(null, "插件运行中任务完成行为：", () => ref C.TaskCompletedBehaviorAuto, names: Lang.TaskCompletedBehaviorNames)
        .TextWrapped(ImGuiColors.DalamudGrey, "前 3 项中的“关闭雇员列表并禁用插件”选项在多角色模式下会被强制执行。")
        .Checkbox("若有雇员探险将在 5 分钟内完成，则停留在雇员菜单", () => ref C.Stay5, "该选项在多角色模式运行期间会被强制应用。")
        .Checkbox($"关闭雇员列表时自动禁用插件", () => ref C.AutoDisable, "仅在你手动退出菜单时生效；其他情况遵循上方设置。")
        .Checkbox($"不显示插件状态图标", () => ref C.HideOverlayIcons)
        .Checkbox($"显示多角色模式类型选择器", () => ref C.DisplayMMType)
        .Checkbox($"在部队工房界面显示飞空艇/潜水艇复选框", () => ref C.ShowDeployables)
        .Checkbox("启用脱困模块", () => ref C.EnableBailout)
        .InputInt(150f, "AutoRetainer 尝试脱离卡死状态前的超时（秒）", () => ref C.BailoutTimeout)

        .Section("设置")
        .Checkbox("允许处理未设置职业的雇员", () => ref C.AllowUnemployed)
        .Widget("跳过旅馆登录过场动画", text =>
        {
            ImGui.SetNextItemWidth(200);
            if(ImGuiEx.EnumCombo(text, ref C.CutsceneSkipMode, Lang.CutsceneSkipModeNames))
            {
                S.InnCutsceneSkip.RefreshAccordingToConfig();
            }
            ImGuiEx.HelpMarker("服务器端可检测到跳过过场动画，可能增加封禁风险", EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString());
        })
        .Checkbox($"禁用排序与折叠/展开", () => ref C.NoCurrentCharaOnTop)
        .Checkbox($"在插件快捷栏显示 MultiMode 复选框", () => ref C.MultiModeUIBar)
        .SliderIntAsFloat(100f, "雇员菜单延迟（秒）", () => ref C.RetainerMenuDelay.ValidateRange(0, 2000), 0, 2000)
        .Checkbox($"允许探险计时显示负数", () => ref C.TimerAllowNegative)
        .Checkbox($"不对探险规划器进行错误检查", () => ref C.NoErrorCheckPlanner2)
        .Checkbox("启用手动重登后的角色后处理流程", () => ref C.AllowManualPostprocess, "即使 AutoRetainer 处于后处理锁定状态，也允许手动命令调用。")
        .Widget("市场冷却覆盖层", (x) =>
        {
            if(ImGui.Checkbox(x, ref C.MarketCooldownOverlay))
            {
                if(C.MarketCooldownOverlay)
                {
                    P.Memory.OnReceiveMarketPricePacketHook?.Enable();
                }
                else
                {
                    P.Memory.OnReceiveMarketPricePacketHook?.Disable();
                }
            }
        })

        .Section("联动")
        .Checkbox($"Artisan 联动", () => ref C.ArtisanIntegration, "与 Artisan 联动：当附近有传唤铃且雇员探险可领取时，AutoRetainer 会自动启用并暂停 Artisan；处理完成后会重新启用 Artisan，并继续此前的操作。")

        .Section("服务器时间")
        .Checkbox("使用服务器时间而非本机时间", () => ref C.UseServerTime)

        .Section("实用工具")
        .Widget("清理幽灵雇员", (x) =>
        {
            if(ImGui.Button(x))
            {
                var i = 0;
                foreach(var d in C.OfflineData)
                {
                    i += d.RetainerData.RemoveAll(x => x.Name == "");
                }
                DuoLog.Information($"已清理 {i} 条记录");
            }
        })

        .Section("导入/导出")
        .Widget(() =>
        {
            if(ImGui.Button("不含角色数据导出"))
            {
                var clone = C.JSONClone();
                clone.OfflineData = null;
                clone.AdditionalData = null;
                clone.FCData = null;
                clone.SelectedRetainers = null;
                clone.Blacklist = null;
                clone.AutoLogin = "";
                Copy(EzConfig.DefaultSerializationFactory.Serialize(clone, false));
            }
            if(ImGui.Button("导入并与角色数据合并"))
            {
                try
                {
                    var c = EzConfig.DefaultSerializationFactory.Deserialize<Config>(Paste());
                    c.OfflineData = C.OfflineData;
                    c.AdditionalData = C.AdditionalData;
                    c.FCData = C.FCData;
                    c.SelectedRetainers = C.SelectedRetainers;
                    c.Blacklist = C.Blacklist;
                    c.AutoLogin = C.AutoLogin;
                    if(c.GetType().GetFieldPropertyUnions().Any(x => x.GetValue(c) == null)) throw new NullReferenceException();
                    EzConfig.SaveConfiguration(C, $"Backup_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.json");
                    P.SetConfig(c);
                }
                catch(Exception e)
                {
                    e.LogDuo();
                }
            }
        });
}
