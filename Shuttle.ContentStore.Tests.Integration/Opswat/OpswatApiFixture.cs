using Shuttle.ContentStore.Opswat;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;

namespace Shuttle.ContentStore.Tests.Integration.Opswat
{
    [TestFixture]
    public class OpswatApiFixture
    {
        [Test]
        public void Should_be_able_to_retrieve_well_known_hash()
        {
            var configuration = OpswatSection.Configuration();
            var api = new OpswatApi(configuration);

            var response =
                api.GetResponse(
                    new RestRequest(api.GetFullApiUrl("hash/6A5C19D9FFE8804586E8F4C0DFCC66DE"),
                        DataFormat.Json))
                    .AsDynamic();

            Assert.That(response.scan_results.scan_all_result_a.ToString(), Is.EqualTo("Infected"));
        }

        [Test]
        public void Should_be_able_to_get_valid_api_url_paths()
        {
            const string expected = "http://apiurl/path";

            var api = new OpswatApi(new OpswatConfiguration("http://apiurl/", "api-key", string.Empty));

            Assert.That(api.GetFullApiUrl("/path"), Is.EqualTo(expected));
            Assert.That(api.GetFullApiUrl("path"), Is.EqualTo(expected));

            api = new OpswatApi(new OpswatConfiguration("http://apiurl", "api-key", string.Empty));

            Assert.That(api.GetFullApiUrl("/path"), Is.EqualTo(expected));
            Assert.That(api.GetFullApiUrl("path"), Is.EqualTo(expected));
        }
    }
}