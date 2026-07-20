using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainerAPI.Configuration;
using Dalamud.Utility;
using ECommons.ExcelServices;
using ECommons.ExcelServices.Sheets;
using ECommons.Interop;
using Lumina.Excel.Sheets;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;
using GrandCompany = ECommons.ExcelServices.GrandCompany;

namespace AutoRetainer;

internal static class Lang
{
    internal const string CharPlant = "";
    internal const string CharLevel = "";
    internal const string CharItemLevel = "";
    internal const string CharDice = "";
    internal const string CharDeny = "";
    internal const string CharQuestion = "";
    internal const string CharLevelSync = "";
    internal const string CharP = "";
    internal const string StrDCV = "";

    internal const string IconRefresh = "\uf2f9";
    internal const string IconMultiMode = "\uf021";
    internal const string IconDuplicate = "\uf24d";
    internal const string IconGil = "\uf51e";
    internal const string IconPlanner = "\uf0ae";
    internal const string IconSettings = "\uf013";
    internal const string IconWarning = "\uf071";

    internal const string IconAnchor = "\uf13d";
    internal const string IconLevelup = "\ue098";
    internal const string IconResend = "\ue4bb";
    internal const string IconUnlock = "\uf13e";
    internal const string IconRepeat = "\uf363";
    internal const string IconPath = "\uf55b";
    internal const string IconFire = "\uf06d";

    internal static string Bool(bool? value) => value == null ? "未知" : value.Value ? "是" : "否";

    internal static string LogOutAndExitGame => Svc.Data.GetExcelSheet<Addon>().GetRow(116).Text.GetText(true).Cleanup();

    internal static readonly ReadOnlyDictionary<UnlockMode, string> UnlockModeNames = new(new Dictionary<UnlockMode, string>()
    {
        { UnlockMode.MultiSelect, "选择尽可能多的目的地" },
        { UnlockMode.SpamOne, "重复派遣单一目的地" },
        { UnlockMode.WhileLevelling, "升级时包含一个待解锁目的地" },
    });

    internal static readonly ReadOnlyDictionary<VesselBehavior, string> VesselBehaviorNames = new(new Dictionary<VesselBehavior, string>()
    {
        { VesselBehavior.Finalize, "仅结算奖励" },
        { VesselBehavior.Redeploy, "再次派遣" },
        { VesselBehavior.LevelUp, "最佳经验航线" },
        { VesselBehavior.Unlock, "解锁目的地" },
        { VesselBehavior.Use_plan, "使用目的地方案" },
    });

    internal static readonly ReadOnlyDictionary<PlanCompleteBehavior, string> PlanCompleteBehaviorNames = new(new Dictionary<PlanCompleteBehavior, string>()
    {
        { PlanCompleteBehavior.Restart_plan, "重新开始方案" },
        { PlanCompleteBehavior.Assign_Quick_Venture, "执行自由探索委托" },
        { PlanCompleteBehavior.Do_nothing, "不执行操作" },
        { PlanCompleteBehavior.Repeat_last_venture, "重复上次探险" },
    });

    internal static readonly ReadOnlyDictionary<UnavailableVentureDisplay, string> UnavailableVentureDisplayNames = new(new Dictionary<UnavailableVentureDisplay, string>()
    {
        { UnavailableVentureDisplay.Hide, "隐藏" },
        { UnavailableVentureDisplay.Display, "显示" },
        { UnavailableVentureDisplay.Allow_selection, "允许选择" },
    });

    internal static readonly ReadOnlyDictionary<MultiModeType, string> MultiModeTypeNames = new(new Dictionary<MultiModeType, string>()
    {
        { MultiModeType.Retainers, "仅雇员" },
        { MultiModeType.Submersibles, "仅远航探索" },
        { MultiModeType.Everything, "全部" },
    });

    internal static readonly ReadOnlyDictionary<GCDeliveryType, string> GCDeliveryTypeNames = new(new Dictionary<GCDeliveryType, string>()
    {
        { GCDeliveryType.Disabled, "禁用" },
        { GCDeliveryType.Hide_Armoury_Chest_Items, "隐去兵装库中的物品" },
        { GCDeliveryType.Hide_Gear_Set_Items, "隐去套装中的物品" },
        { GCDeliveryType.Show_All_Items, "显示所有物品" },
    });

