namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal class DebugArtisan : DebugSectionBase
{
    public override void Draw()
    {
        foreach(var d in C.OfflineData)
        {
            foreach(var r in d.RetainerData)
            {
                ImGuiEx.Text($"雇员 {r.Name}：{r.VentureEndsAt}");
                ImGui.SameLine();
                if(ImGui.Button($"1 分钟##{r.Identity}"))
                {
                    r.VentureEndsAt = P.Time + 60;
                }
                ImGui.SameLine();
                if(ImGui.Button($"15 秒##{r.Identity}"))
                {
                    r.VentureEndsAt = P.Time + 15;
                }
            }
        }
        ImGui.Separator();
        ImGui.Checkbox("Artisan 已暂停", ref ArtisanManager.WasPaused);
        {
            var r = SchedulerMain.Reason;
            if(ImGuiEx.EnumCombo("启用原因", ref r, Lang.PluginEnableReasonNames)) SchedulerMain.Reason = r;
            try
            {
                if(ImGui.Button("设置耐久状态：是")) Artisan.SetEnduranceStatus(true);
                if(ImGui.Button("设置耐久状态：否")) Artisan.SetEnduranceStatus(false);
                if(ImGui.Button("设置列表暂停：是")) Artisan.SetListPause(true);
                if(ImGui.Button("设置列表暂停：否")) Artisan.SetListPause(false);
                if(ImGui.Button("设置停止请求：是")) Artisan.SetStopRequest(true);
                if(ImGui.Button("设置停止请求：否")) Artisan.SetStopRequest(false);
                ImGuiEx.Text($"列表已暂停：{Lang.Bool(Artisan.IsListPaused())}");
                ImGuiEx.Text($"列表运行中：{Lang.Bool(Artisan.IsListRunning())}");
                ImGuiEx.Text($"耐久状态：{Lang.Bool(Artisan.GetEnduranceStatus())}");
                ImGuiEx.Text($"停止请求：{Lang.Bool(Artisan.GetStopRequest())}");
            }
            catch(Exception e)
            {
                ImGuiEx.Text(EColor.Red, $"{e.Message}");
            }
        }
    }
}
