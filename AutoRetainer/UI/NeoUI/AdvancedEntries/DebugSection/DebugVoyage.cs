using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Diagnostics;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugVoyage : DebugSectionBase
{
    private static string data1 = "";
    private static VoyageType data2 = default;
    private static int r1, r2, r3, r4, r5 = -1;
    public override void Draw()
    {
        if(ImGui.CollapsingHeader("调试"))
        {
            try
            {
                var h = HousingManager.Instance()->WorkshopTerritory;
                if(h != null)
                {
                    foreach(var x in h->Submersible.Data)
                    {
                        ImGuiEx.Text($"{x.Name.Read()}/{x.ReturnTime}/{x.CurrentExplorationPoints.ToArray().Print()}");
                    }
                }
                if(ImGui.Button("清除离线数据"))
                {
                    Data.OfflineAirshipData.Clear();
                    Data.OfflineSubmarineData.Clear();
                }
                if(ImGui.Button("修理 1")) VoyageScheduler.TryRepair(0);
                if(ImGui.Button("修理 2")) VoyageScheduler.TryRepair(1);
                if(ImGui.Button("修理 3")) VoyageScheduler.TryRepair(2);
                if(ImGui.Button("修理 4")) VoyageScheduler.TryRepair(3);
                if(ImGui.Button("关闭修理窗口")) VoyageScheduler.CloseRepair();
                ImGui.InputText("数据 1", ref data1, 50);
                ImGuiEx.EnumCombo("数据 2", ref data2, Lang.VoyageTypeNames);
                if(CurrentSubmarine.Get() != null)
                {
                    ImGuiEx.Text($"{CurrentSubmarine.Get()->CurrentExp}/{CurrentSubmarine.Get()->NextLevelExp}");
                }
                ImGuiEx.Text($"位于航行管制面板：{Lang.Bool(VoyageUtils.IsInVoyagePanel())}，{Lang.PanelName}");
                if(ImGui.Button("检查飞空艇/潜水艇是否需要修理"))
                {
                    try
                    {
                        DuoLog.Information($"{VoyageUtils.GetIsVesselNeedsRepair(data1, data2, out var log).Print()}\n{log.Join("\n")}");
                    }
                    catch(Exception e)
                    {
                        e.LogDuo();
                    }
                }
                if(ImGui.Button("按名称获取潜水艇索引"))
                {
                    try
                    {
                        DuoLog.Information($"{VoyageUtils.GetVesselIndexByName(data1, VoyageType.Submersible)}");
                    }
                    catch(Exception e)
                    {
                        e.LogDuo();
                    }
                }
                ImGuiEx.Text($"可达传唤铃：{Utils.GetReachableRetainerBell(false)}");
                ImGuiEx.Text($"可达传唤铃（忽略视线）：{Utils.GetReachableRetainerBell(true)}");
                ImGuiEx.TextWrapped($"已启用潜水艇：{Data.GetVesselData(VoyageType.Submersible).Select(x => $"{x.Name}, {x.GetRemainingSeconds()}").Print()}");
                ImGuiEx.Text($"存在可处理的飞空艇/潜水艇：{Lang.Bool(Data.AnyEnabledVesselsAvailable())}");
                ImGuiEx.Text($"面板类型：{Lang.PanelTypeNames[VoyageUtils.GetCurrentWorkshopPanelType()]}");
                if(TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out var addon) && IsAddonReady(addon))
                {
                    var button = addon->UldManager.NodeList[3]->GetAsAtkComponentButton();
                    ImGuiEx.Text($"按钮已启用：{Lang.Bool(button->IsEnabled)}");
                }
                if(ImGui.Button("与最近的面板交互"))
                {
                    TaskInteractWithNearestPanel.Enqueue();
                }
            }
            catch(Exception e)
            {
                ImGuiEx.TextWrapped(e.ToString());
            }
        }
        ImGuiEx.Text($"雇员流程是否被远航探索阻止：{Lang.Bool(VoyageUtils.IsRetainerBlockedByVoyage())}");
        if(ImGui.CollapsingHeader("数据"))
        {
            try
            {
                ImGuiEx.Text($"当前指针：{(nint)CurrentSubmarine.Get()}");
                if(CurrentSubmarine.Get() != null)
                {
                    ImGuiEx.Text($"名称：{CurrentSubmarine.Get()->Name.Read()}");
                    ImGuiEx.Text($"船体 ID：{CurrentSubmarine.Get()->HullId}");
                    ImGuiEx.Text($"船尾 ID：{CurrentSubmarine.Get()->SternId}");
                    ImGuiEx.Text($"舰桥 ID：{CurrentSubmarine.Get()->BridgeId}");
                    ImGuiEx.Text($"船首 ID：{CurrentSubmarine.Get()->BowId}");
                    ImGuiEx.Text($"等级 ID：{CurrentSubmarine.Get()->RankId}");
                    if(ImGui.Button("输出最佳经验"))
                    {
                        CurrentSubmarine.GetBestExps();
                    }
                    if(ImGui.Button("选择最佳航线"))
                    {
                        TaskCalculateAndPickBestExpRoute.Enqueue();
                    }
                    ImGuiEx.Text($"目的地：{CurrentSubmarine.Get()->CurrentExplorationPoints.ToArray().Print()}");
                    ImGuiEx.Text($"目的地：{CurrentSubmarine.Get()->CurrentExplorationPoints.ToArray().Select(x => VoyageUtils.GetSubmarineExplorationName(x)).Print()}");
                }
            }
            catch(Exception e)
            {
                ImGuiEx.TextWrapped(e.ToString());
            }
            var curPlotId = (long*)(Process.GetCurrentProcess().MainModule.BaseAddress + 0x215FB68);
            ImGuiEx.TextCopy($"门牌 ID：{*curPlotId:X16}");
            ImGuiEx.Text($"房屋 ID：{HousingManager.Instance()->GetCurrentIndoorHouseId()}");
            if(HousingManager.Instance()->WorkshopTerritory != null)
            {
                ImGuiEx.Text($"飞空艇数量：{HousingManager.Instance()->WorkshopTerritory->Airship.AirshipCount}");
                {
                    var data = HousingManager.Instance()->WorkshopTerritory->Airship.Data;
                    for(var i = 0; i < data.Length; i++)
                    {
                        var d = data[i];
                        ImGuiEx.Text($"飞空艇：{d.Name.Read()}，返航时间 {d.GetReturnTime()}，当前经验 {d.CurrentExp}");
                    }
                }
                {
                    var data = HousingManager.Instance()->WorkshopTerritory->Submersible.Data;
                    for(var i = 0; i < data.Length; i++)
                    {
                        var d = data[i];
                        ImGuiEx.Text($"潜水艇：{d.Name.Read()}，返航时间 {d.GetReturnTime()}，当前经验 {d.CurrentExp}");
                    }
                }
            }
        }
        if(ImGui.CollapsingHeader("工具"))
        {
            ImGui.InputInt("目的地索引", ref r1);
            if(ImGui.Button("选择"))
            {
                P.Memory.SelectRoutePointUnsafe(r1);
            }
        }
        if(ImGui.CollapsingHeader("控制"))
        {
            if(ImGui.Button($"锁定目标")) DuoLog.Information($"{VoyageScheduler.Lockon()}");
            if(ImGui.Button($"接近面板")) DuoLog.Information($"{VoyageScheduler.Approach()}");
            if(ImGui.Button($"自动移出面板")) DuoLog.Information($"{VoyageScheduler.AutomoveOffPanel()}");
            if(ImGui.Button($"与航行管制面板交互")) DuoLog.Information($"{VoyageScheduler.InteractWithVoyagePanel()}");
            if(ImGui.Button($"选择飞空艇管理")) DuoLog.Information($"{VoyageScheduler.SelectAirshipManagement()}");
            if(ImGui.Button($"选择潜水艇管理")) DuoLog.Information($"{VoyageScheduler.SelectSubManagement()}");
            ImGui.InputText("目标名称", ref data1, 100);
            if(ImGui.Button($"按名称选择潜水艇")) DuoLog.Information($"{VoyageScheduler.SelectVesselByName(data1, VoyageType.Submersible)}");
            if(ImGui.Button($"再次派遣潜水艇")) DuoLog.Information($"{VoyageScheduler.RedeployVessel()}");
            if(ImGui.Button($"派遣潜水艇")) DuoLog.Information($"{VoyageScheduler.DeployVessel()}");
            if(ImGui.Button($"派遣至最佳经验航线")) DuoLog.Information($"{TaskDeployOnBestExpVoyage.Deploy()}");
            if(ImGui.Button($"输出接近面板方法")) DuoLog.Information($"{VoyageScheduler.Approach}");
        }
        if(ImGui.CollapsingHeader("测试任务管理器"))
        {
            if(ImGui.Button("测试再次派遣飞空艇"))
            {
                P.TaskManager.Enqueue(VoyageScheduler.Lockon);
                P.TaskManager.Enqueue(VoyageScheduler.Approach);
                P.TaskManager.Enqueue(VoyageScheduler.AutomoveOffPanel);
                P.TaskManager.Enqueue(VoyageScheduler.InteractWithVoyagePanel);
                P.TaskManager.Enqueue(VoyageScheduler.SelectAirshipManagement);
                P.TaskManager.Enqueue(() => VoyageScheduler.SelectVesselByName(data1, VoyageType.Airship));
                P.TaskManager.Enqueue(VoyageScheduler.WaitUntilFinalizeDeployAddonExists);
                P.TaskManager.Enqueue(VoyageScheduler.RedeployVessel);
                P.TaskManager.Enqueue(VoyageScheduler.DeployVessel);
            }
        }
    }
}
