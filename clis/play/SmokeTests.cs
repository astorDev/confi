namespace Confi.Clis.Playground;

[TestClass]
public class SmokeTests
{
    [TestMethod]
    public void Basic()
    {
        var hello = "Hello, Tests!";

        Console.WriteLine(hello);

        hello.ShouldBe("Hello, Tests!");
    }
}