using NUnit.Framework;
using PHPAnalysis.Analysis.PHPDefinitions;
using PHPAnalysis.Configuration;

namespace PHPAnalysis.Tests.Analysis
{
    public abstract class ConfigDependentTests
    {
        protected Config Config;

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            Config = Config.ReadConfiguration(TestSettings.ConfigFile);
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            Config = null;
        }
    }
}
