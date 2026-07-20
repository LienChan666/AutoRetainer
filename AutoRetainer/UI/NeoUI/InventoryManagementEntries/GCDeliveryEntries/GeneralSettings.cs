using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "筹备稀有品/通用设置";

    public override NuiBuilder Builder => new NuiBuilder()
        .Section("通用设置")
        .Checkbox("启用“筹备稀有品”自动循环", () => ref C.AutoGCContinuation)
        .TextWrapped($"""
            启用后，插件会自动循环执行以下流程：
            - 交纳符合条件的装备，获得军票；
            - 使用军票兑换“兑换列表”中配置的物品；
            - 兑换完成后，继续交纳剩余装备。

            未配置兑换列表时，将自动兑换探险币。
            请确保“角色配置”中的“交纳模式”未设为“禁用”。

            当没有可交纳的装备，或无法继续兑换物品时，流程结束。
            """)

        .Section("多角色模式筹备稀有品")
        .TextWrapped($"""
        启用时：
        - 多角色模式运行期间，已启用传送且军衔满足要求的角色会自动执行“筹备稀有品”交纳，并按兑换方案购买物品。
        """)
        .Checkbox("启用多角色模式“筹备稀有品”", () => ref C.FullAutoGCDelivery)
        .Checkbox("仅在工作站未锁定时", () => ref C.FullAutoGCDeliveryOnlyWsUnlocked)
        .InputInt(150f, "触发交纳的剩余背包空位数（小于等于）", () => ref C.FullAutoGCDeliveryInventory, "仅统计主背包，不统计兵装库")
        .Checkbox("探险币耗尽时触发", () => ref C.FullAutoGCDeliveryDeliverOnVentureExhaust, "这可能导致你每次登录后都只会前往大国防联军兑换窗口。请先配置可购买足够探险币的采购方案。")
        .Indent()
        .InputInt(150f, "触发交纳的剩余探险币数量（小于等于）", () => ref C.FullAutoGCDeliveryDeliverOnVentureLessThan)
        .Unindent()
        .Checkbox("可用时使用军票预支单", () => ref C.FullAutoGCDeliveryUseBuffItem)
        .Checkbox("可用时使用部队特效“军票提高”", () => ref C.FullAutoGCDeliveryUseBuffFCAction)
        .Checkbox("交纳后传回住宅/旅馆", () => ref C.TeleportAfterGCExchange)
        .Indent()
        .Checkbox("仅在多角色模式启用时", () => ref C.TeleportAfterGCExchangeMulti)
        .Unindent()
        ;
}
