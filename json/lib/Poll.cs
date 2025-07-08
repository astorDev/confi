using Backi.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Confi;

public class PolledConfigurationProvider : ConfigurationProvider
{
    public PolledConfigurationProvider(ConfigurationPoller poller)
    {
        poller.AddListener(config =>
        {
            Data = config;
            OnReload();
        });
    }
}

public abstract class ConfigurationPoller : SafeNotifier<IDictionary<string, string?>>
{
    public ConfigurationPoller(TimeSpan refreshInterval)
    {
        _ = SafeTimer.RunNowAndPeriodically(
            refreshInterval,
            async () =>
            {
                var config = await Get();
                Notify(config);
            },
            onException: NotifyError
        );
    }

    public void PollSync()
    {
        var config = GetSync();
        Notify(config);
    }

    public abstract Task<IDictionary<string, string?>> Get();
    public IDictionary<string, string?> GetSync() => Get().GetAwaiter().GetResult();
}

public class SafeNotifier<T> : Notifier<T>, IErrorListenable
{
    private readonly List<Action<Exception>> _errorListeners = new();

    public void AddListener(Action<Exception> listener)
    {
        _errorListeners.Add(listener);
    }

    protected void NotifyError(Exception ex)
    {
        foreach (var listener in _errorListeners)
        {
            listener(ex);
        }
    }
}

public class Notifier<T> : IListenable<T>
{
    private readonly List<Action<T>> _listeners = new();

    public void AddListener(Action<T> listener)
    {
        _listeners.Add(listener);
    }

    public void Notify(T value)
    {
        foreach (var listener in _listeners)
        {
            listener(value);
        }
    }
}

public interface IListenable<T>
{
    void AddListener(Action<T> listener);
}

public interface IErrorListenable : IListenable<Exception>
{
}

public static class ErrorListenableExtensions
{
    public static void RegisterLogging(this IErrorListenable errorListenable, ILogger logger, string message)
    {
        errorListenable.AddListener(ex => logger.LogError(ex, message));
    }
}