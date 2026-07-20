using AutoRetainerAPI.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public unsafe sealed class DiscardList : InventoryManagementBase
{
    public override string Name => "背包清理/丢弃列表";
    private InventoryManagementCommon InventoryManagementCommon = new();

    public override int DisplayPriority => -1;

    private DiscardList()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .TextWrapped("这些物品无论来源都会被丢弃，只要其堆叠数量不超过你在下方设定的阈值。每次可能变更背包的操作前后都会触发丢弃。丢弃优先级最高：即使同一物品也在出售或分解列表中，仍会优先丢弃。受保护物品不会被丢弃。")
            .InputInt(150f, $"最大丢弃堆叠数量", () => ref InventoryCleanupCommon.SelectedPlan.IMDiscardStackLimit)
            .Widget(() => InventoryManagementCommon.DrawListNew(
                itemId => InventoryCleanupCommon.SelectedPlan.AddItemToList(IMListKind.Discard, itemId, out _),
                itemId => InventoryCleanupCommon.SelectedPlan.IMDiscardList.Remove(itemId),
                InventoryCleanupCommon.SelectedPlan.IMDiscardList,
                (x) =>
                {
                    ImGui.SameLine();
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.CollectionButtonCheckbox(FontAwesomeIcon.Database.ToIconString(), x, InventoryCleanupCommon.SelectedPlan.IMDiscardIgnoreStack);
                    ImGui.PopFont();
                    ImGuiEx.Tooltip($"忽略该物品的堆叠设置");
                }))
            .Separator()
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(InventoryCleanupCommon.SelectedPlan.IMDiscardList);
            });
    }
}