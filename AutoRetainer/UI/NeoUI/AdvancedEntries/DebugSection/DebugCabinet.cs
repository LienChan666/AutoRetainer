using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text;
using TerraFX.Interop.Windows;
using Cabinet = Lumina.Excel.Sheets.Cabinet;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

public unsafe class DebugCabinet : DebugSectionBase
{
    public override void Draw()
    {
        if(ImGui.CollapsingHeader("自动存入幻化台"))
        {
            if(ImGui.Button("前往旅馆并存入幻化台"))
            {
                S.MirageManager.EnqueueGoToInnAndDeliverEverything();
            }
            foreach(var inv in Utils.PlayerEntireInventory)
            {
                if(ImGui.CollapsingHeader($"{inv}"))
                {
                    ImGui.Indent();
                    var invCont = InventoryManager.Instance()->GetInventoryContainer(inv);
                    for(int i = 0; i < invCont->Size; i++)
                    {
                        var item = invCont->Items[i];
                        if(item.ItemId != 0 && Svc.Data.GetExcelSheet<Item>().TryGetRow(item.ItemId, out var data) && Utils.CanItemBePartOfMirageSet(item.ItemId))
                        {
                            ImGuiEx.Text(Utils.IsItemAlreadyMiraged(item.ItemId) ? EColor.GreenBright : null, $"{data.RowId} {data.Name}");
                        }
                    }
                    ImGui.Unindent();
                }
            }
        }
        ImGui.Separator();
        ImGuiEx.Text($"可存入收藏柜：{Lang.Bool(S.CabinetManager.CanDeliverCabinet())}");
        if(ImGui.Button("存入物品")) S.CabinetManager.EnqueueAllDeliverableItems();
        if(ImGui.Button("前往旅馆并存入全部物品")) S.CabinetManager.EnqueueGoToInnAndDeliverEverything();
        if(S.CabinetManager.TryGetStoredCabinetItems(out var cached, out var items))
        {
            ImGuiEx.Text($"已缓存：{Lang.Bool(cached)}");
        }
    }
}