    internal static readonly ReadOnlyDictionary<CutsceneSkipMode, string> CutsceneSkipModeNames = new(new Dictionary<CutsceneSkipMode, string>()
    {
        { CutsceneSkipMode.Never, "从不" },
        { CutsceneSkipMode.When_Multi_Mode_is_on, "仅多角色模式启用时" },
        { CutsceneSkipMode.Always, "始终" },
    });

    internal static readonly ReadOnlyDictionary<OpenBellBehavior, string> OpenBellBehaviorNames = new(new Dictionary<OpenBellBehavior, string>()
    {
        { OpenBellBehavior.Do_nothing, "不执行操作" },
        { OpenBellBehavior.Enable_AutoRetainer, "启用 AutoRetainer" },
        { OpenBellBehavior.Disable_AutoRetainer, "禁用 AutoRetainer" },
        { OpenBellBehavior.Pause_AutoRetainer, "暂停 AutoRetainer" },
    });

    internal static readonly ReadOnlyDictionary<TaskCompletedBehavior, string> TaskCompletedBehaviorNames = new(new Dictionary<TaskCompletedBehavior, string>()
    {
        { TaskCompletedBehavior.Close_retainer_list_and_disable_plugin, "关闭雇员列表并禁用插件" },
        { TaskCompletedBehavior.Close_retainer_list_and_keep_plugin_enabled, "关闭雇员列表并保持插件启用" },
        { TaskCompletedBehavior.Stay_in_retainer_list_and_disable_plugin, "停留在雇员列表并禁用插件" },
        { TaskCompletedBehavior.Stay_in_retainer_list_and_keep_plugin_enabled, "停留在雇员列表并保持插件启用" },
    });

    internal static readonly ReadOnlyDictionary<PluginEnableReason, string> PluginEnableReasonNames = new(new Dictionary<PluginEnableReason, string>()
    {
        { PluginEnableReason.Access, "访问传唤铃" },
        { PluginEnableReason.Manual, "手动" },
        { PluginEnableReason.Auto, "自动" },
        { PluginEnableReason.MultiMode, "多角色模式" },
        { PluginEnableReason.Artisan, "Artisan 联动" },
    });

    internal static readonly ReadOnlyDictionary<RetainersVisualOrder, string> RetainersVisualOrderNames = new(new Dictionary<RetainersVisualOrder, string>()
    {
        { RetainersVisualOrder.Ventures, "探险币" },
        { RetainersVisualOrder.Inventory_Slots, "背包空位" },
        { RetainersVisualOrder.Region_JP, "日本大区" },
        { RetainersVisualOrder.Region_NA, "北美大区" },
        { RetainersVisualOrder.Region_EU, "欧洲大区" },
        { RetainersVisualOrder.Region_OC, "大洋洲大区" },
        { RetainersVisualOrder.World, "服务器" },
        { RetainersVisualOrder.DataCenter, "数据中心" },
        { RetainersVisualOrder.Name, "名称" },
    });

    internal static readonly ReadOnlyDictionary<DeployablesVisualOrder, string> DeployablesVisualOrderNames = new(new Dictionary<DeployablesVisualOrder, string>()
    {
        { DeployablesVisualOrder.Ceruleum, "桶装青磷水" },
        { DeployablesVisualOrder.Repair_Kits, "魔导机械修理材料" },
        { DeployablesVisualOrder.Inventory_Slots, "背包空位" },
        { DeployablesVisualOrder.Region_JP, "日本大区" },
        { DeployablesVisualOrder.Region_NA, "北美大区" },
        { DeployablesVisualOrder.Region_EU, "欧洲大区" },
        { DeployablesVisualOrder.Region_OC, "大洋洲大区" },
        { DeployablesVisualOrder.World, "服务器" },
        { DeployablesVisualOrder.DataCenter, "数据中心" },
        { DeployablesVisualOrder.Name, "名称" },
    });

    internal static readonly ReadOnlyDictionary<ExcelWorldHelper.Region, string> RegionNames = new(new Dictionary<ExcelWorldHelper.Region, string>()
    {
        { ExcelWorldHelper.Region.JP, "日本大区" },
        { ExcelWorldHelper.Region.NA, "北美大区" },
        { ExcelWorldHelper.Region.EU, "欧洲大区" },
        { ExcelWorldHelper.Region.OC, "大洋洲大区" },
        { (ExcelWorldHelper.Region)5, "中国大区" },
        { (ExcelWorldHelper.Region)6, "韩国大区" },
        { (ExcelWorldHelper.Region)7, "北美云大区" },
    });

