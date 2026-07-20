using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugGCAuto : DebugSectionBase
{
    public override void Draw()
    {
        if(ImGui.CollapsingHeader("筹备稀有品"))
        {
            foreach(var x in AutoGCHandin.GetHandinItems())
            {
                ImGuiEx.Text(x.ToString() + "/" + ExcelItemHelper.GetName(x.ItemID));
            }
        }
        if(ImGui.Button("加入初始化任务")) GCContinuation.EnqueueInitiation(true);
        if(ImGui.Button("加入关闭兑换窗口任务")) GCContinuation.EnqueueDeliveryClose();
        if(ImGui.Button("启用单步模式")) P.TaskManager.StepMode = true;
        ImGui.SameLine();
        if(ImGui.Button("关闭单步模式")) P.TaskManager.StepMode = false;
        ImGui.SameLine();
        if(ImGui.Button("执行一步")) P.TaskManager.Step();
        if(ImGui.CollapsingHeader("筹备稀有品列表"))
        {
            if(TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon))
            {
                var reader = new ReaderGrandCompanySupplyList(addon);
                if(reader.IsLoaded)
                {
                    var ptr = (GCExpectEntry*)*(nint*)((nint)addon + 648);
                    for(var i = 0; i < reader.NumItems; i++)
                    {
                        var entry = ptr[i];
                        ImGuiEx.Text($"{entry.Unk112}/{entry.Unk116}/{entry.Seals}/{entry.ItemID} {ExcelItemHelper.GetName(entry.ItemID)}/{entry.Unk136}/{entry.Unk145}");
                    }
                }
            }
        }
        if(ImGui.CollapsingHeader("大国防联军兑换"))
        {
            if(TryGetAddonByName<AtkUnitBase>("GrandCompanyExchange", out var addon) && IsAddonReady(addon))
            {
                var reader = new ReaderGrandCompanyExchange(addon);
                List<ImGuiEx.EzTableEntry> entries = [];
                foreach(var x in reader.Items)
                {
                    entries.Add(new("物品", () => ImGuiEx.TextCopy($"{x.Name}")));
                    entries.Add(new("ID", () => ImGuiEx.TextCopy($"{x.ItemID}")));
                    entries.Add(new("背包", () => ImGuiEx.TextCopy($"{x.Bag}")));
                    entries.Add(new("图标 ID", () => ImGuiEx.TextCopy($"{x.IconID}")));
                    entries.Add(new("军衔要求", () => ImGuiEx.TextCopy($"{x.RankReq}")));
                    entries.Add(new("军票", () => ImGuiEx.TextCopy($"{x.Seals}")));
                    entries.Add(new("未知字段 350", () => ImGuiEx.TextCopy($"{x.Unk350}")));
                    entries.Add(new("未知字段 450", () => ImGuiEx.TextCopy($"{x.OpenCurrencyExchange}")));
                }
                ImGuiEx.EzTable(entries);
            }
        }
        ImGuiEx.Text($"军票倍率：{Utils.GetGCSealMultiplier()}");
        if(ImGui.Button("选择兑换")) DuoLog.Information($"{GCContinuation.SelectExchange()}");
        if(ImGui.Button("确认兑换")) DuoLog.Information($"{GCContinuation.ConfirmExchange()}");
        if(ImGui.Button("选择兑换纵向标签")) DuoLog.Information($"{GCContinuation.SelectGCExchangeVerticalTab(0)}");
        if(ImGui.Button("选择兑换横向标签")) DuoLog.Information($"{GCContinuation.SelectGCExchangeHorizontalTab(2)}");
        if(ImGui.Button("与商店交互")) DuoLog.Information($"{GCContinuation.InteractWithShop()}");
        if(ImGui.Button("与兑换 NPC 交互")) DuoLog.Information($"{GCContinuation.InteractWithExchange()}");
        if(ImGui.Button("选择筹备任务")) DuoLog.Information($"{GCContinuation.SelectProvisioningMission()}");
        if(ImGui.Button("选择筹备列表标签")) DuoLog.Information($"{GCContinuation.SelectSupplyListTab(2)}");
        if(ImGui.Button("条件允许时启用交纳")) DuoLog.Information($"{GCContinuation.EnableDeliveringIfPossible()}");
        if(ImGui.Button("关闭筹备列表")) DuoLog.Information($"{GCContinuation.CloseSupplyList()}");
        if(ImGui.Button("关闭选项窗口")) DuoLog.Information($"{GCContinuation.CloseSelectString()}");
        if(ImGui.Button("关闭兑换窗口")) DuoLog.Information($"{GCContinuation.CloseExchange()}");
        if(ImGui.Button("打开军票窗口")) DuoLog.Information($"{GCContinuation.OpenSeals()}");
    }
}
