namespace Confi;

public record Version(string Value)
{
    public static implicit operator string(Version version) => version.Value;

    public static bool ImpliesUpdate(string? currentVersion, string candidateVersion)
    {
        if (currentVersion == null)
        {
            return true;
        }

        return new Version(candidateVersion) >= new Version(currentVersion);
    }

    public static bool operator >(Version left, Version right) => 
        String.Compare(left.Value, right.Value) > 0;

    public static bool operator <(Version left, Version right) => 
        String.Compare(left.Value, right.Value) < 0;

    public static bool operator >=(Version left, Version right) => 
        String.Compare(left.Value, right.Value) >= 0;

    public static bool operator <=(Version left, Version right) => 
        String.Compare(left.Value, right.Value) <= 0;
}