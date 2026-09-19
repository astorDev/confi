using System.CommandLine;
using Microsoft.Extensions.Configuration;

namespace Confi;

public static class CliOptionsConfiguration
{
    public record Source(string[] Args, params ICliOptionConfigurator[] Rules) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(this);
    }

    public class Provider(Source Source) : ConfigurationProvider
    {
        public override void Load()
        {
            var parser = new Command("parser");
            foreach (var rule in Source.Rules) parser.Add(rule.Option);

            var parseResult = parser.Parse(Source.Args);
            foreach (var rule in Source.Rules)
            {
                var result = rule.Search(parseResult);
                if (result.HasValue)
                {
                    Data[result.Value.Key] = result.Value.Value;
                }
            }
        }
    }
    
    public static void AddCliOptions(this IConfigurationBuilder builder, string[] args, params ICliOptionConfigurator[] rules) => builder.Add(new Source(args, rules));
}
