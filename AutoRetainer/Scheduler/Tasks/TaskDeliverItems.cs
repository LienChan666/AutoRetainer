using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Singletons;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Tasks;
public static unsafe class TaskDeliverItems
{
    static bool UsingItemBuff = false;
    public static bool Enqueue(bool force = false)
    {
        UsingItemBuff = false;
        var gcInfo = GCContinuation.GetFullGCInfo();
        if(gcInfo == null)
        {
            Notify.Error("当前角色尚未加入大国防联军");
            return false;
        }
        if(Data.GCDeliveryType == AutoRetainerAPI.Configuration.GCDeliveryType.Disabled)
        {
            Notify.Error("当前角色已禁用筹备稀有品，无法加入任务队列");
            return false;
        }
        if(Lifestream.IsBusy())
        {
            Notify.Error("Lifestream 正忙");
            return false;
        }
        if(!force && Utils.IsBusy)
        {
            Notify.Error("AutoRetainer 正忙");
            return false;
        }
        P.TaskManager.Enqueue(() =>
        {
            if(C.FullAutoGCDeliveryUseBuffItem)
            {
                if(Player.Object.IsCasting(14946, ActionType.Item))
                {
                    UsingItemBuff = true;
                    return true;
                }
                else if(!Player.Status.Any(x => x.StatusId == 1078) && InventoryManager.Instance()->GetInventoryItemCount(14946) > 0)
                {
                    if(EzThrottler.Throttle("UseFCBuffItem", 1000))
                    {
                        AgentInventoryContext.Instance()->UseItem(14946);
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }, "使用军票预支单", new(timeLimitMS: 30000, abortOnTimeout: false));
        P.TaskManager.Enqueue(() => !Player.Object.IsCasting() && !Player.IsAnimationLocked, "等待角色空闲");
        P.TaskManager.Enqueue(() =>
        {
            if(UsingItemBuff) return;
            if(!C.FullAutoGCDeliveryUseBuffFCAction) return;
            if(Player.Character->FreeCompanyTagString != "" && Player.IsInHomeWorld)
            {
                P.TaskManager.InsertStack(() =>
                {
                    TaskActivateSealSweetener.EnqueueThrottled();
                });
            }
            return;
        });
        if(!gcInfo.IsReadyToExchange())
        {
            P.TaskManager.Enqueue(() => Lifestream.ExecuteCommand("gc " + Player.GrandCompany switch
            {
                ECommons.ExcelServices.GrandCompany.ImmortalFlames => "if",
                ECommons.ExcelServices.GrandCompany.Maelstrom => "m",
                ECommons.ExcelServices.GrandCompany.TwinAdder => "ta",
                _ => throw new ArgumentOutOfRangeException()
            }), "传送至大国防联军总部");
        }
        P.TaskManager.Enqueue(() => !Lifestream.IsBusy(), "等待传送完成", new(timeLimitMS: 5 * 60 * 1000) { CompanionAction = _ => EzThrottler.Throttle("GcBusy", 60000, true)});
        P.TaskManager.Enqueue(() => GCContinuation.EnqueueInitiation(true), "开始筹备稀有品");
        return true;
    }
}
