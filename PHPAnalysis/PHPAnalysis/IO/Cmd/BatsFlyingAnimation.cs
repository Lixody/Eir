namespace PHPAnalysis.IO.Cmd
{
    internal class BatsFlyingAnimation : AnimationIndicator
    {
        protected override string[] Steps
        {
            get
            {
                return new[] {
                                 @"                   /^v^\
         /^v^\                      /^v^\
                /^v^\

  /^v^\                                    ",
                                 @"                   \^v^/
         \^v^/                      \^v^/
                \^v^/

  \^v^/                                    "
                             };

            }
        }

        protected override string FinalStep
        {
            get { return Steps[1]; }
        }

        public BatsFlyingAnimation(int max = 100) : base(max)
        {
        }
    }
}