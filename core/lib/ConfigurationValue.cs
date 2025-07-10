namespace Confi.Core;

public record ConfigurationValue<T>
{
    public T? Value { get; set; }
    public Func<IServiceProvider, string?, T>? Resolver { get; set; }
    public string? Path { get; set; }

    public ConfigurationValue(
        T? value = default,
        Func<IServiceProvider, string?, T>? resolver = null,
        string? path = null)
    {
        if (resolver == null && value == null)
            throw new ArgumentException("Either value or resolver must be provided.");

        Value = value;
        Resolver = resolver;
        Path = path;
    }

    public static ConfigurationValue<T> ResolvedFromPath(string path, Func<IServiceProvider, string?, T> resolver)
    {
        return new ConfigurationValue<T>(default, resolver, path);
    }

    public T Resolve(IServiceProvider serviceProvider)
    {
        if (Value != null) return Value;
        return Resolver!(serviceProvider, Path);
    }

    public static implicit operator ConfigurationValue<T>(T value) => new(value);
    public static implicit operator ConfigurationValue<T>(Func<T> resolver) => new(default, (_, __) => resolver());
    public static implicit operator ConfigurationValue<T>(Func<IServiceProvider?, string?, T> resolver) => new(default, resolver);
}
