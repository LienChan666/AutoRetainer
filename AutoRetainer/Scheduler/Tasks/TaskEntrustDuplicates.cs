using AutoRetainer.Internal.InventoryManagement;
using AutoRetainer.Scheduler.Handlers;
using AutoRetainerAPI.Configuration;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace AutoRetainer.Scheduler.Tasks;

internal static unsafe class TaskEntrustDuplicates
{
    internal static int RequestEntrustQuantity = 0;
    internal static List<(uint ID, uint Quantity)> CapturedInventoryState = [];
    internal static bool WasOpen = false;

    public static void EnqueueNew(EntrustPlan plan)
    {
        P.TaskManager.Enqueue((System.Action)(() => WasOpen = false), "将 WasOpen 设置为 false");
        P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon), "等待 SelectString 界面就绪");
        P.TaskManager.Enqueue(() => RecursivelyEntrustItems(plan), $"递归转存物品（{plan.Guid} | {plan.Name}）", new(timeLimitMS: 60 * 60 * 1000));
        P.TaskManager.Enqueue(() => !WasOpen || TaskVendorItems.CloseInventory() == true);
    }
    public static void EnqueueNewReverse(EntrustPlan plan)
    {
        P.TaskManager.Enqueue((System.Action)(() => WasOpen = false), "将 WasOpen 设置为 false");
        P.TaskManager.Enqueue(() => TryGetAddonByName<AtkUnitBase>("SelectString", out var addon) && IsAddonReady(addon), "等待 SelectString 界面就绪");
        P.TaskManager.Enqueue(() => RecursivelyReverseEntrustItems(plan), $"递归从雇员处取回物品（{plan.Guid} | {plan.Name}）", new(timeLimitMS: 60 * 60 * 1000));
        P.TaskManager.Enqueue(() => !WasOpen || TaskVendorItems.CloseInventory() == true);
    }

    private static bool? RecursivelyEntrustItems(EntrustPlan plan)
    {
        try
        {
            var s = Data.GetIMSettings();
            var allowedPlayerInventories = plan.GetAllowedInventories();
            if(TryGetAddonByName<AtkUnitBase>("InputNumeric", out var numeric))
            {
                if(IsAddonReady(numeric))
                {
                    var maxAmount = numeric->AtkValues[3].UInt;
                    var result = Math.Clamp(RequestEntrustQuantity, 1, maxAmount);
                    if(EzThrottler.Throttle("EntrustItemInputNumeric", 200))
                    {
                        InternalLog.Information($"正在处理数字输入：{result}（最大值：{maxAmount}）");
                        Callback.Fire(numeric, true, (int)result);
                    }
                }
                return false;
            }
            if(!EzThrottler.Check("InventoryTimeout") && Utils.GetCapturedInventoryState(allowedPlayerInventories).SequenceEqual(CapturedInventoryState))
            {
                return false;
            }
            if(EzThrottler.Check("EntrustItem") && EzThrottler.Throttle("EntrustItem", Utils.GenerateRandomDelay()))
            {
                List<(uint ItemID, int ToKeep)> itemList = [];
                foreach(var x in plan.EntrustItems)
                {
                    var add = (x, plan.EntrustItemsAmountToKeep.SafeSelect(x));
                    if(plan.ExcludeProtected && s.IMProtectList.Contains(add.Item1)) continue;
                    if(S.CabinetManager.ShouldExcludeItemFromProcessing(add.Item1)) continue;
                    itemList.Add(add);
                    InternalLog.Debug($"[TED] 已从 EntrustItems 添加物品：{ExcelItemHelper.GetName(add.Item1, true)}，保留数量={add.Item2}");
                }
                foreach(var x in Utils.GetItemsInInventory(allowedPlayerInventories))
                {
                    if(plan.ExcludeProtected && s.IMProtectList.Contains(x)) continue;
                    if(S.CabinetManager.ShouldExcludeItemFromProcessing(x)) continue;
                    var item = ExcelItemHelper.Get(x);
                    if(item == null) continue;
                    if(itemList.Any(s => s.ItemID == item?.RowId)) continue;
                    if(plan.EntrustCategories.TryGetFirst(c => c.ID == item.Value.ItemUICategory.RowId, out var info))
                    {
                        var add = (item.Value.RowId, info.AmountToKeep);
                        itemList.Add(add);
                        InternalLog.Debug($"[TED] 已从 EntrustCategories 添加物品：{ExcelItemHelper.GetName(add.Item1, true)}，保留数量={add.Item2}");
                    }
                }
                if(plan.Duplicates && plan.DuplicatesMultiStack)
                {
                    foreach(var type in Utils.RetainerInventoriesWithCrystals)
                    {
                        if(type.EqualsAny(InventoryType.Crystals, InventoryType.RetainerCrystals)) continue;
                        var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                        for(var i = 0; i < inv->Size; i++)
                        {
                            var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                            if(item->ItemId != 0 && item->Quantity > 0)
                            {
                                if(plan.ExcludeProtected && s.IMProtectList.Contains(item->ItemId)) continue;
                                if(S.CabinetManager.ShouldExcludeItemFromProcessing(item->ItemId)) continue;
                                if(itemList.Any(s => s.ItemID == item->ItemId)) continue;
                                var data = ExcelItemHelper.Get(item->ItemId);
                                itemList.Add((item->ItemId, 0));
                                InternalLog.Debug($"[TED] 已添加雇员多堆叠同类道具：{ExcelItemHelper.GetName(item->ItemId, true)}");
                            }
                        }
                    }
                }
                // 处理无条件委托
                foreach(var type in allowedPlayerInventories)
                {
                    var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                    for(var i = 0; i < inv->Size; i++)
                    {
                        var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                        if(item->ItemId != 0 && item->Quantity > 0)
                        {
                            if(plan.ExcludeProtected && s.IMProtectList.Contains(item->ItemId)) continue;
                            if(S.CabinetManager.ShouldExcludeItemFromProcessing(item->ItemId)) continue;
                            var itemCount = Utils.GetItemCount(allowedPlayerInventories, item->ItemId);
                            InternalLog.Debug($"[TED] 物品 {ExcelItemHelper.GetName(item->ItemId, true)} 的数量 = {itemCount}");
                            var data = ExcelItemHelper.Get(item->ItemId);
                            if(itemList.TryGetFirst(s => s.ItemID == item->ItemId, out var entrustInfo))
                            {
                                var toKeep = entrustInfo.ToKeep;
                                var toEntrust = itemCount - toKeep;
                                var canFit = Utils.GetAmountThatCanFit(Utils.RetainerInventoriesWithCrystals, item->ItemId, item->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality), out var debugData);
                                InternalLog.Debug($"[TED] 物品 {ExcelItemHelper.GetName(item->ItemId, true)}：待委托={toEntrust}，保留={toKeep}，可容纳={canFit}\n{debugData.Print("\n")}");
                                if(toEntrust > canFit) toEntrust = (int)canFit;
                                if(toEntrust > 0)
                                {
                                    var toEntrustFromStack = Math.Min(item->Quantity, toEntrust);
                                    if(toEntrustFromStack > 0)
                                    {
                                        MoveSlotFromToRetainerInventoryUnsafe(item, (int)toEntrustFromStack, i, type, allowedPlayerInventories);
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                if(plan.Duplicates && !plan.DuplicatesMultiStack)
                {
                // 现在处理同类道具合并递交
                    foreach(var type in Utils.RetainerInventoriesWithCrystals)
                    {
                        if(type.EqualsAny(InventoryType.Crystals, InventoryType.RetainerCrystals)) continue;
                        // 查找未满堆叠，然后从玩家背包中查询
                        var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                        for(var i = 0; i < inv->Size; i++)
                        {
                            var item = inv->GetInventorySlot(i);
                            if(plan.ExcludeProtected && s.IMProtectList.Contains(item->ItemId)) continue;
                            if(S.CabinetManager.ShouldExcludeItemFromProcessing(item->ItemId)) continue;
                            if(item->ItemId != 0 && !itemList.Any(s => s.ItemID == item->ItemId))
                            {
                                var data = ExcelItemHelper.Get(item->ItemId);
                                if(data == null || data.Value.IsUnique) continue;
                                var canFit = data.Value.StackSize - item->Quantity;
                                if(canFit > 0)
                                {
                                    foreach(var playerType in allowedPlayerInventories)
                                    {
                                        var playerInv = InventoryManager.Instance()->GetInventoryContainer(playerType);
                                        for(var q = 0; q < playerInv->Size; q++)
                                        {
                                            var playerItem = playerInv->GetInventorySlot(q);
                                            if(playerItem->ItemId == item->ItemId && playerItem->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality) == item->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality))
                                            {
                                                var toEntrustFromStack = Math.Min(canFit, playerItem->Quantity);
                                                MoveSlotFromToRetainerInventoryUnsafe(playerItem, (int)toEntrustFromStack, q, playerType, allowedPlayerInventories);
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
        }
        catch(Exception e)
        {
            e.Log();
        }
        return false;
    }

    private static bool? RecursivelyReverseEntrustItems(EntrustPlan plan)
    {
        try
        {
            var s = Data.GetIMSettings();
            var allowedRetainerInventories = Utils.RetainerInventoriesWithCrystals;
            if(TryGetAddonByName<AtkUnitBase>("InputNumeric", out var numeric))
            {
                if(IsAddonReady(numeric))
                {
                    var maxAmount = numeric->AtkValues[3].UInt;
                    var result = Math.Clamp(RequestEntrustQuantity, 1, maxAmount);
                    if(EzThrottler.Throttle("EntrustItemInputNumeric", 200))
                    {
                        InternalLog.Information($"正在处理数字输入：{result}（最大值：{maxAmount}）");
                        Callback.Fire(numeric, true, (int)result);
                    }
                }
                return false;
            }
            if(!EzThrottler.Check("InventoryTimeout") && Utils.GetCapturedInventoryState(allowedRetainerInventories).SequenceEqual(CapturedInventoryState))
            {
                return false;
            }
            if(EzThrottler.Check("EntrustItem") && EzThrottler.Throttle("EntrustItem", Utils.GenerateRandomDelay()))
            {
                List<(uint ItemID, int ToKeep)> itemList = [];
                foreach(var x in plan.EntrustItems)
                {
                    var add = (x, plan.EntrustItemsAmountToKeep.SafeSelect(x));
                    if(plan.ExcludeProtected && s.IMProtectList.Contains(add.Item1)) continue;
                    itemList.Add(add);
                    InternalLog.Debug($"[TED] 已从 EntrustItems 添加物品：{ExcelItemHelper.GetName(add.Item1, true)}，保留数量={add.Item2}");
                }
                foreach(var x in Utils.GetItemsInInventory(allowedRetainerInventories))
                {
                    if(plan.ExcludeProtected && s.IMProtectList.Contains(x)) continue;
                    var item = ExcelItemHelper.Get(x);
                    if(item == null) continue;
                    if(itemList.Any(s => s.ItemID == item?.RowId)) continue;
                    if(plan.EntrustCategories.TryGetFirst(c => c.ID == item.Value.ItemUICategory.RowId, out var info))
                    {
                        var add = (item.Value.RowId, info.AmountToKeep);
                        itemList.Add(add);
                        InternalLog.Debug($"[TED] 已从 EntrustCategories 添加物品：{ExcelItemHelper.GetName(add.Item1, true)}，保留数量={add.Item2}");
                    }
                }
                if(plan.Duplicates && plan.DuplicatesMultiStack)
                {
                    foreach(var type in (InventoryType[])[..Utils.PlayerArmory, ..Utils.PlayerInvetoriesWithCrystals])
                    {
                        if(type.EqualsAny(InventoryType.Crystals, InventoryType.RetainerCrystals)) continue;
                        var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                        for(var i = 0; i < inv->Size; i++)
                        {
                            var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                            if(item->ItemId != 0 && item->Quantity > 0)
                            {
                                if(plan.ExcludeProtected && s.IMProtectList.Contains(item->ItemId)) continue;
                                if(itemList.Any(s => s.ItemID == item->ItemId)) continue;
                                var data = ExcelItemHelper.Get(item->ItemId);
                                itemList.Add((item->ItemId, 0));
                                InternalLog.Debug($"[TED] 已添加雇员多堆叠同类道具：{ExcelItemHelper.GetName(item->ItemId, true)}");
                            }
                        }
                    }
                }
                // 处理无条件委托
                foreach(var type in allowedRetainerInventories)
                {
                    var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                    for(var i = 0; i < inv->Size; i++)
                    {
                        var item = InventoryManager.Instance()->GetInventorySlot(type, i);
                        if(item->ItemId != 0 && item->Quantity > 0)
                        {
                            if(plan.ExcludeProtected && s.IMProtectList.Contains(item->ItemId)) continue;
                            var itemCount = Utils.GetItemCount(allowedRetainerInventories, item->ItemId);
                            InternalLog.Debug($"[TED] 物品 {ExcelItemHelper.GetName(item->ItemId, true)} 的数量 = {itemCount}");
                            var data = ExcelItemHelper.Get(item->ItemId);
                            if(itemList.TryGetFirst(s => s.ItemID == item->ItemId, out var entrustInfo))
                            {
                                var toKeep = entrustInfo.ToKeep;
                                var toEntrust = itemCount - toKeep;
                                var canFit = Utils.GetAmountThatCanFit(Utils.PlayerInvetoriesWithCrystals, item->ItemId, item->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality), out var debugData);
                                InternalLog.Debug($"[TED] 物品 {ExcelItemHelper.GetName(item->ItemId, true)}：待取回={toEntrust}，保留={toKeep}，可容纳={canFit}\n{debugData.Print("\n")}");
                                if(toEntrust > canFit) toEntrust = (int)canFit;
                                if(toEntrust > 0)
                                {
                                    var toEntrustFromStack = Math.Min(item->Quantity, toEntrust);
                                    if(toEntrustFromStack > 0)
                                    {
                                        MoveSlotFromToRetainerInventoryUnsafe(item, (int)toEntrustFromStack, i, type, allowedRetainerInventories);
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                if(plan.Duplicates && !plan.DuplicatesMultiStack)
                {
                // 现在处理同类道具合并递交
                    foreach(var type in (InventoryType[])[.. Utils.PlayerArmory, .. Utils.PlayerInvetoriesWithCrystals])
                    {
                        if(type.EqualsAny(InventoryType.Crystals, InventoryType.RetainerCrystals)) continue;
                        // 查找未满堆叠，然后从玩家背包中查询
                        var inv = InventoryManager.Instance()->GetInventoryContainer(type);
                        for(var i = 0; i < inv->Size; i++)
                        {
                            var item = inv->GetInventorySlot(i);
                            if(plan.ExcludeProtected && s.IMProtectList.Contains(item->ItemId)) continue;
                            if(item->ItemId != 0 && !itemList.Any(s => s.ItemID == item->ItemId))
                            {
                                var data = ExcelItemHelper.Get(item->ItemId);
                                if(data == null || data.Value.IsUnique) continue;
                                var canFit = data.Value.StackSize - item->Quantity;
                                if(canFit > 0)
                                {
                                    foreach(var playerType in allowedRetainerInventories)
                                    {
                                        var playerInv = InventoryManager.Instance()->GetInventoryContainer(playerType);
                                        for(var q = 0; q < playerInv->Size; q++)
                                        {
                                            var playerItem = playerInv->GetInventorySlot(q);
                                            if(playerItem->ItemId == item->ItemId && playerItem->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality) == item->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality))
                                            {
                                                var toEntrustFromStack = Math.Min(canFit, playerItem->Quantity);
                                                MoveSlotFromToRetainerInventoryUnsafe(playerItem, (int)toEntrustFromStack, q, playerType, allowedRetainerInventories);
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
        }
        catch(Exception e)
        {
            e.Log();
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="toEntrustFromStack"></param>
    /// <param name="i">栏位 ID</param>
    /// <param name="type"></param>
    private static void MoveSlotFromToRetainerInventoryUnsafe(InventoryItem* item, int toEntrustFromStack, int i, InventoryType type, InventoryType[] allowedSourceInventories)
    {
        RetainerItemCommand command;
        RetainerItemCommand quantityCommand;
        if(type.EqualsAny([..Utils.PlayerInvetoriesWithCrystals, .. Utils.PlayerArmory]))
        {
            command = RetainerItemCommand.EntrustToRetainer;
            quantityCommand = RetainerItemCommand.EntrustQuantity;
        }
        else if(type.EqualsAny(Utils.RetainerEntireInventory))
        {
            command = RetainerItemCommand.RetrieveFromRetainer;
            quantityCommand = RetainerItemCommand.RetrieveQuantity;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }
        if(!InventorySpaceManager.IsRetainerInventoryLoaded())
        {
            if(EzThrottler.Throttle("REI SelectEntrust", 2000))
            {
                DebugLog($"已触发 SelectEntrust");
                WasOpen = true;
                RetainerHandlers.SelectEntrustItems();
            }
        }
        else
        {
            var slot = InventoryManager.Instance()->GetInventoryContainer(type)->GetInventorySlot(i);
            var action = command == RetainerItemCommand.RetrieveFromRetainer ? "从雇员处取回" : "交给雇员保管";
            void printToChat()
            {
                if(C.EnableEntrustChat && ExcelItemHelper.Get(slot->ItemId) != null) Svc.Chat.Print(new SeStringBuilder().Append($"正在{action}：").Append([new ItemPayload(slot->ItemId, slot->Flags.HasFlag(InventoryItem.ItemFlags.HighQuality))]).Append(ExcelItemHelper.GetName(slot->ItemId)).Append([RawPayload.LinkTerminator]).Build());
            }
            if(type == InventoryType.Crystals || type == InventoryType.RetainerCrystals)
            {
                RequestEntrustQuantity = (int)toEntrustFromStack;
                CapturedInventoryState = Utils.GetCapturedInventoryState(allowedSourceInventories);
                EzThrottler.Throttle("InventoryTimeout", 5000, true);
                InternalLog.Debug($"正在{action}水晶，栏位：{i}/{type} - {ExcelItemHelper.GetName(slot->ItemId, true)}，数量 = {toEntrustFromStack}");
                printToChat();
                P.Memory.RetainerItemCommandDetour(InventorySpaceManager.AgentRetainerItemCommandModule, (uint)i, type, 0, command);
            }
            else
            {
                if(item->Quantity <= 1 || item->Quantity == toEntrustFromStack)
                {
                    CapturedInventoryState = Utils.GetCapturedInventoryState(allowedSourceInventories);
                    EzThrottler.Throttle("InventoryTimeout", 5000, true);
                    InternalLog.Debug($"正在{action}，栏位：{i}/{type} - {ExcelItemHelper.GetName(slot->ItemId, true)}，数量 = 全部");
                    printToChat();
                    P.Memory.RetainerItemCommandDetour(InventorySpaceManager.AgentRetainerItemCommandModule, (uint)i, type, 0, command);
                }
                else
                {
                    // 部分委托
                    RequestEntrustQuantity = (int)toEntrustFromStack;
                    CapturedInventoryState = Utils.GetCapturedInventoryState(allowedSourceInventories);
                    EzThrottler.Throttle("InventoryTimeout", 5000, true);
                    InternalLog.Debug($"正在{action}，栏位：{i}/{type} - {ExcelItemHelper.GetName(slot->ItemId, true)}，数量 = {toEntrustFromStack}");
                    printToChat();
                    P.Memory.RetainerItemCommandDetour(InventorySpaceManager.AgentRetainerItemCommandModule, (uint)i, type, 0, quantityCommand);
                }
            }
        }
    }
}
