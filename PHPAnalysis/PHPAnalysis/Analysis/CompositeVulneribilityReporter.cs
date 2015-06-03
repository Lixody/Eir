using System.Collections.Generic;
using System.Linq;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis
{
    internal sealed class CompositeVulneribilityReporter : IVulnerabilityReporter
    {
        private readonly ICollection<IVulnerabilityReporter> _reporters = new List<IVulnerabilityReporter>();

        public uint NumberOfReportedVulnerabilities { get; private set; }

        public CompositeVulneribilityReporter(params IVulnerabilityReporter[] reporters)
        {
            Preconditions.NotNull(reporters, "reporters");
            _reporters.AddRange(reporters);
        }

        public CompositeVulneribilityReporter(IEnumerable<IVulnerabilityReporter> reporters)
        {
            Preconditions.NotNull(reporters, "reporters");
            _reporters.AddRange(reporters);
        }

        public void ReportVulnerability(IVulnerabilityInfo vulnerabilityInfo)
        {
            foreach (var vulnerabilityReporter in _reporters)
            {
                vulnerabilityReporter.ReportVulnerability(vulnerabilityInfo);
            }

            NumberOfReportedVulnerabilities++;
        }

        public void ReportStoredVulnerability(IVulnerabilityInfo[] vulnerabilityPathInfos)
        {
            foreach (var vulnerabilityReporter in _reporters)
            {
                vulnerabilityReporter.ReportStoredVulnerability(vulnerabilityPathInfos);
            }

            NumberOfReportedVulnerabilities++;
        }
    }
}