public static class ConfigurationData
{
    public static bool IsIdentical(Dictionary<string, object> left, Dictionary<string, object> right)
    {
        foreach (var pairFromLeft in left)
        {
            if (!right.TryGetValue(pairFromLeft.Key, out var valueFromRight))
            {
                return false;
            }

            if (!pairFromLeft.Value.Equals(valueFromRight))
            {
                return false;
            }
        }
        
        foreach (var pairFromRight in right)
        {
            if (!left.TryGetValue(pairFromRight.Key, out var valueFromLeft))
            {
                return false;
            }

            if (!pairFromRight.Value.Equals(valueFromLeft))
            {
                return false;
            }
        }
        
        return true;
    }
}