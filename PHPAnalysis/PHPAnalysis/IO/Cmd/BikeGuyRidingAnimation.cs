using System;

namespace PHPAnalysis.IO.Cmd
{
    internal class BikeGuyRidingAnimation : AnimationIndicator
    {
        private string[] UseThisStep { get; set; }
        private int Progrezz { get; set; }
        protected override string[] Steps
        {
            get { return DoBikeGuy(); }
        }

        protected override string FinalStep
        {
            get
            {
                var indentation = new string(' ', GetBikePosition());
                return indentation + @"        PHEW- o  " + Environment.NewLine +
                       indentation + @"            _ \<,_" + Environment.NewLine +
                       indentation + @"           (*)| (*)";
            }
        }

        public BikeGuyRidingAnimation(int max = 100)
            : base(max)
        {
            UseThisStep = new[] {@"
   ---------- __o
  --------  _ \<,_
-------    (*)/ (*)",@"
   ---------- __o
  --------  _ \<,_
-------    (*) \(*)",
                                };
        }

        private string[] DoBikeGuy()
        {
            var indentation = new string(' ', GetBikePosition());
            UseThisStep = new[] { indentation + @"   ---------- __o" + Environment.NewLine +
                                  indentation + @"  --------  _ \<,_" + Environment.NewLine +
                                  indentation + @"-------    (*)/ (*)",
                                  indentation + @"   ---------- __o" + Environment.NewLine +
                                  indentation + @"  --------  _ \<,_" + Environment.NewLine +
                                  indentation + @"-------    (*) \(*)",
                                };
            
            return UseThisStep;
        }

        protected override void Stepped(int progress)
        {
            Progrezz = progress;
        }

        private int GetBikePosition()
        {
            int windowWidth = System.Console.WindowWidth - 25;
            int i = (int)((Progrezz / (float)Max) * windowWidth);
            return i;
        }
    }
}