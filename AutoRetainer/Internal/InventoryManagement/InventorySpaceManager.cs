using AutoRetainer.Scheduler.Tasks;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Internal.InventoryManagement;

public static unsafe class InventorySpaceManager
{
    public static readonly List<string> Log = [];
    public static readonly string[] Addons = ["InventoryRetainer", "InventoryRetainerLarge"];

    public static nint AgentRetainerItemCommandModule => (nint)AgentModule.Instance()->GetAgentByInternalId(AgentId.Retainer) + 40;

    private static bool IsAgentRetainerActive => AgentModule.Instance()->GetAgentByInternalId(AgentId.Retainer)->IsAgentActive();

    public static readonly List<SellSlotTask> SellSlotTasks = [];

    public static InventoryType[] GetAllowedToSellInventoryTypes()
    {
        return Data.GetIMSettings().AllowSellFromArmory ? [.. Utils.PlayerInvetories, .. Utils.PlayerArmory] : Utils.PlayerInvetories;
    }

    public static bool? SafeSellSlot(SellSlotTask Task)
    {
        if(EzThrottler.Check("SellSlot") && EzThrottler.Throttle("SellSlot", Utils.GenerateRandomDelay()))
        {
            var inv = InventoryManager.Instance()->GetInventoryContainer(Task.InventoryType);
            if(inv == null)
            {
                DuoLog.Warning($"背包 {Task.InventoryType} 不存在");
                return true;
            }
            if(Data.GetIMSettings().IMProtectList.Contains(Task.ItemID))
            {
                DuoLog.Warning($"物品 {Task} 受保护，不会出售。");
                return true;
            }
            var slot = inv->Items[Task.Slot];
            if(Task.ItemID != slot.ItemId || slot.ItemId == 0 || slot.Quantity != Task.Quantity)
            {
                DuoLog.Warning($"栏位中的物品为 {ExcelItemHelper.GetName(slot.ItemId)}×{slot.Quantity}，与预期的 {Task} 不同");
                return true;
            }
            if(!IsRetainerInventoryLoaded())
            {
                DuoLog.Warning($"未找到雇员背包");
                return true;
            }
            if(!IsAgentRetainerActive)
            {
                DuoLog.Warning($"雇员代理未激活");
                return true;
            }
            if(!Data.GetIMSettings().IMDry)
            {
                P.Memory.RetainerItemCommandDetour(AgentRetainerItemCommandModule, Task.Slot, Task.InventoryType, 0, RetainerItemCommand.HaveRetainerSellItem);
                DebugLog($"已出售栏位 {Task}");
            }
            else
            {
                DuoLog.Warning($"> 背包管理试运行 > 将出售栏位 {Task}");
            }
            Log.Add($"[{DateTime.Now}] 已在 {Data.Name} 上出售 {Task}");
            return true;
        }
        return false;
    }

    public static bool IsRetainerInventoryLoaded()
    {
        foreach(var addonCheck in Addons)
        {
            if(TryGetAddonByName<AtkUnitBase>(addonCheck, out var addon) && IsAddonReady(addon))
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsSlotEnqueued(InventoryType type, uint slot)
    {
        return SellSlotTasks.Any(x => x.InventoryType == type && x.Slot == slot);
    }

    public static void EnqueueSoftItemIfAllowed(uint ItemId, uint Quantity)
    {
        if(S.CabinetManager.ShouldExcludeItemFromProcessing(ItemId)) return;
        var im = InventoryManager.Instance();
        foreach(var invType in InventorySpaceManager.GetAllowedToSellInventoryTypes())
        {
            var inv = im->GetInventoryContainer(invType);
            for(var i = 0; i < inv->Size; i++)
            {
                var item = inv->Items[i];
                if(item.ItemId != 0 && item.ItemId == ItemId && item.Quantity == Quantity)
                {
                    if(Data.GetIMSettings().IMAutoVendorSoft.Contains(item.ItemId))
                    {
                        var task = new SellSlotTask(invType, (uint)i, item.ItemId, item.Quantity);
                        PluginLog.Information($"将 {task} 加入自由探索委托出售队列");
                        InventorySpaceManager.SellSlotTasks.Add(task);
                        return;
                    }
                }
            }
        }
    }

    public static void EnqueueAllHardItems(bool softAsHard = false)
    {
        var im = InventoryManager.Instance();
        foreach(var invType in InventorySpaceManager.GetAllowedToSellInventoryTypes())
        {
            var inv = im->GetInventoryContainer(invType);
            for(var i = 0; i < inv->Size; i++)
            {
                var item = inv->Items[i];
                if(item.ItemId != 0 && (item.Quantity < Data.GetIMSettings().IMAutoVendorHardStackLimit || Data.GetIMSettings().IMAutoVendorHardIgnoreStack.Contains(item.ItemId)))
                {
                    if((Data.GetIMSettings().IMAutoVendorHard.Contains(item.ItemId) || (softAsHard && Data.GetIMSettings().IMAutoVendorSoft.Contains(item.ItemId))) 
                        && !TaskDesynthItems.DesynthEligible(item.ItemId)
                        && !S.CabinetManager.ShouldExcludeItemFromProcessing(item.ItemId))
                    {
                        var task = new SellSlotTask(invType, (uint)i, item.ItemId, item.Quantity);
                        PluginLog.Information($"将 {task} 加入无条件出售队列");
                        InventorySpaceManager.SellSlotTasks.Add(task);
                    }
                }
            }
        }
    }
}
