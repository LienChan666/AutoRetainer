using ECommons.GameHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public sealed unsafe class AccountWhitelist : NeoUIEntry
{
    public override void Draw()
    {
        ImGuiEx.TextWrapped($"你可以设置账号白名单。若登录的账号不在白名单中，AutoRetainer 将不会记录任何角色、雇员或远航探索数据。");
        if(C.WhitelistedAccounts.Count == 0)
        {
            ImGuiEx.TextWrapped(EColor.GreenBright, "账号白名单状态：已禁用。添加任意账号后即可启用。");
        }
        else
        {
            ImGuiEx.TextWrapped(EColor.YellowBright, "账号白名单状态：已启用。移除全部账号后即可禁用。");
        }

        foreach(var x in C.WhitelistedAccounts)
        {
            ImGui.PushID(x.ToString());
            if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
            {
                new TickScheduler(() => C.WhitelistedAccounts.Remove(x));
            }
            ImGui.SameLine();
            ImGuiEx.TextV($"账号 {x}");
            ImGui.PopID();
        }
    }
}
