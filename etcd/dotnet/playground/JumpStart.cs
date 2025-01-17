using dotnet_etcd;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;

namespace Playground.Etcd;

[TestClass]
public sealed class JumpStart
{
    private readonly EtcdClient etcdClient = new(
        "http://localhost:2379",
        configureChannelOptions: options => 
            options.Credentials = ChannelCredentials.Insecure
    );
    
    [TestMethod]
    public async Task Basic()
    {
        await etcdClient.PutAsync("name", "Egor");
        var received = await etcdClient.GetValAsync("name");
        Console.WriteLine($"Received. name = {received}");
    }

    [TestMethod]
    public async Task Watch()
    {
        etcdClient.WatchAsync(
                "dog",
                (WatchEvent[] response) =>
                {
                    Console.WriteLine("received watch response");

                    foreach (var watchEvent in response)
                    {
                        Console.WriteLine($"Received event: {watchEvent.Key} -> {watchEvent.Value}. ({watchEvent.Type})");
                    }
                }
            );

        etcdClient.Put("dog", "comes");
        etcdClient.Put("dog", "lays");

        await etcdClient.PutAsync("dog", "sits");
        await etcdClient.PutAsync("dog", "runs");
        
        //await Task.Delay(100);
        
        await etcdClient.PutAsync("dog", "barks");
    }
    
    [TestMethod]
    public async Task WatchTransaction()
    {
        var request = new WatchRequest()
        {
            CreateRequest = new()
            {
                Key = ByteString.CopyFromUtf8("animals/"),
                RangeEnd = ByteString.CopyFromUtf8("animals0")
            }
        };
        
        etcdClient.WatchAsync(
            // "animals/",
            request,
            (WatchEvent[] response) =>
            {
                Console.WriteLine("received watch response");

                foreach (var watchEvent in response)
                {
                    Console.WriteLine($"Received event: {watchEvent.Key} -> {watchEvent.Value}. ({watchEvent.Type})");
                }
            }
        );
        
        var transaction = new TxnRequest();
        
        transaction.Success.AddRange(new []
        {
            new RequestOp { 
                RequestPut = new()
                {
                    Key = ByteString.CopyFromUtf8("animals/cow"),
                    Value = ByteString.CopyFromUtf8("moo")
                } 
            },
            new RequestOp { 
                RequestPut = new()
                {
                    Key = ByteString.CopyFromUtf8("animals/chicken"),
                    Value = ByteString.CopyFromUtf8("coo")
                } 
            }
        });

        await etcdClient.TransactionAsync(transaction);
        
        var cow = await etcdClient.GetValAsync("animals/cow");
        var chicken = await etcdClient.GetValAsync("animals/chicken");
        
        Console.WriteLine($"cow = {cow}");
        Console.WriteLine($"chicken = {chicken}");
    }
}
