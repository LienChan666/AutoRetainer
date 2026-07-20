namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal class DebugBailout : DebugSectionBase
{
    public override void Draw()
    {
        ImGui.Checkbox("模拟退出时卡死", ref BailoutManager.SimulateStuckOnQuit);
        ImGui.Checkbox("模拟航行管制面板卡死", ref BailoutManager.SimulateStuckOnVoyagePanel);
        ImGuiEx.Text($"未出现选项窗口：{Environment.TickCount64 - BailoutManager.NoSelectString}");
        ImGuiEx.Text($"大厅卡死：{Environment.TickCount64 - BailoutManager.CharaSelectStuck}");
    }
}
