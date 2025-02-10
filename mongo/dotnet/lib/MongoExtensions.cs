using MongoDB.Bson;

namespace Confi;

public static class MongoConfigurationExtensions
{
    public static Dictionary<string, object> ToConfigurationDictionary(this BsonDocument document, string prefix = "")
    {
        var configs = new Dictionary<string, object>();
        configs.EnrichFromBsonDocument(document);
        return configs;
    }

    private static void EnrichFromBsonDocument(this Dictionary<string, object> configs, BsonDocument document, string prefix = "")
    {
        foreach (var pair in document)
        {
            if (pair.Value.IsBsonDocument)
            {
                configs.EnrichFromBsonDocument(pair.Value.AsBsonDocument, prefix + pair.Name + ":");
            }
            else if (pair.Value.IsBsonArray)
            {
                var array = pair.Value.AsBsonArray;
                for (var i = 0; i < array.Count; i++)
                {
                    configs.EnrichFromBsonDocument(array[i].AsBsonDocument, prefix + pair.Name + ":" + i + ":");
                }
            }
            else
            {
                configs[prefix + pair.Name] = pair.Value.ToJson();
            }
        }
    }
}