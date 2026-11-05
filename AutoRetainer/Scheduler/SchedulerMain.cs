using AutoRetainer.Internal;
using AutoRetainer.Scheduler.Handlers;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainerAPI.Configuration;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Immutable;
using System.Diagnostics;

namespace AutoRetainer.Scheduler;

internal static unsafe class SchedulerMain
{
    internal static bool PluginEnabledInternal;
    internal static bool PluginEnabled
    {
        get
        {
            return PluginEnabledInternal && !IPC.Suppressed;
        }
        private set
        {
            PluginEnabledInternal = value;
        }
    }

    internal static bool CanAssignQuickExploration => C.EnableAssigningQuickExploration && !C.DontReassign && Utils.GetVenturesAmount() > 1;
    internal static volatile uint VentureOverride = 0;
    internal static volatile bool RetainerPostProcessLocked = false;
    internal static volatile bool CharacterPostProcessLocked = false;
    internal static ImmutableList<string> RetainerPostprocess = Array.Empty<string>().ToImmutableList();
    internal static ImmutableList<string> CharacterPostprocess = Array.Empty<string>().ToImmutableList();

    internal static PluginEnableReason Reason { get; set; }

    internal static bool? EnablePlugin(PluginEnableReason reason)
    {
        Reason = reason;
        PluginEnabled = true;
        DebugLog($"插件已启用，原因：{reason}");
        return true;
    }

    internal static bool? DisablePlugin()
    {
        PluginEnabled = false;
        DebugLog($"插件已禁用");
        return true;
    }

