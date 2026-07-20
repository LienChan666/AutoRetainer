using AutoRetainer.Modules.Voyage;
using AutoRetainer.Scheduler.Tasks;
using Dalamud.Utility;
using ECommons.Configuration;
using ECommons.Events;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.Interop;
using ECommons.MathHelpers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using ItemLevel = AutoRetainer.Helpers.ItemLevel;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugMisc : DebugSectionBase
{
    public override void Draw()
    {
        ImGui.Checkbox("禁用渲染详细日志", ref RenderDisableManager.Debug);
        if(ImGui.Button("执行插件终止器"))
        {
            S.PluginTerminator.OnUpdate();
        }
        ImGuiEx.Text($"离线潜水艇数据数量 {Data.OfflineSubmarineData.Count}，潜水艇栏位数 {Data.NumSubSlots}");
        ImGuiEx.Text($"部队等级：{Utils.FCRank}");
        if(ImGui.CollapsingHeader("API 测试 1"))
        {
            try
            {
                ImGuiEx.Text($"{P.API.Config.FCData}");
            }
            catch(Exception e)
            {
                ImGuiEx.Text($"{e}");
            }
        }
        if(ImGuiEx.Button("传送"))
        {
            MultiMode.RunTeleportLogic();
        }
        if(ImGui.CollapsingHeader("资格检查"))
        {
            ImGuiEx.Text($"""
                当前角色：
                已委托探险：{Data?.SentVenturesByDay.Sum(x => x.Value)}
                已派遣远航探索：{Data?.SentVoyagesByDay.Sum(x => x.Value)}
                最大已启用雇员数：{Data?.GetEnabledRetainers(false).Length}
                全部探险委托数：{C.OfflineData.Sum(x => x.SentVenturesByDay.Select(x => x.Value).Sum())}
                全部远航探索派遣数：{C.OfflineData.Sum(x => x.SentVoyagesByDay.Select(x => x.Value).Sum())}
                全局最大已启用雇员数：{C.OfflineData.Select(x => x.GetEnabledRetainers().Length).MaxSafe()}
                已启用雇员的角色数：{C.OfflineData.Where(x => x.GetEnabledRetainers().Length > 0 && x.Enabled).Count()}
                已启用潜水艇的角色数：{C.OfflineData.Where(x => x.GetEnabledVesselsData(Internal.VoyageType.Submersible).Count > 0 && x.WorkshopEnabled).Count()}
                ---------
                按天：
                """);
            var days = C.OfflineData.Select(x => (long[])[..x.SentVenturesByDay.Keys, ..x.SentVoyagesByDay.Keys]).SelectNested(x => x).ToHashSet();
            ImGui.Indent();
            foreach(var x in days)
            {
                ImGuiEx.Text($"{x}：已委托探险：{C.OfflineData.Select(c => c.SentVenturesByDay.SafeSelect(x)).Sum()}，已派遣远航探索：{C.OfflineData.Select(c => c.SentVoyagesByDay.SafeSelect(x)).Sum()}");
            }
            ImGui.Unindent();
            ImGuiEx.Text($"""
                ---------
                按角色：
                """);
            foreach(var x in C.OfflineData)
            {
                ImGuiEx.Text($"{x.NameWithWorld}：已委托探险：{x.SentVenturesByDay.Sum(s => s.Value)}，已派遣远航探索：{x.SentVoyagesByDay.Sum(s => s.Value)}");
            }
        }
        if(ImGui.CollapsingHeader("部队特效"))
        {
            ImGuiEx.Text($"数量：{TaskActivateSealSweetener.NumActions}");
            foreach(var x in TaskActivateSealSweetener.Actions)
            {
                ImGuiEx.Text($"{x} / {Svc.Data.GetExcelSheet<CompanyAction>().GetRowOrDefault((uint)x)?.Name}");
            }
            ImGuiEx.FilteringInputInt("回调值 1", out var val1);
            ImGuiEx.FilteringInputInt("回调值 2", out var val2);
            if(ImGui.Button("在部队窗口触发"))
            {
                if(TryGetAddonByName<AtkUnitBase>("FreeCompany", out var addon) && addon->IsReady())
                {
                    Callback.Fire(addon, true, val1, (uint)val2);
                }
            }
            if(ImGui.Button("在部队特效窗口触发"))
            {
                if(TryGetAddonByName<AtkUnitBase>("FreeCompanyAction", out var addon) && addon->IsReady())
                {
                    Callback.Fire(addon, true, val1, (uint)val2);
                }
            }
            if(ImGui.Button("加入“军票提高”激活任务"))
            {
                TaskActivateSealSweetener.Enqueue();
            }
            if(ImGui.Button("限频加入“军票提高”激活任务"))
            {
                TaskActivateSealSweetener.EnqueueThrottled();
            }
        }
        if(ImGui.CollapsingHeader("618"))
        {
            var a = Svc.Data.GetExcelSheet<Lobby>().GetRow(618).Text.ToDalamudString();
            foreach(var pl in a.Payloads)
            {
                ImGuiEx.Text($"{pl.Type}: {pl.ToString()}");
            }
        }
        if(ImGui.CollapsingHeader("快捷菜单"))
        {
            if(TryGetAddonMaster<AddonMaster.ContextMenu>(out var m) && m.IsAddonReady)
            {
                foreach(var x in m.Entries)
                {
                    ImGuiEx.Text($"{x.Text}/{Lang.Bool(x.Enabled)}");
                }
            }
        }
        if(ImGui.CollapsingHeader("雇员物品属性"))
        {
            var im = InventoryManager.Instance();
            var c = im->GetInventoryContainer(InventoryType.RetainerEquippedItems);
            for(var i = 0; i < c->Size; i++)
            {
                var slot = c->GetInventorySlot(i);
                ImGuiEx.Text($"{i}（{slot->GetItemId()}）：{ExcelItemHelper.GetName(slot->GetItemId() % 1000000)}，获得力：{slot->GetStat(BaseParamEnum.Gathering)} [{slot->GetStatCap(BaseParamEnum.Gathering)}]，鉴别力：{slot->GetStat(BaseParamEnum.Perception)} [{slot->GetStatCap(BaseParamEnum.Perception)}]");
            }
        }
        if(ImGui.Button("通过外部进程写入配置"))
        {
            ExternalWriter.PlaceWriteOrder(new(System.IO.Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "WriterTest.json"), EzConfig.DefaultSerializationFactory.Serialize(C, true)));
        }
        ImGuiEx.Text($"部队战绩：{Utils.FCPoints}");
        if(ImGui.CollapsingHeader("住宅"))
        {
            var h = HousingManager.Instance();
            ImGuiEx.Text($"当前分区：{h->GetCurrentDivision()}");
            ImGuiEx.Text($"当前房屋 ID：{h->GetCurrentIndoorHouseId()}");
            ImGuiEx.Text($"当前门牌：{h->GetCurrentPlot()}");
            ImGuiEx.Text($"当前房间：{h->GetCurrentRoom()}");
            ImGuiEx.Text($"当前小区：{h->GetCurrentWard()}");
            if(ImGui.Button("模拟登录"))
            {
                ProperOnLogin.FireArtificially();
            }
            if(h->OutdoorTerritory != null)
            {
                for(var i = 0; i < 30; i++)
                {
                    ImGuiEx.Text($"是否为住宅居民 {i}：{Lang.Bool(P.Memory.OutdoorTerritory_IsEstateResident((nint)h->OutdoorTerritory, (byte)i) != 0)}");
                }
            }
        }
        if(ImGui.Button("安装回调钩子")) Callback.InstallHook();
        if(ImGui.Button("禁用回调钩子")) Callback.UninstallHook();
        ImGuiEx.TextCopy($"{(nint)(&TargetSystem.Instance()->Target):X16}");
        ImGui.Checkbox($"记录操作码", ref P.LogOpcodes);
        ImGuiEx.Text($"框架帧计数：{CSFramework.Instance()->FrameCounter}");
        if(ImGui.Button("测试同类道具合并递交"))
        {
            if(TryGetAddonByName<AtkUnitBase>("RetainerItemTransferList", out var addon))
            {
                Callback.Fire(addon, true, 0, (uint)29);
            }
        }
        ImGuiEx.Text($"锁定目标：{*(byte*)((nint)TargetSystem.Instance() + 309)}");
        if(ImGui.Button("锁定 ChillFrames"))
        {
            FPSManager.LockChillFrames();
        }
        if(ImGui.Button("解除帧锁定"))
        {
            FPSManager.UnlockChillFrames();
        }
        ImGui.Separator();
        ImGuiEx.Text($"窗口未激活：{Lang.Bool(CSFramework.Instance()->WindowInactive)}");
        ImGuiEx.Text($"临时仅领取键已按下：{Lang.Bool(IsKeyPressed(C.TempCollectB))}");
        ImGuiEx.Text($"按键状态位：{Lang.Bool(Bitmask.IsBitSet(TerraFX.Interop.Windows.Windows.GetKeyState((int)C.TempCollectB), 15))}");
        ImGuiEx.Text($"不重新委托：{Lang.Bool(C.DontReassign)}，按键 {Lang.LimitedKeyNames[C.TempCollectB]}/{(int)C.TempCollectB}");
        foreach(var x in C.OfflineData)
        {
            ImGuiEx.Text($"{x.Name}@{x.World}: {x.Gil + x.RetainerData.Sum(z => z.Gil)}");
        }
        var ocd = Data;
        if(ocd != null)
        {
            ImGuiEx.Text($"等级数组：");
            ImGuiEx.Text(ocd.ClassJobLevelArray.Print());
        }

        ImGuiEx.Text($"{Utils.TryGetCurrentRetainer(out var n)}/{n}");
        ImGuiEx.Text($"{ItemLevel.Calculate(out var g, out var p)}/{g}/{p}");
        if(ImGui.Button("重新生成匿名种子"))
        {
            C.CensorSeed = Guid.NewGuid().ToString();
        }
        var inv = Utils.GetActiveRetainerInventoryName();
        ImGuiEx.Text($"当前雇员背包名称：{inv.Name} {inv.EntrustDuplicatesIndex}");
        ImGuiEx.Text($"条件曾启用：{Lang.Bool(P.ConditionWasEnabled)}");
        if(ImGui.CollapsingHeader("任务调试"))
        {
            ImGuiEx.Text($"正忙：{Lang.Bool(P.TaskManager.IsBusy)}，将在 {P.TaskManager.RemainingTimeMS} 后中止");
            if(ImGui.Button($"生成随机数 1/500"))
            {
                P.TaskManager.Enqueue(() => { var r = new Random().Next(0, 500); InternalLog.Verbose($"生成 1/500：{r}"); return r == 0; });
            }
            if(ImGui.Button($"生成随机数 1/5000"))
            {
                P.TaskManager.Enqueue(() => { var r = new Random().Next(0, 5000); InternalLog.Verbose($"生成 1/5000：{r}"); return r == 0; });
            }
            if(ImGui.Button($"生成随机数 1/100"))
            {
                P.TaskManager.Enqueue(() => { var r = new Random().Next(0, 100); InternalLog.Verbose($"生成 1/100：{r}"); return r == 0; });
            }
        }
        ImGuiEx.Text($"快速出售状态：{Lang.Bool(P.quickSellItems?.openInventoryContextHook?.IsEnabled)}");
        ImGuiEx.Text($"快速出售已就绪：{Lang.Bool(QuickSellItems.IsReadyToUse())}");

        foreach(var x in S.VentureStats.CharTotal)
        {
            ImGuiEx.Text($"{x.Key} : {x.Value}");
        }
        foreach(var x in S.VentureStats.RetTotal)
        {
            ImGuiEx.Text($"{x.Key} : {x.Value}");
        }

        ImGui.Separator();
        {
            if(ImGui.Button("触发") && TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon) && addon->UldManager.NodeList[5]->IsVisible())
            {
                AutoGCHandin.InvokeHandin(addon, 0);
            }
        }

        {
            if(TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon))
            {
                ImGuiEx.Text($"所选筛选条件有效：{Lang.Bool(AutoGCHandin.IsSelectedFilterValid(addon))}");
            }
        }

    }
}
