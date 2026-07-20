using AutoRetainer.Internal;
using AutoRetainerAPI.Configuration;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskDeployOnBestExpVoyage
{
    internal static void Enqueue(string name, VoyageType type, SubmarineUnlockPlan unlock = null)
    {
        VoyageUtils.Log($"任务已加入队列：{nameof(TaskCalculateAndPickBestExpRoute)}（方案：{unlock}）");
        TaskIntelligentRepair.Enqueue(name, type);
        P.TaskManager.Enqueue(SelectDeploy);
        TaskCalculateAndPickBestExpRoute.Enqueue(unlock);
        P.TaskManager.Enqueue(Deploy);
        TaskDeployAndSkipCutscene.Enqueue(true);
    }

    internal static bool? SelectDeploy()
    {
        return Utils.TrySelectSpecificEntry(Lang.DeployOnSubaquaticVoyage, () => Utils.GenericThrottle && EzThrottler.Throttle("Voyage.SelectDeploy", 1000));
    }

    internal static bool? Deploy()
    {
        {
            if(TryGetAddonByName<AtkUnitBase>("AirShipExplorationDetail", out _)) return true;
        }
        {
            if(TryGetAddonByName<AtkUnitBase>("AirShipExploration", out var addon) && IsAddonReady(addon))
            {
                var button = addon->UldManager.NodeList[2]->GetAsAtkComponentButton();
                if(!button->IsEnabled)
                {
                    EzThrottler.Throttle("Voyage.Deploy", 500, true);
                    return false;
                }
                else
                {
                    if(Utils.GenericThrottle && EzThrottler.Throttle("Voyage.Deploy"))
                    {
                        Callback.Fire(addon, true, 0);
                        return false;
                    }
                }
            }
            else
            {
                Utils.RethrottleGeneric();
            }
        }
        return false;
    }
}
