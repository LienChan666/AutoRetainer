using Microsoft.Win32;

namespace AutoRetainer.Services;
public unsafe sealed class WorkstationMonitor : IDisposable
{
    public bool Locked { get; private set; } = false;
    private WorkstationMonitor()
    {
        SystemEvents.SessionSwitch += OnSessionSwitch;
    }

    public void Dispose()
    {
        SystemEvents.SessionSwitch -= OnSessionSwitch;
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if(e.Reason == SessionSwitchReason.SessionLock)
        {
            PluginLog.Debug($"工作站已锁定（{DateTimeOffset.Now}）");
            Locked = true;
        }
        else if(e.Reason == SessionSwitchReason.SessionUnlock)
        {
            PluginLog.Debug($"工作站已解锁（{DateTimeOffset.Now}）");
            Locked = false;
        }
    }
}
