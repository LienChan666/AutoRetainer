using AutoRetainer.Internal.InventoryManagement;
using ECommons.GameHelpers;
using TerraFX.Interop.Windows;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "背包清理/通用设置";

    private GeneralSettings()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .Checkbox($"自动打开雇员的宝箱", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableCofferAutoOpen, "仅在多角色模式下生效：登出前会自动开启全部雇员的宝箱，除非背包空间不足。")
            .Indent()
            .InputInt(100f, "单次最多打开数量", () => ref InventoryCleanupCommon.SelectedPlan.MaxCoffersAtOnce)
            .Unindent()
            .Checkbox($"启用向雇员出售物品", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableAutoVendor, "AutoRetainer 在检查并重新委托雇员时，会按背包清理方案执行出售。")
            .Checkbox($"启用向住宅商人出售物品", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableNpcSell, "AutoRetainer 进入房屋后，会按背包清理方案执行出售。需在房屋入口（非部队房屋入口）附近放置可出售物品的住宅商人，并确保进屋后可立即与其交互。")
            .Indent()
            .Checkbox($"雇员可用时跳过住宅商人", () => ref InventoryCleanupCommon.SelectedPlan.IMSkipVendorIfRetainer)
            .Widget("立即出售", (x) =>
            {
                if(ImGuiEx.Button(x, Player.Interactable && InventoryCleanupCommon.SelectedPlan.IMEnableNpcSell && NpcSaleManager.GetValidNPC() != null && !IsOccupied() && !P.TaskManager.IsBusy))
                {
                    NpcSaleManager.EnqueueIfItemsPresent(true);
                }
            })
            .Unindent()
            .Checkbox($"自动道具分解", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableItemDesynthesis)
            .Indent()
            .Widget("兵装库：", t =>
            {
                ImGuiEx.TextV(t);
                ImGui.SameLine();
                ImGuiEx.RadioButtonBool("道具分解", "跳过", ref InventoryCleanupCommon.SelectedPlan.IMEnableItemDesynthesisFromArmory, true);
            })
            .Unindent()
            .Checkbox($"启用右键菜单联动", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableContextMenu)
            .Checkbox($"允许出售/丢弃兵装库物品", () => ref InventoryCleanupCommon.SelectedPlan.AllowSellFromArmory)
            .Checkbox("多角色模式下将符合条件的物品存入收藏柜", () => ref InventoryCleanupCommon.SelectedPlan.EnableCabinetAutoDelivery, "尚未存入收藏柜的符合条件物品会被自动存入，并在多角色模式运行期间排除出丢弃、分解、委托及筹备稀有品交纳流程。该操作会在多角色模式的筹备稀有品交纳前执行。")
            .Checkbox($"试运行模式", () => ref InventoryCleanupCommon.SelectedPlan.IMDry, "不实际执行出售/丢弃，仅在聊天栏输出原本将被处理的物品")
            ;
    }
}
