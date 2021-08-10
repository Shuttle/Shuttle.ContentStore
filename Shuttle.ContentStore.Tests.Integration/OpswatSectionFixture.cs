using System;
using System.IO;
using Shuttle.ContentStore.Opswat;
using NUnit.Framework;
using Shuttle.Core.Configuration;

namespace Shuttle.ContentStore.Tests.Integration.Opswat
{
    [TestFixture]
    public class ContentSectionFixture
    {
        [Test]
        [TestCase("ContentService.config")]
        [TestCase("ContentService-Grouped.config")]
        public void Should_be_able_to_load_the_configuration(string file)
        {
            var section = ConfigurationSectionProvider.OpenFile<OpswatSection>("shuttle", "contentService",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $".config\\{file}"));

            Assert.That(section, Is.Not.Null);
            Assert.That(section.ApiUrl, Is.EqualTo("api-url"));
        }

	}
}