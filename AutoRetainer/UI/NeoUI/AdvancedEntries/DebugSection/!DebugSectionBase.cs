namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;
public abstract class DebugSectionBase : NeoUIEntry
{
    public override string Path => $"高级/调试/{GetType().Name.Replace("Debug", "") switch
    {
        "AddonMaster" => "插件界面读取",
        "Artisan" => "Artisan 联动",
        "Bailout" => "脱困",
        "Cabinet" => "收藏柜",
        "GCAuto" => "自动筹备稀有品",
        "InventoryManagement" => "背包管理",
        "IPC" => "IPC",
        "Misc" => "杂项",
        "Multi" => "多角色",
        "NeoGCDelivery" => "新版筹备稀有品",
        "NMAPI" => "通知 API",
        "Reader" => "界面读取器",
        "RetainerTaskSupply" => "雇员探险列表",
        "Scheduler" => "调度器",
        "Throttle" => "节流",
        "Venture" => "探险",
        "Voyage" => "远航探索",
        "RetainersOld" => "旧版雇员",
        "SuperSecret" => "隐藏设置",
        _ => "其他",
    }}";
    public override bool ShouldDisplay()
    {
        return C.Verbose;
    }
}
