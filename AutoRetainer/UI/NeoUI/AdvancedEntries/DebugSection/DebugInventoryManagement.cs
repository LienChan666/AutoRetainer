using AutoRetainer.Internal.InventoryManagement;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Lumina.Excel.Sheets;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;
public unsafe class DebugInventoryManagement : DebugSectionBase
{
    private int slot;
    private InventoryType Type;
    private HashSet<uint> Whitelist = [];

    public override void Draw()
    {
        if(ImGui.CollapsingHeader("背包"))
        {
            foreach(var x in Enum.GetValues<InventoryType>())
            {
                ImGuiEx.TreeNodeCollapsingHeader($"背包 {(int)x}", () =>
                {
                    var inv = InventoryManager.Instance()->GetInventoryContainer(x);
                    for(var i = 0; i < inv->Size; i++)
                    {
                        var slot = inv->GetInventorySlot(i);
                        ImGuiEx.Text($"{i}: {ExcelItemHelper.GetName(slot->ItemId)} x{slot->Quantity} {slot->Flags}");
                    }
                });
            }
        }
        if(ImGui.CollapsingHeader("商店出售测试"))
        {
            if(ImGui.BeginCombo("背包类型", $"背包 {(int)Type}"))
            {
                foreach(var type in Enum.GetValues<InventoryType>())
                {
                    if(ImGui.Selectable($"背包 {(int)type}", type == Type)) Type = type;
                }
                ImGui.EndCombo();
            }
            ImGui.InputInt("栏位", ref slot);
            ImGuiEx.Text(ExcelItemHelper.GetName(InventoryManager.Instance()->GetInventoryContainer(Type)->GetInventorySlot(slot)->ItemId));
            if(ImGui.Button("出售"))
            {
                P.Memory.SellItemToShop(Type, slot);
            }
            if(ImGui.Button("存在物品时加入任务"))
            {
                NpcSaleManager.EnqueueIfItemsPresent();
            }
            ImGuiEx.Text($"有效 NPC：{NpcSaleManager.GetValidNPC()}");
            if(ImGui.Button("与目标交互")) TargetSystem.Instance()->InteractWithObject(Svc.Targets.Target.Struct(), false);
            if(TryGetAddonMaster<AddonMaster.SelectIconString>(out var m))
            {
                foreach(var x in m.Entries)
                {
                    if(ImGui.Selectable(x.Text))
                    {
                        x.Select();
                    }
                }
            }
        }
        if(ImGui.CollapsingHeader("商人列表"))
        {
            foreach(var x in Vendors)
            {
                ImGuiEx.Text(Whitelist.Contains(x) ? EColor.GreenBright : null, $"{x}: {Svc.Data.GetExcelSheet<ENpcResident>().GetRowOrDefault(x)?.Plural}");
                if(ImGui.IsItemHovered())
                {
                    if(ImGuiEx.Ctrl)
                    {
                        Whitelist.Add(x);
                    }
                    if(ImGuiEx.Shift) Whitelist.Remove(x);
                }
            }
            if(ImGui.Button("复制")) Copy(Whitelist.Print());
        }
    }

    public IEnumerable<uint> Vendors
    {
        get
        {
            foreach(var x in Svc.Data.GetSubrowExcelSheet<HousingEmploymentNpcList>())
            {
                for(var i = 0; i < x.Count; i++)
                {
                    var ret = x[i];
                    if(ret.RowId != 0) yield return ret.RowId;
                }
            }
        }
    }
}