    internal static string RegionName(ExcelWorldHelper.Region region)
        => RegionNames.TryGetValue(region, out var name) ? name : $"未知大区（{(int)region}）";

    internal static readonly ReadOnlyDictionary<GCExchangeCategoryTab, string> GCExchangeCategoryNames = new(new Dictionary<GCExchangeCategoryTab, string>()
    {
        { GCExchangeCategoryTab.Weapons, "武具" },
        { GCExchangeCategoryTab.Armor, "防具" },
        { GCExchangeCategoryTab.Materiel, "军用品" },
        { GCExchangeCategoryTab.Materials, "展示品" },
    });

    internal static readonly ReadOnlyDictionary<ItemRarity, string> ItemRarityNames = new(new Dictionary<ItemRarity, string>()
    {
        { ItemRarity.White, "白色" },
        { ItemRarity.Green, "绿色" },
        { ItemRarity.Blue, "蓝色" },
        { ItemRarity.Purple, "紫色" },
        { ItemRarity.Pink, "粉色" },
    });

    internal static readonly ReadOnlyDictionary<GrandCompany, string> GrandCompanyNames = new(new Dictionary<GrandCompany, string>()
    {
        { GrandCompany.Unemployed, "未加入" },
        { GrandCompany.Maelstrom, "黑涡团" },
        { GrandCompany.TwinAdder, "双蛇党" },
        { GrandCompany.ImmortalFlames, "恒辉队" },
    });

    internal static readonly ReadOnlyDictionary<VoyageType, string> VoyageTypeNames = new(new Dictionary<VoyageType, string>()
    {
        { VoyageType.Airship, "飞空艇" },
        { VoyageType.Submersible, "潜水艇" },
    });

    internal static readonly ReadOnlyDictionary<PanelType, string> PanelTypeNames = new(new Dictionary<PanelType, string>()
    {
        { PanelType.None, "无" },
        { PanelType.Unknown, "未知" },
        { PanelType.TypeSelector, "类型选择" },
        { PanelType.Airship, "飞空艇" },
        { PanelType.Submersible, "潜水艇" },
    });

    private static string GetKeyName(LimitedKeys key)
    {
        var name = key.ToString();
        if(name.Length == 1 || (name.StartsWith('F') && int.TryParse(name.AsSpan(1), out _))) return name;
        if(name.StartsWith("Digit_")) return name[6..];
        if(name.StartsWith("NumPad")) return $"小键盘 {name[6..]}";
        return key switch
        {
            LimitedKeys.None => "未设置",
            LimitedKeys.LeftMouseButton => "鼠标左键",
            LimitedKeys.RightMouseButton => "鼠标右键",
            LimitedKeys.MiddleMouseButton => "鼠标中键",
            LimitedKeys.XMouseButton1 => "鼠标侧键 1",
            LimitedKeys.XMouseButton2 => "鼠标侧键 2",
            LimitedKeys.Back => "退格键",
            LimitedKeys.Tab => "Tab 键",
            LimitedKeys.Clear => "清除键",
            LimitedKeys.Enter => "回车键",
            LimitedKeys.Pause => "暂停键",
            LimitedKeys.CapsLock => "大写锁定键",
            LimitedKeys.Escape => "Esc 键",
            LimitedKeys.Space => "空格键",
            LimitedKeys.PageUp => "上一页键",
            LimitedKeys.PageDown => "下一页键",
            LimitedKeys.End => "End 键",
            LimitedKeys.Home => "Home 键",
            LimitedKeys.Left => "左方向键",
            LimitedKeys.Up => "上方向键",
            LimitedKeys.Right => "右方向键",
            LimitedKeys.Down => "下方向键",
            LimitedKeys.PrintScreen => "截图键",
            LimitedKeys.Insert => "插入键",
            LimitedKeys.Delete => "删除键",
            LimitedKeys.LeftShiftKey => "左 Shift 键",
            LimitedKeys.RightShiftKey => "右 Shift 键",
            LimitedKeys.LeftControlKey => "左 Ctrl 键",
            LimitedKeys.RightControlKey => "右 Ctrl 键",
            LimitedKeys.LeftAltKey => "左 Alt 键",
            LimitedKeys.RightAltKey => "右 Alt 键",
            _ => $"按键 {(int)key}",
        };
    }

