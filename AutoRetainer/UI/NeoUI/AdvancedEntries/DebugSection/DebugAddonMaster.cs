using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;
public unsafe class DebugAddonMaster : DebugSectionBase
{
    public override void Draw()
    {
        if(ImGui.CollapsingHeader("雇员列表###RetainerList"))
        {
            if(TryGetAddonByName<AtkUnitBase>("RetainerList", out var addon) && IsAddonReady(addon))
            {
                var r = new AddonMaster.RetainerList(addon);
                foreach(var x in r.Retainers)
                {
                    ImGuiEx.Text($"{x.Name}，{Lang.Bool(x.IsActive)}");
                    if(ImGuiEx.HoveredAndClicked())
                    {
                        x.Select();
                    }
                }
            }
        }

        if(ImGui.CollapsingHeader("标题菜单###_TitleMenu"))
        {
            if(TryGetAddonMaster<AddonMaster._TitleMenu>(out var m) && m.IsAddonReady)
            {
                ImGuiEx.Text($"已就绪：{Lang.Bool(m.IsReady)}");
                if(ImGui.Button("开始游戏")) m.Start();
                if(ImGui.Button("数据中心")) m.DataCenter();
                if(ImGui.Button("退出游戏")) m.Exit();
            }
        }

        if(ImGui.CollapsingHeader("标题界面数据中心地图###TitleDCWorldMap"))
        {
            if(TryGetAddonMaster<AddonMaster.TitleDCWorldMap>(out var m) && m.IsAddonReady)
            {
                foreach(var x in AddonMaster.TitleDCWorldMap.PublicDC)
                {
                    if(ImGui.Button(Svc.Data.GetExcelSheet<WorldDCGroupType>().GetRowOrDefault((uint)x)?.Name.ToString() ?? ""))
                    {
                        m.Select(x);
                    }
                }
            }
        }

        if(ImGui.CollapsingHeader("角色选择服务器###_CharaSelectWorldServer"))
        {
            if(TryGetAddonMaster<AddonMaster._CharaSelectWorldServer>(out var m))
            {
                foreach(var x in m.Worlds)
                {
                    if(ImGui.Button(x.Name))
                    {
                        x.Select();
                    }
                }
            }
        }

        if(ImGui.CollapsingHeader("角色选择列表菜单###_CharaSelectListMenu"))
        {
            if(TryGetAddonMaster<AddonMaster._CharaSelectListMenu>(out var m) && m.IsAddonReady)
            {
                if(ImGui.Button("服务器##w"))
                {
                    m.SelectWorld();
                }
                ImGuiEx.Text($"{AgentLobby.Instance()->LobbyUpdateStage}");
                ImGuiEx.Text($"{AgentLobby.Instance()->HoveredCharacterContentId}");
                foreach(var x in m.Characters)
                {
                    if(ImGui.Button($"{x} / 选择##select{x}"))
                    {
                        x.Select();
                    }
                    ImGui.SameLine();
                    if(ImGui.Button($"{x} / 登录##login{x}"))
                    {
                        x.Login();
                    }
                    ImGui.SameLine();
                    if(ImGui.Button($"{x} / 快捷菜单##context{x}"))
                    {
                        x.OpenContextMenu();
                    }
                    if(x.IsSelected)
                    {
                        ImGuiEx.Text($"已选择");
                    }
                }
            }
        }
    }
}
