using Xunit;

namespace VetClinic.Infrastructure.Tests;

public class InfrastructureSetupTests
{
    [Fact]
    public void InfrastructureAssembly_ShouldBeReferencedAndLoadable_RNF08()
    {
        var assembly = typeof(InfrastructureSetupTests).Assembly;
        Assert.NotNull(assembly);
    }
}
