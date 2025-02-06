using Backi.Timers;
using Confi;

namespace V2;

public class Counting
{
    public const string Key = "Counter";

    public class BackgroundService(ILogger<BackgroundService> logger, ConfigurationBackgroundStore store) : IHostedService
    {
        private SafeTimer _timer = null!;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting counting timer");

            _timer = SafeTimer.RunNowAndPeriodically(
                TimeSpan.FromSeconds(1), 
                () => {
                    var currentValue = store.GetValueOrDefault<int>(Key);
                    logger.LogInformation("Incrementing counter. Current value: {currentValue}", currentValue);
                    store.SetValue(Key, currentValue + 1);
                },
                (ex) => logger.LogError(ex, "Error in counting timer")
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping counting timer");

            _timer.Stop();
            return Task.CompletedTask;
        }
    }
}

public class EnvironmentShuffling
{
    public const string Key = "EnvironmentShuffled";

    public class BackgroundService(IHostEnvironment env, ILogger<BackgroundService> logger, ConfigurationBackgroundStore.Factory storeFactory) : IHostedService
    {
        private readonly ConfigurationBackgroundStore store = storeFactory.GetStore(Key);
        private SafeTimer _timer = null!;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting environment shuffling timer");

            _timer = SafeTimer.RunNowAndPeriodically(
                TimeSpan.FromSeconds(1), 
                () => {
                    var currentValue = store.GetValueOrDefault<string>(Key) ?? "";
                    logger.LogInformation("Shuffling environment. Current value: {currentValue}", currentValue);
                    var randomLetter = env.EnvironmentName[Random.Shared.Next(env.EnvironmentName.Length)];
                    store.SetValue(Key, currentValue + randomLetter);
                },
                (ex) => logger.LogError(ex, "Error in environment shuffling timer")
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping environment shuffling timer");

            _timer.Stop();
            return Task.CompletedTask;
        }
    }
}