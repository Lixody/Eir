using System.Collections.Generic;
using System.IO;
using System.Text;
using PHPAnalysis.Annotations;
using PHPAnalysis.Utils;
using PHPAnalysis.Utils.Exceptions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace PHPAnalysis.Configuration
{
    public sealed class Config
    {
        public PHPConfiguration PHPSettings { get; private set; }
        public GraphConfiguration GraphSettings { get; private set; }
        public ComponentConfiguration ComponentSettings { get; private set; }
        public FuncSpecConfiguration FuncSpecSettings { get; private set; }

        private Config(ConfigurationMutable config)
        {
            Preconditions.NotNull(config, "config");

            PHPSettings = new PHPConfiguration(config.PHPConfiguration.PHPPath, config.PHPConfiguration.PHPParsePath, config.PHPConfiguration.PHPExts);
            GraphSettings = new GraphConfiguration(config.GraphConfiguration.GraphvizPath, config.GraphConfiguration.GraphvizArguments);
            ComponentSettings = new ComponentConfiguration(config.ComponentSettings.ComponentFolder, config.ComponentSettings.IncludeComponents);
            FuncSpecSettings = new FuncSpecConfiguration(config.FuncSpecSettings.PHPSpecs, config.FuncSpecSettings.ExtensionSpecs);
        }

        public static Config ReadConfiguration(string configPath)
        {
            Preconditions.NotNull(configPath, "configPath");
            var configInput = new StringReader(File.ReadAllText(configPath));
            var deserializer = new Deserializer(ignoreUnmatched: true);
            try
            {
                var config = deserializer.Deserialize<ConfigurationMutable>(configInput);
                return new Config(config);
            }
            catch (SyntaxErrorException e)
            {
                throw new ConfigurationParseException("Could not parse config file: " + configPath, e);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("PHP Settings: ");
            sb.AppendLine(PHPSettings.ToString());
            sb.AppendLine("Graph Settings: ");
            sb.AppendLine(GraphSettings.ToString());
            sb.AppendLine("Component Settings:");
            sb.AppendLine(ComponentSettings.ToString());
            return sb.ToString();
        }

        [UsedImplicitly]
        internal sealed class ConfigurationMutable
        {
            [YamlMember(Alias = "php-settings")]
            public PHPConfigurationMutable PHPConfiguration { get; set; }

            [YamlMember(Alias = "graph-settings")]
            public GraphConfigurationMutable GraphConfiguration { get; set; }

            [YamlMember(Alias = "component-settings")]
            public ComponentConfigurationMutable ComponentSettings { get; set; }

            [YamlMember(Alias = "func-spec-settings")]
            public FuncSpecConfigurationMutable FuncSpecSettings { get; set; }
        }
    }
}
