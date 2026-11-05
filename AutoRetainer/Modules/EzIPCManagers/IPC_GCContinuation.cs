using ECommons.EzIpcManager;
using ECommons.Throttlers;

namespace AutoRetainer.Modules.EzIPCManagers;
public class IPC_GCContinuation
{
    public IPC_GCContinuation()
    {
        EzIPC.Init(this, $"{Svc.PluginInterface.InternalName}.GC");
    }

    [EzIPC]
    public void EnqueueInitiation()
    {
        GCContinuation.EnqueueInitiation(true);
    }

    [EzIPC]
    public GCInfo? GetGCInfo()
    {
        if(EzThrottler.Throttle("IPCInformObsoleteFunction", 10000))
        {
            PluginLog.Warning($"请勿使用 GetGCInfo IPC 方法，该方法已废弃。");
        }
        return GCContinuation.GetGCInfo();
    }
}
