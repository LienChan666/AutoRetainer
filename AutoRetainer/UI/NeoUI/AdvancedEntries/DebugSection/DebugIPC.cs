
using ECommons.GameHelpers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugIPC : DebugSectionBase
{
    public override void Draw()
    {
        ImGuiEx.Text($"最近雇员探险剩余秒数：{S.EzIPCManager.IPC_PluginState.GetClosestRetainerVentureSecondsRemaining(Player.CID)}");
        ImGui.Checkbox($"API 测试", ref ApiTest.Enabled);
        ImGuiEx.Text($"IPC 已抑制：{Lang.Bool(Svc.PluginInterface.GetIpcSubscriber<bool>("AutoRetainer.GetSuppressed").InvokeFunc())}");
        if(ImGui.Button($"启用抑制"))
        {
            Svc.PluginInterface.GetIpcSubscriber<bool, object>("AutoRetainer.SetSuppressed").InvokeAction(true);
        }
        if(ImGui.Button($"关闭抑制"))
        {
            Svc.PluginInterface.GetIpcSubscriber<bool, object>("AutoRetainer.SetSuppressed").InvokeAction(false);
        }
        if(TryGetAddonByName<AddonSelectString>("SelectString", out var sel))
        {
            var entries = Utils.GetEntries(sel);
            foreach(var x in entries)
            {
                var index = entries.IndexOf(x);
                if(ImGui.SmallButton($"{x} / {index}") && index >= 0)
                {
                    new AddonMaster.SelectString(sel).Entries[index].Select();
                }
            }
        }
    }
}
