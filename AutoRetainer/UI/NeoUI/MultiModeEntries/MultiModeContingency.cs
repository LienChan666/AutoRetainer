using AutoRetainerAPI.Configuration;
using System.Collections.Frozen;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeContingency : NeoUIEntry
{
    private static readonly FrozenDictionary<WorkshopFailAction, string> WorkshopFailActionNames = new Dictionary<WorkshopFailAction, string>()
    {
        [WorkshopFailAction.StopPlugin] = "停止插件全部运行",
        [WorkshopFailAction.ExcludeVessel] = "将当前飞空艇/潜水艇排除在运行流程外",
        [WorkshopFailAction.ExcludeChar] = "将当前角色排除出多角色轮换",
    }.ToFrozenDictionary();

    public override string Path => "多角色模式/应急策略";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("应急策略")
        .TextWrapped("你可以在此配置多种后备动作，用于处理常见失败状态或潜在运行错误。")
        .EnumComboFullWidth(null, "桶装青磷水耗尽", () => ref C.FailureNoFuel, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "当桶装青磷水不足、无法派遣新的远航探索时，执行所选后备动作。")
        .EnumComboFullWidth(null, "无法修理飞空艇/潜水艇", () => ref C.FailureNoRepair, null, WorkshopFailActionNames, "当魔导机械修理材料不足、无法修理飞空艇/潜水艇时，执行所选后备动作。")
        .EnumComboFullWidth(null, "背包已满", () => ref C.FailureNoInventory, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "当当前角色背包空间不足、无法接收远航探索奖励时，执行所选后备动作。")
        .EnumComboFullWidth(null, "关键操作失败", () => ref C.FailureGeneric, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "当发生未知或其他异常时，执行所选后备动作。")
        .Widget("被 GM 关进监狱", (x) =>
        {
            ImGui.BeginDisabled();
            ImGuiEx.SetNextItemFullWidth();
            if(ImGui.BeginCombo("##jailsel", "终止游戏")) { ImGui.EndCombo(); }
            ImGui.EndDisabled();
        }, "当插件运行期间被 GM 监禁时，执行所选后备动作。祝你好运！");
}
