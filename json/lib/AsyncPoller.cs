using Backi.Timers;

namespace Confi;

public class AsyncPoller<T> 
{
    private readonly Notifier<T> _loaded = new();
    private readonly Notifier<T> _updated = new();
    private readonly Notifier<Exception> _error = new();
    private readonly Func<Task<T>> _poll;
    private readonly Func<T, T, bool> _equalityCheck;
    public T? _latest;

    public record ListenablesSet(
        IListenable<T> Loaded,
        IListenable<T> Updated,
        IListenable<Exception> Error
    );

    public ListenablesSet Listenables => new(
        _loaded,
        _updated,
        _error
    );

    public AsyncPoller(TimeSpan refreshInterval, Func<Task<T>> poll, Func<T, T, bool> equalityCheck)
    {
        _poll = poll;
        _equalityCheck = equalityCheck;

        _ = SafeTimer.RunNowAndPeriodically(
            refreshInterval,
            async () =>
            {
                var loadedData = await _poll();
                OnLoaded(loadedData);
            },
            onException: _error.Notify
        );
        this._poll = poll;
    }
    
    private void OnLoaded(T value)
    {
        _loaded.Notify(value);
        if (_latest == null || !_equalityCheck(_latest, value))
        {
            _latest = value;
            _updated.Notify(value);
        }
    }

    public void PollSync()
    {
        var loadedData = GetSync();
        OnLoaded(loadedData);
    }

    public T GetSync() => _poll().GetAwaiter().GetResult();
}