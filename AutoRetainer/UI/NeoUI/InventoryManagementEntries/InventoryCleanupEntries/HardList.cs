using AutoRetainerAPI.Configuration;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class HardList : InventoryManagementBase
{
    public override string Name => "背包清理/无条件出售列表";
    private InventoryManagementCommon InventoryManagementCommon = new();

    private HardList()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .TextWrapped("这些物品无论来源都会被出售，只要其堆叠数量不超过你在下方设定的阈值。此外，向 NPC 出售时仅会出售这些物品。")
            .InputInt(150f, $"最大出售堆叠数量", () => ref InventoryCleanupCommon.SelectedPlan.IMAutoVendorHardStackLimit)
            .Widget(() => InventoryManagementCommon.DrawListNew(
                itemId => InventoryCleanupCommon.SelectedPlan.AddItemToList(IMListKind.HardSell, itemId, out _),
                itemId => InventoryCleanupCommon.SelectedPlan.IMAutoVendorHard.Remove(itemId),
                InventoryCleanupCommon.SelectedPlan.IMAutoVendorHard, 
                (x) =>
                {
                    ImGui.SameLine();
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.CollectionButtonCheckbox(FontAwesomeIcon.Database.ToIconString(), x, InventoryCleanupCommon.SelectedPlan.IMAutoVendorHardIgnoreStack);
                    ImGui.PopFont();
                    ImGuiEx.Tooltip($"忽略该物品的堆叠设置");
                },
                filter: item => item.PriceLow != 0))
            .Separator()
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(InventoryCleanupCommon.SelectedPlan.IMAutoVendorHard);
            });
    }
}
