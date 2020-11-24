using Shuttle.ContentStore.Opswat;
using NUnit.Framework;

namespace Shuttle.ContentStore.Tests.Integration.Opswat
{
    [TestFixture]
    public class OpswatConfigurationFixture
    {
        [Test]
        public void Should_be_able_to_use_sandbox_file_extensions()
        {
            var configuration = new OpswatConfiguration("api-url", "api-key", string.Empty);

            Assert.That(configuration.SandboxFileExtensions, Is.EqualTo(string.Empty));
            Assert.That(configuration.ShouldSandbox("anything"), Is.False);
            
            configuration = new OpswatConfiguration("api-url", "api-key", null);

            Assert.That(configuration.SandboxFileExtensions, Is.EqualTo(string.Empty));
            Assert.That(configuration.ShouldSandbox("anything"), Is.False);
            
            configuration = new OpswatConfiguration("api-url", "api-key", ".fe1,fe2;fe3;.fe4");

            Assert.That(configuration.SandboxFileExtensions, Is.EqualTo(".fe1,fe2;fe3;.fe4"));
            Assert.That(configuration.ShouldSandbox("anything"), Is.False);
            Assert.That(configuration.ShouldSandbox(".fe1"), Is.True);
            Assert.That(configuration.ShouldSandbox("fe2"), Is.True);
            Assert.That(configuration.ShouldSandbox(".fe3"), Is.True);
            Assert.That(configuration.ShouldSandbox("fe4"), Is.True);
        }
    }
}