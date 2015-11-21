using NUnit.Framework;
using PHPAnalysis.Analysis.PHPDefinitions;
using PHPAnalysis.Configuration;

namespace PHPAnalysis.Tests.Analysis
{
    public abstract class ConfigDependentTests
    {
        protected Config Config;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Config = Config.ReadConfiguration(TestSettings.ConfigFile);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Config = null;
        }
    }
}
