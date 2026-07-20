using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage.PartSwapper;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainerAPI.Configuration;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.Throttlers;

namespace AutoRetainer.Modules.Voyage;

internal static unsafe class VoyageMain
{
    private static bool IsInVoyagePanel = false;

    internal static WaitOverlay WaitOverlay;

    internal static void Init()
    {
        Svc.Framework.Update += Tick;
        Svc.Toasts.ErrorToast += Toasts_ErrorToast;
        WaitOverlay = new();
        P.WindowSystem.AddWindow(WaitOverlay);
    }

    private static void Toasts_ErrorToast(ref SeString message, ref bool isHandled)
    {
        if(MultiMode.Active || P.TaskManager.IsBusy)
        {
            var txt = message.GetText();
            if(txt == Lang.VoyageInventoryError)
            {
                DuoLog.Warning($"[远航探索] 背包已满！");
                VoyageScheduler.Enabled = false;
                P.TaskManager.Abort();
                P.TaskManager.Enqueue(VoyageScheduler.SelectQuitVesselSelectorMenu);
                P.TaskManager.Enqueue(VoyageScheduler.SelectExitMainPanel);
                if(C.FailureNoInventory == WorkshopFailAction.StopPlugin)
                {
                    MultiMode.Enabled = false;
                    VoyageScheduler.Enabled = false;
                }
                else if(C.FailureNoInventory == WorkshopFailAction.ExcludeChar)
                {
                    Data.WorkshopEnabled = false;
                }
            }
            if(txt.ContainsAny(StringComparison.OrdinalIgnoreCase, Lang.UnableToRepairVessel))
            {
                TaskRepairAll.Abort = true;
            DuoLog.Warning($"[远航探索] 魔导机械修理材料不足！");
                if(C.FailureNoRepair == WorkshopFailAction.ExcludeVessel)
                {
                    Data.GetEnabledVesselsData(TaskRepairAll.Type).Remove(TaskRepairAll.Name);
                }
                else if(C.FailureNoRepair == WorkshopFailAction.ExcludeChar)
                {
                    Data.WorkshopEnabled = false;
                }
                else if(C.FailureNoRepair == WorkshopFailAction.StopPlugin)
                {
                    MultiMode.Enabled = false;
                    VoyageScheduler.Enabled = false;
                }
            }
        }
    }

    internal static void Shutdown()
    {
        Svc.Framework.Update -= Tick;
        Svc.Toasts.ErrorToast -= Toasts_ErrorToast;
    }