    internal static readonly ReadOnlyDictionary<LimitedKeys, string> LimitedKeyNames = new(Enum.GetValues<LimitedKeys>().Distinct().ToDictionary(x => x, GetKeyName));

    private static readonly Dictionary<string, string> SubmarineClassNames = new()
    {
        { "Shark", "鲨鱼级" },
        { "Unkiu", "甲鲎级" },
        { "Whale", "须鲸级" },
        { "Coelacanth", "腔棘鱼级" },
        { "Syldra", "希尔德拉级" },
        { "ModShark", "鲨鱼改级" },
        { "ModUnkiu", "甲鲎改级" },
        { "ModWhale", "须鲸改级" },
        { "ModCoelacanth", "腔棘鱼改级" },
        { "ModSyldra", "希尔德拉改级" },
    };

    private static ReadOnlyDictionary<T, string> GetSubmarinePartNames<T>() where T : struct, Enum
        => new(Enum.GetValues<T>().ToDictionary(x => x, x => SubmarineClassNames[x.ToString()]));

    internal static readonly ReadOnlyDictionary<Hull, string> HullNames = GetSubmarinePartNames<Hull>();
    internal static readonly ReadOnlyDictionary<Stern, string> SternNames = GetSubmarinePartNames<Stern>();
    internal static readonly ReadOnlyDictionary<Bow, string> BowNames = GetSubmarinePartNames<Bow>();
    internal static readonly ReadOnlyDictionary<Bridge, string> BridgeNames = GetSubmarinePartNames<Bridge>();

    internal static readonly (string Normal, string GameFont) Digits = ("0123456789", "");

    internal static readonly string[] FieldExplorationNames =
    [
        "Field Exploration.",
        "Highland Exploration.",
        "Woodland Exploration.",
        "Waterside Exploration.",
        "探索依頼：平地　　（必要ベンチャースクリップ：2枚）",
        "探索依頼：山岳　　（必要ベンチャースクリップ：2枚）",
        "探索依頼：森林　　（必要ベンチャースクリップ：2枚）",
        "探索依頼：水辺　　（必要ベンチャースクリップ：2枚）",
        "Felderkundung (2 Wertmarken)",
        "Hochlanderkundung (2 Wertmarken)",
        "Forsterkundung (2 Wertmarken)",
        "Gewässererkundung (2 Wertmarken)",
        "Exploration en plaine (2 jetons)",
        "Exploration en montagne (2 jetons)",
        "Exploration en forêt (2 jetons)",
        "Exploration en rivage (2 jetons)",
        "平地探索委托（需要2枚探险币）",
        "山岳探索委托（需要2枚探险币）",
        "森林探索委托（需要2枚探险币）",
        "水岸探索委托（需要2枚探险币）",
        "平地探索委託（需要2枚探險幣）",
        "山岳探索委託（需要2枚探險幣）",
        "森林探索委託（需要2枚探險幣）",
        "水岸探索委託（需要2枚探險幣）",
        "탐색수행: 평지 (필요한 집사 급료: 2개)",
        "탐색수행: 산악 (필요한 집사 급료: 2개)",
        "탐색수행: 삼림 (필요한 집사 급료: 2개)",
        "탐색수행: 물가 (필요한 집사 급료: 2개)",
    ];

    internal static readonly string[] HuntingVentureNames =
    [
        "Hunting.",
        "Mining.",
        "Botany.",
        "Fishing.",
        "調達依頼：渉猟　　（必要ベンチャースクリップ：1枚）",
        "調達依頼：採掘　　（必要ベンチャースクリップ：1枚）",
        "調達依頼：園芸　　（必要ベンチャースクリップ：1枚）",
        "調達依頼：漁猟　　（必要ベンチャースクリップ：1枚）",
        "Beutezug (1 Wertmarke)",
        "Mineraliensuche (1 Wertmarke)",
        "Ernteausflug (1 Wertmarke)",
        "Fischzug (1 Wertmarke)",
        "Travail de chasse (1 jeton)",
        "Travail de mineur (1 jeton)",
        "Travail de botaniste (1 jeton)",
        "Travail de pêche (1 jeton)",
        "狩猎筹集委托（需要1枚探险币）",
        "采矿筹集委托（需要1枚探险币）",
        "采伐筹集委托（需要1枚探险币）",
        "捕鱼筹集委托（需要1枚探险币）",
        "狩獵籌集委託（需要1枚探險幣）",
        "採礦籌集委託（需要1枚探險幣）",
        "採伐籌集委託（需要1枚探險幣）",
        "捕魚籌集委託（需要1枚探險幣）",
        "조달수행: 사냥 (필요한 집사 급료: 1개)",
        "조달수행: 광부 (필요한 집사 급료: 1개)",
        "조달수행: 원예가 (필요한 집사 급료: 1개)",
        "조달수행: 어부 (필요한 집사 급료: 1개)",
    ];

