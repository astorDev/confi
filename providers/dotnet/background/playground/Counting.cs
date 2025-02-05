using Backi.Timers;

public class Counting
{
    public const string Key = "Counter";

    public class BackgroundService(ILogger<BackgroundService> logger, ConfigurationStore store) : IHostedService
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