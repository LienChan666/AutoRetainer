using AutoRetainer.Modules.Voyage;
using ECommons.ExcelServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using ECommons.IPC.Subscribers.LifestreamIPC;

namespace AutoRetainer.Scheduler.Tasks;
public static class TaskTeleportToProperty
{
    public static uint[] Apartments = [Houses.Ingleside_Apartment, Houses.Kobai_Goten_Apartment, Houses.Lily_Hills_Apartment, Houses.Sultanas_Breath_Apartment, Houses.Topmast_Apartment];
    public static bool EnqueueIfNeededAndPossible(bool isSubmersibleOperation)
    {
        if(Player.Territory.EqualsAny(VoyageUtils.Workshops))
        {
            PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 已在部队工房");
            return false;
        }
        if(!isSubmersibleOperation && C.NoTeleportHetWhenNextToBell && Utils.GetReachableRetainerBell(false) != null)
        {
            PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 因已在传唤铃旁而失败");
            return false;
        }
        var fcTeleportEnabled = (Data.GetAllowFcTeleportForRetainers() && !isSubmersibleOperation) || (Data.GetAllowFcTeleportForSubs() && isSubmersibleOperation);
        var data = Lifestream.GetHousePathData(Player.CID);
        var sharedData = Lifestream.GetSharedHousePathData();
        var info = Lifestream.GetCurrentPlotInfo();
        {
            var canPrivate = Data.GetAllowPrivateTeleportForRetainers() && data.Private != null && data.Private.PathToEntrance.Count > 0;
            var canShared = Data.GetAllowSharedTeleportForRetainers() && sharedData != null && sharedData.PathToEntrance.Count > 0;
            PluginLog.Information($"可用共享房屋：{Lang.Bool(canShared)}");
            var canFc = (fcTeleportEnabled && data.FC != null && data.FC.PathToEntrance.Count > 0);
            var shouldTeleportToFc = (isSubmersibleOperation || !(canPrivate || canShared)) && canFc;
            PluginLog.Debug($"""
                传送至部队房屋：
                已启用部队房屋传送={Lang.Bool(fcTeleportEnabled)}
                允许远航探索传送到部队房屋={Lang.Bool(Data.GetAllowFcTeleportForSubs())}
                远航探索操作={Lang.Bool(isSubmersibleOperation)}
                部队房屋={data.FC}
                入口路径={data.FC?.PathToEntrance.Print()}
                应传送至部队房屋={Lang.Bool(shouldTeleportToFc)}
                可用个人房屋={Lang.Bool(canPrivate)}
                可用共享房屋={Lang.Bool(canShared)}
                可用部队房屋={Lang.Bool(canFc)}
                """);
            if(shouldTeleportToFc)
            {
                PluginLog.Information($"传送：部队房屋=是，共享房屋={Lang.Bool(canShared)}");
                return Process(true, canShared);
            }
            if(!isSubmersibleOperation && (canPrivate || canShared))
            {
                PluginLog.Information($"传送：部队房屋=否，共享房屋={Lang.Bool(canShared)}");
                return Process(false, canShared);
            }
        }

        if(C.AllowSimpleTeleport)
        {
            var canFc = fcTeleportEnabled && Lifestream.HasFreeCompanyHouse() != false;
            var canPrivate = Data.GetAllowPrivateTeleportForRetainers() && Lifestream.HasPrivateHouse() != false;
            var canShared = Data.GetAllowSharedTeleportForRetainers() && Lifestream.HasSharedEstate() != false;
            if((isSubmersibleOperation || !(canPrivate || canShared)) && canFc)
            {
                PluginLog.Information($"简单传送：部队房屋=是，共享房屋={Lang.Bool(canShared)}");
                return ProcessSimple(true, canShared);
            }
            if(!isSubmersibleOperation && (canPrivate || canShared))
            {
                PluginLog.Information($"简单传送：部队房屋=否，共享房屋={Lang.Bool(canShared)}");
                return ProcessSimple(false, canShared);
            }
        }

        if(!isSubmersibleOperation && Data.GetIsTeleportEnabledForRetainers())
        {
            // 公寓逻辑
            if(Data.GetAllowApartmentTeleportForRetainers())
            {
                if(Lifestream.HasApartment() == true && Apartments.Contains(Player.Territory)) return false;
                if(Lifestream.HasApartment() != false)
                {
                    P.TaskManager.Enqueue(() => Lifestream.EnterApartment(true));
                    P.TaskManager.Enqueue(() =>
                    {
                        if(!Svc.ClientState.IsLoggedIn)
                        {
                            PluginLog.Warning($"等待返回住宅时已登出；预计正在进行大区旅行。中止并等待重新登录。");
                            return null;
                        }
                        if(Player.Interactable && Lifestream.HasApartment() == false)
                        {
                            PluginLog.Warning("返回住宅后未找到公寓。中止并重试。");
                            return null;
                        }
                        return IsScreenReady() && Player.Interactable && Apartments.Contains(Player.Territory) && !Lifestream.IsBusy();
                    }, new(timeLimitMS: 5 * 60 * 1000));
                    return true;
                }
            }
            // 旅馆逻辑
            if(!Inns.List.Contains((ushort)Player.Territory))
            {
                P.TaskManager.Enqueue(() => Lifestream.EnqueueInnShortcut(null));
                P.TaskManager.Enqueue(() =>
                {
                    if(!Svc.ClientState.IsLoggedIn)
                    {
                        PluginLog.Warning($"等待返回住宅时已登出；预计正在进行大区旅行。中止并等待重新登录。");
                        return null;
                    }
                    return IsScreenReady() && Player.Interactable && Inns.List.Contains((ushort)Player.Territory) && !Lifestream.IsBusy();
                }, new(timeLimitMS: 5 * 60 * 1000));
                PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 成功，前往旅馆");
                return true;
            }
        }

        // 如果前面没有做出决定，则按需调用 HET，进入任意住宅即可

        if(ExcelTerritoryHelper.Get(Player.Territory)?.TerritoryIntendedUse.RowId == (uint)TerritoryIntendedUseEnum.Residential_Area)
        {
            if(TaskNeoHET.IsInMarkerHousingPlot([.. TaskNeoHET.PrivateMarkers, .. TaskNeoHET.FcMarkers, .. (C.SharedHET ? TaskNeoHET.SharedMarkers : [])]))
            {
                TaskNeoHET.Enqueue(null);
                PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 成功，改为加入房屋进入任务");
                return true;
            }
            else if(TaskNeoHET.GetApartmentEntrance() != null && Player.DistanceTo(TaskNeoHET.GetApartmentEntrance()) < 40f)
            {
                TaskNeoHET.Enqueue(null);
                PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 成功，改为加入公寓进入任务");
                return true;
            }
        }

        PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 失败，所有处理分支均未命中");

        return false;

        bool Process(bool fc, bool shared)
        {
            var pathData = fc ? data.FC : (shared?sharedData:data.Private);
            if(info != null
                && info.Value.Plot == pathData.Plot
                && info.Value.Ward == pathData.Ward
                && info.Value.Kind == pathData.ResidentialDistrict)
            {
                if(Player.Territory.EqualsAny([.. Houses.List]))
                {
                    PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 失败，已在房屋内");
                    return false;
                }
                else
                {
                    TaskNeoHET.Enqueue(null);
                    PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 成功，进入房屋并跳过传送");
                    return true; //already here
                }
            }
            P.TaskManager.Enqueue(() => Lifestream.EnqueuePropertyShortcut((fc ? PropertyType.FC : (shared?PropertyType.Shared_Estate:PropertyType.Home)), HouseEnterMode.Walk_to_door));
            P.TaskManager.Enqueue(() =>
            {
                if(!Svc.ClientState.IsLoggedIn)
                {
                    PluginLog.Warning($"等待返回住宅时已登出；预计正在进行大区旅行。中止并等待重新登录。");
                    return null;
                }
                return Player.Interactable
                && Lifestream.GetCurrentPlotInfo()?.Plot == pathData.Plot
                && Lifestream.GetCurrentPlotInfo()?.Ward == pathData.Ward
                && Lifestream.GetCurrentPlotInfo()?.Kind == pathData.ResidentialDistrict
                && !Lifestream.IsBusy();
            }, new(timeLimitMS: 5 * 60 * 1000));
            TaskNeoHET.Enqueue(null);
            PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 成功，完整流程已加入队列");
            return true;
        }

        bool ProcessSimple(bool fc, bool shared)
        {
            var isHere = TaskNeoHET.IsInMarkerHousingPlot(fc ? TaskNeoHET.FcMarkers : (shared?TaskNeoHET.SharedMarkers:TaskNeoHET.PrivateMarkers));
            var noProperty = !(fc ? Lifestream.HasFreeCompanyHouse() : (shared?Lifestream.HasSharedEstate() != false:Lifestream.HasPrivateHouse()));
            if(noProperty == true)
            {
                PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 简单传送失败，未找到住宅");
                return false;
            }
            if(Player.Territory.EqualsAny([.. Houses.List]) && (!fc || TaskNeoHET.GetWorkshopEntrance() != null))
            {
                PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 简单传送失败，已在房屋内");
                return false;
            }
            else if(isHere)
            {
                TaskNeoHET.Enqueue(null);
                PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 简单传送成功，已加入房屋进入任务");
                return true; //already here
            }
            P.TaskManager.Enqueue(() => Lifestream.EnqueuePropertyShortcut(fc ? PropertyType.FC : (shared ? PropertyType.Shared_Estate : PropertyType.Home), HouseEnterMode.Walk_to_door));
            P.TaskManager.Enqueue(() =>
            {
                if(!Svc.ClientState.IsLoggedIn)
                {
                    PluginLog.Warning($"等待返回住宅时已登出；预计正在进行大区旅行。中止并等待重新登录。");
                    return null;
                }
                return Player.Interactable
                && Player.Territory.EqualsAny([.. ResidentalAreas.List])
                && !Lifestream.IsBusy();
            }, new(timeLimitMS: 5 * 60 * 1000));
            TaskNeoHET.Enqueue(null);
            PluginLog.Debug($"TeleportTask：{Lang.Bool(isSubmersibleOperation)} | 简单传送成功，完整流程已加入队列");
            return true;
        }
    }