    internal static readonly string[] QuickExploration =
    [
        "Quick Exploration.",
        "ほりだしもの依頼　（必要ベンチャースクリップ：2枚）",
        "Schneller Streifzug (2 Wertmarken)",
        "Tâche improvisée (2 jetons)",
        "自由探索委托（需要2枚探险币）",
        "自由探索委託（需要2枚探險幣）",
        "발굴수행 (필요한 집사 급료: 2개)",
    ];

    internal static readonly string[] Entrance =
    [
        "ハウスへ入る",
        "进入房屋",
        "進入房屋",
        "Eingang",
        "Entrée",
        "Entrance",
        "주택으로 들어가기",
    ];

    internal static string ApartmentEntrance => Svc.Data.GetExcelSheet<EObjName>().GetRow(2007402).Singular.ToString();

    internal static readonly string[] ConfirmHouseEntrance =
    [
        "「ハウス」へ入りますか？",
        "要进入这间房屋吗？",
        "要進入這間房屋嗎？",
        "Das Gebäude betreten?",
        "Entrer dans la maison ?",
        "Enter the estate hall?",
        "'주택'으로 들어가시겠습니까?",
    ];

    internal static readonly string[] RetainerAskCategoryText =
    [
        "依頼するリテイナーベンチャーを選んでください",
        "请选择要委托的探险",
        "請選擇要委託的探險",
        "Wähle eine Unternehmung, auf die du den Gehilfen schicken möchtest.",
        "Choisissez un type de tâche :",
        "Select a category.",
        "집사 수행의 종류를 선택하십시오.",
    ];

    internal static string[] BellName => [Svc.Data.GetExcelSheet<EObjName>().GetRow(2000401).Singular.GetText(), "リテイナーベル"];

    //0	TEXT_HOUFIXMANSIONENTRANCE_00359_HOUSINGAREA_MENU_ENTER_MYROOM	Go to your apartment
    //0	TEXT_HOUFIXMANSIONENTRANCE_00359_HOUSINGAREA_MENU_ENTER_MYROOM	自分の部屋に移動する
    //0	TEXT_HOUFIXMANSIONENTRANCE_00359_HOUSINGAREA_MENU_ENTER_MYROOM	Die eigene Wohnung betreten
    //0	TEXT_HOUFIXMANSIONENTRANCE_00359_HOUSINGAREA_MENU_ENTER_MYROOM	Aller dans votre appartement

    internal static readonly string[] GoToYourApartment =
    [
        "Go to your apartment",
        "自分の部屋に移動する",
        "移动到自己的房间",
        "移動到自己的房間",
        "Die eigene Wohnung betreten",
        "Aller dans votre appartement",
        "자신의 방으로 이동",
    ];

    internal static readonly string[] SkipCutsceneStr =
    [
        "Skip cutscene?",
        "要跳过这段过场动画吗？",
        "要跳過這段過場動畫嗎？",
        "Videosequenz überspringen?",
        "Passer la scène cinématique ?",
        "このカットシーンをスキップしますか？",
        "영상을 건너뛰시겠습니까?",
    ];
    //11	TEXT_CMNDEFHOUSINGPERSONALROOMENTRANCE_00178_GOTO_WORKSHOP	Move to the company workshop
    //11	TEXT_CMNDEFHOUSINGPERSONALROOMENTRANCE_00178_GOTO_WORKSHOP	地下工房に移動する
    //11	TEXT_CMNDEFHOUSINGPERSONALROOMENTRANCE_00178_GOTO_WORKSHOP	Die Ge<SoftHyphen/>sell<SoftHyphen/>schaftswerkstätte betreten
    //11	TEXT_CMNDEFHOUSINGPERSONALROOMENTRANCE_00178_GOTO_WORKSHOP	Aller dans l'atelier de compagnie
    internal static readonly string[] EnterWorkshop = ["Move to the company workshop", "地下工房に移動する", "移动到部队工房", "移動到部隊工房", "Die Gesellschaftswerkstätte betreten", "Aller dans l'atelier de compagnie", "지하공방으로 이동", Svc.Data.GetExcelSheet<QuestDialogueText>(name: "custom/001/CmnDefHousingPersonalRoomEntrance_00178").GetRow(11).Value.GetText()];

