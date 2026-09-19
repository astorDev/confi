using System.CommandLine;

namespace Confi;

public static class CliOptionConfigurator
{
    public static ICliOptionConfigurator Configuring(this Option<bool?> flag, string key, string trueValue, string falseValue) => 
        new FlagOptionConfigurator(flag, key, trueValue, falseValue);

    public static ICliOptionConfigurator Configuring(this Option<string> option, string configurationKey) => 
        new MirroredOptionConfigurator(option, configurationKey);
}

public record FlagOptionConfigurator(Option<bool?> Flag, string ConfigurationKey, string TrueConfigurationValue, string FalseConfigurationValue) : ICliOptionConfigurator
{
    Option ICliOptionConfigurator.Option => Flag;

    public KeyValuePair<string, string>? Search(ParseResult parseResult)
    {
        var value = parseResult.GetValue(Flag);
        if (value == null || !value.HasValue)
        {
            return null;
        }

        return new KeyValuePair<string, string>(ConfigurationKey, value.Value ? TrueConfigurationValue : FalseConfigurationValue);
    }
}

public record MirroredOptionConfigurator(Option<string> Option, string ConfigurationKey) : ICliOptionConfigurator
{
    Option ICliOptionConfigurator.Option => Option;

    public KeyValuePair<string, string>? Search(ParseResult parseResult)
    {
        var value = parseResult.GetValue(Option);
        if (value == null)
        {
            return null;
        }

        return new KeyValuePair<string, string>(ConfigurationKey, value);
    }
}

public interface ICliOptionConfigurator
{
    public Option Option { get; }

    KeyValuePair<string, string>? Search(ParseResult parseResult);
}