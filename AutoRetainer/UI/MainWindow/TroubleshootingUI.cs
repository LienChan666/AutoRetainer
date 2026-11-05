using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainerAPI.Configuration;
using Dalamud.Game;
using ECommons.GameHelpers;
using ECommons.Reflection;

namespace AutoRetainer.UI.MainWindow;
public static unsafe class TroubleshootingUI
{
    private static readonly Config EmptyConfig = new();

    public static bool IsPluginInstalled(string name)
    {
        return Svc.PluginInterface.InstalledPlugins.Any(x => x.IsLoaded && (x.InternalName.EqualsIgnoreCase(name) || x.Name.EqualsIgnoreCase(name)));
    }

    public static void Draw()
    {
        ImGuiEx.TextWrapped("此页会检查配置中的常见问题，建议先自行排查后再联系支持。");

        if(!Player.Available)
        {
            ImGuiEx.TextWrapped($"未登录时无法执行故障排查。");
            return;
        }

        if(C.CutsceneSkipMode != AutoRetainerAPI.Configuration.CutsceneSkipMode.Never)
        {
            Info($"旅馆过场动画跳过模式已设为“{Lang.CutsceneSkipModeNames[C.CutsceneSkipMode]}”，AutoRetainer 将按此设置跳过旅馆过场动画。");
        }

        if(Data == null)
        {
            Error($"当前角色暂无数据。请打开传唤铃或航行管制面板，或登出一次以生成数据。");
        }

        if(C.IgnoreGCRankCheck)
        {
            Error("已启用“忽略大国防联军军衔检查”。请执行 /ays set IgnoreGCRankCheck false 将其关闭，以恢复插件正常运行。");
        }

        if(!Svc.ClientState.ClientLanguage.EqualsAny(ClientLanguage.Japanese, ClientLanguage.German, ClientLanguage.French, ClientLanguage.English))
        {
            Error($"检测到本地发行商客户端。AutoRetainer 未验证在该发行商 FFXIV 客户端上的兼容性，部分或全部功能可能失效。此外请注意，ottercorp 的中文 Dalamud 分支可能在未经同意且不可关闭的情况下收集你的电脑、角色、已用插件和 Dalamud 配置信息。");
        }

        if(C.DontLogout)
        {
            Error("已启用 DontLogout 调试选项");
        }

        if(C.FullAutoGCDelivery) 
        {
            int maxRetainersWhenGcDelivery = 0;
            var warnSub = false;
            foreach(var x in C.OfflineData)
            {
                if(x.Enabled && x.GCDeliveryType != GCDeliveryType.Disabled)
                {
                    maxRetainersWhenGcDelivery = Math.Max(maxRetainersWhenGcDelivery, x.GetEnabledRetainers(false).Length);
                }
                if(x.WorkshopEnabled && x.GetEnabledVesselsData(VoyageType.Submersible).Count > 0)
                {
                    warnSub = true;
                }
            }
            if(warnSub && C.FullAutoGCDeliveryInventory < 50)
            {
                Warning($"已启用远航探索模块，但多角色模式筹备稀有品的背包空位触发值仅为 {C.FullAutoGCDeliveryInventory}。建议至少设为 50，以免背包溢出。");
            }
            if(C.FullAutoGCDeliveryInventory < maxRetainersWhenGcDelivery * 5)
            {
                Warning($"部分多角色模式角色启用了 {maxRetainersWhenGcDelivery} 名雇员，但筹备稀有品的背包空位触发值仅为 {C.FullAutoGCDeliveryInventory}。强烈建议至少设为 {5 * maxRetainersWhenGcDelivery}（每名雇员预留 5 个空位）。");
            }
        }

        foreach(var x in C.OfflineData)
        {
            if(x.WorkshopEnabled)
            {
                var a = x.OfflineSubmarineData.Select(x => x.Name);
                if(a.Count() > a.Distinct().Count())
                {
                    Error($"角色 {Censor.Character(x.Name, x.World)} 存在重复潜水艇名称。潜水艇名称必须唯一。");
                }
            }
        }

        if((C.GlobalTeleportOptions.Enabled || C.OfflineData.Any(x => x.TeleportOptionsOverride.Enabled == true)) && !Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "Lifestream" && x.IsLoaded))
        {
            Error("已启用传送，但 Lifestream 未安装或未加载。AutoRetainer 无法在此配置下运行；请禁用传送或安装并启用 Lifestream。");
        }

