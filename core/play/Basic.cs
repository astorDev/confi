namespace Confi.Core.Playground;

[TestClass]
public class Basic
{
    [TestMethod]
    public void StraightValue()
    {
        ConfigurationValue<int> value = 42;

        value.Resolve(null!).ShouldBe(42);
    }

    [TestMethod]
    public void ResolverFunction()
    {
        Func<string> value = () => "Hello, World!";
        ConfigurationValue<string> value2 = value;
        value2.Resolve(null!).ShouldBe("Hello, World!");
    }
}