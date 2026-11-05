using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoRetainer.Modules.Voyage.PartSwapper;
public static unsafe class PartSwapperScheduler
{
    public static void EnqueuePartSwappingIfNeeded(string next, VoyageType type)
    {
        if(C.EnableAutomaticComponentsAndPlanChange)
        {
            TaskIntelligentComponentsChange.Enqueue(next, type);

            P.TaskManager.Enqueue(() =>
                                  {
                                      var plan = PartSwapperUtils.GetPlanInLevelRange(Data.GetAdditionalVesselData(next, type).Level);
                                      if(plan == null) return;
                                      var partSwapperList = PartSwapperUtils.GetIsVesselNeedsPartsSwap(next, VoyageType.Submersible, out _);
                                      if(partSwapperList is { Count: 0 })
                                      {
                                          if(plan.FirstSubDifferent && VoyageUtils.GetVesselIndexByName(next, VoyageType.Submersible) == 0)
                                          {
                                              Data.AdditionalSubmarineData[next].UnlockMode = plan.FirstSubUnlockMode;
                                              Data.AdditionalSubmarineData[next].SelectedUnlockPlan = plan.FirstSubSelectedUnlockPlan;
                                              Data.AdditionalSubmarineData[next].VesselBehavior = plan.FirstSubVesselBehavior;
                                              Data.AdditionalSubmarineData[next].SelectedPointPlan = plan.FirstSubSelectedPointPlan;
                                          }
                                          else
                                          {
                                              Data.AdditionalSubmarineData[next].UnlockMode = plan.UnlockMode;
                                              Data.AdditionalSubmarineData[next].SelectedUnlockPlan = plan.SelectedUnlockPlan;
                                              Data.AdditionalSubmarineData[next].VesselBehavior = plan.VesselBehavior;
                                              Data.AdditionalSubmarineData[next].SelectedPointPlan = plan.SelectedPointPlan;
                                          }
                                      }
                                  }, "正在更改潜水艇方案");
        }
    }

    public static bool EnqueueSubmersibleRegistrationIfPossible()
    {
        var neededParts = new[] { (uint)Hull.Shark, (uint)Stern.Shark, (uint)Bow.Shark, (uint)Bridge.Shark };
        PluginLog.Information($"""
            EnqueueSubmersibleRegistrationIfPossible：
            已启用：{C.EnableAutomaticSubRegistration}
            数量检查：{Data.OfflineSubmarineData.Count} < {Data.NumSubSlots}
            配件检查：{neededParts.Select(part => $"{part}: ×{InventoryManager.Instance()->GetInventoryItemCount((uint)part)}").Print()}
            潜水艇登记票检查：{InventoryManager.Instance()->GetInventoryItemCount((uint)Items.DiveCredits)} >= {(2 * Data.NumSubSlots) - 1}
            """);
        if(C.EnableAutomaticSubRegistration
            && Data.OfflineSubmarineData.Count < Data.NumSubSlots
            && neededParts.All(part => InventoryManager.Instance()->GetInventoryItemCount((uint)part) > 0)
            && InventoryManager.Instance()->GetInventoryItemCount((uint)Items.DiveCredits) >= (2 * Data.NumSubSlots) - 1)
        {
            P.TaskManager.Enqueue(PartSwapperTasks.SelectRegisterSub);
            if(EzThrottler.Throttle("DoWorkshopPanelTick.RegisterSub", 1000))
            {
                for(var i = 0; i < 4; i++)
                {
                    var slot = i;
                    P.TaskManager.Enqueue(() => PartSwapperTasks.ChangeComponent(slot, neededParts[slot]), $"更换为 {neededParts[slot]}");
                }

                P.TaskManager.Enqueue(PartSwapperTasks.RegisterSub);
                P.TaskManager.Enqueue(PartSwapperTasks.SetupNewSub);
                P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselMenu);
            }
            return true;
        }
        else
        {
            PluginLog.Information($"无需登记潜水艇");
            return false;
        }
    }
}
