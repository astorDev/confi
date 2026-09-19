using System.CommandLine;
using Microsoft.Extensions.Configuration;

namespace Confi;

public static class CliConfiguration
{
    public record Source(string[] Args, params ICliConfigurationValueRule[] Rules) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(this);
    }

    public class Provider(Source Source) : ConfigurationProvider
    {
        public override void Load()
        {
            var parser = new Command("parser");
            foreach (var rule in Source.Rules) parser.Add(rule.Argument);

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
    
    public static void AddCli(this IConfigurationBuilder builder, string[] args, params ICliConfigurationValueRule[] rules) => builder.Add(new Source(args, rules));
}
