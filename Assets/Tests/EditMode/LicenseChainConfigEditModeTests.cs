using LicenseChain.Unity;
using NUnit.Framework;

namespace LicenseChain.Unity.Tests
{
    public class LicenseChainConfigEditModeTests
    {
        [Test]
        public void DefaultConfig_HasExpectedBaseUrl()
        {
            var config = new LicenseChainConfig();
            Assert.AreEqual("https://api.licensechain.app/v1", config.BaseUrl);
        }

        [Test]
        public void DefaultConfig_TimeoutIsPositive()
        {
            var config = new LicenseChainConfig();
            Assert.Greater(config.Timeout, 0);
        }
    }
}
