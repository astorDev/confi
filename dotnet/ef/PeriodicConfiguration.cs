using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Confi;

public interface IConfigurationReader
{
    Task<IDictionary<string, string?>> ReadAsync();
}

public class PeriodicConfiguration<TReader> where TReader : IConfigurationReader
{
    public class Provider : ConfigurationProvider
    {
        readonly IServiceCollection _services;
        ILogger<Provider>? _logger;
        TReader? _reader;
        Timer _refreshTimer;
        
        public Provider(IServiceCollection services, TimeSpan refreshPeriod)
        {
            _services = services;

            _refreshTimer = new(
                callback: _ =>
                {
                    if (_reader == null)
                    {
                        return;
                    }

                    Task.Run(async () =>
                    {
                        _logger!.LogTrace("periodic configuration reading started");
                        
                        try
                        {
                            Data = await _reader.ReadAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger!.LogError(ex, "Error while reading configuration");
                        }
                    });
                },
                dueTime: TimeSpan.Zero,
                state: null,
                period: refreshPeriod
            );
        }

        public override void Load()
        {
            Task.Run(async () =>
            {
                while (_logger == null)
                {
                    var sp = _services.BuildServiceProvider();
                    _logger = sp.GetService<ILogger<Provider>>();
                    await Task.Delay(50);
                }
                
                while (_reader == null)
                {
                    _logger.LogInformation("Trying to resolve '{readerType}'", typeof(TReader));

                    try
                    {
                        var sp = _services.BuildServiceProvider();
                        _reader = sp.GetService<TReader>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Fail while attempting to resolve reader");
                        throw;
                    }

                    await Task.Delay(50);
                }
                
                _logger.LogInformation("Provider services resolved");
            });
        }
    }
    
    public class Source(IServiceCollection services, TimeSpan refreshPeriod) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(services, refreshPeriod);
    }
}

public static class PeriodicConfigurationExtensions 
{
    public static IConfigurationBuilder AddPeriodic<TReader>(
        this IConfigurationBuilder builder,
        IServiceCollection services,
        Func<IServiceProvider, TReader> readerFactory,
        TimeSpan refreshPeriod
    ) where TReader : class, IConfigurationReader
    {
        services.AddSingleton(sp => readerFactory(sp));
        builder.Add(new PeriodicConfiguration<TReader>.Source(services, refreshPeriod));
        
        return builder;
    }
}