    internal static readonly string[] AirshipManagement = ["Airship Management", "飛空艇の管理", "管理飞空艇", "管理飛空艇", "Luftschiff verwalten", "Contrôle aérien", "비공정 관리"];
    internal static readonly string[] SubmarineManagement = ["Submersible Management", "潜水艦の管理", "管理潜水艇", "管理潛水艇", "Tauchboot verwalten", "Contrôle sous-marin", "잠수함 관리"];
    internal static readonly string[] CancelVoyage = ["Cancel", "キャンセル", "取消", "Abbrechen", "Annuler", "취소"];
    internal static readonly string[] NothingVoyage = ["Nothing.", "やめる", "取消", "Nichts", "Annuler", "그만두기"];
    internal static readonly string[] DeployOnSubaquaticVoyage = ["Deploy submersible on subaquatic voyage", "ボイジャー出港", "出发", "出發", "Auf Erkundung gehen", "Expédier le sous-marin", "탐사 출항"];
    internal static readonly string[] ViewPrevVoyageLog = ["View previous voyage log", "前回のボイジャー報告", "上次的远航报告", "上次的遠航報告", "Bericht der letzten Erkundung", "Consulter le journal de la précédente expédition", "이전 탐사 보고서"];
    internal static readonly string[] VoyageQuitEntry = ["Quit", "やめる", "取消", "Beenden", "Annuler", "그만두기"];
    internal static readonly string[] ChangeSubmersibleComponents = ["Change submersible components", "パーツの変更", "Bauteile austauschen", "Changer les éléments", "부품 변경","更换配件","更換配件"];
    internal static readonly string[] RegisterSub = ["Outfit and register a submersible.", "潜水艦の新規登録", "Registrierung eines neuen Tauchboots", "Enregistrement d'un sous-marin", "새 잠수함 등록","登记新的潜水艇","登記新的潛水艇"]; 

    internal static readonly string[] PanelAirship = ["Select an airship.", "飛空艇を選択してください。", "请选择飞空艇。", "請選擇飛空艇。", "Wähle ein Luftschiff.", "Choisissez un aéronef.", "비공정을 선택하십시오."];
    internal static readonly string[] PanelSubmersible = ["Select a submersible.", "潜水艦を選択してください。", "请选择潜水艇。", "請選擇潛水艇。", "Wähle ein Tauchboot.", "Choisissez un sous-marin.", "잠수함을 선택하십시오."];

    //2004353	entrance to additional chambers	0	entrances to additional chambers	0	1	1	0	0
    internal static string[] AdditionalChambersEntrance =>
    [
        Svc.Data.GetExcelSheet<EObjName>().GetRow(2004353).Singular.GetText(),
        Regex.Replace(Svc.Data.GetExcelSheet<EObjName>().GetRow(2004353).Singular.GetText(), @"\[.*?\]", "")
    ];

    //2005274	voyage control panel	0	voyage control panels	0	0	1	0	0
    internal static string PanelName => Svc.Data.GetExcelSheet<EObjName>().GetRow(2005274).Singular.GetText();

    //4160	60	9	0	False	Unable to retrieve extracted items. Insufficient inventory/crystal inventory space.
    internal static string VoyageInventoryError => Svc.Data.GetExcelSheet<LogMessage>().GetRow(4160).Text.ToDalamudString().GetText();

    internal static string[] UnableToVisitWorld = ["Unable to execute command. Character is currently visiting the", "他のデータセンター", "无法进行该操作，角色正在访问其他数据中心", "無法進行該操作，角色正在訪問其他資料中心", "Der Vorgang kann nicht ausgeführt werden, da der Charakter gerade das Datenzentrum", "Impossible d'exécuter cette commande. Le personnage se trouve dans un autre centre de traitement de données", "다른 데이터 센터"];

