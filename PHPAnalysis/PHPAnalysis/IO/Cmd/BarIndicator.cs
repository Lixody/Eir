using System;

namespace PHPAnalysis.IO.Cmd
{
    internal sealed class BarIndicator : ProgressIndicator
    {
        public int Max { get; private set; }
        public int Min { get; private set; }

        private readonly int _barWidth;
        private int _progress = 0;

        private int ProgressPosition { get { return _barWidth + 3; } }

        public BarIndicator(int max = 100)
        {
            this.Min = 0;
            this.Max = max;
            this._barWidth = 32;
        }

        public override void Step()
        {
            if (_progress >= Max) { return; }

            _progress++;
            UpdateProgress(_progress, Max);
            if (_progress == Max)
            {
                System.Console.Write(Environment.NewLine);
            }
        }

        private void UpdateProgress(int progress, int total)
        {
            DrawEmptyProgressBar();
            System.Console.CursorLeft = 1;

            DrawBar(progress, total);

            DrawProcent(progress, total);
        }

        private void DrawBar(int progress, int total)
        {
            System.Console.CursorLeft = 1;

            float howManyToPrint = (progress / (float)total) * (_barWidth - 2);

            System.Console.Write(new string('#', (int)howManyToPrint));

        }

        private void DrawEmptyProgressBar()
        {
            System.Console.CursorLeft = 0;
            System.Console.Write("[" + new string('-', _barWidth - 2) + "]");
        }

        private void DrawProcent(int progress, int total)
        {
            System.Console.CursorLeft = ProgressPosition;
            System.Console.Write((int)PercentOf(progress, total) + "%");
        }

        private float PercentOf(int value, int target)
        {
            return ((value / (float)target) * 100);
        }
    }
}