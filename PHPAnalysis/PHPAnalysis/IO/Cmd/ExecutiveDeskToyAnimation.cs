using System;

namespace PHPAnalysis.IO.Cmd
{
    internal class ExecutiveDeskToyAnimation : AnimationIndicator
    {
        protected override string[] Steps
        {
            get
            {
                return new[] {
                                 @"╔════╤╤╤╤════╗" + Environment.NewLine +
                                 @"║    │││ \   ║" + Environment.NewLine +
                                 @"║    │││  O  ║" + Environment.NewLine +
                                 @"║    OOO     ║",
                                 @"╔════╤╤╤╤════╗" + Environment.NewLine +
                                 @"║    ││││    ║" + Environment.NewLine +
                                 @"║    ││││    ║" + Environment.NewLine +
                                 @"║    OOOO    ║",
                                 @"╔════╤╤╤╤════╗" + Environment.NewLine +
                                 @"║   / │││    ║" + Environment.NewLine +
                                 @"║  O  │││    ║" + Environment.NewLine +
                                 @"║     OOO    ║",
                                 @"╔════╤╤╤╤════╗" + Environment.NewLine +
                                 @"║    ││││    ║" + Environment.NewLine +
                                 @"║    ││││    ║" + Environment.NewLine +
                                 @"║    OOOO    ║"
                             };
            }
        }

        protected override string FinalStep
        {
            get { return Steps[1]; }
        }

        public ExecutiveDeskToyAnimation(int max = 100) : base(max)
        {

        }
    }
}