        foreach(var x in C.SubmarineUnlockPlans)
        {
            if(x.EnforcePlan)
            {
                Info($"潜水艇解锁方案 {x.Name.NullWhenEmpty() ?? x.GUID} 已设为强制执行；只要存在可解锁内容，就会覆盖潜水艇自身设置。");
            }
        }

        foreach(var x in C.SubmarineUnlockPlans)
        {
            if(x.EnforceDSSSinglePoint)
            {
                Info($"潜水艇解锁方案 {x.Name.NullWhenEmpty() ?? x.GUID} 已设为在溺没海单点派遣，将忽略手动设置的解锁行为。");
            }
        }

        try
        {
            if(DalamudReflector.IsOnStaging())
            {
                Error($"检测到非 release 的 Dalamud 分支，可能导致问题。若可行，请输入 /xlbranch 打开分支切换器，切换到“release”并重启游戏。");
            }
        }
        catch(Exception e)
        {
        }

        if(Player.Available)
        {
            if(Player.CurrentWorld != Player.HomeWorld)
            {
                Error("你当前不在原始服务器。请先返回原始服务器，AutoRetainer 才能继续处理该角色。");
            }
            if(C.Blacklist.Any(x => x.CID == Player.CID))
            {
                Error("当前角色已被完全排除，不会被 AutoRetainer 以任何方式处理。请前往设置 - 排除进行修改。");
            }
            if(Data?.ExcludeRetainer == true)
            {
                Error("当前角色已从雇员列表中排除。请前往设置 - 排除进行修改。");
            }
            if(Data?.ExcludeWorkshop == true)
            {
                Error("当前角色已从远航探索列表中排除。请前往设置 - 排除进行修改。");
            }
        }

        {
            var list = C.OfflineData.Where(x => x.GetAreTeleportSettingsOverriden());
            if(list.Any())
            {
                Info("你的部分角色使用了自定义传送选项。悬停可查看列表。", list.Select(x => $"{x.Name}@{x.World}").Print("\n"));
            }
        }

        if(C.NoTeleportHetWhenNextToBell)
        {
            Warning("角色靠近传唤铃时将禁用传送或进入住宅/公寓。请注意房屋自动拆除倒计时。");
        }



        if(C.AllowSimpleTeleport)
        {
            Warning("已启用简单传送。其稳定性低于在 Lifestream 中登记住宅。若你遇到传送问题，建议禁用此选项并在 Lifestream 中登记房屋。");
        }

        if(!C.EnableEntrustManager && C.AdditionalData.Any(x => x.Value.EntrustPlan != Guid.Empty))
        {
            Warning($"物品转存管理已在全局禁用，但部分雇员仍分配了转存方案。这些方案只能手动执行。");
        }

        if(C.ExtraDebug)
        {
            Info("额外日志选项已启用。会大量刷日志，仅在收集调试信息时使用。");
        }

        if(C.UnsyncCompensation > -5)
        {
            Warning("“时间不同步补偿”设置过高（>-5），可能导致问题。");
        }

        if(UIUtils.GetFPSFromMSPT(C.TargetMSPTIdle) < 10)
        {
            Warning("空闲目标帧率设置过低（<10），可能导致问题。");
        }

        if(UIUtils.GetFPSFromMSPT(C.TargetMSPTRunning) < 20)
        {
            Warning("运行目标帧率设置过低（<20），可能导致问题。");
        }

        if(Data?.GetIMSettings().AllowSellFromArmory == true)
        {
            Info("已启用可从兵装库出售物品。请务必将零式大型任务装备和绝境战武器加入保护列表。");
        }

        {
            var list = C.OfflineData.Where(x => !x.ExcludeRetainer && !x.Enabled && x.RetainerData.Count > 0);
            if(list.Any())
            {
                Warning($"部分角色拥有雇员但未启用雇员多角色模式。悬停可查看列表。", list.Print("\n"));
            }
        }
        {
            var list = C.OfflineData.Where(x => !x.ExcludeRetainer && x.Enabled && x.RetainerData.Count > 0 && C.SelectedRetainers.TryGetValue(x.CID, out var rd) && !x.RetainerData.All(r => rd.Contains(r.Name)));
            if(list.Any())
            {
                Warning($"部分角色并未启用其全部雇员进行处理。悬停可查看列表。", list.Print("\n"));
            }
        }
        {
            var list = C.OfflineData.Where(x => !x.ExcludeWorkshop && !x.WorkshopEnabled && (x.OfflineSubmarineData.Count + x.OfflineAirshipData.Count) > 0);
            if(list.Any())
            {
                Warning($"部分角色已登记飞空艇/潜水艇但未启用远航探索多角色模式。悬停可查看列表。", list.Print("\n"));
            }
        }

