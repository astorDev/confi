namespace Confi.Manager.Tests;

[TestClass]
public class AboutShould : Test
{
    [TestMethod]
    public async Task ReturnValidMetadata()
    {
        var about = await this.Client.GetAbout();
        about.ShouldBe(new(
            "Confi.Manager",
            "1.0.0.0",
            "Development",
            
        ));
    }
}