    //4169	60	9	0	False	Unable to repair vessel component without the required <SheetEn(Item,3,IntegerParameter(1),1,1)/>.
    //4272	60	9	0	False Unable to repair vessel.Insufficient<SheetEn(Item,3,IntegerParameter(1),3,1)/>.
    //4169	60	9	0	False	修理に必要な<Sheet(Item,IntegerParameter(1),0)/>を持っていません。
    //4272	60	9	0	False	修理に必要な<Sheet(Item,IntegerParameter(1),0)/>が足りません。
    //4169	60	9	0	False	未持有修理所必需的<Sheet(Item,IntegerParameter(1),0)/>。
    //4272	60	9	0	False	沒有修理所必需的<Sheet(Item,IntegerParameter(1),0)/>。
    //4272	60	9	0	False	Du hast nicht genug <SheetDe(Item,5,IntegerParameter(1),2,4,1)/> für die Reparatur.
    //4169	60	9	0	False	Für die Reparatur ist <SheetDe(Item,1,IntegerParameter(1),1,1,1)/> erforderlich.
    //4169	60	9	0	False	Réparation impossible. Vous n'avez pas <SheetFr(Item,2,IntegerParameter(1),1,1)/> nécessaire.
    //4272	60	9	0	False	Vous n'avez pas <SheetFr(Item,2,IntegerParameter(1),1,1)/> nécessaire à la réparation.

    internal static readonly string[] UnableToRepairVessel = ["修理に必要な", "修理所必需的", "Unable to repair vessel", "Du hast nicht genug", "Für die Reparatur ist", "Réparation impossible. Vous n'avez pas", "nécessaire à la réparation", "수리에 필요한"];

    //11	TEXT_HOUFIXCOMPANYSUBMARINE_00447_SUBMARINE_CMD_REPAIR_PARTS	パーツの修理
    //11	TEXT_HOUFIXCOMPANYSUBMARINE_00447_SUBMARINE_CMD_REPAIR_PARTS	Bauteile reparieren
    //11	TEXT_HOUFIXCOMPANYSUBMARINE_00447_SUBMARINE_CMD_REPAIR_PARTS	Réparer des éléments
    //11	TEXT_HOUFIXCOMPANYSUBMARINE_00447_SUBMARINE_CMD_REPAIR_PARTS	修理配件
    //10	TEXT_CMNDEFCOMPANYCOMMANDERBOARD_00258_AIRSHIP_CMD_REPAIR_PARTS	パーツの修理
    //10	TEXT_CMNDEFCOMPANYCOMMANDERBOARD_00258_AIRSHIP_CMD_REPAIR_PARTS	Bauteile reparieren
    //10	TEXT_CMNDEFCOMPANYCOMMANDERBOARD_00258_AIRSHIP_CMD_REPAIR_PARTS	Réparer des éléments
    //10	TEXT_CMNDEFCOMPANYCOMMANDERBOARD_00258_AIRSHIP_CMD_REPAIR_PARTS	修理配件

    internal static readonly string[] WorkshopRepair =
    [
        "Repair submersible components",
        "Repair airship components",
        "パーツの修理",
        "Bauteile reparieren",
        "Réparer des éléments",
        "パーツの修理",
        "Bauteile reparieren",
        "Réparer des éléments",
        "修理配件",
        "부품 수리",
    ];

    //Use <If(Equal(IntegerParameter(4),1))>your last <SheetEn(Item,3,IntegerParameter(2),1,1)/><Else/><Value>IntegerParameter(3)</Value> of your <Value>IntegerParameter(4)</Value> <SheetEn(Item,3,IntegerParameter(2),2,1)/></If> to repair your vessel's <SheetEn(Item,3,IntegerParameter(1),1,1)/>?
    //6587	<If(Equal(IntegerParameter(3),1))><Clickable(<SheetDe(Item,2,IntegerParameter(2),1,4,1)/>)/><Else/><Value>IntegerParameter(3)</Value> <SheetDe(Item,5,IntegerParameter(2),2,4,1)/></If> (Besitz: <Value>IntegerParameter(4)</Value>) benutzen, um <SheetDe(Item,2,IntegerParameter(1),1,4,1)/> zu reparieren?
    //6587	Utiliser <If(Equal(IntegerParameter(3),1))><SheetFr(Item,1,IntegerParameter(2),1,1)/><Else/><Value>IntegerParameter(3)</Value> <SheetFr(Item,12,IntegerParameter(2),2,1)/></If> pour réparer <SheetFr(Item,2,IntegerParameter(1),1,1)/> de votre appareil<Indent/>? (<Value>IntegerParameter(4)</Value> possédé<If(LessThanOrEqualTo(IntegerParameter(4),1))><Else/>s</If>)
    /*6587	下記のアイテムを修理しますか？
    <Sheet(Item,IntegerParameter(1),0)/>
    消費:<Sheet(Item,IntegerParameter(2),0)/>×<Value>IntegerParameter(3)</Value>(所持数 <Value>IntegerParameter(4)</Value>)
    */

