using Dalamud.Interface.Components;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal class SuperSecret : DebugSectionBase
{
    public override void Draw()
    {
        ImGuiEx.TextWrapped(ImGuiColors.ParsedOrange, "此处可能发生任何事情。");
        ImGui.Checkbox("旧版雇员感知", ref C.OldRetainerSense);
        ImGuiComponents.HelpMarker("检测并使用玩家有效距离内最近的传唤铃。");
        ImGuiEx.TextWrapped(ImGuiColors.DalamudGrey, "多角色模式运行期间会强制启用雇员感知。");
        ImGui.Separator();
        ImGui.Checkbox($"不安全选项保护", ref C.UnsafeProtection);
        ImGui.SameLine();
        if(ImGui.Button($"写入注册表"))
        {
            Safety.Set(C.UnsafeProtection);
        }
        var g = Safety.Get();
        ImGuiEx.Text(g ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed, $"安全标记：{(g ? "存在" : "不存在")}");
        ImGui.Separator();
        ImGuiEx.Checkbox("多角色模式交纳时忽略军衔检查", ref C.IgnoreGCRankCheck);
    }
}
