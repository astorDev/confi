using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Confi.Tests;

[TestClass]
public class FluenvRead
{
    IConfiguration Configuration =>
        new ConfigurationBuilder()
            .AddFluentEnvironmentVariables("FLUENV_TEST_")
            .Build();
    
    [TestMethod]
    public void WebhookAddresses()
    {
        Environment.SetEnvironmentVariable("FLUENV_TEST_WEBHOOK_ADDRESSES", "wha");

        Console.WriteLine(Configuration["WebhookAddresses"]);
        
        Configuration["WebhookAddresses"].ShouldBe("wha");
    }

    [TestMethod]
    public void InMicrosoftFormat()
    {
        Environment.SetEnvironmentVariable("FLUENV_TEST_MicrosoftFormat__Variable", "ms");

        Console.WriteLine(this.Configuration["MicrosoftFormat:Variable"]);
        
        var section = this.Configuration.GetSection("MicrosoftFormat");
        
        section["Variable"].ShouldBe("ms");
    }

    [TestMethod]
    public void InUnderscoreSeparatedSection()
    {
        Environment.SetEnvironmentVariable("FLUENV_TEST_SECTION_A_VARIABLE_ONE", "ao");
        Environment.SetEnvironmentVariable("FLUENV_TEST_SECTION_A_VARIABLE_TWO", "at");

        var sectionA = this.Configuration.GetSection("SectionA");
        
        sectionA["VariableOne"].ShouldBe("ao");
        sectionA["VariableTwo"].ShouldBe("at");
    }

    [TestMethod]
    public void InDoubleUnderscoreSeparatedSection()
    {
        Environment.SetEnvironmentVariable("FLUENV_TEST_SECTION_B__VARIABLE_ONE", "bo");
        Environment.SetEnvironmentVariable("FLUENV_TEST_SECTION_B__VARIABLE_TWO", "bt");
        
        var sectionB = this.Configuration.GetSection("SectionB");
        
        sectionB["VariableOne"].ShouldBe("bo");
        sectionB["VariableTwo"].ShouldBe("bt");
    }
    
    [TestMethod]
    public void BindableConfigurationSection()
    {
        Environment.SetEnvironmentVariable("FLUENV_TEST_SECTION_C_VARIABLE_ONE", "bo");
        Environment.SetEnvironmentVariable("FLUENV_TEST_SECTION_C_VARIABLE_TWO", "bt");
        
        var sectionB = this.Configuration.GetSection("SectionC").Get<ExampleSection>() ?? throw new ("Unable to bind");
        
        sectionB.VariableOne.ShouldBe("bo");
        sectionB.VariableTwo.ShouldBe("bt");
    }

    public class ExampleSection
    {
        public string VariableOne { get; set; } = null!;
        public string VariableTwo { get; set; } = null!;
    }
}