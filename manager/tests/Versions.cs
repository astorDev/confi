namespace Confi.Manager.Tests;

[TestClass]
public class Versions
{
    [TestMethod]
    public void Compare()
    {
        var version1 = new Version("1.0.0");
        var version2 = new Version("1.0.1");
        var version3 = new Version("2025.103.109");
        var version4 = new Version("2025.123.109");

        Assert.IsTrue(version1 < version2);
        Assert.IsTrue(version2 > version1);
        Assert.IsTrue(version1 < version3);
        Assert.IsTrue(version4 >= version3);
    }
}