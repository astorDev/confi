using System.Text.Json;
using Confi;
using MongoDB.Bson;
using MongoDB.Driver;
using Persic;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

// v1:

// builder.Services.AddMongoCollection<ConfigurationRecord>("configs");
// builder.Services.AddSingleton<MongoConfigurationLoader.Factory>();

// builder.AddBackgroundConfiguration(
//     MongoConfigurationLoader.Key("simple"),
//     sp => {
//         var loader = sp.GetRequiredService<MongoConfigurationLoader.Factory>()
//             .GetLoader("simple");

//         return new MongoBackgroundConfigurationWatcher(loader);
//     }
// );

// v2:

builder.AddMongoConfiguration(documentId: "simple");
builder.AddMongoConfiguration(documentId: "toggles", mode: MongoReadingMode.LongPolling);

builder.Services.AddMongo(
    "mongodb://localhost:27017/?replicaSet=rs0", 
    "confi-playground"
);

var app = builder.Build();

app.MapPut("simple/config", async (IMongoCollection<ConfigurationRecord> collection, IConfiguration configuration, JsonElement body) => {
    var result = await collection.Put(new (
        "simple",
        BsonDocument.Parse(body.ToString()
    )));

    await Task.Delay(100);
    
    return new {
        sent = body,
        config = new {
            name = configuration["name"],
            age = configuration["age"],
        }
    };
});

app.MapPut("toggles/config", async (IMongoCollection<ConfigurationRecord> collection, IConfiguration configuration, JsonElement body) => {
    var result = await collection.Put(new(
        "toggles",
        BsonDocument.Parse(body.ToString()
    )));

    await Task.Delay(100);
    
    return new {
        sent = body,
        config = new {
            featureManagement = new {
                featureA = configuration["featureManagement:featureA"],
                featureB = new {
                    percentage = configuration["featureManagement:featureB:percentage"],
                }
            }
        }
    };
});

app.MapGet("config", (  IConfiguration configuration) => {
    return new {
        featureManagement = new {
            name = configuration["name"],
            age = configuration["age"],
            featureA = configuration["featureManagement:featureA"],
            featureB = new {
                percentage = configuration["featureManagement:featureB:percentage"],
            }
        }
    };
});

app.Run();