using System.Net;
using NUnit.Framework;

namespace Shuttle.ContentStore.Tests.Integration
{
    [SetUpFixture]
    public class Global
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }
    }
}