using System;
using System.Threading;
using Shuttle.ContentStore.McAfee;
using NUnit.Framework;
using RestSharp;

namespace Shuttle.ContentStore.Tests.Integration.McAfee
{
    [TestFixture]
    public class McAfeeApiFixture
    {
        [Test]
        public void Should_be_able_to_get_list_of_analyzers()
        {
            using (var api = new McAfeeApi(McAfeeSection.Configuration()))
            {
                var response = api.GetResponse(new RestRequest(api.GetFullApiUrl("vmprofiles.php"))).AsDynamic();
            }
        }

        [Test]
        public void Should_be_able_to_get_valid_api_url_paths()
        {
            const string expected = "http://apiurl/path";

            var api = new McAfeeApi(new McAfeeConfiguration("http://apiurl/", "user", "pwd", TimeSpan.MaxValue,
                "analyzer-profile", 3, TimeSpan.MaxValue, TimeSpan.MaxValue));

            Assert.That(api.GetFullApiUrl("/path"), Is.EqualTo(expected));
            Assert.That(api.GetFullApiUrl("path"), Is.EqualTo(expected));

            api = new McAfeeApi(new McAfeeConfiguration("http://apiurl", "user", "pwd", TimeSpan.MaxValue,
                "analyzer-profile", 3, TimeSpan.MaxValue, TimeSpan.MaxValue));

            Assert.That(api.GetFullApiUrl("/path"), Is.EqualTo(expected));
            Assert.That(api.GetFullApiUrl("path"), Is.EqualTo(expected));
        }

        [Test]
        public void Should_be_able_to_manage_session()
        {
            var sectionConfiguration = McAfeeSection.Configuration();
            var configuration = new McAfeeConfiguration(sectionConfiguration.Url, sectionConfiguration.Username,
                sectionConfiguration.Password, TimeSpan.FromSeconds(2), "analyzer-profile", 3, TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5));
            var loginCompletedCount = 0;
            var logoutCompletedCount = 0;
            var heartbeatCompletedCount = 0;

            using (var api = new McAfeeApi(configuration))
            {
                Assert.That(api.HasSession, Is.False);

                api.LoginCompleted += (sender, args) => loginCompletedCount++;
                api.LogoutCompleted += (sender, args) => logoutCompletedCount++;
                api.HeartbeatCompleted += (sender, args) => heartbeatCompletedCount++;

                api.Login();

                Assert.That(loginCompletedCount, Is.EqualTo(1));

                var wait = DateTime.Now.AddSeconds(15);

                while (DateTime.Now < wait)
                {
                    Thread.Sleep(250);

                    if (logoutCompletedCount == loginCompletedCount)
                    {
                        api.Login();
                    }
                }

                Assert.That(heartbeatCompletedCount, Is.GreaterThanOrEqualTo(5));
            }

            Assert.That(loginCompletedCount, Is.GreaterThanOrEqualTo(5));
            Assert.That(logoutCompletedCount, Is.GreaterThanOrEqualTo(5));
        }
    }
}