    internal static readonly string[] WorkshopRepairConfirm =
        [
            "repair",
            "下記のアイテムを修理しますか",
            "reparieren",
            "réparer",
            "要修理下列部件吗",
            "要修理下列部件嗎",
            "要修理下列元件嗎",
            "수리하시겠습니까?",
        ];

    // Use the components selected and <If(Equal(IntegerParameter(1),1))>the following item<Else/><Value>IntegerParameter(1)</Value> of the following items</If> to outfit and register your submersible?
    /* 6886 Das Tauchboot mit den gewählten Bauteilen registrieren?
     Verbraucht <Value>IntegerParameter(1)</Value> <If(Equal(IntegerParameter(1),1))>Exemplar<Else/>Exemplare</If> des folgenden Gegenstands:
    */
    // 6886 Utiliser les éléments choisis et <If(Equal(IntegerParameter(1),1))>l'objet suivant<Else/><Value>IntegerParameter(1)</Value> des objets suivants</If> pour équiper et enregistrer le sous-marin<Indent/>?
    /*選択したパーツアイテムと以下のアイテムを
       <Value>IntegerParameter(1)</Value>枚消費して潜水艦を登録します。
       よろしいですか？
    */

    internal static readonly string[] WorkshopRegisterConfirm =
    [
            "to outfit and register your submersible",
            "枚消費して潜水艦を登録します",
            "Das Tauchboot mit den gewählten Bauteilen registrieren",
            "pour équiper et enregistrer le sous-marin",
            "잠수함을 등록하시겠습니까",
            "消耗下列道具登记新的潜水艇吗",
            "消耗下列道具登記新的潛水艇嗎",
            //"",
            //""     (Addonsheet - 6886)
    ];

    //Your retainer will be unable to process item buyback requests once recalled. Are you sure you wish to proceed?
    //215	TEXT_CMNDEFRETAINERCALL_00010_ASK_RETURN_WITH_BUYBACK	Wenn du deinen Gehilfen wegschickst, kannst du die von ihm verkauften Gegenstände nicht mehr zurückkaufen. Möchtest du trotzdem fortfahren?
    //215	TEXT_CMNDEFRETAINERCALL_00010_ASK_RETURN_WITH_BUYBACK	Renvoyer le servant effacera la liste de rachat. Confirmer<Indent/>?

    internal static string[] WillBeUnableToProcessBuyback => field ??= [
        Svc.Data.GetExcelSheet<QuestDialogueText>(name:"custom/000/CmnDefRetainerCall_00010").GetRow(215).Value.GetText(),
        ];

    //3290	<Sheet(Item,IntegerParameter(1),0)/>×<Value>IntegerParameter(2)</Value>を、<Format(IntegerParameter(3),FF022C)/>枚の軍票と交換します。
    //よろしいですか？
    //3290	<Format(IntegerParameter(3),FF022E)/> Staatstaler gegen <If(Equal(IntegerParameter(2),1))><SheetDe(Item,1,IntegerParameter(1),1,4,1)/><Else/><Format(IntegerParameter(2),FF022E)/> <SheetDe(Item,5,IntegerParameter(1),2,4,1)/></If> eintauschen?
    //3290	Acheter <Value>IntegerParameter(2)</Value> <SheetFr(Item,12,IntegerParameter(1),IntegerParameter(2),1)/> pour <Format(IntegerParameter(3),FF05021D0103)/> sceau<If(LessThanOrEqualTo(IntegerParameter(3),1))><Else/>x</If><Indent/>?

    internal static readonly string[] GCSealExchangeConfirm = ["Exchange", "よろしいですか？", "Staatstaler gegen", "Acheter", "要交换吗", "교환하시겠습니까", "要交換嗎"];

    internal static readonly string[] DiscardItem = ["Discard", "を捨てます。", "wegwerfen", "Jeter","确定要舍弃","確定要捨棄"];
}
