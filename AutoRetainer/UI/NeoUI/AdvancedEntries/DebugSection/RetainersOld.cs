using AutoRetainer.Internal;
using Dalamud.Interface.Components;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;
internal unsafe class RetainersOld : DebugSectionBase
{
    private static Dictionary<int, (Vector2 start, Vector2 end)> bars = [];
    public override void Draw()
    {
        if(!(GameRetainerManager.Ready && Svc.ClientState.LocalPlayer != null))
        {
            ImGuiEx.Text("数据尚未就绪");
            return;
        }

        var slots = Utils.GetInventoryFreeSlotCount();
        var ventures = InventoryManager.Instance()->GetInventoryItemCount(21072);
        ImGuiEx.Text($"背包空位：");
        ImGui.SameLine(0, 0);
        ImGuiEx.Text(slots < GameRetainerManager.Count ? ImGuiColors.DalamudRed : slots < 14 * GameRetainerManager.Count ? ImGuiColors.DalamudOrange : ImGuiColors.ParsedGreen,
                $"{slots}");
        ImGui.SameLine();
        ImGuiEx.Text(ImGuiColors.DalamudGrey3, "|");
        ImGui.SameLine();
        ImGuiEx.Text("探险币：");
        ImGui.SameLine(0, 0);
        ImGuiEx.Text(ventures < 2 * GameRetainerManager.Count ? ImGuiColors.DalamudRed : ventures < 24 * GameRetainerManager.Count ? ImGuiColors.DalamudOrange : ImGuiColors.ParsedGreen,
                $"{ventures}");
        ImGuiComponents.HelpMarker("探险币少于 2 枚或背包空位不足时，插件会自动禁用。 ");
        var storePos = ImGui.GetCursorPos();
        for(var i = 0; i < GameRetainerManager.Count; i++)
        {
            if(bars.TryGetValue(i, out var v))
            {
                var ret = GameRetainerManager.Retainers[i];
                if(ret.VentureID == 0 || !ret.Available || ret.Name.ToString().IsNullOrEmpty()) continue;
                ImGui.SetCursorPos(v.start - ImGui.GetStyle().CellPadding with { Y = 0 });
                ImGui.PushStyleColor(ImGuiCol.PlotHistogram, 0xbb500000);
                ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                ImGui.ProgressBar(1f - Math.Min(1f, ret.GetVentureSecondsRemaining(false) / (60f * 60f)),
                        new(ImGui.GetContentRegionAvail().X, v.end.Y - v.start.Y - ImGui.GetStyle().CellPadding.Y), "");
                ImGui.PopStyleColor(2);
            }
        }
        ImGui.SetCursorPos(storePos);
        ImGui.BeginTable("##ertainertable", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders);
        ImGui.TableSetupColumn("名称", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("探险");
        ImGui.TableSetupColumn("交互");
        ImGui.TableHeadersRow();
        var retainers = P.GetSelectedRetainers(Svc.ClientState.LocalContentId);
        for(var i = 0; i < GameRetainerManager.Count; i++)
        {
            var ret = GameRetainerManager.Retainers[i];
            if(!ret.Available || ret.Name.ToString().IsNullOrEmpty()) continue;
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
            var start = ImGui.GetCursorPos();
            var selected = retainers.Contains(ret.Name.ToString());
            if(ImGui.Checkbox((string)$"雇员 {(C.NoNames ? i + 1 : ret.Name)}", ref selected))
            {
                if(selected)
                {
                    retainers.Add(ret.Name.ToString());
                }
                else
                {
                    retainers.Remove(ret.Name.ToString());
                }
            }
            var end = ImGui.GetCursorPos();
            bars[i] = (start, end);
            ImGui.TableNextColumn();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
            ImGuiEx.Text($"{(ret.VentureID == 0 ? "无探险" : Utils.ToTimeString(ret.GetVentureSecondsRemaining(false)))}");
            ImGui.TableNextColumn();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0);
            ImGuiEx.Text($"-");
        }
        ImGui.EndTable();
    }
}