        {
            var list = C.OfflineData.Where(x => !x.ExcludeWorkshop && x.WorkshopEnabled && x.GetEnabledVesselsData(Internal.VoyageType.Airship).Count + x.GetEnabledVesselsData(Internal.VoyageType.Submersible).Count < Math.Min(x.OfflineAirshipData.Count + x.OfflineSubmarineData.Count, 4));
            if(list.Any())
            {
                Warning($"部分角色并未启用其全部飞空艇/潜水艇进行处理。悬停可查看列表。", list.Print("\n"));
            }
        }

        if(C.MultiModeType != AutoRetainerAPI.Configuration.MultiModeType.Everything)
        {
            Warning($"你的多角色模式类型设置为“{Lang.MultiModeTypeNames[C.MultiModeType]}”。这会限制 AutoRetainer 可执行的功能。");
        }

        if(C.OfflineData.Any(x => x.MultiWaitForAllDeployables))
        {
            Info("部分角色启用了“等待飞空艇/潜水艇返航”选项。这些角色会在全部飞空艇和潜水艇返航后再处理。悬停可查看完整列表。", C.OfflineData.Where(x => x.MultiWaitForAllDeployables).Select(x => $"{x.Name}@{x.World}").Print("\n"));
        }

        if(C.MultiModeWorkshopConfiguration.MultiWaitForAll)
        {
            Info("已启用全局选项“等待飞空艇/潜水艇返航”。这意味着所有角色都会等待全部飞空艇和潜水艇返航后再处理，即使某些角色未启用角色级设置。");
        }

        if(C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn)
        {
            Info("已为远航探索启用“即使已登录也等待”。这意味着即使你已登录，AutoRetainer 也会等待该角色全部飞空艇和潜水艇返航后再处理。");
        }

        if(C.DisableRetainerVesselReturn > 0)
        {
            if(C.DisableRetainerVesselReturn > 10)
            {
                Warning("“雇员探险处理截止时间”设置过高，飞空艇或潜水艇即将返航时，雇员重新委托可能出现明显延迟。");
            }
            else
            {
                Info("已启用“雇员探险处理截止时间”选项。飞空艇或潜水艇即将返航时，雇员重新委托可能出现延迟。");
            }
        }

        if(C.MultiModeRetainerConfiguration.MultiWaitForAll)
        {
            Info("已启用“等待探险完成”选项。这意味着 AutoRetainer 会等待该角色所有雇员探险完成后再登录处理。");
        }

        if(C.MultiModeRetainerConfiguration.WaitForAllLoggedIn)
        {
            Info("已为雇员启用“即使已登录也等待”。这意味着即使你已登录，AutoRetainer 也会等待该角色全部雇员探险完成后再处理。");
        }

        {
            var manualList = new List<string>();
            var deletedList = new List<string>();
            foreach(var x in C.OfflineData)
            {
                foreach(var ret in x.RetainerData)
                {
                    var planId = Utils.GetAdditionalData(x.CID, ret.Name).EntrustPlan;
                    var plan = C.EntrustPlans.FirstOrDefault(s => s.Guid == planId);
                    if(plan != null && plan.ManualPlan) manualList.Add($"{Censor.Character(x.Name)} - {Censor.Retainer(ret.Name)}");
                    if(plan == null && planId != Guid.Empty) deletedList.Add($"{Censor.Character(x.Name)} - {Censor.Retainer(ret.Name)}");
                }
            }
            if(manualList.Count > 0)
            {
                Info("部分雇员使用了手动转存方案。这些方案不会在再次派遣雇员探险后自动执行，只能通过覆盖层按钮手动执行。悬停可查看列表。", manualList.Print("\n"));
            }
            if(deletedList.Count > 0)
            {
                Warning("部分雇员所分配的转存方案已被删除。使用已删除方案的雇员不会转存任何物品。悬停可查看列表。", deletedList.Print("\n"));
            }
        }