    internal static void Tick()
    {
        if(PluginEnabled)
        {
            if(C.RetainerSense)
            {
                MultiMode.ValidateAutoAfkSettings();
            }
            if(C.OldRetainerSense)
            {
                MultiMode.ValidateAutoAfkSettings();
            }
            if(TryGetAddonByName<AtkUnitBase>("RetainerList", out var addon) && addon->IsVisible)
            {
                if(Utils.GenericThrottle)
                {
                    if(!P.TaskManager.IsBusy)
                    {
                        if(Utils.IsInventoryFree())
                        {
                            var retainer = GetNextRetainerName();
                            if(retainer != null && Utils.TryGetRetainerByName(retainer, out var ret))
                            {
                                if(EzThrottler.Throttle("ScheduleSelectRetainer", 2000))
                                {
                                    P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(retainer));

                                    var adata = Utils.GetAdditionalData(Svc.ClientState.LocalContentId, ret.Name.ToString());

                                    if(Data.GetIMSettings().IMEnableAutoVendor)
                                    {
                                        TaskVendorItems.Enqueue();
                                    }

                                    VentureOverride = 0;

                                    IPC.FireSendRetainerToVentureEvent(retainer);

                                    if(VentureOverride > 0)
                                    {
                                        DebugLog($"正在使用 VentureOverride = {VentureOverride}");
                                        ret.ProcessVenturePlanner(VentureOverride);
                                    }
                                    else if(!adata.IsVenturePlannerActive())
                                    {
                                        // 重新委托雇员

                                        if(ret.VentureID != 0)
                                        {
                                            if(C.DontReassign || Utils.GetVenturesAmount() < 2)
                                            {
                                                TaskCollectVenture.Enqueue();
                                            }
                                            else
                                            {
                                                TaskReassignVenture.Enqueue();
                                            }
                                        }
                                        else
                                        {
                                            if(CanAssignQuickExploration)
                                            {
                                                TaskAssignQuickVenture.Enqueue();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var next = adata.GetNextPlannedVenture();
                                        DebugLog($"下一项计划探险：{next}，当前探险：{ret.VentureID}");
                                        var completed = adata.IsLastPlannedVenture();
                                        DebugLog($"是否为计划中的最后一项探险：{completed}");
                                        if(next == 0)
                                        {
                                            var t = ($"下一项探险 ID 为 0，将禁用探险规划器");
                                            if(!completed)
                                            {
                                                DuoLog.Warning(t);
                                            }
                                            else
                                            {
                                                DebugLog(t);
                                            }
                                        }
                                        if(next == 0 || (completed && adata.VenturePlan.PlanCompleteBehavior != PlanCompleteBehavior.Restart_plan))
                                        {
                                            DebugLog($"已完成，后续行为为 {adata.VenturePlan.PlanCompleteBehavior}");
                                            if(adata.VenturePlan.PlanCompleteBehavior == PlanCompleteBehavior.Repeat_last_venture)
                                            {
                                                DebugLog($"正在重新委托本次探险并禁用规划器");
                                                TaskReassignVenture.Enqueue();
                                            }
                                            else
                                            {
                                                TaskCollectVenture.Enqueue();
                                                if(adata.VenturePlan.PlanCompleteBehavior == PlanCompleteBehavior.Assign_Quick_Venture)
                                                {
                                                    DebugLog($"正在执行自由探索委托");
                                                    TaskAssignQuickVenture.Enqueue();
                                                }
                                            }
                                            adata.EnablePlanner = false;
                                            DebugLog($"正在禁用规划器");
                                        }
                                        else
                                        {
                                            ret.ProcessVenturePlanner(next);
                                        }
                                        if(completed)
                                        {
                                            adata.VenturePlanIndex = 0;
                                        }
                                        adata.VenturePlanIndex++;
                                    }

                                    var selectedPlan = C.EntrustPlans.FirstOrDefault(x => x.Guid == adata.EntrustPlan && !x.ManualPlan);
                                    if(C.EnableEntrustManager && selectedPlan != null)
                                    {
                                        TaskEntrustDuplicates.EnqueueNew(selectedPlan);
                                    }

                                    // 取出金币
                                    if(adata.WithdrawGil)
                                    {
                                        if(adata.Deposit)
                                        {
                                            if(TaskDepositGil.Gil > 0) TaskDepositGil.Enqueue(adata.WithdrawGilPercent);
                                        }
                                        else
                                        {
                                            TaskWithdrawGil.Enqueue(adata.WithdrawGilPercent);
                                        }
                                    }

                                    if(Data.GetIMSettings().IMEnableAutoVendor)
                                    {
                                        TaskVendorItems.Enqueue();
                                    }

                                    // 触发事件，让其他插件处理雇员
                                    TaskPostprocessRetainerIPC.Enqueue(retainer);

                                    if(C.RetainerMenuDelay > 0)
                                    {
                                        TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
                                    }
                                    P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                                    P.TaskManager.Enqueue(RetainerHandlers.ConfirmCantBuyback);
                                }
                            }
                            else
                            {
                                if((C.Stay5 || MultiMode.Active) && !Utils.IsAllCurrentCharacterRetainersHaveMoreThan5Mins())
                                {
                                    // 不执行操作
                                }
                                else
                                {
                                    if(Reason == PluginEnableReason.MultiMode)
                                    {
                                        DebugLog($"多角色模式正在运行，安排关闭雇员列表并禁用插件");
                                        P.TaskManager.Enqueue(RetainerListHandlers.CloseRetainerList);
                                        P.TaskManager.Enqueue(DisablePlugin);
                                        if(Data.GetIMSettings().IMEnableCofferAutoOpen) TaskOpenAllCoffers.Enqueue();
                                        if(Data.GetIMSettings().IMEnableItemDesynthesis) TaskDesynthItems.Enqueue();
                                    }
                                    else if(Reason == PluginEnableReason.Artisan)
                                    {
                                        DebugLog($"Artisan 正在运行，安排关闭雇员列表");
                                        P.TaskManager.Enqueue(RetainerListHandlers.CloseRetainerList);
                                        P.TaskManager.Enqueue(DisablePlugin);
                                    }
                                    else
                                    {
                                        void Process(TaskCompletedBehavior behavior)
                                        {
                                            if(behavior.EqualsAny(TaskCompletedBehavior.Stay_in_retainer_list_and_disable_plugin, TaskCompletedBehavior.Close_retainer_list_and_disable_plugin))
                                            {
                                                DebugLog($"安排禁用插件（行为={Lang.TaskCompletedBehaviorNames[behavior]}）");
                                                P.TaskManager.Enqueue(DisablePlugin);
                                            }
                                            if(behavior.EqualsAny(TaskCompletedBehavior.Close_retainer_list_and_disable_plugin, TaskCompletedBehavior.Close_retainer_list_and_keep_plugin_enabled))
                                            {
                                                DebugLog($"安排关闭雇员列表（行为={Lang.TaskCompletedBehaviorNames[behavior]}）");
                                                P.TaskManager.Enqueue(RetainerListHandlers.CloseRetainerList);
                                            }
                                        }

                                        if(Reason == PluginEnableReason.Auto)
                                        {
                                            Process(C.TaskCompletedBehaviorAuto);
                                        }
                                        else if(Reason == PluginEnableReason.Manual)
                                        {
                                            Process(C.TaskCompletedBehaviorManual);
                                        }
                                        else if(Reason == PluginEnableReason.Access)
                                        {
                                            Process(C.TaskCompletedBehaviorAccess);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if(EzThrottler.Throttle("CloseRetainerList", 1000))
                            {
                                DuoLog.Warning($"背包已满");
                                if(MultiMode.Active)
                                {
                                    DebugLog($"安排关闭雇员列表（多角色模式）");
                                    P.TaskManager.Enqueue(RetainerListHandlers.CloseRetainerList);
                                }
                                else
                                {
                                    void Process(TaskCompletedBehavior behavior)
                                    {
                                        DebugLog($"行为：{Lang.TaskCompletedBehaviorNames[behavior]}");
                                        if(behavior.EqualsAny(TaskCompletedBehavior.Close_retainer_list_and_disable_plugin, TaskCompletedBehavior.Close_retainer_list_and_keep_plugin_enabled))
                                        {
                                            DebugLog($"安排关闭雇员列表（行为={Lang.TaskCompletedBehaviorNames[behavior]}）");
                                            P.TaskManager.Enqueue(RetainerListHandlers.CloseRetainerList);
                                        }
                                    }

                                    if(Reason == PluginEnableReason.Auto)
                                    {
                                        Process(C.TaskCompletedBehaviorAuto);
                                    }
                                    else if(Reason == PluginEnableReason.Manual)
                                    {
                                        Process(C.TaskCompletedBehaviorManual);
                                    }
                                    else if(Reason == PluginEnableReason.Access)
                                    {
                                        Process(C.TaskCompletedBehaviorAccess);
                                    }
                                }
                                DisablePlugin();
                            }
                        }
                    }
                }
            }
            else
            {
                if(C.OldRetainerSense || SchedulerMain.Reason == PluginEnableReason.Artisan)
                {
                    if(Utils.AnyRetainersAvailableCurrentChara())
                    {
                        if(!IsOccupied())
                        {
                            if(EzThrottler.Check("InteractWithBellDelay") && EzThrottler.Throttle("InteractWithBellGeneralEnqueue", 5000))
                            {
                                TaskInteractWithNearestBell.Enqueue();
                            }
                        }
                        else
                        {
                            EzThrottler.Throttle("InteractWithBellDelay", 2500, true);
                        }
                    }
                }
            }
        }
    }

    internal static string GetNextRetainerName()
    {
        if(GameRetainerManager.Ready)
        {
            if(C.OfflineData.TryGetFirst(x => x.CID == Svc.ClientState.LocalContentId, out var cdata))
            {
                List<OfflineRetainerData> retainerData = [.. cdata.RetainerData];
                if(C.LeastMBSFirst)
                {
                    retainerData = [.. cdata.RetainerData.OrderBy(x => x.MBItems)];
                }

                for(var i = 0; i < retainerData.Count; i++)
                {
                    var r = retainerData[i];
                    var rname = r.Name.ToString();
                    var adata = Utils.GetAdditionalData(Svc.ClientState.LocalContentId, rname);
                    if(P.GetSelectedRetainers(Svc.ClientState.LocalContentId).Contains(rname)
                        && r.GetVentureSecondsRemaining() <= C.UnsyncCompensation && (r.VentureID != 0 || CanAssignQuickExploration || (adata.EnablePlanner && adata.VenturePlan.ListUnwrapped.Count > 0)))
                    {
                        return rname;
                    }
                }
            }
        }
        return null;
    }
}
