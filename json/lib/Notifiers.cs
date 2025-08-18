namespace Confi;

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