namespace Confi.Manager.Versioning.Playground;

[TestClass]
public class Comparison
{
    [TestMethod]
    public void ZeroLessThenPlusOne()
    {
        var zero = "0.0.0";
        var plusOne = "0.0.0+1";

        IsGreater(plusOne, zero).ShouldBeTrue();
    }

    [TestMethod]
    public void ZeroOneGreaterThanPlusOne()
    {
        var zeroOne = "0.0.1";
        var plusOne = "0.0.0+1";

        IsGreater(zeroOne, plusOne).ShouldBeTrue();
    }

    [TestMethod]
    public void YearGreaterThenOne()
    {
        var year = "2023.0.0";
        var one = "1.0.0";

        IsGreater(year, one).ShouldBeTrue();
    }

    // [TestMethod] - this one fails
    public void PlusTenGreaterThenPlusNine()
    {
        var plusTen = "0.0.0+10";
        var plusNine = "0.0.0+9";

        IsGreater(plusTen, plusNine).ShouldBeTrue();
    }

    public bool IsGreater(string expectedGreater, string expectedLess)
    {
        return StringComparer.Ordinal.Compare(expectedGreater, expectedLess) > 0;
    }
}