    public static bool ShouldVoidHET()
    {
        if(!Player.Available) return false;
        if(Data == null) return true;
        var subsSoon = Data.WorkshopEnabled && Data.AnyEnabledVesselsAvailable() && MultiMode.EnabledSubmarines && (!Data.ShouldWaitForAllWhenLoggedIn() || Data.AreAnyEnabledVesselsReturnInNext(1, true));
        var retainersSoon = MultiMode.AnyRetainersAvailable(0) && MultiMode.EnabledRetainers;
        var blockHet = subsSoon || retainersSoon;
        if(C.AllowSimpleTeleport && (Data.GetAllowFcTeleportForRetainers() || Data.GetAllowPrivateTeleportForRetainers() || Data.GetAllowSharedTeleportForRetainers())) return blockHet;
        var data = Lifestream.GetHousePathData(Player.CID);
        var sharedData = Lifestream.GetSharedHousePathData();
        if(Data.GetAllowFcTeleportForRetainers() && data.FC != null && data.FC.PathToEntrance.Count > 0) return blockHet;
        if(Data.GetAllowPrivateTeleportForRetainers() && data.Private != null && data.Private.PathToEntrance.Count > 0) return blockHet;
        if(Data.GetAllowSharedTeleportForRetainers() && sharedData != null && sharedData.PathToEntrance.Count > 0) return blockHet;
        return false;
    }
}
