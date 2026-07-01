using System.Threading.Tasks;
using Unity.Services.Multiplayer;

public static class SessionHolder
{
    private static TaskCompletionSource<bool> _sessionTcs = new TaskCompletionSource<bool>();
    public static Task SessionReady => _sessionTcs.Task;

    private static ISession _session;
    public static ISession Session
    {
        get => _session;
        set
        {
            _session = value;
            if (value != null)
                _sessionTcs.TrySetResult(true);
            else
                _sessionTcs = new TaskCompletionSource<bool>();
        }
    }
}