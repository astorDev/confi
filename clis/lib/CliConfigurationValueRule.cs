using System.CommandLine;

namespace Confi;

public class CliConfig
{
    public static CliConfigurationValueRule<bool> Flag(Option<bool> flag, string configurationKey, string trueConfigurationValue, string falseConfigurationValue) => new(
        flag,
        value => new KeyValuePair<string, string>(configurationKey, value ? trueConfigurationValue : falseConfigurationValue)
    );

    public static CliConfigurationValueRule<string> Mirrored(Option<string> option, string configurationKey) => new(
        option,
        value => new KeyValuePair<string, string>(configurationKey, value)
    );
}


public record CliConfigurationValueRule<T>(Option<T> Argument, Func<T, KeyValuePair<string, string>> resolve) : ICliConfigurationValueRule
{
    Option ICliConfigurationValueRule.Argument => Argument;

    public KeyValuePair<string, string>? Search(ParseResult parseResult)
    {
        var value = parseResult.GetValue<T>(Argument);
        if (value == null)
        {
            return null;
        }

        return resolve(value);
    }
}

public interface ICliConfigurationValueRule
{
    public Option Argument { get; }

    KeyValuePair<string, string>? Search(ParseResult parseResult);
}