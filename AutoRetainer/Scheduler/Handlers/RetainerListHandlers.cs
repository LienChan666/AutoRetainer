using AutoRetainer.Internal.InventoryManagement;
using AutoRetainer.Scheduler.Tasks;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Scheduler.Handlers;

internal static unsafe class RetainerListHandlers
{
    internal static bool? SelectRetainerByName(string name)
    {
        TaskWithdrawGil.forceCheck = false;
        InventorySpaceManager.SellSlotTasks.Clear();
        if(name.IsNullOrEmpty())
        {
            throw new Exception($"名称不能为空");
        }
        if(TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList) && IsAddonReady(retainerList))
        {
            var list = new AddonMaster.RetainerList(retainerList);
            foreach(var retainer in list.Retainers)
            {
                if(retainer.Name == name)
                {
                    if(Utils.GenericThrottle)
                    {
                        DebugLog($"正在选择雇员 {retainer.Name}，索引 {retainer.Index}");
                        retainer.Select();
                        return true;
                    }
                }
            }
        }

        return false;
    }

    internal static bool? CloseRetainerList()
    {
        if(TryGetAddonByName<AtkUnitBase>("RetainerList", out var retainerList) && IsAddonReady(retainerList))
        {
            if(Utils.GenericThrottle)
            {
                var v = stackalloc AtkValue[1]
                {
                    new()
                    {
                        Type = AtkValueType.Int,
                        Int = -1
                    }
                };
                P.IsCloseActionAutomatic = true;
                retainerList->FireCallback(1, v);
                DebugLog($"正在关闭雇员窗口");
                return true;
            }
        }
        return false;
    }
}
