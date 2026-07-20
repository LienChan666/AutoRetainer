using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.MainWindow.MultiModeTab;
public class CharaConfig
{
    public static void Draw(OfflineCharacterData data, bool isRetainer)
    {
        ImGui.PushID(data.CID.ToString());
        SharedUI.DrawMultiModeHeader(data);
        var b = new NuiBuilder()

        .Section("角色专用常规设置")
        .Widget(() =>
        {
            SharedUI.DrawServiceAccSelector(data);
            SharedUI.DrawPreferredCharacterUI(data);
        });
        if(isRetainer)
        {
            b = b.Section("雇员").Widget(() =>
            {
                ImGuiEx.Text($"自动执行大国防联军“筹备稀有品”：");
                if(!AutoGCHandin.Operation)
                {
                    ImGuiEx.SetNextItemWidthScaled(200f);
                    ImGuiEx.EnumCombo("##gcHandin", ref data.GCDeliveryType, Lang.GCDeliveryTypeNames);
                }
                else
                {
                    ImGuiEx.Text($"当前无法修改");
                }
            });
        }
        else
        {
        b = b.Section("远航探索").Widget(() =>
            {
                ImGui.Checkbox($"等待飞空艇/潜水艇返航", ref data.MultiWaitForAllDeployables);
            ImGuiComponents.HelpMarker("""该设置与全局选项一致，但仅作用于单个角色。启用后，AutoRetainer 会等待所有飞空艇和潜水艇返航后再登录该角色。若你因其他原因已登录，仍会再次派遣已完成的飞空艇和潜水艇；除非全局也启用了“即使已登录也等待”。""");
            });
        }
        b = b.Section("传送覆盖", data.GetAreTeleportSettingsOverriden() ? ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg] with { X = 1f } : null, true)
        .Widget(() =>
        {
            ImGuiEx.Text($"你可以按角色覆盖传送设置。");
            bool? demo = null;
            ImGuiEx.Checkbox("带此标记的选项将使用全局配置值", ref demo);
            ImGuiEx.Checkbox("启用", ref data.TeleportOptionsOverride.Enabled);
            ImGui.Indent();
            ImGuiEx.Checkbox("雇员流程传送...", ref data.TeleportOptionsOverride.Retainers);
            ImGui.Indent();
            ImGuiEx.Checkbox("...传送到个人房屋", ref data.TeleportOptionsOverride.RetainersPrivate);
            ImGuiEx.Checkbox("...传送到共享房屋", ref data.TeleportOptionsOverride.RetainersShared);
            ImGuiEx.Checkbox("...传送到部队房屋", ref data.TeleportOptionsOverride.RetainersFC);
            ImGuiEx.Checkbox("...传送到公寓", ref data.TeleportOptionsOverride.RetainersApartment);
            ImGui.Text("若以上选项都禁用或失败，将传送到旅馆。");
            ImGui.Unindent();
            ImGuiEx.Checkbox("远航探索流程传送到部队房屋", ref data.TeleportOptionsOverride.Deployables);
            ImGui.Unindent(); 
        }).Draw();
        SharedUI.DrawExcludeReset(data);
        ImGui.PopID();
    }
}
