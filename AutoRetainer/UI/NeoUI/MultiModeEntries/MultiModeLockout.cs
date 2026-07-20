using ECommons.ExcelServices;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeLockout : NeoUIEntry
{
    public override string Path => "多角色模式/大区锁定";

    private int Num = 12;

    public override void Draw()
    {
        ImGuiEx.TextV("持续时长");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150f);
        ImGui.InputInt("小时...", ref Num.ValidateRange(1, 10000));
        foreach(var x in Enum.GetValues<ExcelWorldHelper.Region>())
        {
            if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Lock, $"...不登录 {Lang.RegionName(x)}角色"))
            {
                C.LockoutTime[x] = DateTimeOffset.Now.ToUnixTimeSeconds() + Num * 60 * 60;
            }
        }
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Unlock, "移除全部锁定"))
        {
            C.LockoutTime.Clear();
        }
    }
}
