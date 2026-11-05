using ECommons.Throttlers;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskContinueHET
{
    internal static bool? SelectEnterWorkshop()
    {
        if(Utils.TrySelectSpecificEntry(Lang.EnterWorkshop, () => EzThrottler.Throttle("HET.SelectEnterWorkshop")))
        {
            DebugLog("已确认前往部队工房");
            return true;
        }
        return false;
    }
}
