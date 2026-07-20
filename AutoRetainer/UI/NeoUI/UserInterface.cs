using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public sealed unsafe class UserInterface : NeoUIEntry
{
    public override string Path => "用户界面";

    public override NuiBuilder Builder => new NuiBuilder()

        .Section("用户界面")
        .Checkbox("匿名化雇员", () => ref C.NoNames, "开启后会在常规界面中隐藏雇员名称，但不会影响调试菜单与插件日志。启用期间，插件不同页面中的角色/雇员编号不保证一一对应（例如雇员页的 1 号不一定是统计页中的同一位雇员）。")
        .Checkbox("在雇员界面显示快捷菜单", () => ref C.UIBar)
        .Checkbox("显示扩展雇员信息", () => ref C.ShowAdditionalInfo, "在主界面显示雇员平均品级/获得力/鉴别力以及当前探险名称。")
        .Widget("按 ESC 时不关闭 AutoRetainer 窗口", (x) =>
        {
            if(ImGui.Checkbox(x, ref C.IgnoreEsc)) Utils.ResetEscIgnoreByWindows();
        })
        .Checkbox("状态栏仅显示最关键图标", () => ref C.StatusBarMSI)
        .SliderInt(120f, "状态栏图标大小", () => ref C.StatusBarIconWidth, 32, 128)
        .Checkbox("游戏启动时打开 AutoRetainer 窗口", () => ref C.DisplayOnStart)
        .Checkbox("启用标题界面按钮（需重启插件）", () => ref C.UseTitleScreenButton)
        .Checkbox("隐藏角色搜索", () => ref C.NoCharaSearch)
        .Checkbox("已完成角色不闪烁背景色", () => ref C.NoGradient)
        .Checkbox("不提示同目录运行第二个游戏实例", () => ref C.No2ndInstanceNotify, "启用后会在第二个同目录游戏实例中自动跳过 AutoRetainer 加载；在主实例关闭此选项前，无法在该实例中加载插件。")

        .Section("雇员页的角色排序")
        .Checkbox("启用", () => ref C.EnableRetainerSort)
        .TextWrapped("这只是界面显示顺序，不会影响角色处理逻辑。")
        .Widget(() => UIUtils.DrawSortableEnumList("雇员顺序", C.RetainersVisualOrders, Lang.RetainersVisualOrderNames))

        .Section("远航探索页的角色排序")
        .Checkbox("启用", () => ref C.EnableDeployablesSort)
        .TextWrapped("这只是界面显示顺序，不会影响角色处理逻辑。")
        .Widget(() => UIUtils.DrawSortableEnumList("远航探索顺序", C.DeployablesVisualOrders, Lang.DeployablesVisualOrderNames));



}
