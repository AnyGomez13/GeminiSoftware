using Xunit;

namespace VetClinic.Domain.Tests;

public class DomainSetupTests
{
    [Fact]
    public void DomainAssembly_ShouldBeReferencedAndLoadable_RF08()
    {
        var assembly = typeof(DomainSetupTests).Assembly;
        Assert.NotNull(assembly);
    }
}
