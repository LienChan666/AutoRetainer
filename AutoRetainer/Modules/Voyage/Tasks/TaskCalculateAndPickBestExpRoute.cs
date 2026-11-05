using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;
using Lumina.Excel.Sheets;

namespace AutoRetainer.Modules.Voyage.Tasks;

internal static unsafe class TaskCalculateAndPickBestExpRoute
{
    private static volatile bool Calculating = false;
    internal static volatile bool Stop = false;
    internal static void Enqueue(SubmarineUnlockPlan unlock = null)
    {
        Stop = false;
        VoyageUtils.Log($"任务已加入队列：{nameof(TaskCalculateAndPickBestExpRoute)}（方案：{unlock}）");
        P.TaskManager.Enqueue(() => Calculate(unlock));
        P.TaskManager.Enqueue(WaitUntilCalculationStopped, new(timeLimitMS: 60 * 60 * 1000));
    }

    internal static void Calculate(SubmarineUnlockPlan unlock)
    {
        if(Stop)
        {
            Stop = false;
            return;
        }
        Calculating = true;
        var calc = new Calculator();
        var curSubMaps = CurrentSubmarine.GetMaps();
        var curSubRank = CurrentSubmarine.Get()->RankId;
        var prioList = unlock?.GetPrioritizedPointList();
        void Run()
        {
            VoyageMain.WaitOverlay.IsProcessing = true;
            try
            {
                double exp = 0;
                uint[] path = null;
                var selectedMap = 0;
                if(prioList != null) VoyageUtils.Log($"优先目的地列表：{prioList.Select(x => $"{VoyageUtils.GetSubmarineExplorationName(x.point)}（{x.justification}）").Print()}");
                var calcCnt = 0;
                void Calc()
                {
                    if(calcCnt > 1) throw new Exception("无法计算最佳航线。");
                    calcCnt++;
                    foreach(var map in curSubMaps)
                    {
                        calc.RouteBuild.Value.ChangeMap((int)map);
                        var doCalc = false;
                        if(prioList != null && prioList.Count > 0)
                        {
                            var point = VoyageUtils.GetSubmarineExploration(prioList[0].point);
                            if(point == null || point?.Map.RowId != map || point?.RankReq > curSubRank)
                            {
                                //
                            }
                            else
                            {
                                doCalc = true;
                                VoyageUtils.Log($"正在添加目的地：{VoyageUtils.GetSubmarineExplorationName(prioList[0].point)}（{prioList[0].justification}）");
                                calc.MustInclude.Add(VoyageUtils.GetSubmarineExploration(prioList[0].point).Value);
                            }
                        }
                        else
                        {
                            doCalc = true;
                        }
                        if(doCalc)
                        {
                            var best = calc.FindBestPath(map);
                            if(best != null && best.Value.path != null)
                            {
                                var xptime = best.Value.exp / (double)best.Value.duration.TotalHours;
                                VoyageUtils.Log($"航线 {best.Value.path.Select(z => $"{z}/{Svc.Data.GetExcelSheet<SubmarineExploration>().GetRowOrDefault(z)?.Location}").Print()} 是航海图 {map} 的最佳航线，耗时 {best.Value.duration}，经验 {best.Value.exp}（{xptime} 经验/小时）");
                                if(xptime > exp)
                                {
                                    selectedMap = (int)map;
                                    exp = xptime;
                                    path = best.Value.path;
                                }
                            }
                        }
                        else
                        {
                            VoyageUtils.Log($"航海图 {map} 没有待解锁目的地，已跳过");
                        }
                    }
                }
                Calc();
                if(path == null)
                {
                    VoyageUtils.Log($"航线为空，正在不使用方案重试……");
                    calc.MustInclude.Clear();
                    prioList = null;
                    Calc();
                }
                VoyageUtils.Log($"航线 {path.Select(z => $"{z}/{Svc.Data.GetExcelSheet<SubmarineExploration>().GetRowOrDefault(z)?.Location}").Print()} 被确定为航海图 {selectedMap} 的最佳航线（{exp} 经验/小时）");
                if(path != null)
                {
                    new TickScheduler(delegate
                    {
                        TaskPickSubmarineRoute.EnqueueImmediate((uint)selectedMap, path);
                        Calculating = false;
                    });
                }
            }
            catch(Exception e)
            {
                DuoLog.Error($"航线优化时发生严重错误：{e.Message}");
                e.Log();
            }
            VoyageMain.WaitOverlay.IsProcessing = false;
        }
        if(C.VoyageDisableCalcMultithreading)
        {
            Run();
        }
        else
        {
            Task.Run(Run);
        }
    }

    internal static bool? WaitUntilCalculationStopped()
    {
        return !Calculating;
    }
}
