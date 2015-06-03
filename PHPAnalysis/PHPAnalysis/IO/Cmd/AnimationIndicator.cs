using System;
using System.Threading;

namespace PHPAnalysis.IO.Cmd
{
    internal abstract class AnimationIndicator : ProgressIndicator
    {
        protected abstract string[] Steps { get; }
        protected abstract string FinalStep { get; }
        public int Max { get; private set; }
        public int Min { get; private set; }

        private int _progress = 0;
        private int counter = 0;
        private Timer timer;
        private readonly int _cursorTopStart;

        protected AnimationIndicator(int max = 100)
        {
            this.Min = 0;
            this.Max = max;
            if (Console.CursorLeft != 0)
                Console.WriteLine();
            this._cursorTopStart = Console.CursorTop;
        }

        public override sealed void Step()
        {
            if (_progress >= Max) { return; }
            if (counter == 0)
            {
                timer = new Timer(RunAnimation, null, 0, 250);
            }

            _progress++;
            Stepped(_progress);
            if (_progress == Max)
            {
                timer.Dispose();
                Console.SetCursorPosition(0, _cursorTopStart);
                Console.Write(FinalStep);
                Console.Write(" " + (int)PercentOf(_progress, Max) + "%");
                Console.Write(Environment.NewLine);
            }
        }

        private void RunAnimation(object stateInfo)
        {
            Console.SetCursorPosition(0, _cursorTopStart);
            counter++;
            Console.Write(Steps[counter % Steps.Length]);
            Console.Write(" " + (int)PercentOf(_progress, Max) + "%");
        }

        private static float PercentOf(int value, int target)
        {
            return ((value / (float)target) * 100);
        }

        protected virtual void Stepped(int progress) { }
    }
}