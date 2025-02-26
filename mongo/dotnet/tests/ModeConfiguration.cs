using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Confi.Mongo.Tests;

[TestClass]
public class Options
{
    [TestMethod]
    public void ResolveLongPollingFromIConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Mode", "LongPolling") })
            .Build();

        var resolvedMode = configuration.GetValue<MongoReadingMode>("Mode");
        Console.Write(resolvedMode);
        resolvedMode.ShouldBe(MongoReadingMode.LongPolling);
    }
    
    [TestMethod]
    public void ResolveCollectionWatchFromIConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Mode", "CollectionWatch") })
            .Build();

        var resolvedMode = configuration.GetValue<MongoReadingMode>("Mode");
        Console.Write(resolvedMode);
        resolvedMode.ShouldBe(MongoReadingMode.CollectionWatching);
    }
    
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ThrowOnNotExistingValue()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Mode", "Unmatched") })
            .Build();

        var resolvedMode = configuration.GetValue<MongoReadingMode?>("Mode");
        Console.Write(resolvedMode);
        resolvedMode.ShouldBe(MongoReadingMode.CollectionWatching);
    }

    [TestMethod]
    public void ResolveToNull()
    {
        var configuration = new ConfigurationBuilder()
            .Build();

        var resolvedMode = configuration.GetValue<MongoReadingMode?>("Mode");
        Console.Write(resolvedMode);
        resolvedMode.ShouldBeNull();
    }
}