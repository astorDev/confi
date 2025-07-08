using System.Text;
using Microsoft.Extensions.Configuration;

namespace Confi;

public static class JsonStringConfiguration
{
    public class Source(string json) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(json);
    }

    public class Provider(string json) : ConfigurationProvider
    {
        public override void Load()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            Data = JsonConfigurationStreamParser.Parse(stream);
        }
    }
}


public static class Registration
{
    public static IConfigurationBuilder AddJsonString(this IConfigurationBuilder builder, string json)
    {
        return builder.Add(new JsonStringConfiguration.Source(json));
    }
}
