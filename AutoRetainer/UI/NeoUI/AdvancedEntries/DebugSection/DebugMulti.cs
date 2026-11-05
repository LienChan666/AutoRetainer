using AutoRetainer.Internal;
using AutoRetainer.Scheduler.Tasks;
using Dalamud.Utility;
using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.ExcelServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using ECommons.Reflection;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugMulti : DebugSectionBase
{
    public override void Draw()
    {
        ImGui.Checkbox("禁用渲染", ref P.TestRenderDisable);
        if(ImGui.CollapsingHeader("排序后数据"))
        {
            ImGuiEx.Text($"{MultiMode.GetRetainerSortedOfflineDatas(true).Where(x => !x.ExcludeRetainer).Select(x => $"{x.Name}@{x.World}").Print("\n")}");
        }
        if(ImGui.CollapsingHeader("NeoHET"))
        {
            if(ImGui.Button("加入住宅进入任务")) TaskNeoHET.Enqueue(null);
            if(ImGui.Button("加入进入工房任务")) TaskNeoHET.TryEnterWorkshop(() => DuoLog.Error("失败"));
            ImGuiEx.Text($"""
                可进入工房：{Lifestream.CanMoveToWorkshop()}
                """);
        }
        if(ImGui.CollapsingHeader("任务"))
        {
            if(ImGui.Button("测试自动移动任务")) P.TaskManager.EnqueueTask(NeoTasks.ApproachObjectViaAutomove(() => Svc.Targets.FocusTarget));
            if(ImGui.Button("测试交互任务")) P.TaskManager.EnqueueTask(NeoTasks.InteractWithObject(() => Svc.Targets.FocusTarget));
            if(ImGui.Button("测试两项任务"))
            {
                P.TaskManager.EnqueueTask(NeoTasks.ApproachObjectViaAutomove(() => Svc.Targets.FocusTarget));
                P.TaskManager.EnqueueTask(NeoTasks.InteractWithObject(() => Svc.Targets.FocusTarget));
            }
        }
        ImGui.Checkbox("不登出", ref C.DontLogout);
        ImGui.Checkbox("已启用", ref MultiMode.Enabled);
        ImGuiEx.Text($"预期角色：{MultiMode.ExpectedCharacter}");
        if(ImGui.Button("强制角色不匹配")) MultiMode.ExpectedCharacter = ("AAAAAAAA", "BBBBBBB");
        if(ImGui.Button("模拟无剩余任务"))
        {
            MultiMode.Relog(null, out var error, RelogReason.MultiMode);
        }
        if(ImGui.Button($"模拟自动启动"))
        {
            MultiMode.PerformAutoStart();
        }
        if(ImGui.Button("删除已加载标记数据"))
        {
            DalamudReflector.DeleteSharedData("AutoRetainer.WasLoaded");
        }
        ImGuiEx.Text($"移动中：{Lang.Bool(AgentMap.Instance()->IsPlayerMoving)}");
        ImGuiEx.Text($"正忙：{Lang.Bool(IsOccupied())}");
        ImGuiEx.Text($"咏唱中：{Lang.Bool(Player.Object?.IsCasting)}");
        ImGuiEx.TextCopy($"角色内容 ID：{Player.CID}");
        ImGuiEx.Text($"{Svc.Data.GetExcelSheet<Addon>()?.GetRow(115).Text.ToDalamudString().GetText()}");
        ImGuiEx.Text($"服务器时间：{CSFramework.GetServerTime()}");
        ImGuiEx.Text($"本机时间：{DateTimeOffset.Now.ToUnixTimeSeconds()}");
        if(ImGui.CollapsingHeader("HET"))
        {
            ImGuiEx.Text($"最近入口：{Utils.GetNearestEntrance(out var d)}，距离={d}");
            if(ImGui.Button("进入房屋"))
            {
                TaskNeoHET.Enqueue(null);
            }
        }
        if(ImGui.CollapsingHeader("住宅区"))
        {
            ImGuiEx.Text(ResidentalAreas.List.Select(x => GenericHelpers.GetTerritoryName(x)).Join("\n"));
            ImGuiEx.Text($"位于住宅区：{Lang.Bool(ResidentalAreas.List.Contains((ushort)Svc.ClientState.TerritoryType))}");
        }
        ImGuiEx.Text($"位于休息区：{Lang.Bool(TerritoryInfo.Instance()->InSanctuary)}");
        ImGuiEx.Text($"区域表判定为休息区：{Lang.Bool(ExcelTerritoryHelper.IsSanctuary(Svc.ClientState.TerritoryType))}");
        ImGui.Checkbox($"绕过休息区检查", ref C.BypassSanctuaryCheck);
        if(Svc.ClientState.LocalPlayer != null && Svc.Targets.Target != null)
        {
            ImGuiEx.Text($"到目标的距离：{Vector3.Distance(Svc.ClientState.LocalPlayer.Position, Svc.Targets.Target.Position)}");
            ImGuiEx.Text($"目标碰撞箱：{Svc.Targets.Target.HitboxRadius}");
            ImGuiEx.Text($"到目标碰撞箱的距离：{Vector3.Distance(Svc.ClientState.LocalPlayer.Position, Svc.Targets.Target.Position) - Svc.Targets.Target.HitboxRadius}");
        }
        if(ImGui.CollapsingHeader("角色选择"))
        {
            foreach(var x in Utils.GetCharacterNames())
            {
                ImGuiEx.Text($"{x.Name}@{x.World}");
            }
        }
    }
}
