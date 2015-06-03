using System;

namespace PHPAnalysis.Utils
{
    internal static class EventExtensions
    {
        public static void RaiseEvent<T>(this EventHandler<T> myEvent, object sender, T e) 
            where T : EventArgs
        {
            if (myEvent != null)
            {
                myEvent(sender, e);
            }
        }

        public static void RaiseEvent(this EventHandler myEvent, object sender, EventArgs e)
        {
            if (myEvent != null)
            {
                myEvent(sender, e);
            }
        }
    }
}
