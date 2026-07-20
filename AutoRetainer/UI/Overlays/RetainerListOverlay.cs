using AutoRetainer.Internal;
using AutoRetainer.Scheduler.Handlers;
using AutoRetainer.Scheduler.Tasks;
using AutoRetainerAPI;
using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoRetainer.UI.Overlays;

internal unsafe class RetainerListOverlay : Window
{
    private float height;
    internal volatile string PluginToProcess = null;

    public RetainerListOverlay() : base("AutoRetainer 雇员列表覆盖层", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoFocusOnAppearing, true)
    {
        P.WindowSystem.AddWindow(this);
        RespectCloseHotkey = false;
        IsOpen = true;
    }

    public override bool DrawConditions()
    {
        if(!C.UIBar) return false;
        if(Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.OccupiedSummoningBell] && TryGetAddonByName<AtkUnitBase>("RetainerList", out var addon) && IsAddonReady(addon))
        {
            Position = new(addon->X, addon->Y - height);
            return true;
        }
        return false;
    }

    public override void Draw()
    {
        var e = SchedulerMain.PluginEnabled;
        var disabled = MultiMode.Active && !ImGui.GetIO().KeyCtrl;
        if(disabled)
        {
            ImGui.BeginDisabled();
        }
        if(ImGui.Checkbox("启用 AutoRetainer", ref e))
        {
            P.WasEnabled = false;
            if(e)
            {
                SchedulerMain.EnablePlugin(PluginEnableReason.Manual);
            }
            else
            {
                SchedulerMain.DisablePlugin();
            }
        }
        if(disabled)
        {
            ImGui.EndDisabled();
            ImGuiComponents.HelpMarker($"此选项由多角色模式控制。按住 CTRL 可临时覆盖。");
        }
        if(P.WasEnabled)
        {
            ImGui.SameLine();
            ImGuiEx.Text(GradientColor.Get(ImGuiColors.DalamudGrey, ImGuiColors.DalamudGrey3, 500), $"已暂停");
        }
        if(C.MultiModeUIBar)
        {
            ImGui.SameLine();
            if(ImGui.Checkbox("多角色模式", ref MultiMode.Enabled))
            {
                MultiMode.OnMultiModeEnabled();
                if(MultiMode.Active)
                {
                    SchedulerMain.EnablePlugin(PluginEnableReason.MultiMode);
                }
            }
        }

        Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.OnMainControlsDraw).SendMessage();

        ImGui.SameLine();

        if(ImGuiEx.IconButton($"{Lang.IconSettings}##Open plugin interface"))
        {
            Svc.Commands.ProcessCommand("/ays");
        }
        ImGuiEx.Tooltip("打开插件设置");
        if(!P.TaskManager.IsBusy)
        {
            ImGui.SameLine();
            if(ImGuiEx.IconButton($"{Lang.IconDuplicate}##Entrust all duplicates"))
            {
                for(var i = 0; i < GameRetainerManager.Count; i++)
                {
                    var ret = GameRetainerManager.Retainers[i];
                    if(ret.Available)
                    {
                        var adata = Utils.GetAdditionalData(Data.CID, ret.Name);
                        var selectedPlan = C.EntrustPlans.FirstOrDefault(x => x.Guid == adata.EntrustPlan);
                        if(selectedPlan != null)
                        {
                            P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                            TaskEntrustDuplicates.EnqueueNew(selectedPlan);
                            if(C.RetainerMenuDelay > 0)
                            {
                                TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
                            }
                            P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                        }
                        else
                        {
                            Notify.Error($"未找到雇员 {ret.Name} 的转存方案");
                        }

                    }
                }
            }
            ImGuiEx.Tooltip("快速转存");

            ImGui.SameLine();
            if(ImGuiEx.IconButton($"{FontAwesomeIcon.ArrowRightToBracket.ToIconString()}##EntrustManually"))
            {
                ImGui.OpenPopup("EntrustManually");
            }
            if(ImGui.BeginPopup("EntrustManually"))
            {
                foreach(var selectedPlan in C.EntrustPlans)
                {
                    ImGui.PushID(selectedPlan.Guid.ToString());
                    if(ImGui.Selectable($"{selectedPlan.Name}"))
                    {
                        for(var i = 0; i < GameRetainerManager.Count; i++)
                        {
                            var ret = GameRetainerManager.Retainers[i];
                            if(ret.Available)
                            {
                                var adata = Utils.GetAdditionalData(Data.CID, ret.Name);
                                if(selectedPlan != null)
                                {
                                    P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                                    TaskEntrustDuplicates.EnqueueNew(selectedPlan);
                                    if(C.RetainerMenuDelay > 0)
                                    {
                                        TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
                                    }
                                    P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                                }
                            }
                        }
                    }
                    ImGui.PopID();
                }
                ImGui.EndPopup();
            }
            ImGuiEx.Tooltip("执行指定转存方案");

            ImGui.SameLine();
            if(ImGuiEx.IconButton($"{FontAwesomeIcon.ArrowRightFromBracket.ToIconString()}##ReverseEntrust"))
            {
                ImGui.OpenPopup("ReverseEntrust");
            }
            if(ImGui.BeginPopup("ReverseEntrust")) 
            { 
                foreach(var selectedPlan in C.EntrustPlans)
                {
                    ImGui.PushID(selectedPlan.Guid.ToString());
                    if(ImGui.Selectable($"{selectedPlan.Name}"))
                    {
                        for(var i = 0; i < GameRetainerManager.Count; i++)
                        {
                            var ret = GameRetainerManager.Retainers[i];
                            if(ret.Available)
                            {
                                var adata = Utils.GetAdditionalData(Data.CID, ret.Name);
                                if(selectedPlan != null)
                                {
                                    P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                                    TaskEntrustDuplicates.EnqueueNewReverse(selectedPlan);
                                    if(C.RetainerMenuDelay > 0)
                                    {
                                        TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
                                    }
                                    P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                                }
                            }
                        }
                    }
                    ImGui.PopID();
                }
                ImGui.EndPopup();
            }
            ImGuiEx.Tooltip("反向执行指定转存方案（按方案从雇员处取回物品）");

            ImGui.SameLine();
            if(ImGuiEx.IconButton($"{Lang.IconGil}##WithdrawGil"))
            {
                for(var i = 0; i < GameRetainerManager.Count; i++)
                {
                    var ret = GameRetainerManager.Retainers[i];
                    if(ret.Available)
                    {
                        P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                        TaskWithdrawGil.Enqueue(100);

                        if(C.RetainerMenuDelay > 0)
                        {
                            TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
                        }
                        P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                    }
                }
            }
            ImGuiEx.Tooltip("快速取出金币");

            {
                ImGui.SameLine();
                if(ImGuiEx.IconButton($"{Lang.IconFire}##vendoritems"))
                {
                    Utils.EnqueueVendorItemsByRetainer();
                }
                if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup("QuickVendorPopup");
                }
                ImGuiEx.Tooltip("快速出售物品");
                if(ImGui.BeginPopup("QuickVendorPopup"))
                {
                    if(ImGui.Selectable("出售“自由探索委托出售列表”内的物品"))
                    {
                        for(var i = 0; i < GameRetainerManager.Count; i++)
                        {
                            var ret = GameRetainerManager.Retainers[i];
                            if(ret.Available)
                            {
                                P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                                TaskVendorItems.Enqueue(true);

                                if(C.RetainerMenuDelay > 0)
                                {
                                    TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
                                }
                                P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                                P.TaskManager.Enqueue(RetainerHandlers.ConfirmCantBuyback);
                                break;
                            }
                        }
                    }
                    ImGui.EndPopup();
                }
            }

            PluginToProcess = null;
            Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.OnRetainerListTaskButtonsDraw).SendMessage();
            if(PluginToProcess != null)
            {
                for(var i = 0; i < GameRetainerManager.Count; i++)
                {
                    var ret = GameRetainerManager.Retainers[i];
                    if(ret.Available)
                    {
                        P.TaskManager.Enqueue(() => RetainerListHandlers.SelectRetainerByName(ret.Name.ToString()));
                        TaskPostprocessRetainerIPC.Enqueue(ret.Name.ToString(), PluginToProcess);

                        if(C.RetainerMenuDelay > 0)
                        {
                            TaskWaitSelectString.Enqueue(C.RetainerMenuDelay);
                        }
                        P.TaskManager.Enqueue(RetainerHandlers.SelectQuit);
                        P.TaskManager.Enqueue(RetainerHandlers.ConfirmCantBuyback);
                    }
                }
            }
        }
        height = ImGui.GetWindowSize().Y;
    }

}
