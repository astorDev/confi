using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Confi;

public class DbContextConfigurationReader<TDbContext, TRecord>(IDbContextFactory<TDbContext> dbFactory,
    Func<TDbContext, DbSet<TRecord>> setAccessor, 
    Func<TRecord, string> keySelector,
    Func<TRecord, string?> valueSelector) : IConfigurationReader where TDbContext : DbContext where TRecord : class
{
    public async Task<IDictionary<string, string?>> ReadAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var set = setAccessor(db);
        var records = await set.ToArrayAsync();
        return records.ToDictionary(keySelector, valueSelector);
    }
}

public class ConfigurationRecord
{
    public string Id { get; set; } = null!;
    public string Value { get; set; } = null!;
}

public static class DbContextConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddDbContext<TDbContext>(
        this IConfigurationBuilder builder,
        IServiceCollection services
        ) where TDbContext : DbContext
    {
        return builder.AddDbContext<TDbContext>(
            services,
            setSelector: ctx => ctx.Set<ConfigurationRecord>()
        );
    }
    
    public static IConfigurationBuilder AddDbContext<TDbContext>(
        this IConfigurationBuilder builder,
        IServiceCollection services,
        Func<TDbContext, DbSet<ConfigurationRecord>> setSelector
    ) where TDbContext : DbContext
    {
        return builder.AddDbContext(
            services,
            setSelector,
            keySelector: r => r.Id,
            valueSelector: r => r.Value
        );
    }
    
    public static IConfigurationBuilder AddDbContext<TDbContext, TRecord>(
        this IConfigurationBuilder builder,
        IServiceCollection services,
        Func<TDbContext, DbSet<TRecord>> setAccessor,
        Func<TRecord, string> keySelector,
        Func<TRecord, string?> valueSelector
        
    ) where TDbContext : DbContext where TRecord : class
    {
        return builder.AddPeriodic(
            services,
            readerFactory: sp => new DbContextConfigurationReader<TDbContext, TRecord>(
                sp.GetRequiredService<IDbContextFactory<TDbContext>>(),
                setAccessor,
                keySelector,
                valueSelector
            ),
            refreshPeriod: TimeSpan.FromMinutes(5)
        );
    }
}