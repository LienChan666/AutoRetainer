using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.Readers;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainer.UiHelpers;
using ECommons.UIHelpers.AtkReaderImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugReader : DebugSectionBase
{
    public override void Draw()
    {
        {
            if(TryGetAddonByName<AtkUnitBase>("FreeCompanyCreditShop", out var a) && IsAddonReady(a))
            {
                var reader = new ReaderFreeCompanyCreditShop(a);
                ImGuiEx.Text($"""
                    部队等级：{reader.FCRank}
                    部队战绩：{reader.Credits}
                    条目数：{reader.Count}
                    """);
                for(var i = 0; i < reader.Count; i++)
                {
                    var x = reader.Listings[i];
                    ImGuiEx.Text($"{x}");
                    if(ImGuiEx.HoveredAndClicked()) new FreeCompanyCreditShop(a).Buy(0);
                    var amount = Math.Floor((float)reader.Credits / (float)(x.Price));
                }

                if(ImGui.Button("运行购买任务")) TaskRecursivelyBuyFuel.Enqueue();
            }
        }

        {
            if(TryGetAddonByName<AtkUnitBase>("RetainerList", out var a) && IsAddonReady(a))
            {
                var reader = new ReaderRetainerList(a);
                foreach(var x in reader.Retainers)
                {
                    ImGuiEx.Text($"{x.Name}/激活 {Lang.Bool(x.IsActive)}/金币 {x.Gil}/等级 {x.Level}/背包 {x.Inventory}");
                }
            }
        }
        {
            if(TryGetAddonByName<AtkUnitBase>("RetainerItemTransferList", out var a) && IsAddonReady(a))
            {
                var reader = new ReaderRetainerItemTransferList(a);
                foreach(var r in reader.Items)
                {
                    ImGuiEx.Text($"物品 {r.ItemID}，优质={Lang.Bool(r.IsHQ)}");
                }
            }
        }
        {
            if(TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var a) && IsAddonReady(a))
            {
                var reader = new ReaderAirShipExploration(a);
                ImGuiEx.Text($"距离：{reader.Distance}");
                ImGuiEx.Text($"燃料：{reader.Fuel}");
                foreach(var r in reader.Destinations)
                {
                    ImGuiEx.Text($"目的地 {r.NameFull}，等级={r.RequiredRank}，状态={r.StatusFlag}，可选择={Lang.Bool(r.CanBeSelected)}");
                }
            }
        }
        {
            if(TryGetAddonByName<AtkUnitBase>("SubmarineExplorationMapSelect", out var a) && IsAddonReady(a))
            {
                var reader = new ReaderSubmarineExplorationMapSelect(a);
                ImGuiEx.Text($"当前等级：{reader.SubmarineRank}");
                foreach(var r in reader.Maps)
                {
                    ImGuiEx.Text($"航海图 {r.Name}，等级={r.RequiredRank}");
                }
            }
        }
        {
            if(TryGetAddonByName<AtkUnitBase>("SelectString", out var a) && IsAddonReady(a))
            {
                var reader = new ReaderSelectString(a);
                foreach(var r in reader.Entries)
                {
                    ImGuiEx.Text($"{r.Text}");
                }
            }
        }
    }
}
