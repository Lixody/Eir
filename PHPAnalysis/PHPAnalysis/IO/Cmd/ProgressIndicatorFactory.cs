using System;

namespace PHPAnalysis.IO.Cmd
{
    internal static class ProgressIndicatorFactory
    {
        public static ProgressIndicator CreateProgressIndicator(int maxValue)
        {
            Random rand = new Random();
            int r = rand.Next();
            ProgressIndicator progrssIndicator;

            switch (r % 4)
            {
                case 0:
                    progrssIndicator = new BatsFlyingAnimation(maxValue);
                    break;
                case 1:
                    progrssIndicator = new BikeGuyRidingAnimation(maxValue);
                    break;
                case 2:
                    progrssIndicator = new ExecutiveDeskToyAnimation(maxValue);
                    break;
                default:
                    progrssIndicator = new BarIndicator(maxValue);
                    break;
            }
            return progrssIndicator;
        }
    }
}