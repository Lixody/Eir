using System;
using PHPAnalysis.Components;
using PHPAnalysis.Configuration;

namespace PHPAnalysis.Analysis
{
    public interface IAnalysisStartingListener
    {
        void AnalysisStarting(object o, AnalysisStartingEventArgs e);
    }

    public interface IAnalysisEndedListener
    {
        void AnalysisEnding(object o, AnalysisEndedEventArgs e);
    }

    public sealed class AnalysisEndedEventArgs : EventArgs
    {
        public TimeSpan TimeElapsed { get; private set; }

        public AnalysisEndedEventArgs(TimeSpan timeElapsed)
        {
            this.TimeElapsed = timeElapsed;
        }
    }

    public sealed class AnalysisStartingEventArgs : EventArgs
    {
        public Config Configuration { get; private set; }
        public Arguments Arguments { get; private set; }

        public AnalysisStartingEventArgs(Config config, Arguments arguments)
        {
            this.Configuration = config;
            this.Arguments = arguments;
        }
    }
}