    internal static void Tick(object _)
    {
        if(VoyageUtils.IsVoyageCondition())
        {
            if(Svc.Targets.Target.IsVoyagePanel())
            {
                if(!IsInVoyagePanel)
                {
            DebugLog($"已进入航行管制面板");
                    IsInVoyagePanel = true;
                    if(IsKeyPressed(C.Suppress))
                    {
                        Notify.Warning("用户未请求任何操作");
                    }
                    else
                    {
                        if(C.SubsAutoResend2)
                        {
                            if(Data.AnyEnabledVesselsAvailable())
                            {
                                VoyageScheduler.Enabled = true;
                                DebugLog($"<!> 已启用远航探索调度器");
                            }
                            else
                            {
                                Notify.Warning($"警告！\n当前没有可处理内容，因此未启用远航探索模块");
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if(IsInVoyagePanel)
            {
                IsInVoyagePanel = false;
                VoyageScheduler.Enabled = false;
            DebugLog($"<!> 已退出航行管制面板并禁用远航探索调度器");
            }
        }

        if(VoyageUtils.IsInVoyagePanel())
        {
            if(EzThrottler.Throttle("Voyage.WriteOfflineData", 100))
            {
                VoyageUtils.WriteOfflineData();
            }
        }

        if(VoyageScheduler.Enabled)
        {
            DoWorkshopPanelTick();
        }
    }

    private static void DoWorkshopPanelTick()
    {
        if(!P.TaskManager.IsBusy)
        {
            if(FrameThrottler.Check("SchedulerRestartCooldown"))
            {
                var data = Data;
                var panel = VoyageUtils.GetCurrentWorkshopPanelType();
                if(panel == PanelType.TypeSelector)
                {
                    if(data.AnyEnabledVesselsAvailable(VoyageType.Airship))
                    {
                        if(EzThrottler.Throttle("DoWorkshopPanelTick.EnqueuePanelSelector", 1000))
                        {
                            TaskRecursiveItemDiscard.EnqueueIfNeeded();
                            P.TaskManager.Enqueue(VoyageScheduler.SelectAirshipManagement);
                        }
                    }
                    else if(data.AnyEnabledVesselsAvailable(VoyageType.Submersible))
                    {
                        if(EzThrottler.Throttle("DoWorkshopPanelTick.EnqueuePanelSelector", 1000))
                        {
                            TaskRecursiveItemDiscard.EnqueueIfNeeded();
                            P.TaskManager.Enqueue(VoyageScheduler.SelectSubManagement);
                        }
                    }
                    else if(!data.AreAnyEnabledVesselsReturnInNext(5 * 60))
                    {
                        if(EzThrottler.Throttle("DoWorkshopPanelTick.EnqueuePanelSelector", 1000))
                        {
                            P.TaskManager.Enqueue(VoyageScheduler.SelectExitMainPanel);
                            TaskRecursiveItemDiscard.EnqueueIfNeeded();
                            if(C.VendorItemAfterVoyage && Data.RetainerData.Count != 0)
                            {
                                P.TaskManager.Enqueue(() =>
                                {
                                    P.TaskManager.InsertStack(TaskVendorItems.EnqueueFromCommand);
                                });
                            }
                        }
                    }
                }
                else if(panel == PanelType.Submersible)
                {
                    ScheduleResend(VoyageType.Submersible);
                }
                else if(panel == PanelType.Airship)
                {
                    ScheduleResend(VoyageType.Airship);
                }
            }
        }
        else
        {
            FrameThrottler.Throttle("SchedulerRestartCooldown", 10, true);
        }
    }

    private static void ScheduleResend(VoyageType type)
    {
        var next = VoyageUtils.GetNextCompletedVessel(type);
        if(next != null)
        {
            var adata = Data.GetAdditionalVesselData(next, type);
            var data = Data.GetOfflineVesselData(next, type) ?? throw new NullReferenceException($"{next} 的离线{Lang.VoyageTypeNames[type]}数据为空");
            if((VoyageUtils.DontReassign || adata.VesselBehavior == VesselBehavior.Finalize || (C.FinalizeBeforeResend && Data.AreAnyEnabledVesselsReturnInNext(0, false, true))) && data.ReturnTime != 0)
            {
                if(EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResend", 1000))
                {
                    TaskFinalizeVessel.Enqueue(next, type, true);
                }
            }
            else
            {
                if(adata.VesselBehavior.EqualsAny(VesselBehavior.LevelUp, VesselBehavior.Unlock, VesselBehavior.Use_plan, VesselBehavior.Redeploy))
                {
                    if(EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResend", 1000))
                    {
                        if(data.ReturnTime != 0)
                        {
                            TaskFinalizeVessel.Enqueue(next, type, false);
                        }
                        else
                        {
                            TaskSelectVesselByName.Enqueue(next, type);
                        }

                        PartSwapperScheduler.EnqueuePartSwappingIfNeeded(next, type);

                        P.TaskManager.EnqueueMulti(
                            new(() => CurrentSubmarine.Get() != null),
                            new(() =>
                            {
                                P.TaskManager.BeginStack();
                                try
                                {
                                    foreach(var x in C.SubmarineUnlockPlans)
                                    {
                                        if(x.EnforcePlan)
                                        {
                                            PluginLog.Information($"解锁方案 {x.Name} 已设为强制执行");
                                            if(TaskDeployOnUnlockRoute.GetUnlockPointsFromPlan(x, UnlockMode.SpamOne).TryGetFirst(out var unlockPoint) && !x.ExcludedRoutes.Any(s => s == unlockPoint.point))
                                            {
                                                PluginLog.Information($"正在对当前{Lang.VoyageTypeNames[type]}强制执行方案 {x.Name}");
                                                TaskDeployOnUnlockRoute.Enqueue(next, type, x, UnlockMode.SpamOne);
                                                goto EndTask;
                                            }
                                        }
                                    }
                                    if(adata.VesselBehavior == VesselBehavior.LevelUp)
                                    {
                                        TaskDeployOnBestExpVoyage.Enqueue(next, type);
                                    }
                                    else if(adata.VesselBehavior == VesselBehavior.Unlock)
                                    {
                                        var mode = adata.UnlockMode;
                                        var plan = VoyageUtils.GetSubmarineUnlockPlanByGuid(adata.SelectedUnlockPlan) ?? VoyageUtils.GetDefaultSubmarineUnlockPlan();
                                        if(plan.EnforceDSSSinglePoint && TaskDeployOnUnlockRoute.GetUnlockPointsFromPlan(plan, UnlockMode.SpamOne).TryGetFirst(out var unlockPoint) && VoyageUtils.GetSubmarineExploration(unlockPoint.point).Value.Map.RowId == 1)
                                        {
                                            PluginLog.Information($"正在将解锁模式覆盖为 {Lang.UnlockModeNames[UnlockMode.SpamOne]}");
                                            mode = UnlockMode.SpamOne;
                                        }
                                        if(mode == UnlockMode.WhileLevelling)
                                        {
                                            TaskDeployOnBestExpVoyage.Enqueue(next, type, plan);
                                        }
                                        else if(mode.EqualsAny(UnlockMode.SpamOne, UnlockMode.MultiSelect))
                                        {
                                            TaskDeployOnUnlockRoute.Enqueue(next, type, plan, mode);
                                        }
                                        else
                                        {
                                            throw new ArgumentOutOfRangeException(nameof(mode));
                                        }
                                    }
                                    else if(adata.VesselBehavior == VesselBehavior.Use_plan)
                                    {
                                        var plan = VoyageUtils.GetSubmarinePointPlanByGuid(adata.SelectedPointPlan);
                                        if(plan != null && plan.Points.Count >= 1 && plan.Points.Count <= 5)
                                        {
                                            var current = CurrentSubmarine.Get()->CurrentExplorationPoints.ToArray().Select(x => (uint)x).Where(x => x != 0);
                                            if(!current.SequenceEqual(plan.Points))
                                            {
                                                TaskDeployOnPointPlan.Enqueue(next, type, plan);
                                            }
                                            else
                                            {
                                                TaskRedeployVessel.Enqueue(next, type);
                                            }
                                        }
                                        else
                                        {
                                            DuoLog.Error($"所选方案无效（目的地数量={plan.Points.Count}）");
                                        }
                                    }
                                    else if(adata.VesselBehavior == VesselBehavior.Redeploy)
                                    {
                                        TaskRedeployVessel.Enqueue(next, type);
                                    }
                                }
                                catch(Exception e)
                                {
                                    e.Log();
                                }
                            EndTask:
                                P.TaskManager.InsertStack();
                            })
                        );

                    }
                }
            }
        }
        else
        {
            if(PartSwapperScheduler.EnqueueSubmersibleRegistrationIfPossible())
            {
                PluginLog.Information($"已加入潜水艇登记任务队列");
            }
            else if(!Data.AreAnyEnabledVesselsReturnInNext(type, 1 * 60))
            {
                if(EzThrottler.Throttle("DoWorkshopPanelTick.ScheduleResendQuitPanel", 1000))
                {
                    TaskQuitMenu.Enqueue();
                }
            }
        }
    }
}