        if(C.No2ndInstanceNotify)
        {
            Info("你已启用“同目录第二游戏实例不再警告”选项。这会在使用同一 Dalamud 目录的第二个游戏实例中自动跳过 AutoRetainer 加载。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "SimpleTweaksPlugin" && x.IsLoaded))
        {
            Info("检测到 Simple Tweaks 插件。与雇员或远航探索相关的调整可能影响 AutoRetainer 功能。请确保相关调整不会干扰 AutoRetainer。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "PandorasBox" && x.IsLoaded))
        {
            Info("检测到 Pandora's Box 插件。AutoRetainer 启用期间自动释放技能可能影响功能。请将 Pandora's Box 配置为在 AutoRetainer 运行时不执行自动操作。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "Automaton" && x.IsLoaded))
        {
            Info("检测到 Automaton 插件。AutoRetainer 启用期间自动释放技能和自动数字输入可能影响功能。请将 Automaton 配置为在 AutoRetainer 运行时不执行自动操作。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "RotationSolver" && x.IsLoaded))
        {
            Info("检测到 RotationSolver 插件。AutoRetainer 启用期间自动释放技能可能影响功能。请将 RotationSolver 配置为在 AutoRetainer 运行时不执行自动操作。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName.StartsWith("BossMod") && x.IsLoaded))
        {
            Info("检测到 BossMod 插件。AutoRetainer 启用期间自动释放技能可能影响功能。请将 BossMod 配置为在 AutoRetainer 运行时不执行自动操作。");
        }

        ImGui.Separator();
        ImGuiEx.TextWrapped("专家设置会改变开发者的预设行为。请先确认你的问题并非由专家设置配置错误导致。");
        CheckExpertSetting("访问传唤铃时（无可处理探险）动作", nameof(C.OpenBellBehaviorNoVentures));
        CheckExpertSetting("访问传唤铃时（有可处理探险）动作", nameof(C.OpenBellBehaviorWithVentures));
        CheckExpertSetting("访问传唤铃后任务完成行为", nameof(C.TaskCompletedBehaviorAccess));
        CheckExpertSetting("手动启用后任务完成行为", nameof(C.TaskCompletedBehaviorManual));
        CheckExpertSetting("若有雇员探险将在 5 分钟内完成，则停留在雇员菜单", nameof(C.Stay5));
        CheckExpertSetting("关闭雇员列表时自动禁用插件", nameof(C.AutoDisable));
        CheckExpertSetting("不显示插件状态图标", nameof(C.HideOverlayIcons));
        CheckExpertSetting("显示多角色模式类型选择器", nameof(C.DisplayMMType));
        CheckExpertSetting("在部队工房界面显示飞空艇/潜水艇复选框", nameof(C.ShowDeployables));
        CheckExpertSetting("启用脱困模块", nameof(C.EnableBailout));
        CheckExpertSetting("AutoRetainer 尝试脱离卡死状态前的超时（秒）", nameof(C.BailoutTimeout));
        CheckExpertSetting("禁用排序与折叠/展开", nameof(C.NoCurrentCharaOnTop));
        CheckExpertSetting("在插件快捷栏显示“多角色模式”复选框", nameof(C.MultiModeUIBar));
        CheckExpertSetting("雇员菜单延迟（秒）", nameof(C.RetainerMenuDelay));
        CheckExpertSetting("不对探险规划器进行错误检查", nameof(C.NoErrorCheckPlanner2));
        CheckExpertSetting("启用多角色模式时，尝试进入附近房屋", nameof(C.MultiHETOnEnable));
        CheckExpertSetting("Artisan 联动", nameof(C.ArtisanIntegration));
        CheckExpertSetting("使用服务器时间而非本机时间", nameof(C.UseServerTime));
    }

    private static void Error(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.RedBright, "\uf057");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.RedBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void Warning(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.OrangeBright, "\uf071");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.OrangeBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void Info(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.YellowBright, "\uf05a");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.YellowBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void CheckExpertSetting(string setting, string nameOfSetting)
    {
        var original = EmptyConfig.GetFoP(nameOfSetting);
        var current = C.GetFoP(nameOfSetting);
        if(!original.Equals(current))
        {
            Info($"专家设置“{setting}”与默认值不同", $"默认值为“{original}”，当前值为“{current}”。");
        }
    }
}
