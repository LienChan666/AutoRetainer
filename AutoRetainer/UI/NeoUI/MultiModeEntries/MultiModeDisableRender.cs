using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;

public class MultiModeDisableRender : NeoUIEntry
{
    public override string Path => "多角色模式/禁用渲染";

    public override NuiBuilder Builder => new NuiBuilder()
        .Section("禁用渲染")
        .Checkbox("多角色模式运行时禁用渲染", () => ref C.MultiDisableRender, "多角色模式运行期间停止渲染游戏世界。")
        .Checkbox("仅夜间模式下", () => ref C.MultiDisableRenderNightModeOnly)
        .Checkbox("仅游戏窗口未激活时", () => ref C.MultiDisableRenderOnlyInactive);
}
